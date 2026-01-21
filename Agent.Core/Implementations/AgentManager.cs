using Agent.Core.Abstractions;
using Agent.Core.Abstractions.LLM;
using Agent.Core.Abstractions.Persistents;
using Agent.Core.Entities;
using Agent.Core.Enums;
using Agent.Core.Implementations.LLM;
using Agent.Core.Implementations.Persistents;
using Agent.Core.Models;
using Agent.Core.Specialists;
using Agent.Core.VectorRecords;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Agent.Core.Implementations;

public class AgentManager : IAgentManager
{
	private readonly ISemanticKernelBuilder _kernelBuilder;
	private readonly IChatMessageStoreFactory _chatMessageStoreFactory;
	private readonly IDbContextFactory<ChatDbContext> _dbContextFactory;
	private readonly ILoggerFactory _loggerFactory;
	private readonly IIntentClassificationService _intentClassificationService;
	private readonly IQdrantRepository<SkillRoutingRecord> _qdrandSkillRoutingRecord;
	public ICurrentThreadContext CurrentThreadContext { get; private set; }

	public AgentManager(ILoggerFactory loggerFactory,
		ISemanticKernelBuilder kernelBuilder,
		IDbContextFactory<ChatDbContext> dbContextFactory,
		IIntentClassificationService intentClassificationService,
		IChatMessageStoreFactory chatMessageStoreFactory,
		IQdrantRepository<SkillRoutingRecord> qdrandSkillRoutingRecord)
	{
		_loggerFactory = loggerFactory;
		_kernelBuilder = kernelBuilder;
		_dbContextFactory = dbContextFactory;
		_intentClassificationService = intentClassificationService;
		_chatMessageStoreFactory = chatMessageStoreFactory;
		_qdrandSkillRoutingRecord = qdrandSkillRoutingRecord;
	}

	private async Task<(ChatThreadEntity thread, bool isNewConversation)> GetOrCreateChatThreadEntity(Guid? threadId,
		string userMessage,
		ChatMessageStoreEnum chatMessageStore,
		CancellationToken ct = default) 
	{
		bool isNewConversation = threadId.HasValue ? false : true;

		var newThreadId = !threadId.HasValue ? Guid.NewGuid() : threadId.Value;

		if (chatMessageStore == ChatMessageStoreEnum.Memory)
		{
			return (new ChatThreadEntity
			{
				Id = newThreadId,
				Title = GenerateTitle(userMessage),
				CreatedAt = DateTimeOffset.UtcNow,
				UpdatedAt = DateTimeOffset.UtcNow
			}, isNewConversation);
		}

		var dbContext = _dbContextFactory.CreateDbContext();

		var thread = await dbContext.ChatThreads.AsNoTracking()
			.FirstOrDefaultAsync(t => t.Id == newThreadId);

		if (thread == null)
		{
			isNewConversation = true;

			thread = new ChatThreadEntity
			{
				Id = newThreadId,
				Title = GenerateTitle(userMessage),
				CreatedAt = DateTimeOffset.UtcNow,
				UpdatedAt = DateTimeOffset.UtcNow
			};

			dbContext.ChatThreads.Add(thread);

			await dbContext.SaveChangesAsync(ct);
		}

		return (thread, isNewConversation);
	}

	public async Task<(IAgent agent, ChatThreadEntity thread, bool isNewConversation)> 
		GetOrCreateAsync(Guid? agentId,
		Guid? threadId,
		string userMessage,
		ChatMessageStoreEnum chatMessageStore = ChatMessageStoreEnum.Postgresql,
		CancellationToken ct = default)
	{
		var (thread, isNewConversation) = await GetOrCreateChatThreadEntity(threadId, userMessage, chatMessageStore, ct);
		
		Guid inferredAgentId = Guid.Parse("00000000-0000-0000-0000-000000000001");

		if (agentId.HasValue) 
		{
			inferredAgentId = agentId.Value;
		}

		var agent = CreateAgent(inferredAgentId, thread.Id, chatMessageStore);

		return (agent, thread, isNewConversation);
	}

	public async Task<object> DryRunAsync(string userMessage, CancellationToken ct = default)
	{
		return await _intentClassificationService.IntentAsync(userMessage, ct);
	}

	private IAgent CreateAgent(Guid agentId,
		Guid threadId,
		ChatMessageStoreEnum chatMessageStore)
	{
		var state = JsonSerializer.SerializeToElement(new { threadId });

		if (chatMessageStore == ChatMessageStoreEnum.Memory)
		{
			CurrentThreadContext = new CurrentThreadContext(agentId, threadId);
		}

		var builder = new AgentBuilder()
			.WithKernel(_kernelBuilder)
			.WithMessageStore(_chatMessageStoreFactory.Create(state, chatMessageStore: chatMessageStore));

		return agentId switch
		{
			var id when id == Guid.Parse("00000000-0000-0000-0000-000000000001")
				=> builder
				.WithLogger<GeneralAgent>(_loggerFactory)
				.WithAIContextProvider(new UserMemoryProvider())
				.Build<GeneralAgent>(),

			var id when id == Guid.Parse("00000000-0000-0000-0000-000000000002")
				=> builder
				.WithLogger<ProductOwnerAgent>(_loggerFactory)
				.WithAIContextProvider(new AIContextSkillRoutingProvider(_qdrandSkillRoutingRecord,
				_dbContextFactory,
				CurrentThreadContext))
				.Build<ProductOwnerAgent>(),

			var id when id == Guid.Parse("00000000-0000-0000-0000-000000000003")
				=> builder.WithLogger<ProjectManagerAgent>(_loggerFactory).Build<ProjectManagerAgent>(),

			var id when id == Guid.Parse("00000000-0000-0000-0000-000000000004")
				=> builder.WithLogger<SoftwareArchitectAgent>(_loggerFactory).Build<SoftwareArchitectAgent>(),

			var id when id == Guid.Parse("00000000-0000-0000-0000-000000000005")
			    => builder.WithLogger<BackendDeveloperAgent>(_loggerFactory).Build<BackendDeveloperAgent>(),

			var id when id == Guid.Parse("00000000-0000-0000-0000-000000000006")
			   => builder.WithLogger<FrontendDeveloperAgent>(_loggerFactory).Build<FrontendDeveloperAgent>(),
			
			var id when id == Guid.Parse("00000000-0000-0000-0000-000000000007")
			=> builder.WithLogger<DevopsAgent>(_loggerFactory).Build<DevopsAgent>(),

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
