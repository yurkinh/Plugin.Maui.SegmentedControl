using Plugin.Maui.SegmentedControl.Handlers;

namespace Plugin.Maui.SegmentedControl;

public static class AppHostBuilderExtensions
{
    /// <summary>
    /// Configures the Plugin.Maui.SegmentedControl package.
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static MauiAppBuilder UseSegmentedControl(this MauiAppBuilder builder)
    {
        builder.ConfigureMauiHandlers(handlers =>
        {
            handlers.AddHandler<SegmentedControl, SegmentedControlHandler>();
        });

        // Windows: the WinUI styles are merged by the handler on first use (SegmentedResources.EnsureLoaded).

        return builder;
    }
}


