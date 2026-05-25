namespace WorktreeReviewTool.Models;

public sealed class RepoValidationResult
{
    public RepoValidationResult(RepoConfig repo)
    {
        Repo = repo;
    }

    public RepoConfig Repo { get; }
    public List<string> Errors { get; } = [];
    public List<string> Warnings { get; } = [];
    public bool IsSelectable => Errors.Count == 0;
}
