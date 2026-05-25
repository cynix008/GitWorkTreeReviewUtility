namespace WorktreeReviewTool.Models;

public sealed class WorktreeDisplayItem
{
    public required GitWorktree Worktree { get; init; }
    public required bool HasChanges { get; init; }
    public required List<string> ChangedFiles { get; init; }
    public DateTime? LastModified { get; init; }

    public string Status => HasChanges ? "Has Changes" : "Clean";
}
