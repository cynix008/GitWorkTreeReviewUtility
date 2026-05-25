using WorktreeReviewTool.Models;
using WorktreeReviewTool.Services;
using System.Text.RegularExpressions;

namespace WorktreeReviewTool.Validation;

public sealed class RepositoryValidator
{
    private readonly GitService _git;

    public RepositoryValidator(GitService git)
    {
        _git = git;
    }

    public List<RepoValidationResult> ValidateAll(IEnumerable<RepoConfig> repositories)
    {
        return repositories.Select(Validate).ToList();
    }

    public RepoValidationResult Validate(RepoConfig repo)
    {
        var result = new RepoValidationResult(repo);
        var displayName = string.IsNullOrWhiteSpace(repo.Name) ? "(missing name)" : repo.Name;

        if (string.IsNullOrWhiteSpace(repo.Name))
            result.Errors.Add("Name is missing.");

        if (string.IsNullOrWhiteSpace(repo.Path))
        {
            result.Errors.Add($"Repo path is missing for repo: {displayName}");
        }
        else if (!Directory.Exists(repo.Path))
        {
            result.Errors.Add($"Repo folder does not exist: {repo.Path}");
        }
        else if (!_git.IsGitRepository(repo.Path))
        {
            result.Errors.Add($"Folder is not a Git repository: {repo.Path}");
        }

        if (string.IsNullOrWhiteSpace(repo.MainBranch))
            result.Errors.Add("MainBranch is missing.");

        ValidateWorktreeRoot(repo, result);
        ValidateReviewCommand(repo, result);

        return result;
    }

    private static void ValidateWorktreeRoot(RepoConfig repo, RepoValidationResult result)
    {
        if (!string.IsNullOrWhiteSpace(repo.WorktreeRoot))
            return;

        if (string.IsNullOrWhiteSpace(repo.Path) || Directory.GetParent(repo.Path) is null)
        {
            result.Errors.Add("WorktreeRoot is missing and could not be inferred from Path.");
            return;
        }

        result.Warnings.Add("WorktreeRoot is missing; it will be inferred from the repo path.");
    }

    private static void ValidateReviewCommand(RepoConfig repo, RepoValidationResult result)
    {
        if (string.IsNullOrWhiteSpace(repo.ReviewCommand))
        {
            result.Errors.Add("ReviewCommand is missing.");
            return;
        }

        if (!repo.ReviewCommand.Equals("cmd.exe", StringComparison.OrdinalIgnoreCase))
            return;

        var codeCmdArgument = repo.ReviewArguments.FirstOrDefault(argument =>
            argument.Contains("code.cmd", StringComparison.OrdinalIgnoreCase));

        if (string.IsNullOrWhiteSpace(codeCmdArgument))
            return;

        var candidate = ExtractCodeCmdPath(codeCmdArgument);
        if (Path.IsPathRooted(candidate) && !File.Exists(candidate))
        {
            result.Errors.Add($"ReviewArguments references a code.cmd path that does not exist: {candidate}");
        }
        else if (!Path.IsPathRooted(candidate))
        {
            result.Warnings.Add("ReviewArguments references code.cmd without an absolute path; run 'where code.cmd' if VS Code does not open.");
        }
    }

    private static string ExtractCodeCmdPath(string argument)
    {
        var trimmed = argument.Trim().Trim('"');
        var match = Regex.Match(trimmed, "[A-Za-z]:\\\\[^\"']*?code\\.cmd", RegexOptions.IgnoreCase);
        return match.Success ? match.Value : trimmed;
    }
}
