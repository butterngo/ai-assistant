using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using StackExchange.Redis;
using System.Text.Json;

namespace Agent.Core.Implementations;

public sealed class RedisChatMessageStore : ChatMessageStore
{
	private readonly IConnectionMultiplexer _redis;
	private readonly IDatabase _db;
	private readonly string _keyPrefix;
	private readonly int? _maxMessages;

	public string ThreadId { get; private set; }

	public RedisChatMessageStore(
		IConnectionMultiplexer redis,
		string? threadId = null,
		string keyPrefix = "chat_messages",
		int? maxMessages = null)
	{
		_redis = redis ?? throw new ArgumentNullException(nameof(redis));
		_db = _redis.GetDatabase();
		_keyPrefix = keyPrefix;
		_maxMessages = maxMessages;
		ThreadId = threadId ?? Guid.NewGuid().ToString("N");
	}

	private string RedisKey => $"{_keyPrefix}:{ThreadId}";

	public override async Task AddMessagesAsync(
		IEnumerable<ChatMessage> messages,
		CancellationToken cancellationToken = default)
	{
		var serializedMessages = messages
			.Select(m => (RedisValue)JsonSerializer.Serialize(m))
			.ToArray();

		if (serializedMessages.Length == 0)
			return;

		// Add messages to the end of the list (chronological order)
		await _db.ListRightPushAsync(RedisKey, serializedMessages);

		// Trim to max messages if configured
		if (_maxMessages.HasValue)
		{
			var currentCount = await _db.ListLengthAsync(RedisKey);
			if (currentCount > _maxMessages.Value)
			{
				// Keep only the most recent messages
				await _db.ListTrimAsync(RedisKey, -_maxMessages.Value, -1);
			}
		}
	}

	public override async Task<IEnumerable<ChatMessage>> GetMessagesAsync(
		CancellationToken cancellationToken = default)
	{
		// Get all messages from Redis list (oldest to newest)
		var redisMessages = await _db.ListRangeAsync(RedisKey, 0, -1);

		var messages = new List<ChatMessage>();
		foreach (var item in redisMessages)
		{
			if (!item.IsNullOrEmpty)
			{
				var message = JsonSerializer.Deserialize<ChatMessage>(item.ToString());
				if (message != null)
				{
					messages.Add(message);
				}
			}
		}

		return messages;
	}

	public override JsonElement Serialize(JsonSerializerOptions? jsonSerializerOptions = null)
	{
		var state = new RedisStoreState
		{
			ThreadId = ThreadId,
			KeyPrefix = _keyPrefix,
			MaxMessages = _maxMessages
		};

		return JsonSerializer.SerializeToElement(state, jsonSerializerOptions);
	}

	public async Task ClearAsync()
	{
		await _db.KeyDeleteAsync(RedisKey);
	}

	public async Task SetExpirationAsync(TimeSpan expiration)
	{
		await _db.KeyExpireAsync(RedisKey, expiration);
	}

	private sealed class RedisStoreState
	{
		public string ThreadId { get; set; } = "";
		public string KeyPrefix { get; set; } = "chat_messages";
		public int? MaxMessages { get; set; }
	}
}
