using WorktreeReviewTool.Models;

namespace WorktreeReviewTool.Parsing;

public static class GitWorktreeParser
{
    public static List<GitWorktree> Parse(string porcelainOutput)
    {
        var result = new List<GitWorktree>();
        GitWorktree? current = null;

        var lines = porcelainOutput.Split(
            new[] { "\r\n", "\n" },
            StringSplitOptions.None);

        foreach (var rawLine in lines)
        {
            var line = rawLine.Trim();

            if (string.IsNullOrWhiteSpace(line))
                continue;

            if (line.StartsWith("worktree ", StringComparison.OrdinalIgnoreCase))
            {
                if (current is not null)
                    result.Add(current);

                current = new GitWorktree
                {
                    Path = line["worktree ".Length..].Trim()
                };

                continue;
            }

            if (line.StartsWith("branch ", StringComparison.OrdinalIgnoreCase) && current is not null)
            {
                current.Branch = CleanBranchRef(line["branch ".Length..].Trim());
            }
        }

        if (current is not null)
            result.Add(current);

        return result;
    }

    private static string CleanBranchRef(string branchRef)
    {
        return branchRef.Replace("refs/heads/", "", StringComparison.OrdinalIgnoreCase);
    }
}