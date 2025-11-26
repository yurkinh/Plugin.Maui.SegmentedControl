#if ANDROID
#nullable enable

using Android.Graphics.Drawables;
using Android.Views;
using Android.Widget;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using Plugin.Maui.SegmentedControl.Control;
using static Android.Views.ViewGroup;
using RadioButton = Android.Widget.RadioButton;

namespace Plugin.Maui.SegmentedControl.Handlers;

public class SegmentedControlHandler : ViewHandler<SegmentedControl, RadioGroup>
{
    RadioButton? _selectedRadioButton;

    public static IPropertyMapper<SegmentedControl, SegmentedControlHandler> Mapper = new PropertyMapper<SegmentedControl, SegmentedControlHandler>(ViewMapper)
    {
        [nameof(SegmentedControl.IsEnabled)] = MapIsEnabled,
        [nameof(SegmentedControl.SelectedSegment)] = MapSelectedSegment,
        [nameof(SegmentedControl.TintColor)] = MapTintColor,
        [nameof(SegmentedControl.TextColor)] = MapTextColor,
        [nameof(SegmentedControl.SelectedTextColor)] = MapSelectedTextColor,

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

    /// <summary>
    /// Helper method to create and configure a RadioButton for a segment.
    /// Extracted to reduce duplication between CreatePlatformView and MapChildren.
    /// </summary>
    private RadioButton? CreateRadioButton(LayoutInflater layoutInflater, int index, SegmentedControlOption segment, int totalCount)
    {
        if (layoutInflater.Inflate(Resource.Layout.RadioButton, null) is not RadioButton rb)
            return null;

        rb.LayoutParameters = new RadioGroup.LayoutParams(LayoutParams.MatchParent, LayoutParams.WrapContent, 1f);
        rb.Text = segment.Text;

        // Apply first/last/middle background styles
        if (index == 0)
        {
            rb.SetBackgroundResource(Resource.Drawable.segmented_control_first_background);
        }
        else if (index == totalCount - 1)
        {
            rb.SetBackgroundResource(Resource.Drawable.segmented_control_last_background);
        }
        else
        {
            // Middle segments need the default background
            rb.SetBackgroundResource(Resource.Drawable.segmented_control_background);
        }

        var isButtonEnabled = VirtualView.IsEnabled && segment.IsEnabled;
        ConfigureRadioButton(index, isButtonEnabled, rb);

        return rb;
    }

    protected override RadioGroup CreatePlatformView()
    {
        var layoutInflater = LayoutInflater.From(Context);
        if (layoutInflater == null)
            throw new InvalidOperationException("Failed to get LayoutInflater from Context");

        if (layoutInflater.Inflate(Resource.Layout.RadioGroup, null) is not RadioGroup nativeControl)
            throw new InvalidOperationException("Failed to inflate RadioGroup");

        nativeControl.LayoutParameters = new LayoutParams(LayoutParams.MatchParent, LayoutParams.WrapContent);

        var children = VirtualView.Children;
        if (children == null || children.Count == 0)
            return nativeControl;

        for (var i = 0; i < children.Count; i++)
        {
            var segment = children[i];
            if (segment == null)
                continue;

            var rb = CreateRadioButton(layoutInflater, i, segment, children.Count);
            if (rb != null)
            {
                nativeControl.AddView(rb);
            }
        }

        // Set initial selected segment
        if (VirtualView.GroupToggleBehavior == GroupToggleBehavior.Radio
            && VirtualView.SelectedSegment >= 0
            && VirtualView.SelectedSegment < nativeControl.ChildCount)
        {
            if (nativeControl.GetChildAt(VirtualView.SelectedSegment) is RadioButton option)
            {
                option.Checked = true;
            }
        }

        return nativeControl;
    }

    internal static bool NeedsExactMeasure(IView virtualView)
    {
        if (virtualView.VerticalLayoutAlignment != Microsoft.Maui.Primitives.LayoutAlignment.Fill
            && virtualView.HorizontalLayoutAlignment != Microsoft.Maui.Primitives.LayoutAlignment.Fill)
        {
            // Layout Alignments of Start, Center, and End will be laying out the TextView at its measured size,
            // so we won't need another pass with MeasureSpecMode.Exactly
            return false;
        }

        if (virtualView.Width >= 0 && virtualView.Height >= 0)
        {
            // If the Width and Height are both explicit, then we've already done MeasureSpecMode.Exactly in 
            // both dimensions; no need to do it again
            return false;
        }

        // We're going to need a second measurement pass so TextView can properly handle alignments
        return true;
    }


    internal static int MakeMeasureSpecExact(RadioGroup view, double size)
    {
        // Convert to a native size to create the spec for measuring
        var deviceSize = (int)view.Context.ToPixels(size);
        return MeasureSpecMode.Exactly.MakeMeasureSpec(deviceSize);
    }

    internal void PrepareArrange(Rect frame)
    {
        if (frame.Width < 0 || frame.Height < 0)
        {
            return;
        }

        RadioGroup platformView = PlatformView;
        if (platformView == null)
        {
            return;
        }

        var virtualView = VirtualView;
        if (virtualView == null)
        {
            return;
        }

        // Depending on our layout situation, the TextView may need an additional measurement pass at the final size
        // in order to properly handle any TextAlignment properties and some internal bookkeeping
        if (NeedsExactMeasure(virtualView))
        {
            platformView.Measure(
                MakeMeasureSpecExact(platformView, frame.Width),
                MakeMeasureSpecExact(platformView, frame.Height));
        }
    }


    public override void PlatformArrange(Rect frame)
    {
        PrepareArrange(frame);
        base.PlatformArrange(frame);
    }


    protected override void ConnectHandler(RadioGroup platformView)
    {
        base.ConnectHandler(platformView);

        platformView.CheckedChange += PlatformView_CheckedChange;
    }


    protected override void DisconnectHandler(RadioGroup platformView)
    {
        base.DisconnectHandler(platformView);

        platformView.CheckedChange -= PlatformView_CheckedChange;

        // Unsubscribe all RadioButton Click events to prevent memory leaks
        for (int i = 0; i < platformView.ChildCount; i++)
        {
            if (platformView.GetChildAt(i) is RadioButton rb)
            {
                rb.Click -= RadioButton_Click;
            }
        }

        _selectedRadioButton = null;
    }

    void PlatformView_CheckedChange(object? sender, RadioGroup.CheckedChangeEventArgs e)
    {
        if (VirtualView.GroupToggleBehavior == GroupToggleBehavior.None)
            return;

        if (sender is not RadioGroup rg || rg.CheckedRadioButtonId == -1)
            return;

        var radioButton = rg.FindViewById(rg.CheckedRadioButtonId);
        if (radioButton == null)
            return;

        var radioId = rg.IndexOfChild(radioButton);
        if (radioId < 0 || radioId >= rg.ChildCount)
            return;

        if (rg.GetChildAt(radioId) is not RadioButton rb)
            return;

        //set newly selected button properties
        var isNewButtonEnabled = VirtualView.IsEnabled && rb.Enabled;

        var selectedTextColor = isNewButtonEnabled ?
            VirtualView.SelectedTextColor.ToPlatform() :
            VirtualView.DisabledTextColor.ToPlatform();

        var selectedTintColor = isNewButtonEnabled ?
            VirtualView.TintColor.ToPlatform() :
            VirtualView.DisabledTintColor.ToPlatform();

        rb.SetTextColor(selectedTextColor);
        SetTintColor(rb, selectedTintColor);

        //reset old selected button properties
        if (_selectedRadioButton != null)
        {
            var isOldButtonEnabled = _selectedRadioButton.Enabled;
            var textColor = isOldButtonEnabled ?
                VirtualView.TextColor.ToPlatform() :
                VirtualView.DisabledTextColor.ToPlatform();

            var tintColor = isOldButtonEnabled ?
                VirtualView.TintColor.ToPlatform() :
                VirtualView.DisabledBackgroundColor.ToPlatform();

            _selectedRadioButton.SetTextColor(textColor);
            SetTintColor(_selectedRadioButton, tintColor);
        }

        _selectedRadioButton = rb;

        VirtualView.SelectedSegment = radioId;
    }

    void ConfigureRadioButton(int i, bool isEnabled, RadioButton rb)
    {
        bool isButtonEnabled = VirtualView.IsEnabled && isEnabled;

        if (rb.Enabled != isButtonEnabled)
            rb.Enabled = isButtonEnabled;

        var isSelected = VirtualView.GroupToggleBehavior == GroupToggleBehavior.Radio
            && i == VirtualView.SelectedSegment;

        var tintColor = GetTintColor(isSelected, isButtonEnabled);

        if (i == VirtualView.SelectedSegment
            && VirtualView.GroupToggleBehavior == GroupToggleBehavior.Radio)
        {
            var selectedTextColor = isButtonEnabled ?
                VirtualView.SelectedTextColor.ToPlatform() :
                VirtualView.DisabledTextColor.ToPlatform();

            rb.SetTextColor(selectedTextColor);
            _selectedRadioButton = rb;
        }
        else
        {
            var textColor = isButtonEnabled ?
                VirtualView.TextColor.ToPlatform()
                : VirtualView.DisabledTextColor.ToPlatform();

            rb.SetTextColor(textColor);
        }

        SetTintColor(rb, tintColor);

        // Apply font size
        rb.SetTextSize(Android.Util.ComplexUnitType.Sp, (float)VirtualView.FontSize);

        // Apply padding
        var padding = VirtualView.Padding;
        rb.SetPadding(
            (int)Context.ToPixels(padding.Left),
            (int)Context.ToPixels(padding.Top),
            (int)Context.ToPixels(padding.Right),
            (int)Context.ToPixels(padding.Bottom));

        rb.Tag = i;
        // Unsubscribe first to prevent duplicate event subscriptions when ConfigureRadioButton 
        // is called on existing RadioButtons (e.g., in OnPropertyChanged)
        rb.Click -= RadioButton_Click;
        rb.Click += RadioButton_Click;

    }

    private void RadioButton_Click(object? sender, EventArgs e)
    {
        if (sender is RadioButton rb && rb.Tag != null)
        {
            var t = (int)rb.Tag;
            VirtualView.SendSegmentTapped(t);

            if (VirtualView.GroupToggleBehavior == GroupToggleBehavior.None)
                rb.Checked = false;
        }
    }

    private Android.Graphics.Color GetTintColor(bool selected, bool enabled)
    {
        return enabled ?
            VirtualView.TintColor.ToPlatform() :
            VirtualView.DisabledTintColor.ToPlatform();

        // 'tint' is an outline + selected button color, so 
        //the backgroundcolor for the segmented control can't be used as 'tint'

        //TODO we should have a separate outline color 
        // and ability to pick a background color for selected(checked) segment
    }

    private void SetTintColor(RadioButton rb, Android.Graphics.Color tintColor)
    {
        // Minimum API level check - this code requires API 18+
        if (Android.OS.Build.VERSION.SdkInt < Android.OS.BuildVersionCodes.JellyBeanMr2)
        {
            // Fallback for API < 18: just skip the tint color customization
            return;
        }

        try
        {
            // Safe pattern matching to validate drawable types
            if (rb.Background is not StateListDrawable gradientDrawable)
                return;

            if (gradientDrawable.GetConstantState() is not DrawableContainer.DrawableContainerState drawableContainerState)
                return;

            var children = drawableContainerState.GetChildren();
            if (children == null || children.Length < 2)
                return;

            // Safely extract GradientDrawables from children
            GradientDrawable? selectedShape = children[0] switch
            {
                GradientDrawable gd => gd,
                InsetDrawable id when id.Drawable is GradientDrawable gd => gd,
                _ => null
            };

            GradientDrawable? unselectedShape = children[1] switch
            {
                GradientDrawable gd => gd,
                InsetDrawable id when id.Drawable is GradientDrawable gd => gd,
                _ => null
            };

            // Apply tint colors if we successfully extracted the shapes
            selectedShape?.SetStroke(3, tintColor);
            selectedShape?.SetColor(tintColor);
            unselectedShape?.SetStroke(3, tintColor);
        }
        catch (Exception)
        {
            // Silently handle any unexpected drawable configuration
            // This prevents crashes if the drawable structure changes
        }
    }

    static void MapTintColor(SegmentedControlHandler handler, SegmentedControl control) => OnPropertyChanged(handler, control);

    static void MapSelectedSegment(SegmentedControlHandler handler, SegmentedControl control)
    {
        if (handler.VirtualView.GroupToggleBehavior == GroupToggleBehavior.None)
            return;

        if (control.SelectedSegment < 0 || control.SelectedSegment >= handler.PlatformView.ChildCount)
            return;

        if (handler.PlatformView.GetChildAt(control.SelectedSegment) is RadioButton option
            && handler.VirtualView.GroupToggleBehavior == GroupToggleBehavior.Radio)
        {
            option.Checked = true;
        }

        control.SendValueChanged();
    }


    static void MapIsEnabled(SegmentedControlHandler handler, SegmentedControl control) => OnPropertyChanged(handler, control);

    static void MapSelectedTextColor(SegmentedControlHandler handler, SegmentedControl control)
    {
        if (handler.VirtualView.GroupToggleBehavior == GroupToggleBehavior.None)
            return;

        if (control.SelectedSegment < 0 || control.SelectedSegment >= handler.PlatformView.ChildCount)
            return;

        if (handler.PlatformView.GetChildAt(control.SelectedSegment) is RadioButton v)
        {
            v.SetTextColor(control.SelectedTextColor.ToPlatform());
        }
    }

    static void MapDisabledTintColor(SegmentedControlHandler handler, SegmentedControl control)
    {
        if (control.SelectedSegment < 0
            || control.SelectedSegment >= handler.PlatformView.ChildCount
            || handler.VirtualView.GroupToggleBehavior == GroupToggleBehavior.None)
            return;

        if (handler.PlatformView.GetChildAt(control.SelectedSegment) is RadioButton v)
        {
            bool isButtonEnabled = control.IsEnabled && v.Enabled;
            var tintColor = handler.GetTintColor(true, isButtonEnabled);
            handler.SetTintColor(v, tintColor);
        }
    }


    static void MapDisabledTextColor(SegmentedControlHandler handler, SegmentedControl control)
    {
        //go through children and update disabled segments
        for (int i = 0; i < handler.PlatformView.ChildCount; i++)
        {
            if (handler.PlatformView.GetChildAt(i) is RadioButton v)
            {
                if (!v.Enabled || !control.IsEnabled)
                {
                    v.SetTextColor(control.DisabledTextColor.ToPlatform());
                }
            }
        }
    }

    static void MapDisabledBackgroundColor(SegmentedControlHandler handler, SegmentedControl control)
    {
        //go through children and update disabled segments
        for (int i = 0; i < handler.PlatformView.ChildCount; i++)
        {
            if (handler.PlatformView.GetChildAt(i) is RadioButton v)
            {
                var tintColor = handler.GetTintColor(v.Checked, !v.Enabled || !control.IsEnabled);
                handler.SetTintColor(v, tintColor);
            }
        }
    }


    static void MapChildren(SegmentedControlHandler handler, SegmentedControl control)
    {
        //entire Children property has been changed -- woo hoo we essentialy have to
        //re-create all the segments now

        if (handler.PlatformView == null || control?.Children == null)
            return;

        var radioGroup = handler.PlatformView;
        int count = radioGroup.ChildCount;

        // First, unsubscribe events and remove old children
        if (count > 0)
        {
            for (int i = count - 1; i >= 0; i--)
            {
                var child = radioGroup.GetChildAt(i);
                if (child is RadioButton rb)
                {
                    // Unsubscribe event to prevent memory leaks
                    rb.Click -= handler.RadioButton_Click;
                    radioGroup.RemoveViewAt(i);
                }
            }
        }

        // Next, add new children using the helper method
        var layoutInflater = LayoutInflater.From(handler.Context);
        if (layoutInflater == null)
            return;

        var vv = handler.VirtualView;
        for (var i = 0; i < vv.Children.Count; i++)
        {
            var segment = vv.Children[i];
            if (segment == null)
                continue;

            var rb = handler.CreateRadioButton(layoutInflater, i, segment, vv.Children.Count);
            if (rb != null)
            {
                radioGroup.AddView(rb);
            }
        }

        // Set initial selected segment
        if (vv.GroupToggleBehavior == GroupToggleBehavior.Radio
            && vv.SelectedSegment >= 0
            && vv.SelectedSegment < radioGroup.ChildCount)
        {
            if (radioGroup.GetChildAt(vv.SelectedSegment) is RadioButton option)
            {
                option.Checked = true;
            }
        }
    }

    static void MapTextColor(SegmentedControlHandler handler, SegmentedControl control)
    {
        for (int i = 0; i < handler.PlatformView.ChildCount; i++)
        {
            if (handler.PlatformView.GetChildAt(i) is RadioButton v)
            {
                if (i != control.SelectedSegment)
                {
                    v.SetTextColor(control.TextColor.ToPlatform());
                }
                else
                {
                    v.SetTextColor(control.SelectedTextColor.ToPlatform());
                }
            }
        }
    }

    static void MapFontSize(SegmentedControlHandler handler, SegmentedControl control)
    {
        for (int i = 0; i < handler.PlatformView.ChildCount; i++)
        {
            var v = (RadioButton)handler.PlatformView.GetChildAt(i);
            v.SetTextSize(Android.Util.ComplexUnitType.Sp, (float)control.FontSize);
        }
    }

    static void MapPadding(SegmentedControlHandler handler, SegmentedControl control)
    {
        var padding = control.Padding;
        for (int i = 0; i < handler.PlatformView.ChildCount; i++)
        {
            var v = (RadioButton)handler.PlatformView.GetChildAt(i);
            v.SetPadding(
                (int)handler.Context.ToPixels(padding.Left),
                (int)handler.Context.ToPixels(padding.Top),
                (int)handler.Context.ToPixels(padding.Right),
                (int)handler.Context.ToPixels(padding.Bottom));
        }
    }

    static void OnPropertyChanged(SegmentedControlHandler handler, SegmentedControl control)
    {
        if (handler.PlatformView == null || control?.Children == null)
            return;

        for (var i = 0; i < control.Children.Count && i < handler.PlatformView.ChildCount; i++)
        {
            var child = control.Children[i];
            if (child == null)
                continue;

            if (handler.PlatformView.GetChildAt(i) is RadioButton rb)
            {
                if (rb.Text != child.Text)
                    rb.Text = child.Text;
                handler.ConfigureRadioButton(i, control.Children[i].IsEnabled, rb);
            }
        }
    }
}

#endif