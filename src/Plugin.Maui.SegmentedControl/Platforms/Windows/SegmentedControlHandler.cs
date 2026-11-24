#if WINDOWS
#nullable enable
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Plugin.Maui.SegmentedControl.Control;
using Plugin.Maui.SegmentedControl.Windows;
using WinBrush = Microsoft.UI.Xaml.Media.SolidColorBrush;
using WinListViewSelectionMode = Microsoft.UI.Xaml.Controls.ListViewSelectionMode;

namespace Plugin.Maui.SegmentedControl.Handlers;

public class SegmentedControlHandler : ViewHandler<SegmentedControl, Segmented>
{
    // Cached brushes to avoid per-update allocations
    WinBrush tintBrush = new(Microsoft.UI.Colors.Transparent);
    WinBrush selectedTextBrush = new(Microsoft.UI.Colors.Transparent);
    WinBrush textBrush = new(Microsoft.UI.Colors.Transparent);
    WinBrush disabledTextBrush = new(Microsoft.UI.Colors.Transparent);
    WinBrush disabledBackgroundBrush = new(Microsoft.UI.Colors.Transparent);
    WinBrush disabledTintBrush = new(Microsoft.UI.Colors.Transparent);

    public static IPropertyMapper<SegmentedControl, SegmentedControlHandler> Mapper =
        new PropertyMapper<SegmentedControl, SegmentedControlHandler>(ViewMapper)
        {
            [nameof(SegmentedControl.IsEnabled)] = MapIsEnabled,
            [nameof(SegmentedControl.SelectedSegment)] = MapSelectedSegment,
            [nameof(SegmentedControl.TintColor)] = MapTintColor,
            [nameof(SegmentedControl.SelectedTextColor)] = MapSelectedTextColor,
            [nameof(SegmentedControl.TextColor)] = MapTextColor,
            [nameof(SegmentedControl.DisabledBackgroundColor)] = MapDisabledBackgroundColor,
            [nameof(SegmentedControl.DisabledTextColor)] = MapDisabledTextColor,
            [nameof(SegmentedControl.DisabledTintColor)] = MapDisabledTintColor,
            [nameof(SegmentedControl.Children)] = MapChildren,
            [nameof(SegmentedControl.FontSize)] = MapFontSize,
            [nameof(SegmentedControl.Padding)] = MapPadding,
        };

    public SegmentedControlHandler() : base(Mapper)
    {
    }

    public SegmentedControlHandler(IPropertyMapper mapper) : base(mapper ?? Mapper)
    {
    }

    protected override Segmented CreatePlatformView()
    {
        var segmented = new Segmented
        {
            SelectionMode = VirtualView.GroupToggleBehavior == GroupToggleBehavior.Radio
                ? WinListViewSelectionMode.Single
                : WinListViewSelectionMode.None,
            HorizontalAlignment = Microsoft.UI.Xaml.HorizontalAlignment.Stretch,
        };

        ApplyColors(segmented);
        BuildItems(segmented);

        if (VirtualView.GroupToggleBehavior == GroupToggleBehavior.Radio
            && VirtualView.SelectedSegment >= 0)
        {
            segmented.SelectedIndex = VirtualView.SelectedSegment;
        }

        return segmented;
    }

    protected override void ConnectHandler(Segmented platformView)
    {
        base.ConnectHandler(platformView);
        platformView.SelectionChanged += OnSelectionChanged;
        platformView.Tapped += OnTapped;
    }

    protected override void DisconnectHandler(Segmented platformView)
    {
        platformView.SelectionChanged -= OnSelectionChanged;
        platformView.Tapped -= OnTapped;
        base.DisconnectHandler(platformView);
    }

    void OnSelectionChanged(object sender, Microsoft.UI.Xaml.Controls.SelectionChangedEventArgs e)
    {
        if (VirtualView.GroupToggleBehavior != GroupToggleBehavior.Radio)
        {
            return;
        }

        var newIndex = PlatformView.SelectedIndex;
        if (newIndex < 0 || newIndex >= VirtualView.Children.Count)
        {
            return;
        }

        VirtualView.SelectedSegment = newIndex;
    }

    void OnTapped(object sender, Microsoft.UI.Xaml.Input.TappedRoutedEventArgs e)
    {
        // Resolve which item was tapped by walking up the visual tree
        if (e.OriginalSource is not Microsoft.UI.Xaml.FrameworkElement source)
        {
            return;
        }

        var item = FindAncestorOrSelf<SegmentedItem>(source);
        if (item is null)
        {
            return;
        }

        var index = PlatformView.IndexFromContainer(item);
        if (index < 0 || index >= VirtualView.Children.Count)
        {
            return;
        }

        VirtualView.SendSegmentTapped(index);
    }

    static T? FindAncestorOrSelf<T>(Microsoft.UI.Xaml.DependencyObject obj) where T : Microsoft.UI.Xaml.DependencyObject
    {
        while (obj is not null)
        {
            if (obj is T result)
            {
                return result;
            }

            obj = Microsoft.UI.Xaml.Media.VisualTreeHelper.GetParent(obj);
        }

        return null;
    }

    void BuildItems(Segmented segmented)
    {
        segmented.Items.Clear();

        var children = VirtualView.Children;
        if (children is null || children.Count == 0)
        {
            return;
        }

        foreach (var child in children)
        {
            segmented.Items.Add(new SegmentedItem
            {
                Content = child.Text,
                IsEnabled = VirtualView.IsEnabled && child.IsEnabled,
                FontSize = VirtualView.FontSize,
                Padding = GetItemPadding(),
            });
        }
    }

    Microsoft.UI.Xaml.Thickness GetItemPadding()
    {
        var p = VirtualView.Padding;
        return p == new Thickness(0)
            ? new Microsoft.UI.Xaml.Thickness(11)
            : new Microsoft.UI.Xaml.Thickness(p.Left, p.Top, p.Right, p.Bottom);
    }

    void ApplyColors(Segmented segmented)
    {
        tintBrush = new WinBrush(VirtualView.TintColor.ToWindowsColor());
        selectedTextBrush = new WinBrush(VirtualView.SelectedTextColor.ToWindowsColor());
        textBrush = new WinBrush(VirtualView.TextColor.ToWindowsColor());
        disabledTextBrush = new WinBrush(VirtualView.DisabledTextColor.ToWindowsColor());
        // DisabledBackgroundColor: background of disabled unselected segments
        disabledBackgroundBrush = new WinBrush(VirtualView.DisabledBackgroundColor.ToWindowsColor());
        // DisabledTintColor: border/outline color when the control is disabled
        disabledTintBrush = new WinBrush(VirtualView.DisabledTintColor.ToWindowsColor());

        segmented.Resources["ButtonItemBackgroundSelected"] = tintBrush;
        segmented.Resources["ButtonItemBackgroundSelectedPointerOver"] = tintBrush;
        segmented.Resources["ButtonItemBackgroundSelectedPressed"] = tintBrush;
        segmented.Resources["ButtonItemForegroundSelected"] = selectedTextBrush;
        segmented.Resources["ButtonItemForegroundSelectedPointerOver"] = selectedTextBrush;
        segmented.Resources["ButtonItemForegroundSelectedPressed"] = selectedTextBrush;
        segmented.Resources["ButtonItemForeground"] = textBrush;
        segmented.Resources["ButtonItemForegroundPointerOver"] = textBrush;
        segmented.Resources["ButtonItemForegroundPressed"] = textBrush;
        segmented.Resources["ButtonItemForegroundDisabled"] = disabledTextBrush;
        // DisabledBackgroundColor controls the item background when disabled
        segmented.Resources["ButtonItemBackgroundDisabled"] = disabledBackgroundBrush;
        // Border reflects TintColor when enabled, DisabledTintColor when disabled
        segmented.Resources["SegmentedBorderBrush"] = VirtualView.IsEnabled
            ? tintBrush
            : disabledTintBrush;
    }

    static void MapIsEnabled(SegmentedControlHandler handler, SegmentedControl control)
    {
        for (int i = 0; i < handler.PlatformView.Items.Count && i < control.Children.Count; i++)
        {
            if (handler.PlatformView.ContainerFromIndex(i) is SegmentedItem item)
            {
                item.IsEnabled = control.IsEnabled && control.Children[i].IsEnabled;
            }
        }

        // Toggle border between TintColor (enabled) and DisabledTintColor (disabled)
        handler.PlatformView.Resources["SegmentedBorderBrush"] = control.IsEnabled
            ? handler.tintBrush
            : handler.disabledTintBrush;
    }

    static void MapSelectedSegment(SegmentedControlHandler handler, SegmentedControl control)
    {
        if (control.GroupToggleBehavior != GroupToggleBehavior.Radio)
        {
            return;
        }

        if (handler.PlatformView.SelectedIndex != control.SelectedSegment)
        {
            handler.PlatformView.SelectedIndex = control.SelectedSegment;
        }

        control.SendValueChanged();
    }

    static void MapTintColor(SegmentedControlHandler handler, SegmentedControl control)
    {
        handler.tintBrush = new WinBrush(control.TintColor.ToWindowsColor());
        handler.PlatformView.Resources["ButtonItemBackgroundSelected"] = handler.tintBrush;
        handler.PlatformView.Resources["ButtonItemBackgroundSelectedPointerOver"] = handler.tintBrush;
        handler.PlatformView.Resources["ButtonItemBackgroundSelectedPressed"] = handler.tintBrush;
        // Only update the border if the control is currently enabled
        if (control.IsEnabled)
        {
            handler.PlatformView.Resources["SegmentedBorderBrush"] = handler.tintBrush;
        }
    }

    static void MapSelectedTextColor(SegmentedControlHandler handler, SegmentedControl control)
    {
        handler.selectedTextBrush = new WinBrush(control.SelectedTextColor.ToWindowsColor());
        handler.PlatformView.Resources["ButtonItemForegroundSelected"] = handler.selectedTextBrush;
        handler.PlatformView.Resources["ButtonItemForegroundSelectedPointerOver"] = handler.selectedTextBrush;
        handler.PlatformView.Resources["ButtonItemForegroundSelectedPressed"] = handler.selectedTextBrush;
    }

    static void MapTextColor(SegmentedControlHandler handler, SegmentedControl control)
    {
        handler.textBrush = new WinBrush(control.TextColor.ToWindowsColor());
        handler.PlatformView.Resources["ButtonItemForeground"] = handler.textBrush;
        handler.PlatformView.Resources["ButtonItemForegroundPointerOver"] = handler.textBrush;
        handler.PlatformView.Resources["ButtonItemForegroundPressed"] = handler.textBrush;
    }

    static void MapDisabledBackgroundColor(SegmentedControlHandler handler, SegmentedControl control)
    {
        // DisabledBackgroundColor controls the background of disabled unselected segments
        handler.disabledBackgroundBrush = new WinBrush(control.DisabledBackgroundColor.ToWindowsColor());
        handler.PlatformView.Resources["ButtonItemBackgroundDisabled"] = handler.disabledBackgroundBrush;
    }

    static void MapDisabledTextColor(SegmentedControlHandler handler, SegmentedControl control)
    {
        handler.disabledTextBrush = new WinBrush(control.DisabledTextColor.ToWindowsColor());
        handler.PlatformView.Resources["ButtonItemForegroundDisabled"] = handler.disabledTextBrush;
    }

    static void MapDisabledTintColor(SegmentedControlHandler handler, SegmentedControl control)
    {
        // DisabledTintColor controls the border/outline when the control is disabled
        handler.disabledTintBrush = new WinBrush(control.DisabledTintColor.ToWindowsColor());
        if (!control.IsEnabled)
        {
            handler.PlatformView.Resources["SegmentedBorderBrush"] = handler.disabledTintBrush;
        }
    }

    static void MapFontSize(SegmentedControlHandler handler, SegmentedControl control)
    {
        for (int i = 0; i < handler.PlatformView.Items.Count; i++)
        {
            if (handler.PlatformView.ContainerFromIndex(i) is SegmentedItem item)
            {
                item.FontSize = control.FontSize;
            }
        }
    }

    static void MapPadding(SegmentedControlHandler handler, SegmentedControl control)
    {
        var padding = control.Padding;
        var thickness = padding == new Thickness(0)
            ? new Microsoft.UI.Xaml.Thickness(11)
            : new Microsoft.UI.Xaml.Thickness(padding.Left, padding.Top, padding.Right, padding.Bottom);

        for (int i = 0; i < handler.PlatformView.Items.Count; i++)
        {
            if (handler.PlatformView.ContainerFromIndex(i) is SegmentedItem item)
            {
                item.Padding = thickness;
            }
        }
    }

    static void MapChildren(SegmentedControlHandler handler, SegmentedControl control)
    {
        handler.BuildItems(handler.PlatformView);

        if (control.GroupToggleBehavior == GroupToggleBehavior.Radio
            && control.SelectedSegment >= 0
            && control.SelectedSegment < handler.PlatformView.Items.Count)
        {
            handler.PlatformView.SelectedIndex = control.SelectedSegment;
        }
    }
}
#endif
