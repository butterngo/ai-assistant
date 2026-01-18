using Agent.Core.Abstractions;
using Agent.Core.Abstractions.LLM;
using Agent.Core.Abstractions.Persistents;
using Agent.Core.Abstractions.Services;
using Agent.Core.Implementations;
using Agent.Core.Implementations.Persistents;
using Agent.Core.Implementations.Persistents.Vectors;
using Agent.Core.Implementations.Services;
using Agent.Core.Specialists;
using Agent.Core.VectorRecords;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel.Connectors.Qdrant;
using Microsoft.SemanticKernel.Memory;
using Qdrant.Client;

namespace Agent.Core;

public static class ConfigurationServices
{
	public static IServiceCollection AddChatMessageStore(
		this IServiceCollection services,
		string connectionString,
		int maxMessages = 50)
	{
		services.AddMemoryCache();

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
		services.AddSingleton<IChatMessageStoreFactory>(sp =>
		{
			var dbContextFactory = sp.GetRequiredService<IDbContextFactory<ChatDbContext>>();
			var memoryCache = sp.GetRequiredService<IMemoryCache>();
			return new ChatMessageStoreFactory(dbContextFactory, memoryCache, maxMessages);
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

		QdrantRepository<T> Create<T>(IServiceProvider p, string vectorName) 
			where T : QdrantRecordBase, IVectorRecord
		{
			QdrantVectorStore vectorStore = p.GetRequiredService<QdrantVectorStore>();
			ISemanticKernelBuilder semanticKernelBuilder = p.GetRequiredService<ISemanticKernelBuilder>();
			return new QdrantRepository<T>(vectorStore, semanticKernelBuilder, vectorName);
		}

		services.AddScoped<IQdrantRepository<IntentClassificationRecord>>(p => Create<IntentClassificationRecord>(p, QdrantCollections.IntentClassifications));

		services.AddScoped<IQdrantRepository<SkillRoutingRecord>>(p => Create<SkillRoutingRecord>(p, QdrantCollections.SkillRouting));

		services.AddScoped<IQdrantRepository<KnowledgeBaseRecord>>(p => Create<KnowledgeBaseRecord>(p, QdrantCollections.KnowledgeBase));

		services.AddScoped<IQdrantRepository<CachedAnswerRecord>>(p => Create<CachedAnswerRecord>(p, QdrantCollections.CachedAnswers));

		return services;
	}
	public static IServiceCollection AddServices(
		this IServiceCollection services)
	{
		services.AddScoped<IIntentClassificationService, IntentClassificationService>();
		services.AddScoped<ICategoryService, CategoryService>();
		services.AddScoped<IToolService, ToolService>();
		services.AddScoped<ISkillService, SkillService>();
		return services;
	}

	public static IServiceCollection AddAgents(
		this IServiceCollection services,
		IConfiguration configuration)
	{
		services.AddScoped<IAgentManager, AgentManager>();

		//services.AddScoped<IAgent, GeneralAgent>();
		//services.AddScoped<IAgent, BackendDeveloperAgent>();
		//services.AddScoped<IAgent, FrontendDeveloperAgent>();
		//services.AddScoped<IAgent, DevopsAgent>();
		//services.AddScoped<IAgent, ProductOwnerAgent>();
		//services.AddScoped<IAgent, ProjectManagerAgent>();
		//services.AddScoped<IAgent, SoftwareArchitectAgent>();

		return services;
	}
}
