using Agent.Core.Abstractions;
using Agent.Core.Abstractions.LLM;
using Agent.Core.Domains;
using Agent.Core.Implementations.Persistents;
using Agent.Core.Specialists;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Agent.Core.Implementations;

public class AgentManager : IAgentManager
{
	private readonly ISemanticKernelBuilder _kernelBuilder;
	private readonly PostgresChatMessageStoreFactory _postgresChatMessageStoreFactory;
	private readonly IDbContextFactory<ChatDbContext> _dbContextFactory;
	private readonly ILoggerFactory _loggerFactory;

	public AgentManager(IConnectionMultiplexer redis,
		ILoggerFactory loggerFactory,
		ISemanticKernelBuilder kernelBuilder,
		IDbContextFactory<ChatDbContext> dbContextFactory,
		PostgresChatMessageStoreFactory postgresChatMessageStoreFactory)
	{
		_loggerFactory = loggerFactory;
		_kernelBuilder = kernelBuilder;
		_dbContextFactory = dbContextFactory;
		_postgresChatMessageStoreFactory = postgresChatMessageStoreFactory;
	}

	public async Task<(GeneralAgent agent, ChatThreadEntity thread)> GetOrCreateAsync(
		string? conversationId,
		string userMessage,
		CancellationToken ct = default)
	{
		var dbContext = _dbContextFactory.CreateDbContext();

		if (string.IsNullOrEmpty(conversationId))
		{
			conversationId = Guid.NewGuid().ToString("n");
		}
		var thread = await dbContext.ChatThreads.AsNoTracking()
			.FirstOrDefaultAsync(t => t.ThreadId == conversationId);

		if (thread == null)
		{
			thread = new ChatThreadEntity
			{
				Id = Guid.NewGuid(),
				ThreadId = conversationId,
				Title = GenerateTitle(userMessage),
				CreatedAt = DateTimeOffset.UtcNow,
				UpdatedAt = DateTimeOffset.UtcNow
			};

			dbContext.ChatThreads.Add(thread);

			await dbContext.SaveChangesAsync(ct);
		}

		var agent = new GeneralAgent(_loggerFactory.CreateLogger<GeneralAgent>(),
			_kernelBuilder,
			_postgresChatMessageStoreFactory);

		agent.SetConversationId(thread.ThreadId);

		return (agent, thread);
	}

	private static string GenerateTitle(string message)
	{
		const int maxLength = 60;
		var title = message.Trim();

		// Remove newlines
		title = title.Replace("\n", " ").Replace("\r", "");

		// Truncate if too long
		if (title.Length > maxLength)
		{
			var lastSpace = title.LastIndexOf(' ', maxLength);
			title = lastSpace > maxLength / 2
				? title[..lastSpace] + "..."
				: title[..maxLength] + "...";
		}

		return title;
	}
}
