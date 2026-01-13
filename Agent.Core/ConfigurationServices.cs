using Agent.Core.Implementations.Persistents;
using Agent.Core.Specialists;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Agent.Core;

public static class ConfigurationServices
{
	public static IServiceCollection AddPostgresChatMessageStore(
		this IServiceCollection services,
		string connectionString,
		int maxMessages = 50)
	{
		// Register DbContext with pooling for better performance
		services.AddPooledDbContextFactory<ChatDbContext>(options =>
		{
			options.UseNpgsql(connectionString, npgsqlOptions =>
			{
				// Enable retry on failure for transient errors
				npgsqlOptions.EnableRetryOnFailure(
					maxRetryCount: 3,
					maxRetryDelay: TimeSpan.FromSeconds(5),
					errorCodesToAdd: null);

				// Set command timeout
				npgsqlOptions.CommandTimeout(30);
			});

			// Performance optimizations
			options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
		});

		// Register the factory as singleton (it's stateless, uses DbContextFactory internally)
		services.AddSingleton<PostgresChatMessageStoreFactory>(sp =>
		{
			var dbContextFactory = sp.GetRequiredService<IDbContextFactory<ChatDbContext>>();
			return new PostgresChatMessageStoreFactory(dbContextFactory, maxMessages);
		});

		return services;
	}

	/// <summary>
	/// Adds PostgreSQL-backed ChatMessageStore with custom DbContext configuration.
	/// </summary>
	public static IServiceCollection AddPostgresChatMessageStore(
		this IServiceCollection services,
		Action<DbContextOptionsBuilder> configureDbContext,
		int maxMessages = 50)
	{
		services.AddPooledDbContextFactory<ChatDbContext>(configureDbContext);

		services.AddSingleton<PostgresChatMessageStoreFactory>(sp =>
		{
			var dbContextFactory = sp.GetRequiredService<IDbContextFactory<ChatDbContext>>();
			return new PostgresChatMessageStoreFactory(dbContextFactory, maxMessages);
		});

		return services;
	}

	public static IServiceCollection AddEmbeddings(
		this IServiceCollection services)
	{
		//services.AddSingleton<ITextEmbeddingGenerationService>(sp =>
		//{
		//	var config = sp.GetRequiredService<IConfiguration>();

		//	return new AzureOpenAITextEmbeddingGenerationService(
		//		deploymentName: "text-embedding-ada-002", // or text-embedding-3-small
		//		endpoint: config["AzureOpenAI:Endpoint"]!,
		//		apiKey: config["AzureOpenAI:ApiKey"]!
		//	);
		//});

		return services;
	}

	public static IServiceCollection AddAgents(
		this IServiceCollection services)
	{
		services.AddTransient<GeneralAgent>();

		return services;
	}
}
