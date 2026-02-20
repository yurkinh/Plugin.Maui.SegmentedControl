#if WINDOWS
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using Microsoft.UI.Xaml.Controls;
using Plugin.Maui.SegmentedControl.Control;
using Plugin.Maui.SegmentedControl.Windows;
using WinBrush = Microsoft.UI.Xaml.Media.SolidColorBrush;
using WinListViewSelectionMode = Microsoft.UI.Xaml.Controls.ListViewSelectionMode;

namespace Plugin.Maui.SegmentedControl.Handlers;

public class SegmentedControlHandler : ViewHandler<SegmentedControl, Segmented>
{
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
        var tintColor = new WinBrush(VirtualView.TintColor.ToWindowsColor());
        var selectedTextColor = new WinBrush(VirtualView.SelectedTextColor.ToWindowsColor());
        var textColor = new WinBrush(VirtualView.TextColor.ToWindowsColor());
        var disabledTextColor = new WinBrush(VirtualView.DisabledTextColor.ToWindowsColor());

        segmented.Resources["ButtonItemBackgroundSelected"] = tintColor;
        segmented.Resources["ButtonItemBackgroundSelectedPointerOver"] = tintColor;
        segmented.Resources["ButtonItemBackgroundSelectedPressed"] = tintColor;
        segmented.Resources["ButtonItemForegroundSelected"] = selectedTextColor;
        segmented.Resources["ButtonItemForegroundSelectedPointerOver"] = selectedTextColor;
        segmented.Resources["ButtonItemForegroundSelectedPressed"] = selectedTextColor;
        segmented.Resources["ButtonItemForeground"] = textColor;
        segmented.Resources["ButtonItemForegroundPointerOver"] = textColor;
        segmented.Resources["ButtonItemForegroundDisabled"] = disabledTextColor;
        segmented.Resources["ButtonItemBackgroundDisabled"] = new WinBrush(VirtualView.DisabledBackgroundColor.ToWindowsColor());
        segmented.Resources["SegmentedBorderBrush"] = tintColor;
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
        var brush = new WinBrush(control.TintColor.ToWindowsColor());
        handler.PlatformView.Resources["ButtonItemBackgroundSelected"] = brush;
        handler.PlatformView.Resources["ButtonItemBackgroundSelectedPointerOver"] = brush;
        handler.PlatformView.Resources["ButtonItemBackgroundSelectedPressed"] = brush;
        handler.PlatformView.Resources["SegmentedBorderBrush"] = brush;
    }

    static void MapSelectedTextColor(SegmentedControlHandler handler, SegmentedControl control)
    {
        var brush = new WinBrush(control.SelectedTextColor.ToWindowsColor());
        handler.PlatformView.Resources["ButtonItemForegroundSelected"] = brush;
        handler.PlatformView.Resources["ButtonItemForegroundSelectedPointerOver"] = brush;
        handler.PlatformView.Resources["ButtonItemForegroundSelectedPressed"] = brush;
    }

    static void MapTextColor(SegmentedControlHandler handler, SegmentedControl control)
    {
        var brush = new WinBrush(control.TextColor.ToWindowsColor());
        handler.PlatformView.Resources["ButtonItemForeground"] = brush;
        handler.PlatformView.Resources["ButtonItemForegroundPointerOver"] = brush;
    }

    static void MapDisabledBackgroundColor(SegmentedControlHandler handler, SegmentedControl control)
    {
        handler.PlatformView.Resources["ButtonItemBackgroundDisabled"] = new WinBrush(control.DisabledBackgroundColor.ToWindowsColor());
    }

    static void MapDisabledTextColor(SegmentedControlHandler handler, SegmentedControl control)
    {
        handler.PlatformView.Resources["ButtonItemForegroundDisabled"] = new WinBrush(control.DisabledTextColor.ToWindowsColor());
    }

    static void MapDisabledTintColor(SegmentedControlHandler handler, SegmentedControl control)
    {
        // DisabledTintColor affects disabled selected background
        handler.PlatformView.Resources["ButtonItemBackgroundDisabled"] = new WinBrush(control.DisabledTintColor.ToWindowsColor());
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
