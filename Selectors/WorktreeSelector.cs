using WorktreeReviewTool.Models;
using WorktreeReviewTool.Utilities;

namespace WorktreeReviewTool.Selectors;

public static class WorktreeSelector
{
    public static WorktreeDisplayItem? Select(List<WorktreeDisplayItem> worktrees)
    {
        Console.Write("\nSelect worktree to remove: ");
        if (!int.TryParse(Console.ReadLine(), out var selected) || selected < 1 || selected > worktrees.Count)
        {
            ConsoleUi.Pause("Invalid selection.");
            return null;
        }

        return worktrees[selected - 1];
    }
}
