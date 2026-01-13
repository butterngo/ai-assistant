using Agent.Core.Abstractions;
using Agent.Core.Abstractions.LLM;
using Agent.Core.Abstractions.Persistents;
using Agent.Core.Implementations;
using Agent.Core.Implementations.LLM;
using Agent.Core.Implementations.Persistents;
using Agent.Core.Specialists;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel.Connectors.Qdrant;
using Qdrant.Client;

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

	public static IServiceCollection AddVectorDB(this IServiceCollection services, IConfiguration configuration)
	{
		var qdrantConnectionString = configuration.GetConnectionString("Qdrant");

		if (string.IsNullOrEmpty(qdrantConnectionString)) 
		{
			throw new InvalidOperationException("Qdrant connection string is not configured.");
		}

		services.AddSingleton(sp => new QdrantClient(new Uri(qdrantConnectionString)));

		services.AddQdrantVectorStore(clientProvider: (p) =>
		{
			var qdrantClient = p.GetRequiredService<QdrantClient>();
			return qdrantClient;
		},
			optionsProvider: p =>
			{
				var semanticKernelBuilder = p.GetRequiredService<ISemanticKernelBuilder>();

				var embeddingGenerator = semanticKernelBuilder.GetEmbeddingGenerator();

				return new QdrantVectorStoreOptions
				{
					EmbeddingGenerator = embeddingGenerator
				};
			});

		services.AddScoped<IIntentClassificationRepository, IntentClassificationRepository>();

		return services;
	}

	public static IServiceCollection AddAgents(
		this IServiceCollection services,
		IConfiguration configuration)
	{

		services.AddTransient<GeneralAgent>();
		services.AddScoped<IIntentClassificationService, IntentClassificationService>();
		services.AddScoped<IAgentManager, AgentManager>();
		

		return services;
	}
}
