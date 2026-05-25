namespace WorktreeReviewTool.Utilities;

public sealed class ConsoleLogger
{
    public void Command(string message) => WriteColored(message, ConsoleColor.Cyan);

    public void Success(string message) => WriteColored(message, ConsoleColor.Green);

    public void Warning(string message) => WriteColored(message, ConsoleColor.Yellow);

    public void Error(string message) => WriteColored(message, ConsoleColor.Red);

    public void Info(string message) => Console.WriteLine(message);

    private static void WriteColored(string message, ConsoleColor color)
    {
        var previousColor = Console.ForegroundColor;

        try
        {
            Console.ForegroundColor = color;
            Console.WriteLine(message);
        }
        finally
        {
            Console.ForegroundColor = previousColor;
        }
    }
}
