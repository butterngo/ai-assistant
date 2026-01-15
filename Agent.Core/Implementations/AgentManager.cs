using Agent.Core.Abstractions;
using Agent.Core.Abstractions.LLM;
using Agent.Core.Entities;
using Agent.Core.Implementations.Persistents;
using Agent.Core.Specialists;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Agent.Core.Implementations;

public class AgentManager : IAgentManager
{
	private readonly ISemanticKernelBuilder _kernelBuilder;
	private readonly PostgresChatMessageStoreFactory _postgresChatMessageStoreFactory;
	private readonly IDbContextFactory<ChatDbContext> _dbContextFactory;
	private readonly ILoggerFactory _loggerFactory;
	private readonly IIntentClassificationService _intentClassificationService;

	public AgentManager(ILoggerFactory loggerFactory,
		ISemanticKernelBuilder kernelBuilder,
		IDbContextFactory<ChatDbContext> dbContextFactory,
		IIntentClassificationService intentClassificationService,
		PostgresChatMessageStoreFactory postgresChatMessageStoreFactory)
	{
		_loggerFactory = loggerFactory;
		_kernelBuilder = kernelBuilder;
		_dbContextFactory = dbContextFactory;
		_intentClassificationService = intentClassificationService;
		_postgresChatMessageStoreFactory = postgresChatMessageStoreFactory;
	}

	public async Task<(GeneralAgent agent, ChatThreadEntity thread, bool isNewConversation)> GetOrCreateAsync(
		Guid? threadId,
		string userMessage,
		CancellationToken ct = default)
	{
		var dbContext = _dbContextFactory.CreateDbContext();
		
		bool isNewConversation = false;

		if (!threadId.HasValue)
		{
			threadId = Guid.NewGuid();
		}

		var thread = await dbContext.ChatThreads.AsNoTracking()
			.FirstOrDefaultAsync(t => t.Id == threadId);

		if (thread == null)
		{
			isNewConversation = true;

			thread = new ChatThreadEntity
			{
				Id = threadId.Value,
				Title = GenerateTitle(userMessage),
				CreatedAt = DateTimeOffset.UtcNow,
				UpdatedAt = DateTimeOffset.UtcNow
			};

			dbContext.ChatThreads.Add(thread);

			await dbContext.SaveChangesAsync(ct);
		}

		var agent = new GeneralAgent(_loggerFactory.CreateLogger<GeneralAgent>(),
			_kernelBuilder,
			_postgresChatMessageStoreFactory, () => 
			{
				return JsonSerializer.SerializeToElement(new { threadId = thread.Id });
			});

		return (agent, thread, isNewConversation);
	}

	public async Task<object> DryRunAsync(string userMessage, CancellationToken ct = default)
	{
		return await _intentClassificationService.IntentAsync(userMessage, ct);
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
