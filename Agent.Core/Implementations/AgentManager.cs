using Agent.Core.Abstractions;
using Agent.Core.Abstractions.LLM;
using Agent.Core.Abstractions.Persistents;
using Agent.Core.Entities;
using Agent.Core.Implementations.Persistents;
using Agent.Core.Implementations.Persistents.Postgresql;
using Agent.Core.Specialists;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Text.Json;
using System.Threading;

namespace Agent.Core.Implementations;

public class AgentManager : IAgentManager
{
	private readonly ISemanticKernelBuilder _kernelBuilder;
	private readonly IChatMessageStoreFactory _chatMessageStoreFactory;
	private readonly IDbContextFactory<ChatDbContext> _dbContextFactory;
	private readonly ILoggerFactory _loggerFactory;
	private readonly IIntentClassificationService _intentClassificationService;
	
	private readonly ConcurrentDictionary<Guid, Func<IAgent>> _agents = new();

	public AgentManager(ILoggerFactory loggerFactory,
		ISemanticKernelBuilder kernelBuilder,
		IDbContextFactory<ChatDbContext> dbContextFactory,
		IIntentClassificationService intentClassificationService,
		IChatMessageStoreFactory chatMessageStoreFactory)
	{
		_loggerFactory = loggerFactory;
		_kernelBuilder = kernelBuilder;
		_dbContextFactory = dbContextFactory;
		_intentClassificationService = intentClassificationService;
		_chatMessageStoreFactory = chatMessageStoreFactory;
	}

	public async Task<(IAgent agent, ChatThreadEntity thread, bool isNewConversation)> GetOrCreateAsync(Guid? agentId,
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

		Guid inferredAgentId = Guid.Parse("00000000-0000-0000-0000-000000000001");

		if (agentId.HasValue) 
		{
			inferredAgentId = agentId.Value;
		}

		var agent = CreateAgent(inferredAgentId, thread.Id);

		return (agent, thread, isNewConversation);
	}

	public async Task<object> DryRunAsync(string userMessage, CancellationToken ct = default)
	{
		return await _intentClassificationService.IntentAsync(userMessage, ct);
	}

	private IAgent CreateAgent(Guid categoryId, Guid threadId)
	{
		var state = JsonSerializer.SerializeToElement(new { threadId });

		var builder = new AgentBuilder()
			.WithLogger<GeneralAgent>(_loggerFactory)
			.WithKernel(_kernelBuilder)
			.WithMessageStore(_chatMessageStoreFactory.Create(state));

		IAgent Create<T>() where T : IAgent
			=> builder.WithLogger<T>(_loggerFactory).Build<T>();

		return categoryId switch
		{
			var id when id == Guid.Parse("00000000-0000-0000-0000-000000000001")
				=> Create<GeneralAgent>(),

			var id when id == Guid.Parse("00000000-0000-0000-0000-000000000002")
				=> Create<ProductOwnerAgent>(),

			var id when id == Guid.Parse("00000000-0000-0000-0000-000000000003")
			=> Create<ProjectManagerAgent>(),

			var id when id == Guid.Parse("00000000-0000-0000-0000-000000000004")
			=> Create<SoftwareArchitectAgent>(),

			var id when id == Guid.Parse("00000000-0000-0000-0000-000000000005")
			=> Create<BackendDeveloperAgent>(),

			var id when id == Guid.Parse("00000000-0000-0000-0000-000000000006")
			=> Create<FrontendDeveloperAgent>(),

			var id when id == Guid.Parse("00000000-0000-0000-0000-000000000007")
			=> Create<DevopsAgent>(),

			_ => builder.WithLogger<GeneralAgent>(_loggerFactory).Build<GeneralAgent>()
		};
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
