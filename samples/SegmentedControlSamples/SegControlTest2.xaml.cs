using Plugin.Maui.SegmentedControl;

namespace SegmentedControlSamples;

public partial class SegControlTest2 : ContentView
{
    Test2ViewModel _viewModel;

    public SegControlTest2()
    {
        InitializeComponent();
        
        SegmentedControl.BindingContext = _viewModel = new Test2ViewModel();
    }

    private void SegmentedControl_ValueChanged(object sender, Plugin.Maui.SegmentedControl.ValueChangedEventArgs e)
    {
        segmentIndexLabel.Text = e.NewValue.ToString();
    }

    PlayerStatus _playerStatus = PlayerStatus.Stopped;
    private void SegmentedControl_SegmentTapped(object sender, SegmentTappedEventArgs e)
    {
        if (e.Index == 1)
        {
            if (_playerStatus == PlayerStatus.Playing)
            {
                SegmentedControl.Children[1].Text = "Play";
                _playerStatus = PlayerStatus.Stopped;
            }
            else
            {
                SegmentedControl.Children[1].Text = "Stop";
                _playerStatus = PlayerStatus.Playing;
            }
        }
    }
}