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
                SegContent.Content = new SegControlTest1();
                (SegContent as IView).InvalidateMeasure();
                break;
            case 1:
                SegContent.Content = new SegControlTest2();
                (SegContent as IView).InvalidateMeasure();
                break;
            case 2:
                SegContent.Content = new Label() { Text = "TEST 3 tab selected" };
                break;
            case 3:
                SegContent.Content = new Label() { Text = "TEST 4 tab selected" };
                break;
        }
    }

}


