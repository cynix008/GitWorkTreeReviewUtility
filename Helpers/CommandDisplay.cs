namespace WorktreeReviewTool.Helpers;

public static class CommandDisplay
{
    public static string Format(IEnumerable<string> arguments)
    {
        return string.Join(" ", arguments.Select(Escape));
    }

    private static string Escape(string argument)
    {
        if (!argument.Contains(' ') && !argument.Contains('"'))
            return argument;

        return $"\"{argument.Replace("\"", "\\\"")}\"";
    }
}
