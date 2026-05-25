using WorktreeReviewTool.Models;
using System.Text.RegularExpressions;

namespace WorktreeReviewTool.Helpers;

public static class WorktreePathHelper
{
    public static string GetWorktreeRoot(RepoConfig repo)
    {
        if (!string.IsNullOrWhiteSpace(repo.WorktreeRoot))
            return repo.WorktreeRoot;

        var repoParent = Directory.GetParent(repo.Path)?.FullName
            ?? throw new InvalidOperationException("Could not resolve repo parent folder.");

        return Path.Combine(repoParent, "worktrees", new DirectoryInfo(repo.Path).Name);
    }

    public static string GetAvailableWorktreePath(string worktreeRoot, string folderName)
    {
        var safeFolderName = MakeSafeFolderName(folderName);
        var path = Path.Combine(worktreeRoot, safeFolderName);

        if (!Directory.Exists(path))
            return path;

        var counter = 2;
        while (true)
        {
            var candidate = Path.Combine(worktreeRoot, $"{safeFolderName}-{counter}");
            if (!Directory.Exists(candidate))
                return candidate;

            counter++;
        }
    }

    public static string MakeSafeFolderName(string value)
    {
        var invalidCharacters = Regex.Escape(new string(Path.GetInvalidFileNameChars()));
        var safe = Regex.Replace(value.Trim(), $"[{invalidCharacters}]", "-");
        safe = Regex.Replace(safe, "-{2,}", "-").Trim('-', '.', ' ');

        return string.IsNullOrWhiteSpace(safe) ? "review" : safe;
    }
}
