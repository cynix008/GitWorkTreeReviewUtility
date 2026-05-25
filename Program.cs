using WorktreeReviewTool.Services;

const string ConfigFileName = "repos.json";

var configPath = Path.Combine(Environment.CurrentDirectory, ConfigFileName);
var configResult = ConfigService.LoadOrCreate(configPath);

if (configResult.WasCreated)
{
    Console.WriteLine($"Config Repository file was created here: {configPath}");
    Console.WriteLine("Edit repos.json with your repo folders, then run this app again.");
    return;
}

var app = new WorktreeReviewApp(configResult.Config);
app.Run();
