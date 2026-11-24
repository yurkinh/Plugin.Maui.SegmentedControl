﻿#if WINDOWS
#nullable enable
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using Plugin.Maui.SegmentedControl.Control;
using Plugin.Maui.SegmentedControl.Windows;
using WinBrush = Microsoft.UI.Xaml.Media.SolidColorBrush;
using WinListViewSelectionMode = Microsoft.UI.Xaml.Controls.ListViewSelectionMode;

namespace Plugin.Maui.SegmentedControl.Handlers;

public class SegmentedControlHandler : ViewHandler<SegmentedControl, Segmented>
{
    // Theme resource keys used by SegmentedStyles.xaml, grouped by the SegmentedControl color that drives them.
    static readonly string[] TintKeys = ["ButtonItemBackgroundSelected", "ButtonItemBackgroundSelectedPointerOver", "ButtonItemBackgroundSelectedPressed"];
    static readonly string[] SelectedTextKeys = ["ButtonItemForegroundSelected", "ButtonItemForegroundSelectedPointerOver", "ButtonItemForegroundSelectedPressed"];
    static readonly string[] TextKeys = ["ButtonItemForeground", "ButtonItemForegroundPointerOver", "ButtonItemForegroundPressed"];
    const string DisabledTextKey = "ButtonItemForegroundDisabled";
    const string DisabledBackgroundKey = "ButtonItemBackgroundDisabled";
    const string DisabledSelectedBackgroundKey = "ButtonItemBackgroundDisabledSelected";
    const string BorderKey = "SegmentedBorderBrush";

    Microsoft.UI.Xaml.Input.TappedEventHandler? tappedHandler;

    // One brush per resource key. Each brush is inserted into PlatformView.Resources once and afterwards only its
    // Color changes: WinUI throws (0x800F0902) when an existing entry is replaced, and bound brushes update live.
    readonly Dictionary<string, WinBrush> brushes = [];

    public static IPropertyMapper<SegmentedControl, SegmentedControlHandler> Mapper =
        new PropertyMapper<SegmentedControl, SegmentedControlHandler>(ViewMapper)
        {
            [nameof(SegmentedControl.IsEnabled)] = MapIsEnabled,
            [nameof(SegmentedControl.SelectedSegment)] = MapSelectedSegment,
            [nameof(SegmentedControl.GroupToggleBehavior)] = MapGroupToggleBehavior,
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
            SelectionMode = ToSelectionMode(VirtualView.GroupToggleBehavior),
            HorizontalAlignment = Microsoft.UI.Xaml.HorizontalAlignment.Stretch,
        };

        BuildItems(segmented);
        ApplySelectedSegment(segmented, VirtualView);

        return segmented;
    }

    protected override void ConnectHandler(Segmented platformView)
    {
        base.ConnectHandler(platformView);
        platformView.SelectionChanged += OnSelectionChanged;

        // ListViewItem marks Tapped as handled, so a plain "Tapped +=" subscription is never called.
        tappedHandler = OnTapped;
        platformView.AddHandler(Microsoft.UI.Xaml.UIElement.TappedEvent, tappedHandler, handledEventsToo: true);
    }

    protected override void DisconnectHandler(Segmented platformView)
    {
        platformView.SelectionChanged -= OnSelectionChanged;
        if (tappedHandler is not null)
        {
            platformView.RemoveHandler(Microsoft.UI.Xaml.UIElement.TappedEvent, tappedHandler);
            tappedHandler = null;
        }

        base.DisconnectHandler(platformView);
    }

    static WinListViewSelectionMode ToSelectionMode(GroupToggleBehavior behavior)
        => behavior == GroupToggleBehavior.Radio ? WinListViewSelectionMode.Single : WinListViewSelectionMode.None;

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

    static T? FindAncestorOrSelf<T>(Microsoft.UI.Xaml.DependencyObject? obj) where T : Microsoft.UI.Xaml.DependencyObject
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
            var item = new SegmentedItem
            {
                FontSize = VirtualView.FontSize,
                Padding = ToItemPadding(VirtualView.Padding),
            };
            UpdateItem(item, child);

            // The style's Foreground setter resolves ButtonItemForeground where the style is defined, so it never
            // sees this control's override. Set it locally; the visual states still animate the content presenter.
            if (brushes.TryGetValue(TextKeys[0], out var textBrush))
            {
                item.Foreground = textBrush;
            }

            segmented.Items.Add(item);
        }
    }

    void UpdateItem(SegmentedItem item, SegmentedControlOption child)
    {
        var text = child.Text ?? string.Empty;
        if (!Equals(item.Content, text))
        {
            item.Content = text;
        }

        // Narrator reads the segment's text; selection state and "n of m" come from the ListViewItem peer.
        Microsoft.UI.Xaml.Automation.AutomationProperties.SetName(item, text);
        item.IsEnabled = VirtualView.IsEnabled && child.IsEnabled;
    }

    static Microsoft.UI.Xaml.Thickness ToItemPadding(Thickness padding)
        => padding == new Thickness(0)
            ? new Microsoft.UI.Xaml.Thickness(11)
            : new Microsoft.UI.Xaml.Thickness(padding.Left, padding.Top, padding.Right, padding.Bottom);

    IEnumerable<SegmentedItem> Containers()
    {
        for (int i = 0; i < PlatformView.Items.Count; i++)
        {
            if (PlatformView.Items[i] is SegmentedItem item)
            {
                yield return item;
            }
        }
    }

    WinBrush? SetBrush(string key, Color? color)
    {
        if (color is null)
        {
            return brushes.GetValueOrDefault(key);
        }

        var winColor = color.ToWindowsColor();
        if (brushes.TryGetValue(key, out var brush))
        {
            brush.Color = winColor;
            return brush;
        }

        brush = new WinBrush(winColor);
        brushes[key] = brush;
        PlatformView.Resources[key] = brush;
        return brush;
    }

    void SetBrushes(string[] keys, Color? color)
    {
        foreach (var key in keys)
        {
            SetBrush(key, color);
        }
    }

    void UpdateBorder()
    {
        // Border reflects TintColor when enabled, DisabledTintColor when disabled.
        // Set BorderBrush directly too: like Foreground, it comes from a style setter that ignores the override.
        var border = SetBrush(BorderKey, VirtualView.IsEnabled ? VirtualView.TintColor : VirtualView.DisabledTintColor);
        if (border is not null)
        {
            PlatformView.BorderBrush = border;
        }
    }

    static void ApplySelectedSegment(Segmented segmented, SegmentedControl control)
    {
        if (control.GroupToggleBehavior != GroupToggleBehavior.Radio)
        {
            return;
        }

        var index = control.SelectedSegment;
        if (index < -1 || index >= segmented.Items.Count || segmented.SelectedIndex == index)
        {
            return;
        }

        segmented.SelectedIndex = index;
    }

    static void MapIsEnabled(SegmentedControlHandler handler, SegmentedControl control)
    {
        var children = control.Children;
        var i = 0;
        foreach (var item in handler.Containers())
        {
            item.IsEnabled = control.IsEnabled && (children is null || i >= children.Count || children[i].IsEnabled);
            i++;
        }

        handler.UpdateBorder();
    }

    static void MapSelectedSegment(SegmentedControlHandler handler, SegmentedControl control)
    {
        if (control.GroupToggleBehavior != GroupToggleBehavior.Radio)
        {
            return;
        }

        ApplySelectedSegment(handler.PlatformView, control);

        // Like Android: only report values that point at a segment (not -1, not out of range).
        if (control.SelectedSegment >= 0 && control.SelectedSegment < handler.PlatformView.Items.Count)
        {
            control.SendValueChanged();
        }
    }

    static void MapGroupToggleBehavior(SegmentedControlHandler handler, SegmentedControl control)
    {
        var mode = ToSelectionMode(control.GroupToggleBehavior);
        if (handler.PlatformView.SelectionMode == mode)
        {
            return;
        }

        // Switching to None clears the native selection; switching back restores SelectedSegment.
        handler.PlatformView.SelectionMode = mode;
        ApplySelectedSegment(handler.PlatformView, control);
    }

    static void MapTintColor(SegmentedControlHandler handler, SegmentedControl control)
    {
        handler.SetBrushes(TintKeys, control.TintColor);
        handler.UpdateBorder();
    }

    static void MapSelectedTextColor(SegmentedControlHandler handler, SegmentedControl control)
        => handler.SetBrushes(SelectedTextKeys, control.SelectedTextColor);

    static void MapTextColor(SegmentedControlHandler handler, SegmentedControl control)
    {
        var hadBrush = handler.brushes.ContainsKey(TextKeys[0]);
        handler.SetBrushes(TextKeys, control.TextColor);

        // First time only: the items were built before the brush existed. Later changes update the shared brush.
        if (!hadBrush && handler.brushes.TryGetValue(TextKeys[0], out var textBrush))
        {
            foreach (var item in handler.Containers())
            {
                item.Foreground = textBrush;
            }
        }
    }

    static void MapDisabledBackgroundColor(SegmentedControlHandler handler, SegmentedControl control)
        => handler.SetBrush(DisabledBackgroundKey, control.DisabledBackgroundColor);

    static void MapDisabledTextColor(SegmentedControlHandler handler, SegmentedControl control)
        => handler.SetBrush(DisabledTextKey, control.DisabledTextColor);

    static void MapDisabledTintColor(SegmentedControlHandler handler, SegmentedControl control)
    {
        // Like iOS/Android: DisabledTintColor is the selected segment's color (and the border) while disabled.
        handler.SetBrush(DisabledSelectedBackgroundKey, control.DisabledTintColor);
        handler.UpdateBorder();
    }

    static void MapFontSize(SegmentedControlHandler handler, SegmentedControl control)
    {
        foreach (var item in handler.Containers())
        {
            item.FontSize = control.FontSize;
        }
    }

    static void MapPadding(SegmentedControlHandler handler, SegmentedControl control)
    {
        var thickness = ToItemPadding(control.Padding);
        foreach (var item in handler.Containers())
        {
            item.Padding = thickness;
        }
    }

    static void MapChildren(SegmentedControlHandler handler, SegmentedControl control)
    {
        var children = control.Children;
        var items = handler.PlatformView.Items;

        // An option's Text/IsEnabled change re-maps Children. Update the existing items in place when the count
        // is unchanged, so focus and selection survive; rebuild only when segments were added or removed.
        if (children is not null && children.Count == items.Count && children.Count > 0)
        {
            for (int i = 0; i < children.Count; i++)
            {
                if (items[i] is SegmentedItem item)
                {
                    handler.UpdateItem(item, children[i]);
                }
            }
        }
        else
        {
            handler.BuildItems(handler.PlatformView);
        }

        ApplySelectedSegment(handler.PlatformView, control);
    }
}
#endif
