#if WINDOWS
#nullable enable
using WinApplication = Microsoft.UI.Xaml.Application;
using WinResourceDictionary = Microsoft.UI.Xaml.ResourceDictionary;

namespace Plugin.Maui.SegmentedControl.Windows;

/// <summary>
/// Merges SegmentedStyles.xaml (the default styles for <see cref="Segmented"/> and <see cref="SegmentedItem"/>)
/// into the application resources, once.
/// </summary>
static class SegmentedResources
{
    static readonly Uri StylesSource = new("ms-appx:///Plugin.Maui.SegmentedControl/Platforms/Windows/Themes/SegmentedStyles.xaml");

    /// <summary>
    /// Called before a <see cref="Segmented"/> is created, so the styles are there when its template is applied.
    /// Safe to call repeatedly: it does nothing when the dictionary is already merged or there is no application yet.
    /// </summary>
    internal static void EnsureLoaded()
    {
        var resources = WinApplication.Current?.Resources;
        if (resources is null)
        {
            return;
        }

        foreach (var dictionary in resources.MergedDictionaries)
        {
            if (dictionary.Source == StylesSource)
            {
                return;
            }
        }

        resources.MergedDictionaries.Add(new WinResourceDictionary { Source = StylesSource });
    }
}
#endif
