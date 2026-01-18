using Agent.Core.Abstractions.Persistents;
using Agent.Core.Implementations.Persistents;
using Agent.Core.VectorRecords;
using Microsoft.Agents.AI;
using Microsoft.EntityFrameworkCore;

namespace Agent.Core.Implementations.LLM;

internal class AIContextSkillRoutingProvider(IQdrantRepository<SkillRoutingRecord> qdrantRepository,
	IDbContextFactory<ChatDbContext> dbContextFactory)
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

		var skillRoutingRecords = await qdrantRepository.SearchAsync(requestMessage.Text,
			top: 5,
			similarityThreshold: 0.4f,
			cancellationToken: cancellationToken);

		if (!skillRoutingRecords.Any())
		{
			return aiContext;
		}

		var skillCodes = skillRoutingRecords.Select(x => x.SkillCode).Distinct().ToList();

		var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

		var instructions = await dbContext.Skills
			.Where(x => skillCodes.Contains(x.Code))
			.Select(x => x.SystemPrompt)
			.ToArrayAsync(cancellationToken);

		aiContext.Instructions = string.Join("\n\n", instructions);

		return aiContext;
	}
}
