namespace Agent.Core.Models;

public class UserInfo
{
	public string? Name { get; set; }
	public string? Email { get; set; }
	public string? PreferredLanguage { get; set; }
	public List<string> Interests { get; set; } = new();
	public Dictionary<string, string> Preferences { get; set; } = new();
	public ConnectionTool? ConnectionTool { get; set; }
}
