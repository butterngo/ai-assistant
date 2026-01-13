using Microsoft.Extensions.AI;
using System.Text.Json;
using System.ComponentModel;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;


namespace Agent.Core.Implementations.LLM;

public sealed class OpenApiToolFactory : IDisposable
{
	private readonly HttpClient _httpClient;
	private readonly string _baseUrl;
	private readonly JsonSerializerOptions _jsonOptions;
	private readonly OpenApiDocument _document;

	public OpenApiToolFactory(
		string swaggerJson,
		string baseUrl,
		HttpClient? httpClient = null)
	{
		_httpClient = httpClient ?? new HttpClient();
		_baseUrl = baseUrl.TrimEnd('/');
		_jsonOptions = new JsonSerializerOptions
		{
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
			WriteIndented = false
		};

		var reader = new OpenApiStringReader();
		_document = reader.Read(swaggerJson, out var diagnostic);

		if (diagnostic.Errors.Any())
		{
			throw new InvalidOperationException(
				$"OpenAPI parse errors: {string.Join(", ", diagnostic.Errors.Select(e => e.Message))}");
		}
	}

	public static async Task<OpenApiToolFactory> CreateFromUrlAsync(
		string swaggerUrl,
		string baseUrl,
		HttpClient? httpClient = null)
	{
		var client = httpClient ?? new HttpClient();
		var swaggerJson = await client.GetStringAsync(swaggerUrl);
		return new OpenApiToolFactory(swaggerJson, baseUrl, client);
	}

	public IReadOnlyList<AITool> GetAllTools()
	{
		var tools = new List<AITool>();

		foreach (var (path, pathItem) in _document.Paths)
		{
			foreach (var (method, operation) in pathItem.Operations)
			{
				if (string.IsNullOrEmpty(operation.OperationId))
				{
					continue;
				}

				var tool = CreateTool(path, method, operation);
				tools.Add(tool);
			}
		}

		return tools;
	}

	private AIFunction CreateTool(
		string pathTemplate,
		OperationType method,
		OpenApiOperation operation)
	{
		var name = SanitizeName(operation.OperationId);
		var description = BuildDescription(operation);

		return AIFunctionFactory.Create(
			async ([Description("JSON object containing all parameters")] string parametersJson) =>
			{
				var args = string.IsNullOrEmpty(parametersJson)
					? new Dictionary<string, object?>()
					: JsonSerializer.Deserialize<Dictionary<string, object?>>(parametersJson, _jsonOptions)
					  ?? new Dictionary<string, object?>();

				return await ExecuteApiCallAsync(pathTemplate, method, operation, args);
			},
			name: name,
			description: $"{description}. Parameters: {BuildParametersDescription(operation)}"
		);
	}

	private string BuildDescription(OpenApiOperation operation)
	{
		var parts = new List<string>();

		if (!string.IsNullOrEmpty(operation.Summary))
			parts.Add(operation.Summary);

		if (!string.IsNullOrEmpty(operation.Description))
			parts.Add(operation.Description);

		return parts.Count > 0 ? string.Join(". ", parts) : $"Calls {operation.OperationId}";
	}

	private string BuildParametersDescription(OpenApiOperation operation)
	{
		var paramDescs = new List<string>();

		foreach (var param in operation.Parameters)
		{
			var required = param.Required ? "(required)" : "(optional)";
			paramDescs.Add($"{param.Name} {required}: {param.Description ?? param.Name}");
		}

		if (operation.RequestBody != null)
		{
			paramDescs.Add($"requestBody: {operation.RequestBody.Description ?? "Request body JSON"}");
		}

		return paramDescs.Count > 0
			? string.Join(", ", paramDescs)
			: "No parameters";
	}

	private async Task<string> ExecuteApiCallAsync(
		string pathTemplate,
		OperationType method,
		OpenApiOperation operation,
		IDictionary<string, object?> args)
	{
		try
		{
			var url = pathTemplate;
			foreach (var param in operation.Parameters.Where(p => p.In == ParameterLocation.Path))
			{
				if (args.TryGetValue(param.Name, out var value) && value != null)
				{
					url = url.Replace($"{{{param.Name}}}", Uri.EscapeDataString(value.ToString()!));
				}
			}

			var queryParams = new List<string>();
			foreach (var param in operation.Parameters.Where(p => p.In == ParameterLocation.Query))
			{
				if (args.TryGetValue(param.Name, out var value) && value != null)
				{
					queryParams.Add($"{param.Name}={Uri.EscapeDataString(value.ToString()!)}");
				}
			}
			if (queryParams.Count > 0)
			{
				url += "?" + string.Join("&", queryParams);
			}

			var request = new HttpRequestMessage(ToHttpMethod(method), $"{_baseUrl}{url}");

			foreach (var param in operation.Parameters.Where(p => p.In == ParameterLocation.Header))
			{
				if (args.TryGetValue(param.Name, out var value) && value != null)
				{
					request.Headers.TryAddWithoutValidation(param.Name, value.ToString());
				}
			}

			if (args.TryGetValue("requestBody", out var body) && body != null)
			{
				var json = body is string s ? s : JsonSerializer.Serialize(body, _jsonOptions);
				request.Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
			}

			var response = await _httpClient.SendAsync(request);
			var content = await response.Content.ReadAsStringAsync();

			if (!response.IsSuccessStatusCode)
			{
				return JsonSerializer.Serialize(new
				{
					success = false,
					statusCode = (int)response.StatusCode,
					error = content
				}, _jsonOptions);
			}

			return content;
		}
		catch (Exception ex)
		{
			return JsonSerializer.Serialize(new
			{
				success = false,
				error = ex.Message
			}, _jsonOptions);
		}
	}

	private static HttpMethod ToHttpMethod(OperationType type) => type switch
	{
		OperationType.Get => HttpMethod.Get,
		OperationType.Post => HttpMethod.Post,
		OperationType.Put => HttpMethod.Put,
		OperationType.Delete => HttpMethod.Delete,
		OperationType.Patch => HttpMethod.Patch,
		OperationType.Head => HttpMethod.Head,
		OperationType.Options => HttpMethod.Options,
		_ => HttpMethod.Get
	};

	private static string SanitizeName(string name)
	{
		return new string(name
			.Select(c => char.IsLetterOrDigit(c) ? c : '_')
			.ToArray());
	}

	public void Dispose()
	{
		_httpClient?.Dispose();
	}
}