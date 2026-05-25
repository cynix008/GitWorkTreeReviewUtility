namespace WorktreeReviewTool.Models;

public sealed record WorktreePlan(
    string RepoName,
    string RepoPath,
    string MainBranch,
    string SourceRef,
    string RemoteSourceBranch,
    string ReviewBranchName,
    string WorktreeRoot,
    string WorktreePath);
