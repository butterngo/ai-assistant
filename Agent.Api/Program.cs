using Agent.Core;
using Agent.Core.Options;
using Agent.Api.Endpoints;
using Agent.Api.Extensions;
using Agent.Core.Abstractions.LLM;
using Agent.Core.Implementations.LLM;
using Microsoft.OpenApi;

internal class Program
{
	private static void Main(string[] args)
	{
		var builder = WebApplication.CreateBuilder(args);

		var services = builder.Services;
		var configuration = builder.Configuration;

		builder.Services.AddCors(options =>
		{
			options.AddDefaultPolicy(policy =>
			{
				policy.WithOrigins("http://localhost:3001", "http://localhost:5173")  // React app
					  .AllowAnyHeader()
					  .AllowAnyMethod()
					  .AllowCredentials();  // Needed for SSE
			});
		});

		services.AddOptions(configuration);

		//builder.Services.AddSingleton<IConnectionMultiplexer>(_ =>
		//	ConnectionMultiplexer.Connect(configuration.GetConnectionString("Redis")));

		builder.Services.AddChatMessageStore(
		 connectionString: configuration.GetConnectionString("Postgresql"),
		 maxMessages: 50
	 );

		builder.Services.AddSingleton<ISemanticKernelBuilder>(sp =>
		{
			var options = builder.Configuration
				.GetSection(LLMProviderOptions.SectionName)
				.Get<LLMProviderOptions>();
			return new SemanticKernelBuilder(options);
		});

		builder.Services.AddVectorDB(configuration);

		builder.Services.AddServices();

		builder.Services.AddAgents(configuration);

		builder.Services.AddOpenApi(options =>
		{
			options.AddDocumentTransformer((document, context, ct) =>
			{
				document.Info = new OpenApiInfo
				{
					Title = "Skill Management API",
					Version = "v1",
					Description = "API for managing coding assistant skills"
				};
				return Task.CompletedTask;
			});
		});

		var app = builder.Build();

		app.MapOpenApi();

		app.UseCors();

		app.MapChatBotEndPoints();

		app.MapConversationEndPoints();

		app.MapSkillEndPoints();

		app.MapCategoryEndPoints();

		app.MapToolEndPoints();

		app.Run();
	}
}