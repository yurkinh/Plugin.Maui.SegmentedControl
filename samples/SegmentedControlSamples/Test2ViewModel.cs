using Plugin.Maui.SegmentedControl;
using System.ComponentModel;
using System.Windows.Input;

namespace SegmentedControlSamples
{
    public enum PlayerStatus
    {
        Stopped = 0,
        Playing = 1,
    }

    public class Test2ViewModel : INotifyPropertyChanged
    {
        public List<string> _playList =
        [
            "Song One",
            "Song Two",
            "Song Three",
            "Song Four",
        ];

        readonly Timer timer;        
        public ICommand SegmentTappedCommand { get; }

        public Test2ViewModel()
        {
            timer = new Timer(OnTimer);

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

        private void OnTimer(object s)
        {

        }

        private PlayerStatus playerStatus;
        private PlayerStatus PlayerStatus
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

        private string playButtonText;
        public string PlayButtonText
        {
            get => playButtonText;
            set { playButtonText = value; OnPropertyChanged(new PropertyChangedEventArgs(nameof(PlayButtonText))); }
        }

        private bool backButtonEnabled = true;
        public bool BackButtonEnabled
        {
            get => backButtonEnabled;
            set { backButtonEnabled = value; OnPropertyChanged(new PropertyChangedEventArgs(nameof(BackButtonEnabled))); }
        }

        private bool forwardButtonEnabled = true;
        public bool ForwardButtonEnabled
        {
            get => forwardButtonEnabled;
            set { forwardButtonEnabled = value; OnPropertyChanged(new PropertyChangedEventArgs(nameof(ForwardButtonEnabled))); }
        }

        private string _forwardButtonText;
        public string ForwardButtonText
        {
            get => _forwardButtonText;
            set { _forwardButtonText = value; OnPropertyChanged(new PropertyChangedEventArgs(nameof(ForwardButtonText))); }
        }


        private int selectedSegment;
        public int SelectedSegment
        {
            get => selectedSegment;
            set
            {
                selectedSegment = value;
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(SelectedSegment)));
            }
        }
        private IList<SegmentedControlOption> segmentItemsSource;
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

}
