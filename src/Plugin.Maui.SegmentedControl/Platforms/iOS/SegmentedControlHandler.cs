#if IOS
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using UIKit;

namespace Plugin.Maui.SegmentedControl.Handlers;

public class SegmentedControlHandler : ViewHandler<SegmentedControl, UISegmentedControl>
{
    public static IPropertyMapper<SegmentedControl, SegmentedControlHandler> Mapper = new PropertyMapper<SegmentedControl, SegmentedControlHandler>(ViewMapper)
    {
        [nameof(SegmentedControl.IsEnabled)] = MapIsEnabled,
        [nameof(SegmentedControl.SelectedSegment)] = MapSelectedSegment,
        [nameof(SegmentedControl.TintColor)] = MapTintColor,
        [nameof(SegmentedControl.SelectedTextColor)] = MapSelectedTextColor,
        [nameof(SegmentedControl.TextColor)] = MapTextColor,
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

    protected override UISegmentedControl CreatePlatformView()
    {
        var segmentControl = new UISegmentedControl();
        for (var i = 0; i < VirtualView.Children.Count; i++)
        {
            segmentControl.InsertSegment(VirtualView.Children[i].Text, i, false);
        }

        for (var i = 0; i < VirtualView.Children.Count; i++)
        {
            var child = VirtualView.Children[i];
            segmentControl.SetEnabled(child.IsEnabled && VirtualView.IsEnabled, i);
        }


        segmentControl.Enabled = VirtualView.IsEnabled;
        segmentControl.TintColor = VirtualView.IsEnabled ? VirtualView.TintColor.ToPlatform() : VirtualView.DisabledTintColor.ToPlatform();
        
        // Apply text attributes with font size
        var font = UIKit.UIFont.SystemFontOfSize((nfloat)VirtualView.FontSize);
        segmentControl.SetTitleTextAttributes(new UIStringAttributes() 
        { 
            ForegroundColor = VirtualView.SelectedTextColor.ToPlatform(),
            Font = font
        }, UIControlState.Selected);
        segmentControl.SetTitleTextAttributes(new UIStringAttributes() 
        { 
            ForegroundColor = VirtualView.TextColor.ToPlatform(),
            Font = font
        }, UIControlState.Normal);
        
        // Apply padding (content insets)
        var padding = VirtualView.Padding;
        if (OperatingSystem.IsIOSVersionAtLeast(14, 0) || OperatingSystem.IsMacCatalystVersionAtLeast(14, 0))
        {
            segmentControl.Layer.MasksToBounds = false;
            segmentControl.ContentOffset = new CoreGraphics.CGSize(padding.Left - padding.Right, padding.Top - padding.Bottom);
        }
        
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

    static void MapChildren(SegmentedControlHandler handler, SegmentedControl control)
    {
        UISegmentedControl segmentControl = handler.PlatformView;
        segmentControl.RemoveAllSegments();
        for (var i = 0; i < handler.VirtualView.Children.Count; i++)
        {
            segmentControl.InsertSegment(handler.VirtualView.Children[i].Text, i, false);
        }

        for (var i = 0; i < handler.VirtualView.Children.Count; i++)
        {
            var child = handler.VirtualView.Children[i];
            segmentControl.SetEnabled(child.IsEnabled, i);
        }
        segmentControl.SelectedSegment = handler.VirtualView.SelectedSegment;
    }

    static void MapTintColor(SegmentedControlHandler handler, SegmentedControl control)
    {
        handler.PlatformView.SelectedSegmentTintColor = control.IsEnabled 
            ? control.TintColor.ToPlatform() 
            : control.DisabledTintColor.ToPlatform();
    }

    static void MapSelectedSegment(SegmentedControlHandler handler, SegmentedControl control)
    {
        handler.PlatformView.SelectedSegment = control.SelectedSegment;
        control.SendValueChanged();
    }

    static void MapIsEnabled(SegmentedControlHandler handler, SegmentedControl control)
    {
        handler.PlatformView.Enabled = control.IsEnabled;
        handler.PlatformView.TintColor = control.IsEnabled 
            ? control.TintColor.ToPlatform() 
            : control.DisabledTintColor.ToPlatform();
    }

    static void MapSelectedTextColor(SegmentedControlHandler handler, SegmentedControl control)
    {
        var font = UIKit.UIFont.SystemFontOfSize((nfloat)control.FontSize);
        handler.PlatformView.SetTitleTextAttributes(new UIStringAttributes() 
        { 
            ForegroundColor = control.SelectedTextColor.ToPlatform(),
            Font = font
        }, UIControlState.Selected);        
    }

    static void MapTextColor(SegmentedControlHandler handler, SegmentedControl control)
    {
        var font = UIKit.UIFont.SystemFontOfSize((nfloat)control.FontSize);
        handler.PlatformView.SetTitleTextAttributes(new UIStringAttributes() 
        { 
            ForegroundColor = control.TextColor.ToPlatform(),
            Font = font
        }, UIControlState.Normal);
    }

    static void MapFontSize(SegmentedControlHandler handler, SegmentedControl control)
    {
        var font = UIKit.UIFont.SystemFontOfSize((nfloat)control.FontSize);
        handler.PlatformView.SetTitleTextAttributes(new UIStringAttributes() 
        { 
            ForegroundColor = control.SelectedTextColor.ToPlatform(),
            Font = font
        }, UIControlState.Selected);
        handler.PlatformView.SetTitleTextAttributes(new UIStringAttributes() 
        { 
            ForegroundColor = control.TextColor.ToPlatform(),
            Font = font
        }, UIControlState.Normal);
    }

    static void MapPadding(SegmentedControlHandler handler, SegmentedControl control)
    {
        var padding = control.Padding;
        if (OperatingSystem.IsIOSVersionAtLeast(14, 0) || OperatingSystem.IsMacCatalystVersionAtLeast(14, 0))
        {
            handler.PlatformView.Layer.MasksToBounds = false;
            handler.PlatformView.ContentOffset = new CoreGraphics.CGSize(padding.Left - padding.Right, padding.Top - padding.Bottom);
        }
    }

}
#endif
