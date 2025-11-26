using Plugin.Maui.SegmentedControl;

namespace SegmentedControlSamples;

public partial class SegControlTest2 : ContentView
{
    Test2ViewModel viewModel;

    public SegControlTest2()
    {
        InitializeComponent();

        SegmentedControl.BindingContext = viewModel = new Test2ViewModel();
    }

    void SegmentedControl_ValueChanged(object sender, Plugin.Maui.SegmentedControl.ValueChangedEventArgs e)
    {
        segmentIndexLabel.Text = e.NewValue.ToString();
    }

    PlayerStatus playerStatus = PlayerStatus.Stopped;
    void SegmentedControl_SegmentTapped(object sender, SegmentTappedEventArgs e)
    {
        if (e.Index == 1)
        {
            if (playerStatus == PlayerStatus.Playing)
            {
                SegmentedControl.Children[1].Text = "Play";
                playerStatus = PlayerStatus.Stopped;
            }
            else
            {
                SegmentedControl.Children[1].Text = "Stop";
                playerStatus = PlayerStatus.Playing;
            }
        }
    }
}