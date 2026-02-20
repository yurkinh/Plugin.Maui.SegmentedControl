#if WINDOWS
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Plugin.Maui.SegmentedControl.Control;

namespace Plugin.Maui.SegmentedControl.Handlers;

public class SegmentedControlHandler : ViewHandler<SegmentedControl, Grid>
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

    protected override Grid CreatePlatformView()
    {
        var grid = new Grid
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        BuildSegments(grid);

        return grid;
    }

    void BuildSegments(Grid grid)
    {
        foreach (UIElement child in grid.Children)
        {
            if (child is Button btn)
            {
                btn.Click -= Button_Click;
            }
        }

        grid.Children.Clear();
        grid.ColumnDefinitions.Clear();

        var children = VirtualView.Children;
        if (children is null || children.Count == 0)
        {
            return;
        }

        for (int i = 0; i < children.Count; i++)
        {
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        }

        for (int i = 0; i < children.Count; i++)
        {
            var btn = CreateSegmentButton(i, children[i], children.Count);
            Grid.SetColumn(btn, i);
            grid.Children.Add(btn);
        }
    }

    Button CreateSegmentButton(int index, SegmentedControlOption segment, int totalCount)
    {
        bool isEnabled = VirtualView.IsEnabled && segment.IsEnabled;

        var btn = new Button
        {
            Content = segment.Text,
            Tag = index,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            HorizontalContentAlignment = HorizontalAlignment.Center,
            CornerRadius = GetCornerRadius(index, totalCount),
            BorderThickness = index == 0
                ? new Thickness(1)
                : new Thickness(0, 1, 1, 1),
        };

        ConfigureButton(btn, index, isEnabled);
        btn.Click += Button_Click;

        return btn;
    }

    static CornerRadius GetCornerRadius(int index, int totalCount)
    {
        const double radius = 4.0;
        if (totalCount == 1)
        {
            return new CornerRadius(radius);
        }

        if (index == 0)
        {
            return new CornerRadius(radius, 0, 0, radius);
        }

        if (index == totalCount - 1)
        {
            return new CornerRadius(0, radius, radius, 0);
        }

        return new CornerRadius(0);
    }

    void ConfigureButton(Button btn, int index, bool isEnabled)
    {
        bool isSelected = VirtualView.GroupToggleBehavior == GroupToggleBehavior.Radio
            && index == VirtualView.SelectedSegment;

        btn.IsEnabled = isEnabled;

        var tintColor = (isEnabled ? VirtualView.TintColor : VirtualView.DisabledTintColor).ToPlatform();
        var textColor = (!isEnabled
            ? VirtualView.DisabledTextColor
            : isSelected ? VirtualView.SelectedTextColor
            : VirtualView.TextColor).ToPlatform();

        btn.BorderBrush = new SolidColorBrush(tintColor);
        btn.Background = isSelected
            ? new SolidColorBrush(tintColor)
            : new SolidColorBrush(new Windows.UI.Color { A = 0, R = 0, G = 0, B = 0 });
        btn.Foreground = new SolidColorBrush(textColor);
        btn.FontSize = VirtualView.FontSize;

        var padding = VirtualView.Padding;
        btn.Padding = new Thickness(padding.Left, padding.Top, padding.Right, padding.Bottom);
    }

    protected override void ConnectHandler(Grid platformView)
    {
        base.ConnectHandler(platformView);
    }

    protected override void DisconnectHandler(Grid platformView)
    {
        foreach (UIElement child in platformView.Children)
        {
            if (child is Button btn)
            {
                btn.Click -= Button_Click;
            }
        }

        base.DisconnectHandler(platformView);
    }

    void Button_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button clickedBtn || clickedBtn.Tag is not int index)
        {
            return;
        }

        VirtualView.SendSegmentTapped(index);

        if (VirtualView.GroupToggleBehavior == GroupToggleBehavior.None)
        {
            return;
        }

        VirtualView.SelectedSegment = index;
    }

    static void MapIsEnabled(SegmentedControlHandler handler, SegmentedControl control)
    {
        foreach (UIElement child in handler.PlatformView.Children)
        {
            if (child is Button btn && btn.Tag is int index && index < control.Children.Count)
            {
                bool isEnabled = control.IsEnabled && control.Children[index].IsEnabled;
                handler.ConfigureButton(btn, index, isEnabled);
            }
        }
    }

    static void MapSelectedSegment(SegmentedControlHandler handler, SegmentedControl control)
    {
        if (control.GroupToggleBehavior == GroupToggleBehavior.None)
        {
            return;
        }

        foreach (UIElement child in handler.PlatformView.Children)
        {
            if (child is Button btn && btn.Tag is int index)
            {
                handler.ConfigureButton(btn, index, btn.IsEnabled);
            }
        }

        control.SendValueChanged();
    }

    static void MapTintColor(SegmentedControlHandler handler, SegmentedControl control)
        => OnPropertyChanged(handler, control);

    static void MapSelectedTextColor(SegmentedControlHandler handler, SegmentedControl control)
        => OnPropertyChanged(handler, control);

    static void MapTextColor(SegmentedControlHandler handler, SegmentedControl control)
        => OnPropertyChanged(handler, control);

    static void MapDisabledBackgroundColor(SegmentedControlHandler handler, SegmentedControl control)
        => OnPropertyChanged(handler, control);

    static void MapDisabledTextColor(SegmentedControlHandler handler, SegmentedControl control)
        => OnPropertyChanged(handler, control);

    static void MapDisabledTintColor(SegmentedControlHandler handler, SegmentedControl control)
        => OnPropertyChanged(handler, control);

    static void MapFontSize(SegmentedControlHandler handler, SegmentedControl control)
    {
        foreach (UIElement child in handler.PlatformView.Children)
        {
            if (child is Button btn)
            {
                btn.FontSize = control.FontSize;
            }
        }
    }

    static void MapPadding(SegmentedControlHandler handler, SegmentedControl control)
    {
        var padding = control.Padding;
        foreach (UIElement child in handler.PlatformView.Children)
        {
            if (child is Button btn)
            {
                btn.Padding = new Thickness(padding.Left, padding.Top, padding.Right, padding.Bottom);
            }
        }
    }

    static void MapChildren(SegmentedControlHandler handler, SegmentedControl control)
        => handler.BuildSegments(handler.PlatformView);

    static void OnPropertyChanged(SegmentedControlHandler handler, SegmentedControl control)
    {
        foreach (UIElement child in handler.PlatformView.Children)
        {
            if (child is Button btn && btn.Tag is int index)
            {
                handler.ConfigureButton(btn, index, btn.IsEnabled);
            }
        }
    }
}

#endif
