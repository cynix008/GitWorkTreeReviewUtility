namespace WorktreeReviewTool.Models;

public sealed record ProcessResult(
    int ExitCode,
    string StandardOutput,
    string StandardError);
