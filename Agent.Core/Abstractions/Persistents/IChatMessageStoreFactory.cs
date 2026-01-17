using Microsoft.Agents.AI;
using System.Text.Json;

namespace Agent.Core.Abstractions.Persistents;

public interface IChatMessageStoreFactory
{
	/// <summary>
	/// Creates a ChatMessageStore from serialized state (for resuming threads).
	/// </summary>
	ChatMessageStore Create(JsonElement serializedState, JsonSerializerOptions? options = null);
}
