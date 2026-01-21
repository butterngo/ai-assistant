using Microsoft.Agents.AI;
using Agent.Core.Abstractions;
using Agent.Core.VectorRecords;
using Microsoft.EntityFrameworkCore;
using Agent.Core.Abstractions.Persistents;
using Agent.Core.Implementations.Persistents;

namespace Agent.Core.Implementations.LLM;

internal class AIContextSkillRoutingProvider(IQdrantRepository<SkillRoutingRecord> qdrantRepository,
	IDbContextFactory<ChatDbContext> dbContextFactory,
	ICurrentThreadContext currentThreadContext)
	: AIContextProvider
{
	public override  ValueTask<AIContext> InvokingAsync(InvokingContext context, CancellationToken cancellationToken = default)
	=> InternalInvokingAsync(context, cancellationToken);

	protected virtual async ValueTask<AIContext> InternalInvokingAsync(InvokingContext context, CancellationToken cancellationToken = default) 
	{
		var requestMessage = context.RequestMessages.FirstOrDefault();

		var aiContext = new AIContext();

		if (requestMessage == null)
		{
			return aiContext;

		}
		
		currentThreadContext.UserMessage = requestMessage.Text;

		var skillRoutingRecords = await qdrantRepository.SearchAsync(requestMessage.Text,
			top: 5,
			similarityThreshold: currentThreadContext.SimilarityThreshold,
			cancellationToken: cancellationToken);

		if (!skillRoutingRecords.Any())
		{
			return aiContext;
		}

		currentThreadContext.SkillRoutingRecords = skillRoutingRecords;

		var skillCodes = skillRoutingRecords.Select(x => x.SkillCode).Distinct().ToList();

		var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

		var instructions = await dbContext.Skills
			.Where(x => skillCodes.Contains(x.Code))
			.Select(x => x.SystemPrompt)
			.FirstOrDefaultAsync(cancellationToken);

		currentThreadContext.Instructions = instructions ?? string.Empty;

		aiContext.Instructions = instructions;

		return aiContext;
	}

	public override ValueTask InvokedAsync(InvokedContext context, CancellationToken cancellationToken = default)
	{
		currentThreadContext.RequestMessages = context.RequestMessages;
		
		currentThreadContext.ResponseMessages = context.ResponseMessages;

		return base.InvokedAsync(context, cancellationToken);
	}
}
