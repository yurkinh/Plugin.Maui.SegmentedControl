#if MACCATALYST
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using UIKit;

namespace Plugin.Maui.SegmentedControl.Handlers;

public partial class SegmentedControlHandler : ViewHandler<SegmentedControl, UISegmentedControl>
{
    public static IPropertyMapper<SegmentedControl, SegmentedControlHandler> Mapper = new PropertyMapper<SegmentedControl, SegmentedControlHandler>(ViewMapper)
    {
        [nameof(SegmentedControl.IsEnabled)] = MapIsEnabled,
        [nameof(SegmentedControl.SelectedSegment)] = MapSelectedSegment,
        [nameof(SegmentedControl.TintColor)] = MapTintColor,
        [nameof(SegmentedControl.SelectedTextColor)] = MapSelectedTextColor,
    };

    public SegmentedControlHandler() : base(Mapper)
    {
    }

    public SegmentedControlHandler(IPropertyMapper mapper) : base(mapper ?? Mapper)
    {
    }

    protected override UISegmentedControl CreatePlatformView()
    {
        var segmentControl = new UISegmentedControl();
        for (var i = 0; i < VirtualView.Children.Count; i++)
        {
            segmentControl.InsertSegment(VirtualView.Children[i].Text, i, false);
        }

        segmentControl.Enabled = VirtualView.IsEnabled;
        segmentControl.TintColor = VirtualView.IsEnabled ? VirtualView.TintColor.ToPlatform() : VirtualView.DisabledColor.ToPlatform();
        segmentControl.SetTitleTextAttributes(new UIStringAttributes() { ForegroundColor = VirtualView.SelectedTextColor.ToPlatform() }, UIControlState.Selected);
        segmentControl.SelectedSegment = VirtualView.SelectedSegment;
        return segmentControl;
    }

    protected override void ConnectHandler(UISegmentedControl platformView)
    {
        base.ConnectHandler(platformView);

        platformView.ValueChanged += PlatformView_ValueChanged;
    }

    protected override void DisconnectHandler(UISegmentedControl platformView)
    {
        base.DisconnectHandler(platformView);
        platformView.ValueChanged -= PlatformView_ValueChanged;
    }

    void PlatformView_ValueChanged(object sender, EventArgs e)
    {
        VirtualView.SelectedSegment = (int)PlatformView.SelectedSegment;
    }

    static void MapTintColor(SegmentedControlHandler handler, SegmentedControl control)
    {
        handler.PlatformView.TintColor = control.IsEnabled ? control.TintColor.ToPlatform() : control.DisabledColor.ToPlatform();
    }

    static void MapSelectedSegment(SegmentedControlHandler handler, SegmentedControl control)
    {
        handler.PlatformView.SelectedSegment = control.SelectedSegment;
        control.SendValueChanged();
    }

    static void MapIsEnabled(SegmentedControlHandler handler, SegmentedControl control)
    {
        handler.PlatformView.Enabled = control.IsEnabled;
        handler.PlatformView.TintColor = control.IsEnabled ? control.TintColor.ToPlatform() : control.DisabledColor.ToPlatform();
    }

    static void MapSelectedTextColor(SegmentedControlHandler handler, SegmentedControl control)
    {
        handler.PlatformView.SetTitleTextAttributes(new UIStringAttributes() { ForegroundColor = control.SelectedTextColor.ToPlatform() }, UIControlState.Selected);
    }

}

#endif