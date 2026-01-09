using Agent.Core.Implementations;
using Microsoft.EntityFrameworkCore;
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
}
