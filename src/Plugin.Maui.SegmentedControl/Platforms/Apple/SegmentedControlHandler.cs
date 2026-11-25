#if IOS || MACCATALYST
using Foundation;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using Plugin.Maui.SegmentedControl.Control;
using UIKit;

namespace Plugin.Maui.SegmentedControl.Handlers;

/// <summary>
/// Shared iOS / Mac Catalyst handler backed by a native <see cref="UISegmentedControl"/>.
/// Tap and toggle behaviour matches the Android and Windows handlers:
/// <list type="bullet">
/// <item><see cref="SegmentedControl.SegmentTapped"/> is raised for every tap on an enabled segment,
/// including a tap on the segment that is already selected.</item>
/// <item><see cref="GroupToggleBehavior.Radio"/> keeps the tapped segment selected and updates
/// <see cref="SegmentedControl.SelectedSegment"/>; <see cref="GroupToggleBehavior.None"/> keeps nothing selected
/// (the native control runs in momentary mode) and leaves <see cref="SegmentedControl.SelectedSegment"/> alone.</item>
/// <item><see cref="SegmentedControl.ValueChanged"/> is raised only in Radio mode, and only for an index that
/// points at a segment (never for -1).</item>
/// </list>
/// </summary>
public partial class SegmentedControlHandler : ViewHandler<SegmentedControl, UISegmentedControl>
{
    // UISegmentedControlNoSegment: no segment selected.
    const int NoSegment = -1;

    public static IPropertyMapper<SegmentedControl, SegmentedControlHandler> Mapper = new PropertyMapper<SegmentedControl, SegmentedControlHandler>(ViewMapper)
    {
        [nameof(SegmentedControl.IsEnabled)] = MapIsEnabled,
        [nameof(SegmentedControl.SelectedSegment)] = MapSelectedSegment,
        [nameof(SegmentedControl.GroupToggleBehavior)] = MapGroupToggleBehavior,
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
        var segmentControl = new TapAwareSegmentedControl();
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
        var font = UIFont.SystemFontOfSize((nfloat)VirtualView.FontSize);
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

        // Apply padding via content position adjustment
        var padding = VirtualView.Padding;
        var horizontalOffset = (nfloat)(padding.Left - padding.Right);
        var verticalOffset = (nfloat)(padding.Top - padding.Bottom);
        segmentControl.SetContentPositionAdjustment(
            new UIOffset(horizontalOffset / 2, verticalOffset / 2),
            UISegmentedControlSegment.Any,
            UIBarMetrics.Default);

        ApplySelection(segmentControl, VirtualView);
        return segmentControl;
    }

    protected override void ConnectHandler(UISegmentedControl platformView)
    {
        base.ConnectHandler(platformView);

        platformView.ValueChanged += PlatformView_ValueChanged;
        if (platformView is TapAwareSegmentedControl tapAware)
        {
            tapAware.SelectedSegmentTapped += PlatformView_SelectedSegmentTapped;
        }
    }

    protected override void DisconnectHandler(UISegmentedControl platformView)
    {
        platformView.ValueChanged -= PlatformView_ValueChanged;
        if (platformView is TapAwareSegmentedControl tapAware)
        {
            tapAware.SelectedSegmentTapped -= PlatformView_SelectedSegmentTapped;
        }

        base.DisconnectHandler(platformView);
    }

    // UIKit raises ValueChanged when a tap selects a different segment (Radio), and on every tap in momentary mode (None).
    void PlatformView_ValueChanged(object sender, EventArgs e)
    {
        var index = (int)PlatformView.SelectedSegment;
        if (index < 0)
        {
            return;
        }

        // Same order as Android and Windows: selection (ValueChanged) first, then SegmentTapped.
        if (VirtualView.GroupToggleBehavior == GroupToggleBehavior.Radio)
        {
            VirtualView.SelectedSegment = index;
        }

        VirtualView.SendSegmentTapped(index);
    }

    // UIKit raises nothing when the already-selected segment is tapped again; the native subclass reports it.
    void PlatformView_SelectedSegmentTapped(object sender, int index) => VirtualView.SendSegmentTapped(index);

    // Radio: the native control keeps SelectedSegment selected. None: momentary, nothing stays selected.
    static void ApplySelection(UISegmentedControl platformView, SegmentedControl control)
    {
        var isRadio = control.GroupToggleBehavior == GroupToggleBehavior.Radio;
        platformView.Momentary = !isRadio;

        var selected = control.SelectedSegment;
        platformView.SelectedSegment = isRadio && selected >= 0 && selected < platformView.NumberOfSegments
            ? selected
            : NoSegment;
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

        ApplySelection(segmentControl, control);
    }

    static void MapTintColor(SegmentedControlHandler handler, SegmentedControl control)
    {
        handler.PlatformView.SelectedSegmentTintColor = control.IsEnabled
            ? control.TintColor.ToPlatform()
            : control.DisabledTintColor.ToPlatform();
    }

    static void MapSelectedSegment(SegmentedControlHandler handler, SegmentedControl control)
    {
        // Like Android and Windows: selection only exists in Radio mode.
        if (control.GroupToggleBehavior != GroupToggleBehavior.Radio)
        {
            return;
        }

        ApplySelection(handler.PlatformView, control);

        // Only report values that point at a segment (not -1, not out of range).
        if (control.SelectedSegment >= 0 && control.SelectedSegment < handler.PlatformView.NumberOfSegments)
        {
            control.SendValueChanged();
        }
    }

    // Switching to None clears the native selection; switching back to Radio restores SelectedSegment.
    static void MapGroupToggleBehavior(SegmentedControlHandler handler, SegmentedControl control)
        => ApplySelection(handler.PlatformView, control);

    static void MapIsEnabled(SegmentedControlHandler handler, SegmentedControl control)
    {
        handler.PlatformView.Enabled = control.IsEnabled;
        handler.PlatformView.TintColor = control.IsEnabled
            ? control.TintColor.ToPlatform()
            : control.DisabledTintColor.ToPlatform();
    }

    static void MapSelectedTextColor(SegmentedControlHandler handler, SegmentedControl control)
    {
        var font = UIFont.SystemFontOfSize((nfloat)control.FontSize);
        handler.PlatformView.SetTitleTextAttributes(new UIStringAttributes()
        {
            ForegroundColor = control.SelectedTextColor.ToPlatform(),
            Font = font
        }, UIControlState.Selected);
    }

    static void MapTextColor(SegmentedControlHandler handler, SegmentedControl control)
    {
        var font = UIFont.SystemFontOfSize((nfloat)control.FontSize);
        handler.PlatformView.SetTitleTextAttributes(new UIStringAttributes()
        {
            ForegroundColor = control.TextColor.ToPlatform(),
            Font = font
        }, UIControlState.Normal);
    }

    static void MapFontSize(SegmentedControlHandler handler, SegmentedControl control)
    {
        var font = UIFont.SystemFontOfSize((nfloat)control.FontSize);
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
        var horizontalOffset = (nfloat)(padding.Left - padding.Right);
        var verticalOffset = (nfloat)(padding.Top - padding.Bottom);
        handler.PlatformView.SetContentPositionAdjustment(
            new UIOffset(horizontalOffset / 2, verticalOffset / 2),
            UISegmentedControlSegment.Any,
            UIBarMetrics.Default);
    }

    /// <summary>
    /// <see cref="UISegmentedControl"/> raises ValueChanged only when the selection changes, so a second tap on the
    /// selected segment is silent. This subclass reports that tap through <see cref="SelectedSegmentTapped"/>.
    /// </summary>
    sealed class TapAwareSegmentedControl : UISegmentedControl
    {
        nint selectedAtTouchBegin = NoSegment;

        public event EventHandler<int> SelectedSegmentTapped;

        public override void TouchesBegan(NSSet touches, UIEvent evt)
        {
            selectedAtTouchBegin = SelectedSegment;
            base.TouchesBegan(touches, evt);
        }

        public override void TouchesEnded(NSSet touches, UIEvent evt)
        {
            base.TouchesEnded(touches, evt);

            // Momentary (None) taps and selection changes already arrive through ValueChanged.
            if (Momentary || selectedAtTouchBegin < 0 || SelectedSegment != selectedAtTouchBegin)
            {
                return;
            }

            if (touches.AnyObject is not UITouch touch)
            {
                return;
            }

            var location = touch.LocationInView(this);
            if (Bounds.Contains(location) && SegmentIndexAt(location.X) == selectedAtTouchBegin)
            {
                SelectedSegmentTapped?.Invoke(this, (int)selectedAtTouchBegin);
            }
        }

        // The handler never sets custom segment widths, so segments share the width equally.
        nint SegmentIndexAt(nfloat x)
        {
            var count = NumberOfSegments;
            if (count <= 0 || Bounds.Width <= 0)
            {
                return NoSegment;
            }

            var index = (nint)Math.Floor((double)x / ((double)Bounds.Width / count));
            index = Math.Clamp(index, 0, count - 1);

            return EffectiveUserInterfaceLayoutDirection == UIUserInterfaceLayoutDirection.RightToLeft
                ? count - 1 - index
                : index;
        }
    }
}
#endif
