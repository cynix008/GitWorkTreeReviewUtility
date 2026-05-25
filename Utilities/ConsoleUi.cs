namespace WorktreeReviewTool.Utilities;

public static class ConsoleUi
{
    public static void Clear()
    {
        try
        {
            Console.Clear();
        }
        catch
        {
            // Some terminals do not support clearing. Ignore safely.
        }
    }

    public static string? PromptRequired(string message)
    {
        Console.Write(message);
        var value = Console.ReadLine()?.Trim();

        if (!string.IsNullOrWhiteSpace(value))
            return value;

        Pause("Value is required.");
        return null;
    }

    public static bool Confirm(string message)
    {
        Console.Write($"{message} [y/N]: ");
        var answer = Console.ReadLine()?.Trim();

        return string.Equals(answer, "y", StringComparison.OrdinalIgnoreCase)
            || string.Equals(answer, "yes", StringComparison.OrdinalIgnoreCase);
    }

    public static void Pause(string? message = null)
    {
        if (!string.IsNullOrWhiteSpace(message))
            Console.WriteLine(message);

        Console.WriteLine("\nPress Enter to continue...");
        Console.ReadLine();
    }
}
