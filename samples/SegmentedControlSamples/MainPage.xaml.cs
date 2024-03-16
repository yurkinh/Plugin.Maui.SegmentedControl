namespace SegmentedControlSamples;

public partial class MainPage : ContentPage
{


	public MainPage()
	{
		InitializeComponent();

        Title = "Segmented Control";        
    }

    void Handle_ValueChanged(object sender, Plugin.Maui.SegmentedControl.ValueChangedEventArgs e)
    {
        switch (e.NewValue)
        {
            case 0:
                SegContent.Children.Clear();
                SegContent.Children.Add(new SegControlTest1());
                break;
            case 1:
                SegContent.Children.Clear();
                SegContent.Children.Add(new Label() { Text = "TEST 2 tab selected" });
                break;
            case 2:
                SegContent.Children.Clear();
                SegContent.Children.Add(new Label() { Text = "TEST 3 tab selected" });
                break;
            case 3:
                SegContent.Children.Clear();
                SegContent.Children.Add(new Label() { Text = "TEST 4  tab selected" });
                break;
        }
    }    

}


