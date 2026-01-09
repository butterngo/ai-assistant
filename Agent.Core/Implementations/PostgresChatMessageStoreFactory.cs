using Microsoft.Agents.AI;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Agent.Core.Implementations;

public class PostgresChatMessageStoreFactory
{
	private readonly IDbContextFactory<ChatDbContext> _dbContextFactory;
	private readonly int _maxMessages;

	public PostgresChatMessageStoreFactory(
		IDbContextFactory<ChatDbContext> dbContextFactory,
		int maxMessages = 50)
	{
		_dbContextFactory = dbContextFactory ?? throw new ArgumentNullException(nameof(dbContextFactory));
		_maxMessages = maxMessages;
	}

	/// <summary>
	/// Creates a new ChatMessageStore for a new conversation.
	/// Use this as the chat_message_store_factory delegate for ChatClientAgent.
	/// </summary>
	public ChatMessageStore Create()
	{
		return new PostgresChatMessageStore(_dbContextFactory, _maxMessages);
	}

	/// <summary>
	/// Creates a ChatMessageStore from serialized state (for resuming threads).
	/// </summary>
	public ChatMessageStore Create(JsonElement serializedState, JsonSerializerOptions? options = null)
	{
		return new PostgresChatMessageStore(_dbContextFactory, serializedState, options);
	}

	/// <summary>
	/// Returns a Func delegate suitable for ChatClientAgentOptions.ChatMessageStoreFactory
	/// </summary>
	public Func<ChatMessageStore> AsFactory() => Create;
}
