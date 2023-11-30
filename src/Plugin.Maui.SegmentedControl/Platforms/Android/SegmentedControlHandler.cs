#if ANDROID
using Android.Graphics.Drawables;
using Android.Views;
using Android.Widget;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using static Android.Views.ViewGroup;
using RadioButton = Android.Widget.RadioButton;

namespace Plugin.Maui.SegmentedControl.Handlers;

public class SegmentedControlHandler : ViewHandler<SegmentedControl, RadioGroup>
{
    RadioButton _rb;

    public static IPropertyMapper<SegmentedControl, SegmentedControlHandler> Mapper = new PropertyMapper<SegmentedControl, SegmentedControlHandler>(ViewMapper)
    {
        [nameof(SegmentedControl.IsEnabled)] = MapIsEnabled,
        [nameof(SegmentedControl.SelectedSegment)] = MapSelectedSegment,
        [nameof(SegmentedControl.TintColor)] = MapTintColor,
        [nameof(SegmentedControl.SelectedTextColor)] = MapSelectedTextColor,
        [nameof(SegmentedControl.TextColor)] = MapTextColor
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

            ConfigureRadioButton(i, rb);

            nativeControl.AddView(rb);
        }

        var option = (RadioButton)nativeControl.GetChildAt(VirtualView.SelectedSegment);

        if (option != null)
            option.Checked = true;

        return nativeControl;
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
        _rb = null;
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

            var color = VirtualView.IsEnabled ? VirtualView.TextColor.ToPlatform() : VirtualView.DisabledColor.ToPlatform();
            _rb?.SetTextColor(color);
            rb.SetTextColor(VirtualView.SelectedTextColor.ToPlatform());
            _rb = rb;

            VirtualView.SelectedSegment = radioId;
        }
    }

    void ConfigureRadioButton(int i, RadioButton rb)
    {
        if (i == VirtualView.SelectedSegment)
        {
            rb.SetTextColor(VirtualView.SelectedTextColor.ToPlatform());
            _rb = rb;
        }
        else
        {
            var textColor = VirtualView.IsEnabled ? VirtualView.TintColor.ToPlatform() : VirtualView.DisabledColor.ToPlatform();
            rb.SetTextColor(textColor);
        }

        GradientDrawable selectedShape;
        GradientDrawable unselectedShape;

        var gradientDrawable = (StateListDrawable)rb.Background;
        var drawableContainerState = (DrawableContainer.DrawableContainerState)gradientDrawable.GetConstantState();
        var children = drawableContainerState.GetChildren();

        // Doesnt works on API < 18
        selectedShape = children[0] is GradientDrawable drawable ? drawable : (GradientDrawable)((InsetDrawable)children[0]).Drawable;
        unselectedShape = children[1] is GradientDrawable drawable1 ? drawable1 : (GradientDrawable)((InsetDrawable)children[1]).Drawable;

        var color = VirtualView.IsEnabled ? VirtualView.TintColor.ToPlatform() : VirtualView.DisabledColor.ToPlatform();

        selectedShape.SetStroke(3, color);
        selectedShape.SetColor(color);
        unselectedShape.SetStroke(3, color);

        rb.Enabled = VirtualView.IsEnabled;
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
        v.SetTextColor(control.SelectedTextColor.ToPlatform());
    }

    static void MapTextColor(SegmentedControlHandler handler, SegmentedControl control)
    {
        for (int i = 0; i < handler.PlatformView.ChildCount; i++)
        {
            var v = (RadioButton)handler.PlatformView.GetChildAt(i);
            if (i != control.SelectedSegment) v.SetTextColor(control.TextColor.ToPlatform());
        }
    }

    static void OnPropertyChanged(SegmentedControlHandler handler, SegmentedControl control)
    {
        if (handler.PlatformView != null && control != null)
        {
            for (var i = 0; i < control.Children.Count; i++)
            {
                var rb = (RadioButton)handler.PlatformView.GetChildAt(i);

                handler.ConfigureRadioButton(i, rb);
            }
        }
    }
}

#endif