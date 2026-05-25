using WorktreeReviewTool.Helpers;

namespace WorktreeReviewTool.Services;

public sealed class GitCommandException : InvalidOperationException
{
    public GitCommandException(
        string workingDirectory,
        IReadOnlyList<string> arguments,
        int exitCode,
        string standardError,
        string standardOutput)
        : base(BuildMessage(workingDirectory, arguments, exitCode, standardError, standardOutput))
    {
        WorkingDirectory = workingDirectory;
        Arguments = arguments;
        ExitCode = exitCode;
        StandardError = standardError;
        StandardOutput = standardOutput;
    }

    public string WorkingDirectory { get; }
    public IReadOnlyList<string> Arguments { get; }
    public int ExitCode { get; }
    public string StandardError { get; }
    public string StandardOutput { get; }

    private static string BuildMessage(
        string workingDirectory,
        IReadOnlyList<string> arguments,
        int exitCode,
        string standardError,
        string standardOutput)
    {
        var command = $"git {CommandDisplay.Format(arguments)}";
        var errorOutput = string.IsNullOrWhiteSpace(standardError) ? standardOutput : standardError;
        var suggestion = GetSuggestion(arguments, errorOutput);

        var lines = new List<string>
        {
            $"Git command failed: {command}",
            $"Working directory: {workingDirectory}",
            $"Exit code: {exitCode}",
            "Error output:",
            string.IsNullOrWhiteSpace(errorOutput) ? "(none)" : errorOutput.Trim()
        };

        if (!string.IsNullOrWhiteSpace(suggestion))
            lines.Add($"Suggested next step: {suggestion}");

        return string.Join(Environment.NewLine, lines);
    }

    private static string? GetSuggestion(IReadOnlyList<string> arguments, string output)
    {
        if (output.Contains("authentication", StringComparison.OrdinalIgnoreCase)
            || output.Contains("permission denied", StringComparison.OrdinalIgnoreCase)
            || output.Contains("could not read username", StringComparison.OrdinalIgnoreCase)
            || output.Contains("repository not found", StringComparison.OrdinalIgnoreCase))
        {
            return "Check your Git credentials and Git Credential Manager sign-in.";
        }

        if (arguments.Count >= 2
            && arguments[0] == "worktree"
            && arguments[1] == "remove")
        {
            return "Close VS Code, terminals, or running processes inside that worktree folder, then try again.";
        }

        return null;
    }
}
