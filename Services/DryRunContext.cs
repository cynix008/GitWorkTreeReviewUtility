namespace WorktreeReviewTool.Services;

public sealed class DryRunContext
{
    public bool IsEnabled { get; private set; }

    public void Toggle()
    {
        IsEnabled = !IsEnabled;
    }
}
