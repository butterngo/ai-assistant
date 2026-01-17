using System.Text.Json;
using Microsoft.Agents.AI;
using Microsoft.EntityFrameworkCore;
using Agent.Core.Abstractions.Persistents;

namespace Agent.Core.Implementations.Persistents.Postgresql;

internal class PostgresChatMessageStoreFactory : IChatMessageStoreFactory
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
	/// Creates a ChatMessageStore from serialized state (for resuming threads).
	/// </summary>
	public ChatMessageStore Create(JsonElement serializedState, JsonSerializerOptions? options = null)
	{
		return new PostgresChatMessageStore(_dbContextFactory, serializedState, _maxMessages, options);
	}
}
