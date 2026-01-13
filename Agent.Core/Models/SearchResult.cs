namespace Agent.Core.Models;

public class SearchResult
{
	public long Id { get; set; }
	public string Content { get; set; } = string.Empty;
	public double Similarity { get; set; }
}
