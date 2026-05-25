using WorktreeReviewTool.Helpers;
using WorktreeReviewTool.Models;

namespace WorktreeReviewTool.Planning;

public static class WorktreePlanner
{
    private const int ShortWorktreeNameLength = 6;

    public static WorktreePlan CreatePlan(RepoConfig repo, string sourceBranch)
    {
        var remoteSourceBranch = BranchNameHelper.GetRemoteBranchName(sourceBranch);
        var sourceRef = $"origin/{remoteSourceBranch}";
        var shortName = BranchNameHelper.MakeShortName(sourceBranch, ShortWorktreeNameLength);
        var worktreeRoot = WorktreePathHelper.GetWorktreeRoot(repo);
        var repoFolderName = WorktreePathHelper.MakeSafeFolderName(new DirectoryInfo(repo.Path).Name);
        var folderName = $"{repoFolderName}-review-{shortName}";
        var worktreePath = WorktreePathHelper.GetAvailableWorktreePath(worktreeRoot, folderName);
        var reviewBranchName = $"review/{Path.GetFileName(worktreePath)}";

        return new WorktreePlan(
            RepoName: repo.Name,
            RepoPath: repo.Path,
            MainBranch: repo.MainBranch,
            SourceRef: sourceRef,
            RemoteSourceBranch: remoteSourceBranch,
            ReviewBranchName: reviewBranchName,
            WorktreeRoot: worktreeRoot,
            WorktreePath: worktreePath);
    }
}
