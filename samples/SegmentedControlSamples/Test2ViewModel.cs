using Plugin.Maui.SegmentedControl;
using System.ComponentModel;
using System.Windows.Input;

namespace SegmentedControlSamples;

public enum PlayerStatus
{
    Stopped = 0,
    Playing = 1,
}

public class Test2ViewModel : INotifyPropertyChanged
{
    public List<string> PlayList =
    [
        "Song One",
        "Song Two",
        "Song Three",
        "Song Four",
        ];

    public ICommand SegmentTappedCommand { get; }

    public Test2ViewModel()
    {
        SegmentTappedCommand = new Command<SegmentTappedEventArgs>((a) =>
        {
            if (a.Index == 1)
            {
                if (PlayerStatus == PlayerStatus.Playing)
                {
                    PlayerStatus = PlayerStatus.Stopped;
                }
                else
                {
                    PlayerStatus = PlayerStatus.Playing;
                }
            }
        });
        PlayerStatus = PlayerStatus.Stopped;
        PlayButtonText = "Play";
        ForwardButtonEnabled = true;
        ForwardButtonText = "Forward";
        SelectedSegment = -1;
    }

    PlayerStatus playerStatus;
    PlayerStatus PlayerStatus
    {
        get { return playerStatus; }
        set
        {
            playerStatus = value;
            OnPropertyChanged(new PropertyChangedEventArgs(nameof(PlayerStatusText)));
            PlayButtonText = playerStatus == PlayerStatus.Playing ? "STOP" : "Play";
        }
    }

    public string PlayerStatusText
    {
        get { return PlayerStatus.ToString(); }
    }

    string playButtonText;
    public string PlayButtonText
    {
        get => playButtonText;
        set { playButtonText = value; OnPropertyChanged(new PropertyChangedEventArgs(nameof(PlayButtonText))); }
    }

    bool backButtonEnabled = true;
    public bool BackButtonEnabled
    {
        get => backButtonEnabled;
        set { backButtonEnabled = value; OnPropertyChanged(new PropertyChangedEventArgs(nameof(BackButtonEnabled))); }
    }

    bool forwardButtonEnabled = true;
    public bool ForwardButtonEnabled
    {
        get => forwardButtonEnabled;
        set { forwardButtonEnabled = value; OnPropertyChanged(new PropertyChangedEventArgs(nameof(ForwardButtonEnabled))); }
    }

    string forwardButtonText;
    public string ForwardButtonText
    {
        get => forwardButtonText;
        set { forwardButtonText = value; OnPropertyChanged(new PropertyChangedEventArgs(nameof(ForwardButtonText))); }
    }

    int selectedSegment;
    public int SelectedSegment
    {
        get => selectedSegment;
        set
        {
            selectedSegment = value;
            OnPropertyChanged(new PropertyChangedEventArgs(nameof(SelectedSegment)));
        }
    }

    IList<SegmentedControlOption> segmentItemsSource;
    public IList<SegmentedControlOption> SegmentItemsSource
    {
        get => segmentItemsSource;
        set { segmentItemsSource = value; OnPropertyChanged(new PropertyChangedEventArgs(nameof(SegmentItemsSource))); }
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        PropertyChanged?.Invoke(this, e);
    }
}
