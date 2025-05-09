namespace BotSharp.Abstraction.MLTasks.Settings;

public class LlmModelSetting
{
    /// <summary>
    /// Model Id, like "gpt-4", "gpt-4o", "o1".
    /// </summary>
    public string Id { get; set; } = null!;

    /// <summary>
    /// Deployment model name
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// Model version
    /// </summary>
    public string Version { get; set; } = "1106-Preview";

    /// <summary>
    /// Api version
    /// </summary>
    public string? ApiVersion { get; set; }

    /// <summary>
    /// Deployment same functional model in a group.
    /// It can be used to deploy same model in different regions.
    /// </summary>
    public string? Group { get; set; }

    public string ApiKey { get; set; } = null!;
    public string? Endpoint { get; set; }
    public LlmModelType Type { get; set; } = LlmModelType.Chat;

    /// <summary>
    /// If true, allow sending images/vidoes to this model
    /// </summary>
    public bool MultiModal { get; set; }

    /// <summary>
    /// If true, allow generating images
    /// </summary>
    public bool ImageGeneration { get; set; }

    /// <summary>
    /// Embedding dimension
    /// </summary>
    public int Dimension { get; set; }

    public LlmCost Cost { get; set; } = new();

    public override string ToString()
    {
        return $"[{Type}] {Name} {Endpoint}";
    }
}

/// <summary>
/// Cost per 1K tokens
/// </summary>
public class LlmCost
{
    // Input
    public float TextInputCost { get; set; } = 0f;
    public float CachedTextInputCost { get; set; } = 0f;
    public float AudioInputCost { get; set; } = 0f;
    public float CachedAudioInputCost { get; set; } = 0f;

    // Output
    public float TextOutputCost { get; set; } = 0f;
    public float AudioOutputCost { get; set; } = 0f;
}

public enum LlmModelType
{
    Text = 1,
    Chat = 2,
    Image = 3,
    Embedding = 4,
    Audio = 5,
    Realtime = 6,
}
