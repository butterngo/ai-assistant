using Microsoft.Extensions.DependencyInjection;

namespace Agent.Core;

public static class ConfigurationServices
{
	public static IServiceCollection GetFoundationModelOptionsSection(this IServiceCollection services)
	{
		return services;
	}
}
