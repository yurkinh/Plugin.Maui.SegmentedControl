#if ANDROID
using Android.Graphics.Drawables;
using Android.Graphics.Drawables.Shapes;
using Android.Locations;
using Android.Net.Wifi;
using Android.Views;
using Android.Widget;
using Microsoft.Maui.Graphics.Text;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using static Android.Views.ViewGroup;
using RadioButton = Android.Widget.RadioButton;

namespace Plugin.Maui.SegmentedControl.Handlers;

public class SegmentedControlHandler : ViewHandler<SegmentedControl, RadioGroup>
{
    RadioButton _selectedRadioButton;

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
        [nameof(SegmentedControl.Children)] = MapChildren
    };

    public SegmentedControlHandler() : base(Mapper)
    {
    }

    public SegmentedControlHandler(IPropertyMapper mapper) : base(mapper ?? Mapper)
    {
    }

    protected override RadioGroup CreatePlatformView()
    {
        var layoutInflater = LayoutInflater.From(Context);       

        var nativeControl = (RadioGroup)layoutInflater.Inflate(Resource.Layout.RadioGroup, null);

        for (var i = 0; i < VirtualView.Children.Count; i++)
        {
            var o = VirtualView.Children[i];
            var isButtonEnabled = VirtualView.IsEnabled && o.IsEnabled;
            var rb = (RadioButton)layoutInflater.Inflate(Resource.Layout.RadioButton, null);

            rb.LayoutParameters = new RadioGroup.LayoutParams(LayoutParams.MatchParent, LayoutParams.WrapContent, 1f);
            rb.Text = o.Text;

            if (i == 0)
            {
                rb.SetBackgroundResource(Resource.Drawable.segmented_control_first_background);
            }                           
            else if (i == VirtualView.Children.Count - 1)
            {
                rb.SetBackgroundResource(Resource.Drawable.segmented_control_last_background);
            }               

            ConfigureRadioButton(i, isButtonEnabled, rb);

            nativeControl.AddView(rb);
        }

        var option = (RadioButton)nativeControl.GetChildAt(VirtualView.SelectedSegment);

        if (option != null)
            option.Checked = true;

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

        RadioGroup platformView = this.PlatformView;
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

        _selectedRadioButton = null;
    }

    void PlatformView_CheckedChange(object sender, RadioGroup.CheckedChangeEventArgs e)
    {
        var rg = (RadioGroup)sender;
        if (rg.CheckedRadioButtonId != -1)
        {
            var id = rg.CheckedRadioButtonId;
            var radioButton = rg.FindViewById(id);
            var radioId = rg.IndexOfChild(radioButton);

            var rb = (RadioButton)rg.GetChildAt(radioId);

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
            if(_selectedRadioButton != null) 
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
    }

    void ConfigureRadioButton(int i, bool isEnabled, RadioButton rb)
    {
        bool isButtonEnabled = VirtualView.IsEnabled && isEnabled;

        if (rb.Enabled != isButtonEnabled)
            rb.Enabled = isButtonEnabled;

        var tintColor = GetTintColor(i == VirtualView.SelectedSegment, isButtonEnabled);

        if (i == VirtualView.SelectedSegment)
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
        GradientDrawable selectedShape;
        GradientDrawable unselectedShape;

        //do not call SetBackgroundColor, that sets the state to ColorDrawable & makes invalid cast
        var gradientDrawable = (StateListDrawable)rb.Background;
        var drawableContainerState = (DrawableContainer.DrawableContainerState)gradientDrawable.GetConstantState();
        var children = drawableContainerState.GetChildren();

        
        // Doesnt works on API < 18
        selectedShape = children[0] is GradientDrawable drawable ? drawable : (GradientDrawable)((InsetDrawable)children[0]).Drawable;
        unselectedShape = children[1] is GradientDrawable drawable1 ? drawable1 : (GradientDrawable)((InsetDrawable)children[1]).Drawable;

        selectedShape.SetStroke(3, tintColor);
        selectedShape.SetColor(tintColor);
        unselectedShape.SetStroke(3, tintColor);
    }

    static void MapTintColor(SegmentedControlHandler handler, SegmentedControl control) => OnPropertyChanged(handler, control);

    static void MapSelectedSegment(SegmentedControlHandler handler, SegmentedControl control)
    {
        var option = (RadioButton)handler.PlatformView.GetChildAt(control.SelectedSegment);

        if (option != null)
            option.Checked = true;               

        control.SendValueChanged();
    }

    static void MapIsEnabled(SegmentedControlHandler handler, SegmentedControl control) => OnPropertyChanged(handler, control);

    static void MapSelectedTextColor(SegmentedControlHandler handler, SegmentedControl control)
    {
        var v = (RadioButton)handler.PlatformView.GetChildAt(control.SelectedSegment);
        v?.SetTextColor(control.SelectedTextColor.ToPlatform());
    }

    static void MapDisabledTintColor(SegmentedControlHandler handler, SegmentedControl control)
    {
        if (control.SelectedSegment < 0)
            return;

        var v = (RadioButton)handler.PlatformView.GetChildAt(control.SelectedSegment);

        bool isButtonEnabled = control.IsEnabled && v.Enabled;
        var tintColor = handler.GetTintColor(true, isButtonEnabled);
        handler.SetTintColor(v, tintColor);
    }


    static void MapDisabledTextColor(SegmentedControlHandler handler, SegmentedControl control)
    {
        //go through children and update disabled segments
        for (int i = 0; i < handler.PlatformView.ChildCount; i++)
        {
            var v = (RadioButton)handler.PlatformView.GetChildAt(i);
            if (!v.Enabled || !control.IsEnabled)
            {
                v.SetTextColor(control.DisabledTextColor.ToPlatform());
            }
        }
    }

    static void MapDisabledBackgroundColor(SegmentedControlHandler handler, SegmentedControl control)
    {
        //go through children and update disabled segments
        for (int i = 0; i < handler.PlatformView.ChildCount; i++)
        {
            var v = (RadioButton)handler.PlatformView.GetChildAt(i);
            var tintColor = handler.GetTintColor(v.Checked, !v.Enabled || !control.IsEnabled);
            handler.SetTintColor(v, tintColor);
        }
    }


    static void MapChildren(SegmentedControlHandler handler, SegmentedControl control)
    {
        //redraw everything
        OnPropertyChanged(handler, control);
    }

    static void MapTextColor(SegmentedControlHandler handler, SegmentedControl control)
    {
        for (int i = 0; i < handler.PlatformView.ChildCount; i++)
        {
            var v = (RadioButton)handler.PlatformView.GetChildAt(i);
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

    static void OnPropertyChanged(SegmentedControlHandler handler, SegmentedControl control)
    {
        if (handler.PlatformView != null && control != null)
        {
            for (var i = 0; i < control.Children.Count; i++)
            {
                var child = control.Children[i];
                var rb = (RadioButton)handler.PlatformView.GetChildAt(i);
                if(rb.Text != child.Text)
                    rb.Text = child.Text;
                handler.ConfigureRadioButton(i, control.Children[i].IsEnabled, rb);
            }
        }
    }
}

#endif