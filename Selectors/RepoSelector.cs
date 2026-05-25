using WorktreeReviewTool.Models;
using WorktreeReviewTool.Utilities;

namespace WorktreeReviewTool.Selectors;

public static class RepoSelector
{
    public static RepoConfig? Select(List<RepoConfig> repositories)
    {
        Console.WriteLine("\nPreconfigured repositories:");

        for (var i = 0; i < repositories.Count; i++)
        {
            Console.WriteLine($"{i + 1}. {repositories[i].Name} - {repositories[i].Path}");
        }

        Console.Write("\nSelect repo: ");
        if (!int.TryParse(Console.ReadLine(), out var selected) || selected < 1 || selected > repositories.Count)
        {
            ConsoleUi.Pause("Invalid repo selection.");
            return null;
        }

        return repositories[selected - 1];
    }
}
