namespace Agent.Core.Abstractions;

public abstract class AIContextProviderBase
{
	public abstract Task<string> GetInstructions();
	public abstract Task<string> GetTools();
}
