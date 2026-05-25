using WorktreeReviewTool.Models;
using WorktreeReviewTool.Utilities;

namespace WorktreeReviewTool.Services;

public sealed class ReviewToolLauncher
{
    private readonly ConsoleLogger _logger;
    private readonly DryRunContext _dryRun;

    public ReviewToolLauncher(ConsoleLogger logger, DryRunContext dryRun)
    {
        _logger = logger;
        _dryRun = dryRun;
    }

    public void Start(RepoConfig repo, string worktreePath)
    {
        if (string.IsNullOrWhiteSpace(repo.ReviewCommand))
        {
            _logger.Warning("No review command configured. Open the worktree manually.");
            return;
        }

        try
        {
            var arguments = repo.ReviewArguments.Count > 0 ? repo.ReviewArguments : ["."];
            var resolvedArguments = arguments
                .Select(argument => argument.Replace("{path}", worktreePath))
                .ToArray();

            if (_dryRun.IsEnabled)
            {
                _logger.Command($"[DRY RUN] {repo.ReviewCommand} {string.Join(" ", resolvedArguments)}");
                return;
            }

            ProcessRunner.StartDetached(repo.ReviewCommand, worktreePath, resolvedArguments);
            _logger.Success($"Started review tool: {repo.ReviewCommand}");
        }
        catch (Exception ex)
        {
            _logger.Error("Worktree was created, but the review tool could not be started.");
            _logger.Error(ex.Message);

            if (IsCodeCommand(repo))
                _logger.Warning("Suggested next step: run 'where code.cmd' and update ReviewCommand or ReviewArguments with the correct VS Code path.");
        }
    }

    private static bool IsCodeCommand(RepoConfig repo)
    {
        return repo.ReviewCommand?.Contains("code", StringComparison.OrdinalIgnoreCase) == true
            || repo.ReviewArguments.Any(argument => argument.Contains("code.cmd", StringComparison.OrdinalIgnoreCase));
    }
}
