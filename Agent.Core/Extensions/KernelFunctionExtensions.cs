using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel;
using System.Text.Json;

namespace Agent.Core.Extensions;

internal static class KernelFunctionExtensions
{
	private static readonly JsonSerializerOptions JsonOptions = new()
	{
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
		PropertyNameCaseInsensitive = true
	};

	public static AIFunction WrapForAgentFramework(this KernelFunction kernelFunc, Kernel kernel)
	{
		return AIFunctionFactory.Create(
			async (string? parametersJson) =>
			{
				var args = ParseParameters(parametersJson);
				var result = await kernelFunc.InvokeAsync(kernel, args);
				return result?.ToString() ?? string.Empty;
			},
			name: kernelFunc.Name,
			description: BuildDescription(kernelFunc)
		);
	}

	public static IEnumerable<AIFunction> WrapForAgentFramework(this KernelPlugin plugin, Kernel kernel)
	{
		return plugin.Select(func => func.WrapForAgentFramework(kernel));
	}

	public static IEnumerable<AIFunction> WrapAllForAgentFramework(this Kernel kernel)
	{
		return kernel.Plugins
			.SelectMany(plugin => plugin)
			.Select(func => func.WrapForAgentFramework(kernel));
	}

	public static IReadOnlyList<AITool> GetToolsForAgentFramework(this Kernel kernel)
	{
		return kernel.WrapAllForAgentFramework()
			.Cast<AITool>()
			.ToList();
	}

	private static KernelArguments ParseParameters(string? json)
	{
		var args = new KernelArguments();

		if (string.IsNullOrWhiteSpace(json))
			return args;

		try
		{
			var dict = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json, JsonOptions);
			if (dict == null) return args;

			foreach (var (key, value) in dict)
			{
				args[key] = ConvertJsonElement(value);
			}
		}
		catch
		{
			// If parsing fails, return empty args
		}

		return args;
	}

	private static object? ConvertJsonElement(JsonElement element)
	{
		return element.ValueKind switch
		{
			JsonValueKind.String => element.GetString(),
			JsonValueKind.Number => element.TryGetInt64(out var l) ? l : element.GetDouble(),
			JsonValueKind.True => true,
			JsonValueKind.False => false,
			JsonValueKind.Null => null,
			JsonValueKind.Array => element.EnumerateArray().Select(ConvertJsonElement).ToList(),
			JsonValueKind.Object => element.GetRawText(),
			_ => element.GetRawText()
		};
	}

	private static string BuildDescription(KernelFunction func)
	{
		var desc = func.Description ?? func.Name;

		var paramHints = func.Metadata.Parameters
			.Select(p =>
			{
				var required = p.IsRequired ? "required" : "optional";
				var paramDesc = p.Description ?? p.Name;
				return $"{p.Name}({required}): {paramDesc}";
			})
			.ToList();

		if (paramHints.Count == 0)
			return desc;

		return $"{desc}. Parameters as JSON: {{{string.Join(", ", paramHints)}}}";
	}
}
