using System.Text.RegularExpressions;

namespace WorktreeReviewTool.Helpers;

public static class BranchNameHelper
{
    public static string GetRemoteBranchName(string branchName)
    {
        var cleanBranchName = branchName.Trim();

        if (cleanBranchName.StartsWith("origin/", StringComparison.OrdinalIgnoreCase))
            cleanBranchName = cleanBranchName["origin/".Length..];

        return cleanBranchName;
    }

    public static string MakeShortName(string branchName, int maxLength)
    {
        var clean = RemoveOriginPrefix(branchName);
        clean = GetLastBranchSegment(clean);
        clean = KeepOnlySafeCharacters(clean);

        if (string.IsNullOrWhiteSpace(clean))
            clean = "review";

        return clean.Length <= maxLength ? clean : clean[..maxLength];
    }

    private static string RemoveOriginPrefix(string branchName)
    {
        var clean = branchName.Trim();

        if (clean.StartsWith("origin/", StringComparison.OrdinalIgnoreCase))
            clean = clean["origin/".Length..];

        return clean;
    }

    private static string GetLastBranchSegment(string branchName)
    {
        var parts = branchName.Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        return parts.Length > 0 ? parts[^1] : branchName;
    }

    private static string KeepOnlySafeCharacters(string value)
    {
        return Regex.Replace(value.ToLowerInvariant(), "[^a-z0-9]", "");
    }
}
