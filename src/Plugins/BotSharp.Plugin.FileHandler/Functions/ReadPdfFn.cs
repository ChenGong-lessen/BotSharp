using BotSharp.Abstraction.Routing;

namespace BotSharp.Plugin.FileHandler.Functions;

public class ReadPdfFn : IFunctionCallback
{
    public string Name => "util-file-read_pdf";
    public string Indication => "Reading pdf";

    private readonly IServiceProvider _services;
    private readonly ILogger<ReadPdfFn> _logger;

    private readonly IEnumerable<string> _pdfContentTypes = new List<string>
    {
        MediaTypeNames.Application.Pdf
    };

    public ReadPdfFn(
        IServiceProvider services,
        ILogger<ReadPdfFn> logger)
    {
        _services = services;
        _logger = logger;
    }

    public async Task<bool> Execute(RoleDialogModel message)
    {
        var args = JsonSerializer.Deserialize<LlmContextIn>(message.FunctionArgs);
        var conv = _services.GetRequiredService<IConversationService>();
        var routingCtx = _services.GetRequiredService<IRoutingContext>();
        var agentService = _services.GetRequiredService<IAgentService>();

        Agent? fromAgent = null;
        if (!string.IsNullOrEmpty(message.CurrentAgentId))
        {
            fromAgent = await agentService.LoadAgent(message.CurrentAgentId);
        }

        var agent = new Agent
        {
            Id = BuiltInAgentId.UtilityAssistant,
            Name = "Utility Agent",
            Instruction = fromAgent?.Instruction ?? args?.UserRequest ?? "Please describe the pdf file(s).",
            TemplateDict = new Dictionary<string, object>()
        };

        var wholeDialogs = routingCtx.GetDialogs();
        if (wholeDialogs.IsNullOrEmpty())
        {
            wholeDialogs = conv.GetDialogHistory();
        }

        var dialogs = await AssembleFiles(conv.ConversationId, wholeDialogs);
        var response = await GetChatCompletion(agent, dialogs);
        message.Content = response;
        return true;
    }

    private async Task<List<RoleDialogModel>> AssembleFiles(string conversationId, List<RoleDialogModel> dialogs)
    {
        if (dialogs.IsNullOrEmpty())
        {
            return new List<RoleDialogModel>();
        }

        var fileStorage = _services.GetRequiredService<IFileStorageService>();
        var messageIds = dialogs.Select(x => x.MessageId).Distinct().ToList();
        var screenshots = await fileStorage.GetMessageFileScreenshotsAsync(conversationId, messageIds);

        if (screenshots.IsNullOrEmpty()) return dialogs;

        foreach (var dialog in dialogs)
        {
            var found = screenshots.Where(x => x.MessageId == dialog.MessageId).ToList();
            if (found.IsNullOrEmpty()) continue;

            dialog.Files = found.Select(x => new BotSharpFile
            {
                ContentType = x.ContentType,
                FileUrl = x.FileUrl,
                FileStorageUrl = x.FileStorageUrl
            }).ToList();
        }

        return dialogs;
    }

    private async Task<string> GetChatCompletion(Agent agent, List<RoleDialogModel> dialogs)
    {
        try
        {
            var llmProviderService = _services.GetRequiredService<ILlmProviderService>();
            var provider = llmProviderService.GetProviders().FirstOrDefault(x => x == "openai");
            var model = llmProviderService.GetProviderModel(provider: provider, id: "gpt-4o", multiModal: true);
            var completion = CompletionProvider.GetChatCompletion(_services, provider: provider, model: model.Name);
            var response = await completion.GetChatCompletions(agent, dialogs);
            return response.Content;
        }
        catch (Exception ex)
        {
            var error = $"Error when analyzing pdf file(s).";
            _logger.LogWarning(ex, $"{error}");
            return error;
        }
    }
}
