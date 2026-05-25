using WorktreeReviewTool.Helpers;
using WorktreeReviewTool.Models;
using WorktreeReviewTool.Planning;
using WorktreeReviewTool.Selectors;
using WorktreeReviewTool.Utilities;
using WorktreeReviewTool.Validation;

namespace WorktreeReviewTool.Services;

public sealed class WorktreeReviewApp
{
    private readonly AppConfig _config;
    private readonly GitService _git;
    private readonly ReviewToolLauncher _reviewToolLauncher;
    private readonly WorktreeStatusService _worktreeStatusService;
    private readonly WorktreeDisplayService _worktreeDisplayService;
    private readonly RepositoryValidator _repositoryValidator;
    private readonly ConsoleLogger _logger;
    private readonly DryRunContext _dryRun;
    private List<RepoConfig> _selectableRepositories = [];

    public WorktreeReviewApp(AppConfig config)
    {
        _config = config;
        _logger = new ConsoleLogger();
        _dryRun = new DryRunContext();
        _git = new GitService(_logger, _dryRun);
        _reviewToolLauncher = new ReviewToolLauncher(_logger, _dryRun);
        _worktreeStatusService = new WorktreeStatusService(_git);
        _worktreeDisplayService = new WorktreeDisplayService();
        _repositoryValidator = new RepositoryValidator(_git);
    }

    public void Run()
    {
        if (_config.Repositories.Count == 0)
        {
            _logger.Warning("No repositories found in repos.json.");
            return;
        }

        ValidateRepositoriesOnStartup();
        if (_selectableRepositories.Count == 0)
        {
            ConsoleUi.Pause("No selectable repositories remain after validation.");
            return;
        }

        while (true)
        {
            ConsoleUi.Clear();
            Console.WriteLine("Git Worktree Review Utility");
            if (_dryRun.IsEnabled)
                _logger.Warning("[DRY RUN ENABLED]");
            Console.WriteLine("===========================");
            Console.WriteLine("1. Create worktree for code review");
            Console.WriteLine("2. Remove worktree and review branch");
            Console.WriteLine("3. Toggle dry-run mode");
            Console.WriteLine("0. Exit");
            Console.Write("\nChoose an option: ");

            var option = Console.ReadLine()?.Trim();

            switch (option)
            {
                case "1":
                    CreateWorktreeForReview();
                    break;
                case "2":
                    RemoveWorktreeAndReviewBranch();
                    break;
                case "3":
                    _dryRun.Toggle();
                    ConsoleUi.Pause(_dryRun.IsEnabled ? "Dry-run mode enabled." : "Dry-run mode disabled.");
                    break;
                case "0":
                    return;
                default:
                    ConsoleUi.Pause("Invalid option.");
                    break;
            }
        }
    }

    private void CreateWorktreeForReview()
    {
        ConsoleUi.Clear();
        Console.WriteLine("Create Worktree");
        Console.WriteLine("===============");

        var repo = RepoSelector.Select(_selectableRepositories);
        if (repo is null) return;

        var sourceBranch = ConsoleUi.PromptRequired("\nEnter source branch name, for example feature/ABC-123-long-name: ");
        if (sourceBranch is null) return;

        var plan = WorktreePlanner.CreatePlan(repo, sourceBranch);
        DisplayCreatePlan(plan);

        if (!ConsoleUi.Confirm("\nCreate this worktree?")) return;

        try
        {
            FetchRequiredRemoteBranches(repo, plan.RemoteSourceBranch);

            if (_dryRun.IsEnabled)
                _logger.Command($"[DRY RUN] create directory {plan.WorktreeRoot}");
            else
                Directory.CreateDirectory(plan.WorktreeRoot);

            _git.AddWorktree(
                repoPath: repo.Path,
                worktreePath: plan.WorktreePath,
                reviewBranchName: plan.ReviewBranchName,
                sourceRef: plan.SourceRef);

            _logger.Success(_dryRun.IsEnabled ? "\nDry run complete. No worktree was created." : "\nWorktree created successfully.");
            Console.WriteLine($"Location: {plan.WorktreePath}");

            _reviewToolLauncher.Start(repo, plan.WorktreePath);
        }
        catch (Exception ex)
        {
            _logger.Error("\nFailed to create worktree.");
            _logger.Error(ex.Message);
        }

        ConsoleUi.Pause();
    }

    private void RemoveWorktreeAndReviewBranch()
    {
        ConsoleUi.Clear();
        Console.WriteLine("Remove Worktree");
        Console.WriteLine("===============");

        var repo = RepoSelector.Select(_selectableRepositories);
        if (repo is null) return;

        try
        {
            var worktrees = _git.GetWorktrees(repo.Path)
                .Where(worktree => !PathHelper.SamePath(worktree.Path, repo.Path))
                .ToList();

            if (worktrees.Count == 0)
            {
                ConsoleUi.Pause("No extra worktrees found for this repository.");
                return;
            }

            var displayItems = _worktreeStatusService.BuildDisplayItems(worktrees);
            _worktreeDisplayService.DisplayAvailableWorktrees(displayItems);

            var selectedItem = WorktreeSelector.Select(displayItems);
            if (selectedItem is null) return;

            var worktree = selectedItem.Worktree;

            DisplayRemovePlan(worktree);

            if (!ConsoleUi.Confirm("\nRemove this worktree and delete its review branch?")) return;

            var latestStatus = _worktreeStatusService.GetStatus(worktree);
            if (latestStatus.HasChanges && !ConfirmDirtyWorktreeRemoval(latestStatus))
                return;

            _git.RemoveWorktree(repo.Path, worktree.Path);
            DeleteReviewBranchIfSafe(repo.Path, worktree.Branch);
            _git.PruneWorktrees(repo.Path);

            _logger.Success(_dryRun.IsEnabled ? "\nDry run complete. No worktree was removed." : "\nWorktree cleanup completed.");
        }
        catch (Exception ex)
        {
            _logger.Error("\nFailed to remove worktree.");
            _logger.Error(ex.Message);
        }

        ConsoleUi.Pause();
    }

    private void ValidateRepositoriesOnStartup()
    {
        var results = _repositoryValidator.ValidateAll(_config.Repositories);

        foreach (var result in results)
        {
            var repoName = string.IsNullOrWhiteSpace(result.Repo.Name) ? "(missing name)" : result.Repo.Name;
            Console.WriteLine($"\nValidated Repository: {repoName}");
            Console.WriteLine(result.Repo.Path);

            if (result.Errors.Count == 0 && result.Warnings.Count == 0)
            {
                _logger.Success("  OK");
                continue;
            }

            foreach (var warning in result.Warnings)
                _logger.Warning($"  Warning: {warning}");

            foreach (var error in result.Errors)
                _logger.Error($"  Error: {error}");
        }

        _selectableRepositories = results
            .Where(result => result.IsSelectable)
            .Select(result => result.Repo)
            .ToList();

        ConsoleUi.Pause();
    }

    private void FetchRequiredRemoteBranches(RepoConfig repo, string remoteSourceBranch)
    {
        Console.WriteLine("\nFetching latest configured main branch from origin...");
        _git.FetchRemoteBranch(repo.Path, repo.MainBranch);

        Console.WriteLine("\nFetching selected source branch from origin...");
        _git.FetchRemoteBranch(repo.Path, remoteSourceBranch);

        _logger.Success("\nYour current working branch was not changed.");
    }


    private void DeleteReviewBranchIfSafe(string repoPath, string branchName)
    {
        if (string.IsNullOrWhiteSpace(branchName))
        {
            _logger.Warning("Skipped branch delete because no branch was detected.");
            return;
        }

        if (!branchName.StartsWith("review/", StringComparison.OrdinalIgnoreCase))
        {
            _logger.Warning("Skipped branch delete because the branch was not a review/* branch.");
            return;
        }

        _git.DeleteLocalBranch(repoPath, branchName);
        _logger.Success(_dryRun.IsEnabled
            ? $"Review branch delete planned: {branchName}"
            : $"Deleted local review branch: {branchName}");
    }

    private bool ConfirmDirtyWorktreeRemoval(WorktreeDisplayItem item)
    {
        _logger.Warning("\nWARNING: This worktree has uncommitted changes.");
        _logger.Warning("The following files may be lost:");

        foreach (var changedFile in item.ChangedFiles)
            Console.WriteLine(changedFile);

        return ConsoleUi.Confirm("\nForce remove this worktree and discard these changes?");
    }

    private static void DisplayCreatePlan(WorktreePlan plan)
    {
        Console.WriteLine("\nSummary");
        Console.WriteLine("-------");
        Console.WriteLine($"Repo:             {plan.RepoName}");
        Console.WriteLine($"Repo path:        {plan.RepoPath}");
        Console.WriteLine($"Main branch:      {plan.MainBranch}");
        Console.WriteLine($"Source branch:    {plan.SourceRef}");
        Console.WriteLine($"Review branch:    {plan.ReviewBranchName}");
        Console.WriteLine($"Worktree path:    {plan.WorktreePath}");
    }

    private static void DisplayRemovePlan(GitWorktree worktree)
    {
        Console.WriteLine("\nSummary");
        Console.WriteLine("-------");
        Console.WriteLine($"Worktree path: {worktree.Path}");
        Console.WriteLine($"Branch:        {worktree.Branch}");
    }
}
