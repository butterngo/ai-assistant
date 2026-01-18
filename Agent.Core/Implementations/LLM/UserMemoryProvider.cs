using Agent.Core.Models;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using System.Text;
using System.Text.Json;

namespace Agent.Core.Implementations.LLM;

public sealed class UserMemoryProvider : AIContextProvider
{
	public UserInfo UserInfo { get; set; }

	// Constructor for new threads
	public UserMemoryProvider(UserInfo? userInfo = null)
	{
		UserInfo = userInfo ?? new UserInfo();
	}

	// Constructor for deserialization (required!)
	public UserMemoryProvider(
		IChatClient chatClient,
		JsonElement serializedState,
		JsonSerializerOptions? jsonSerializerOptions = null)
	{
		// Restore state from serialized data
		UserInfo = serializedState.ValueKind == JsonValueKind.Object
			? serializedState.Deserialize<UserInfo>(jsonSerializerOptions) ?? new UserInfo()
			: new UserInfo();
	}

	

	/// <summary>
	/// Called BEFORE the agent invokes the LLM
	/// Inject memories/context into the request
	/// </summary>
	public override async ValueTask<AIContext?> InvokingAsync(
		InvokingContext context,
		CancellationToken cancellationToken = default)
	{
		var aiContext = new AIContext();

		var contextBuilder = new StringBuilder();

		if (!string.IsNullOrEmpty(UserInfo.Name))
		{
			contextBuilder.AppendLine($"User's name is {UserInfo.Name}.");
		}

		if (!string.IsNullOrEmpty(UserInfo.PreferredLanguage))
		{
			contextBuilder.AppendLine($"User prefers responses in {UserInfo.PreferredLanguage}.");
		}

		if (UserInfo.Interests.Count > 0)
		{
			contextBuilder.AppendLine($"User is interested in: {string.Join(", ", UserInfo.Interests)}.");
		}

		foreach (var pref in UserInfo.Preferences)
		{
			contextBuilder.AppendLine($"User preference - {pref.Key}: {pref.Value}.");
		}

		if (contextBuilder.Length > 0)
		{
			aiContext.Instructions = $"Known information about the user:\n{contextBuilder}";
		}

		if (UserInfo.ConnectionTool != null)
		{
			aiContext.Tools = await UserInfo.ConnectionTool.GetToolsAsync(cancellationToken);
		}

		return aiContext;
	}

	/// <summary>
	/// Called AFTER the agent receives LLM response
	/// Extract and store memories from the conversation
	/// </summary>
	public override ValueTask InvokedAsync(
		InvokedContext context,
		CancellationToken cancellationToken = default)
	{
		return base.InvokedAsync(context, cancellationToken);
	}

	/// <summary>
	/// Serialize state for thread persistence
	/// </summary>
	public override JsonElement Serialize(JsonSerializerOptions? jsonSerializerOptions = null)
	{
		return JsonSerializer.SerializeToElement(UserInfo, jsonSerializerOptions);
	}
}
