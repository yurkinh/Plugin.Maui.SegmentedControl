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
                SegContent.Children.Add(new Label() { Text = "Items tab selected" });
                break;
            case 1:
                SegContent.Children.Clear();
                SegContent.Children.Add(new Label() { Text = "Notes tab selected" });
                break;
            case 2:
                SegContent.Children.Clear();
                SegContent.Children.Add(new Label() { Text = "Approvers tab selected" });
                break;
            case 3:
                SegContent.Children.Clear();
                SegContent.Children.Add(new Label() { Text = "Attachments tab selected" });
                break;
        }
    }    

}


