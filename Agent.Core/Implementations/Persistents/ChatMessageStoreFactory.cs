using Agent.Core.Enums;
using System.Text.Json;
using Microsoft.Agents.AI;
using Microsoft.EntityFrameworkCore;
using Agent.Core.Abstractions.Persistents;
using Microsoft.Extensions.Caching.Memory;

namespace Agent.Core.Implementations.Persistents;

internal class ChatMessageStoreFactory : IChatMessageStoreFactory
{
	private readonly IDbContextFactory<AgentDbContext> _dbContextFactory;
	private readonly IMemoryCache _memoryCache;
	private readonly int _maxMessages;

	public ChatMessageStoreFactory(
		IDbContextFactory<AgentDbContext> dbContextFactory,
		IMemoryCache memoryCache,
		int maxMessages = 50)
	{
		_dbContextFactory = dbContextFactory ?? throw new ArgumentNullException(nameof(dbContextFactory));
		_memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
		_maxMessages = maxMessages;
	}

	/// <summary>
	/// Creates a ChatMessageStore from serialized state (for resuming threads).
	/// </summary>
	public ChatMessageStore Create(JsonElement serializedState,
		JsonSerializerOptions? options = null,
		ChatMessageStoreEnum chatMessageStore = ChatMessageStoreEnum.Postgresql)
	{
		return chatMessageStore switch
		{
			ChatMessageStoreEnum.Postgresql => new PostgresChatMessageStore(_dbContextFactory,
			serializedState,
			_maxMessages,
			options),
			ChatMessageStoreEnum.Memory => new MemoryChatMessageStore(_memoryCache,
			serializedState,
			_maxMessages,
			options: options),
			_ => throw new NotSupportedException($"ChatMessageStore '{chatMessageStore}' is not supported."),
		};
	}
}
