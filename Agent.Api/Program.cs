using Agent.Api;
using Agent.Api.Extensions;
using Agent.Core.Abstractions;
using Agent.Core.Implementations;
using Agent.Core.Options;
using StackExchange.Redis;
using Agent.Api.Endpoints;

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

		builder.Services.AddSingleton<IConnectionMultiplexer>(_ =>
			ConnectionMultiplexer.Connect(
				builder.Configuration.GetValue<string>("Redis:Connection") ?? "localhost:6379"));

		builder.Services.AddSingleton<ISemanticKernelBuilder>(sp =>
		{
			var options = builder.Configuration
				.GetSection(LLMProviderOptions.SectionName)
				.Get<LLMProviderOptions>();
			return new SemanticKernelBuilder(options);
		});

		builder.Services.AddSingleton<AgentManager>();

		var app = builder.Build();

		app.UseCors();

		app.MapChatBot();

		app.MapConversations();

		app.Run();
	}
}