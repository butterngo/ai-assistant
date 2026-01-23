using System.Text.Json;
using Agent.Core.Entities;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.EntityFrameworkCore;

namespace Agent.Core.Implementations.Persistents;

internal sealed class PostgresChatMessageStore : ChatMessageStore
{
	private readonly IDbContextFactory<AgentDbContext> _dbContextFactory;
	private readonly int _maxMessages;
	private Guid _threadId;

	public Guid ThreadId => _threadId;

	protected PostgresChatMessageStore(
		IDbContextFactory<AgentDbContext> dbContextFactory,
		int maxMessages = 50)
	{
		_dbContextFactory = dbContextFactory ?? throw new ArgumentNullException(nameof(dbContextFactory));
		_maxMessages = maxMessages;
	}

	public PostgresChatMessageStore(
		IDbContextFactory<AgentDbContext> dbContextFactory,
		JsonElement serializedState,
		int maxMessages = 50,
		JsonSerializerOptions? options = null)
		: this(dbContextFactory, maxMessages)
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
	}

	public override async ValueTask<IEnumerable<ChatMessage>> InvokingAsync(
		InvokingContext context,
		CancellationToken cancellationToken = default)
	{
		await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

		// Get most recent messages, ordered by sequence descending, then reverse
		var entities = await dbContext.ChatMessages
			.AsNoTracking()
			.Where(m => m.ThreadId == _threadId)
			.OrderByDescending(m => m.SequenceNumber)
			.Take(_maxMessages)
			.ToListAsync(cancellationToken);

		// Reverse to chronological order (oldest first)
		entities.Reverse();

		return entities
			.Select(e => DeserializeMessage(e.SerializedMessage))
			.Where(m => m is not null)
			.Cast<ChatMessage>()
			.ToList();
	}

	public override async ValueTask InvokedAsync(
		InvokedContext context,
		CancellationToken cancellationToken = default)
	{
		// Don't persist if there was an exception (optional: you might want to log failed attempts)
		if (context.InvokeException is not null)
		{
			return;
		}

		await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

		var timestamp = DateTimeOffset.UtcNow;
		var currentSequence = await GetNextSequenceNumberAsync(dbContext, _threadId, cancellationToken);

		// Persist request messages (user input)
		foreach (var message in context.RequestMessages)
		{
			var entity = CreateEntity(message, timestamp, currentSequence++);
			dbContext.ChatMessages.Add(entity);
		}

		// Persist response messages (assistant output)
		if (context.ResponseMessages is not null)
		{
			foreach (var message in context.ResponseMessages)
			{
				var entity = CreateEntity(message, timestamp, currentSequence++);
				dbContext.ChatMessages.Add(entity);
			}
		}

		await dbContext.SaveChangesAsync(cancellationToken);
	}

	public override JsonElement Serialize(JsonSerializerOptions? jsonSerializerOptions = null)
	{
		return JsonSerializer.SerializeToElement(_threadId, jsonSerializerOptions);
	}

	#region Private Helpers

	private ChatMessageEntity CreateEntity(ChatMessage message, DateTimeOffset timestamp, long sequenceNumber)
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
			// Log deserialization failure if needed
			return null;
		}
	}

	private static async Task<long> GetNextSequenceNumberAsync(
		AgentDbContext dbContext,
		Guid threadId,
		CancellationToken cancellationToken)
	{
		var maxSequence = await dbContext.ChatMessages
			.Where(m => m.ThreadId == threadId)
			.MaxAsync(m => (long?)m.SequenceNumber, cancellationToken);

		return (maxSequence ?? 0) + 1;
	}

	#endregion
}