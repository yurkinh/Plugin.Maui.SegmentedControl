using Plugin.Maui.SegmentedControl.Handlers;

namespace Plugin.Maui.SegmentedControl;

public static class AppHostBuilderExtensions
{
    /// <summary>
    /// Configures the SimpleToolkit.Core package.
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static MauiAppBuilder UseSegmentedControl(this MauiAppBuilder builder)
    {
        builder.ConfigureMauiHandlers(handlers =>
        {
            handlers.AddHandler<SegmentedControl, SegmentedControlHandler>();                
        });

        return builder;
    }
}

