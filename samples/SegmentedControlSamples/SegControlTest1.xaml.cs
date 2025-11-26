using Plugin.Maui.SegmentedControl;

namespace SegmentedControlSamples;

public partial class SegControlTest1 : ContentView
{
	public SegControlTest1()
	{
		InitializeComponent();
        SegmentedControl.Children = list1;

    }

    private readonly SegmentedControlOption[] list1 = [
            new() {Text="Test0A"},
            new() {Text="Test1A"},
            new() {Text="Test2A"}
        ];

    internal SegmentedControlOption[] List2 = [
            new() {Text="Item1B"},
            new() {Text="Item2B"},
            new() {Text="Item3B"},
            new() {Text="Item4B"},
            new() {Text="Item5B"}
        ];
    bool _isList2 = false;


    private void ChangeChildren_Clicked(object sender, EventArgs e)
    {
        if(_isList2)
        {
            SegmentedControl.Children = list1;
        }
        else
        {
            SegmentedControl.Children = List2;
        }
        _isList2 = !_isList2;
    }


    private void Button_OnClicked(object sender, EventArgs e)
    {
        SegmentWithStack.Children.Remove(SegmentedControl);
    }

    private void ButtonTintColor_OnClicked(object sender, EventArgs e)
    {
        if (SegmentedControl.SelectedTextColor != Colors.Aqua)
        {
            SegmentedControl.TintColor = Colors.Aqua;
        }
        else
        {
            SegmentedControl.TintColor = Colors.BlueViolet;
        }
    }

    private void ButtonSelectedTextColor_OnClicked(object sender, EventArgs e)
    {
        if(SegmentedControl.SelectedTextColor != Colors.Red)
        {
            SegmentedControl.SelectedTextColor = Colors.Red;
        }
        else
        {
            SegmentedControl.SelectedTextColor = Colors.Green;
        }

    }

    private void ButtonBorderColor_OnClicked(object sender, EventArgs e)
    {
        //SegmentedControl.BorderColor = Color.Crimson;
    }

    private void ButtonBorderWidth_OnClicked(object sender, EventArgs e)
    {
        //SegmentedControl.BorderWidth = (SegmentedControl.BorderWidth + 1) % 3;
    }

    private void ButtonBackgroundColor_OnClicked(object sender, EventArgs e)
    {
        SegmentedControl.BackgroundColor = Colors.HotPink;
    }

    private void Disable_OnClicked(object sender, EventArgs e)
    {
        SegmentedControl.IsEnabled = false;
    }

    private void Enable_OnClicked(object sender, EventArgs e)
    {
        SegmentedControl.IsEnabled = true;
    }

    private bool _isDisabledColorChanged;
    private Color _defaultDisabledColor;
    private void ChangeDisabledTintColor_OnClicked(object sender, EventArgs e)
    {
        if (_isDisabledColorChanged)
        {
            SegmentedControl.DisabledTintColor = _defaultDisabledColor;
        }
        else
        {
            _defaultDisabledColor = SegmentedControl.DisabledTintColor;
            SegmentedControl.DisabledTintColor = Colors.Red;
        }
        _isDisabledColorChanged = !_isDisabledColorChanged;
    }

    private void ChangeSelectedSegment(object sender, EventArgs e)
    {
        if(SegmentedControl.SelectedSegment < 0)
        {
            SegmentedControl.SelectedSegment = 0;
            return;
        }

        if(SegmentedControl.SelectedSegment < SegmentedControl.Children.Count) 
        {
            SegmentedControl.SelectedSegment += 1;
            return;
        }

        SegmentedControl.SelectedSegment = 0;
        
    }

    private void ChangeFirstText(object sender, EventArgs e)
    {
        const string boundText = "Item 1B";
        SegmentedControl.Children[0].Text = 
            SegmentedControl.Children[0].Text == boundText 
            ? "Item1" 
            : boundText;
    }

    public void DisableFirstSegment_OnClicked(object sender, EventArgs e)
    {
        SegmentedControl.Children[0].IsEnabled = false;
    }

    public void EnableFirstSegment_OnClicked(object sender, EventArgs e)
    {
        SegmentedControl.Children[0].IsEnabled = true;
    }

    //public void OnElementChildrenChanging(object sender, ElementChildrenChanging e)
    //{
    //    if (e.OldValues != null && e.OldValues.Count > 0)
    //    {
    //        e.OldValues[0].RemoveBinding(SegmentedControlOption.TextProperty);
    //    }
    //    if (e.NewValues != null && e.NewValues.Count > 0)
    //    {
    //        e.NewValues[0].SetBinding(SegmentedControlOption.TextProperty, nameof(_viewModel.ChangeText));
    //    }
    //}

    public void ChangeTextSize_OnClicked(object sender, EventArgs e)
    {
        //SegmentedControl.FontSize = SegmentedControl.FontSize < 20 ? 20 : 12;
    }

    public void ChangeFontFamily_OnClicked(object sender, EventArgs e)
    {
        //switch (Device.RuntimePlatform)
        //{
        //    case Device.Android:
        //        SegmentedControl.FontFamily = SegmentedControl.FontFamily == "monospace" ? "serif" : "monospace";
        //        break;

        //    case Device.iOS:
        //    case Device.macOS:
        //        SegmentedControl.FontFamily = SegmentedControl.FontFamily == "Baskerville" ? "HelveticaNeue" : "Baskerville";
        //        break;

        //    case Device.UWP:
        //        SegmentedControl.FontFamily = SegmentedControl.FontFamily == "Courier New" ? "Microsoft Sans Serif" : "Courier New";
        //        break;

        //}
    }

 
    private bool isTextColorChanged;
    private Color defaultTextColor;

    private void Button_TextColor(object sender, EventArgs e)
    {
        if (!isTextColorChanged)
        {
            defaultTextColor = SegmentedControl.TextColor;
            SegmentedControl.TextColor = Colors.Red;
            isTextColorChanged = true;
        }
        else
        {
            isTextColorChanged = false;
            SegmentedControl.TextColor = defaultTextColor;
        }
    }
}