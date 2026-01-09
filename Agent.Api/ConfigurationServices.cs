using Agent.Core.Options;

namespace Agent.Api.Extensions;

public static class ConfigurationServices
{
	public static IServiceCollection AddOptions(this IServiceCollection services, IConfiguration configuration)
	{
		services.Configure<LLMProviderOptions>(configuration.GetSection(LLMProviderOptions.SectionName));

		return services;
	}
}
