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

#if WINDOWS
        // Merge WinUI3 styles for the extracted Segmented control so DefaultStyleKey resolves correctly
        var resourceDict = new Microsoft.UI.Xaml.ResourceDictionary();
        resourceDict.Source = new Uri("ms-appx:///Plugin.Maui.SegmentedControl/Platforms/Windows/Themes/SegmentedStyles.xaml");
        Microsoft.UI.Xaml.Application.Current?.Resources?.MergedDictionaries?.Add(resourceDict);
#endif

        return builder;
    }
}


