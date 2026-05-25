using System.Text.Json;
using WorktreeReviewTool.Models;

namespace WorktreeReviewTool.Services;

public static class ConfigService
{
    public static ConfigLoadResult LoadOrCreate(string configPath)
    {
        Console.WriteLine($"Loading configuration from: {configPath}"); 
        if (!File.Exists(configPath))
        {
            CreateSampleConfig(configPath);
            return new ConfigLoadResult(new AppConfig(), WasCreated: true);
        }

        var json = File.ReadAllText(configPath);
        var config = JsonSerializer.Deserialize<AppConfig>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true
        });

        return new ConfigLoadResult(config ?? new AppConfig(), WasCreated: false);
    }

    private static void CreateSampleConfig(string configPath)
    {
        var sample = new AppConfig
        {
            Repositories =
            [
                new RepoConfig
                {
                    Name = "Example API",
                    Path = @"C:\dev\company-api",
                    MainBranch = "main",
                    WorktreeRoot = @"C:\dev\worktrees\company-api",
                    ReviewCommand = "code",
                    ReviewArguments = ["."]
                },
                new RepoConfig
                {
                    Name = "Example Web",
                    Path = @"C:\dev\company-web",
                    MainBranch = "master",
                    WorktreeRoot = @"C:\dev\worktrees\company-web",
                    ReviewCommand = "code",
                    ReviewArguments = ["."]
                }
            ]
        };

        var json = JsonSerializer.Serialize(sample, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        File.WriteAllText(configPath, json);
    }
}
