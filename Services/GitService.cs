using WorktreeReviewTool.Helpers;
using WorktreeReviewTool.Models;
using WorktreeReviewTool.Parsing;
using WorktreeReviewTool.Utilities;

namespace WorktreeReviewTool.Services;

public sealed class GitService
{
    private readonly ConsoleLogger _logger;
    private readonly DryRunContext _dryRun;

    public GitService(ConsoleLogger logger, DryRunContext dryRun)
    {
        _logger = logger;
        _dryRun = dryRun;
    }

    public void FetchRemoteBranch(string repoPath, string branchName)
    {
        var refSpec = $"+refs/heads/{branchName}:refs/remotes/origin/{branchName}";
        Run(repoPath, "fetch", "--verbose", "--progress", "--prune", "origin", refSpec);
    }

    public void AddWorktree(string repoPath, string worktreePath, string reviewBranchName, string sourceRef)
    {
        Run(repoPath, "worktree", "add", "-b", reviewBranchName, worktreePath, sourceRef);
    }

    public void RemoveWorktree(string repoPath, string worktreePath)
    {
        Run(repoPath, "worktree", "remove", "--force", worktreePath);
    }

    public void DeleteLocalBranch(string repoPath, string branchName)
    {
        Run(repoPath, "branch", "-D", branchName);
    }

    public void PruneWorktrees(string repoPath)
    {
        Run(repoPath, "worktree", "prune");
    }

    public List<GitWorktree> GetWorktrees(string repoPath)
    {
        var output = Capture(repoPath, "worktree", "list", "--porcelain");
        return GitWorktreeParser.Parse(output);
    }

    public string GetStatusPorcelain(string worktreePath)
    {
        return Capture(worktreePath, "status", "--porcelain");
    }

    public bool IsGitRepository(string repoPath)
    {
        if (_dryRun.IsEnabled)
            return Directory.Exists(repoPath);

        var result = ProcessRunner.Run("git", repoPath, "rev-parse", "--is-inside-work-tree");
        return result.ExitCode == 0 && result.StandardOutput.Trim().Equals("true", StringComparison.OrdinalIgnoreCase);
    }

    private void Run(string workingDirectory, params string[] arguments)
    {
        LogCommand(arguments);

        if (_dryRun.IsEnabled)
            return;

        var result = ProcessRunner.Run("git", workingDirectory, arguments);

        if (!string.IsNullOrWhiteSpace(result.StandardOutput))
            Console.WriteLine(result.StandardOutput.Trim());

        if (!string.IsNullOrWhiteSpace(result.StandardError))
            Console.WriteLine(result.StandardError.Trim());

        if (result.ExitCode != 0)
            throw new GitCommandException(workingDirectory, arguments, result.ExitCode, result.StandardError, result.StandardOutput);
    }

    private string Capture(string workingDirectory, params string[] arguments)
    {
        if (_dryRun.IsEnabled)
        {
            LogDryRunCommand(arguments);
            return "";
        }

        var result = ProcessRunner.Run("git", workingDirectory, arguments);

        if (result.ExitCode != 0)
            throw new GitCommandException(workingDirectory, arguments, result.ExitCode, result.StandardError, result.StandardOutput);

        return result.StandardOutput;
    }

    private void LogCommand(IReadOnlyList<string> arguments)
    {
        if (_dryRun.IsEnabled)
        {
            LogDryRunCommand(arguments);
            return;
        }

        _logger.Command($"\n> git {CommandDisplay.Format(arguments)}");
    }

    private void LogDryRunCommand(IReadOnlyList<string> arguments)
    {
        _logger.Command($"[DRY RUN] git {CommandDisplay.Format(arguments)}");
    }
}
