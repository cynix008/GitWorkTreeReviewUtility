using System.Diagnostics;
using WorktreeReviewTool.Models;

namespace WorktreeReviewTool.Services;

public static class ProcessRunner
{
    public static ProcessResult Run(string fileName, string workingDirectory, params string[] arguments)
    {
        var psi = CreateProcessStartInfo(fileName, workingDirectory, arguments);
        psi.RedirectStandardOutput = true;
        psi.RedirectStandardError = true;

        using var process = Process.Start(psi) ?? throw new InvalidOperationException($"Failed to start process: {fileName}");
        var stdout = process.StandardOutput.ReadToEnd();
        var stderr = process.StandardError.ReadToEnd();
        process.WaitForExit();

        return new ProcessResult(process.ExitCode, stdout, stderr);
    }

    public static void StartDetached(string fileName, string workingDirectory, params string[] arguments)
    {
        var psi = CreateProcessStartInfo(fileName, workingDirectory, arguments);
        Process.Start(psi);
    }

    private static ProcessStartInfo CreateProcessStartInfo(string fileName, string workingDirectory, params string[] arguments)
    {
        var psi = new ProcessStartInfo
        {
            FileName = fileName,
            WorkingDirectory = workingDirectory,
            UseShellExecute = false
        };

        foreach (var argument in arguments)
            psi.ArgumentList.Add(argument);

        return psi;
    }
}
