namespace Agent.Core.Options;

public enum LLMProviderType
{
	OpenAI,
	AzureOpenAI,
	HuggingFace,
	Anthropic
}

public class LLMProviderOptions
{
	public static string SectionName => "LLMProviders";
	public OpenAIOptions OpenAI { get; set; } = new();
	public AzureOpenAIOptions AzureOpenAI { get; set; } = new();
	public HuggingFaceOptions HuggingFace { get; set; } = new();
	public AnthropicOptions Anthropic { get; set; } = new();
}

public class OpenAIOptions
{
	public string? ApiKey { get; set; }
	public string? ModelId { get; set; } = "gpt-4";
	public string? Endpoint { get; set; } = "https://api.openai.com/v1";
	public int MaxTokens { get; set; } = 2000;
	public double Temperature { get; set; } = 0.7;
}

public class AzureOpenAIOptions
{
	public string? Endpoint { get; set; }

	public string? ApiKey { get; set; }

	public string? ModelId { get; set; }
}

public class AnthropicOptions
{
	public string? Endpoint { get; set; }
	public string? ApiKey { get; set; }
	public string? ModelId { get; set; }
}

public class HuggingFaceOptions
{
	public string? Endpoint { get; set; }

	public string? ApiKey { get; set; }
}
