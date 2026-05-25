namespace WorktreeReviewTool.Models;

public sealed class RepoConfig
{
    public string Name { get; set; } = "";
    public string Path { get; set; } = "";
    public string MainBranch { get; set; } = "main";
    public string? WorktreeRoot { get; set; }
    public string? ReviewCommand { get; set; } = "code";
    public List<string> ReviewArguments { get; set; } = ["."];
}
