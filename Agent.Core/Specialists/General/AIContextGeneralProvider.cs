using Agent.Core.Implementations.Persistents;
using Microsoft.Agents.AI;

namespace Agent.Core.Specialists;

internal class AIContextGeneralProvider(ChatDbContext chatDbContext) : AIContextProvider
{
	public override ValueTask<AIContext> InvokingAsync(InvokingContext context, CancellationToken cancellationToken = default)
	{
		throw new NotImplementedException();
	}
}
