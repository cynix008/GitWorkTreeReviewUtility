using WorktreeReviewTool.Models;

namespace WorktreeReviewTool.Services;

public sealed class WorktreeStatusService
{
    private readonly GitService _git;

    public WorktreeStatusService(GitService git)
    {
        _git = git;
    }

    public List<WorktreeDisplayItem> BuildDisplayItems(IEnumerable<GitWorktree> worktrees)
    {
        return worktrees.Select(GetStatus).ToList();
    }

    public WorktreeDisplayItem GetStatus(GitWorktree worktree)
    {
        var statusOutput = Directory.Exists(worktree.Path)
            ? _git.GetStatusPorcelain(worktree.Path)
            : "";

        var changedFiles = statusOutput
            .Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries)
            .ToList();

        return new WorktreeDisplayItem
        {
            Worktree = worktree,
            HasChanges = changedFiles.Count > 0,
            ChangedFiles = changedFiles,
            LastModified = GetLastModified(worktree.Path)
        };
    }

    private static DateTime? GetLastModified(string path)
    {
        if (!Directory.Exists(path))
            return null;

        try
        {
            return Directory.GetLastWriteTime(path);
        }
        catch
        {
            return null;
        }
    }
}
