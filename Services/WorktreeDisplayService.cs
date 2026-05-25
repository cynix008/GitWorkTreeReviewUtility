using WorktreeReviewTool.Models;

namespace WorktreeReviewTool.Services;

public sealed class WorktreeDisplayService
{
    public void DisplayAvailableWorktrees(IReadOnlyList<WorktreeDisplayItem> items)
    {
        Console.WriteLine("\nAvailable worktrees:");

        for (var i = 0; i < items.Count; i++)
        {
            var worktree = items[i].Worktree;
            var branch = string.IsNullOrWhiteSpace(worktree.Branch)
                ? "no branch detected"
                : worktree.Branch;

            Console.WriteLine();
            Console.WriteLine($"{i + 1}. {branch}");
            Console.WriteLine($"   Path: {worktree.Path}");
            Console.WriteLine($"   Status: {items[i].Status}");

            if (items[i].LastModified is not null)
                Console.WriteLine($"   Last modified: {items[i].LastModified:yyyy-MM-dd HH:mm}");
        }
    }
}
