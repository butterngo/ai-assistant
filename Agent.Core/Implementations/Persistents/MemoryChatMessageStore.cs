using System.Text.Json;
using Agent.Core.Entities;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Caching.Memory;

namespace Agent.Core.Implementations.Persistents;

/// <summary>
/// In-memory chat message store for testing and temporary sessions.
/// Does not persist data beyond application lifetime.
/// </summary>
internal sealed class MemoryChatMessageStore : ChatMessageStore
{
	private readonly IMemoryCache _memoryCache;
	private readonly int _maxMessages;
	private readonly TimeSpan _cacheExpiration;
	private Guid _threadId;

	public Guid ThreadId => _threadId;

	protected MemoryChatMessageStore(
		IMemoryCache memoryCache,
		int maxMessages = 50,
		TimeSpan? cacheExpiration = null)
	{
		_memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
		_maxMessages = maxMessages;
		_cacheExpiration = cacheExpiration ?? TimeSpan.FromHours(1);
	}

	public MemoryChatMessageStore(
		IMemoryCache memoryCache,
		JsonElement serializedState,
		int maxMessages = 50,
		TimeSpan? cacheExpiration = null,
		JsonSerializerOptions? options = null)
		: this(memoryCache, maxMessages, cacheExpiration)
	{
		if (serializedState.ValueKind == JsonValueKind.String)
		{
			_threadId = serializedState.GetGuid();
		}
		else if (serializedState.ValueKind == JsonValueKind.Object)
		{
			// Support for extended state format
			if (serializedState.TryGetProperty("threadId", out var threadIdElement))
			{
				_threadId = threadIdElement.GetGuid();
			}
		}

		// Initialize empty message list for this thread if not exists
		EnsureThreadExists();
	}

	public override ValueTask<IEnumerable<ChatMessage>> InvokingAsync(
		InvokingContext context,
		CancellationToken cancellationToken = default)
	{
		var cacheKey = GetCacheKey(_threadId);

		if (!_memoryCache.TryGetValue(cacheKey, out List<ChatMessageEntity>? messages))
		{
			return ValueTask.FromResult(Enumerable.Empty<ChatMessage>());
		}

		// Get most recent messages (up to _maxMessages)
		var recentMessages = messages!
			.OrderBy(m => m.SequenceNumber)
			.TakeLast(_maxMessages)
			.Select(m => DeserializeMessage(m.SerializedMessage))
			.Where(m => m is not null)
			.Cast<ChatMessage>()
			.ToList();

		return ValueTask.FromResult<IEnumerable<ChatMessage>>(recentMessages);
	}

	public override ValueTask InvokedAsync(
		InvokedContext context,
		CancellationToken cancellationToken = default)
	{
		// Don't persist if there was an exception
		if (context.InvokeException is not null)
		{
			return ValueTask.CompletedTask;
		}

		var cacheKey = GetCacheKey(_threadId);
		var timestamp = DateTimeOffset.UtcNow;

		// Get or create message list
		var messages = _memoryCache.GetOrCreate(cacheKey, entry =>
		{
			entry.AbsoluteExpirationRelativeToNow = _cacheExpiration;
			entry.SlidingExpiration = TimeSpan.FromMinutes(30);
			return new List<ChatMessageEntity>();
		})!;

		var currentSequence = messages.Count > 0
			? messages.Max(m => m.SequenceNumber) + 1
			: 1;

		// Add request messages (user input)
		foreach (var message in context.RequestMessages)
		{
			messages.Add(CreateStoredMessage(message, timestamp, currentSequence++));
		}

		// Add response messages (assistant output)
		if (context.ResponseMessages is not null)
		{
			foreach (var message in context.ResponseMessages)
			{
				messages.Add(CreateStoredMessage(message, timestamp, currentSequence++));
			}
		}

		// Update cache
		_memoryCache.Set(cacheKey, messages, new MemoryCacheEntryOptions
		{
			AbsoluteExpirationRelativeToNow = _cacheExpiration,
			SlidingExpiration = TimeSpan.FromMinutes(30)
		});

		return ValueTask.CompletedTask;
	}

	public override JsonElement Serialize(JsonSerializerOptions? jsonSerializerOptions = null)
	{
		return JsonSerializer.SerializeToElement(_threadId, jsonSerializerOptions);
	}

	#region Private Helpers

	private void EnsureThreadExists()
	{
		var cacheKey = GetCacheKey(_threadId);

		if (!_memoryCache.TryGetValue(cacheKey, out _))
		{
			_memoryCache.Set(cacheKey, new List<ChatMessageEntity>(), new MemoryCacheEntryOptions
			{
				AbsoluteExpirationRelativeToNow = _cacheExpiration,
				SlidingExpiration = TimeSpan.FromMinutes(30)
			});
		}
	}

	private static string GetCacheKey(Guid threadId)
	{
		return $"chat:thread:{threadId}";
	}

	private ChatMessageEntity CreateStoredMessage(
		ChatMessage message,
		DateTimeOffset timestamp,
		long sequenceNumber)
	{
		return new ChatMessageEntity
		{
			Id = Guid.NewGuid(),
			ThreadId = _threadId,
			Role = message.Role.Value,
			Content = ExtractTextContent(message),
			SerializedMessage = JsonSerializer.Serialize(message),
			CreatedAt = timestamp,
			SequenceNumber = sequenceNumber
		};
	}

	private static string ExtractTextContent(ChatMessage message)
	{
		if (message.Contents.Count == 0)
		{
			return message.Text ?? string.Empty;
		}

		var textParts = message.Contents
			.OfType<TextContent>()
			.Select(tc => tc.Text);

		return string.Join("\n", textParts);
	}

	private static ChatMessage? DeserializeMessage(string json)
	{
		try
		{
			return JsonSerializer.Deserialize<ChatMessage>(json);
		}
		catch (JsonException)
		{
			return null;
		}
	}

	#endregion
}
