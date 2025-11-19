using Plugin.Maui.SegmentedControl;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        public List<string> _playList = new List<string>
        {
            "Song One",
            "Song Two",
            "Song Three",
            "Song Four",
        };

        readonly Timer _timer;        
        public ICommand SegmentTappedCommand { get; }



        public Test2ViewModel()
        {
            _timer = new Timer(OnTimer);

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


        private PlayerStatus _playerStatus;
        private PlayerStatus PlayerStatus
        {
            get { return _playerStatus; }
            set 
            { 
                _playerStatus = value;
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(PlayerStatusText)));
                PlayButtonText = _playerStatus == PlayerStatus.Playing ? "STOP" : "Play";
            }
        }

        public string PlayerStatusText
        {
            get { return this.PlayerStatus.ToString(); }
        }

        private string _playButtonText;
        public string PlayButtonText
        {
            get => _playButtonText;
            set { _playButtonText = value; OnPropertyChanged(new PropertyChangedEventArgs(nameof(PlayButtonText))); }
        }

        private bool _backButtonEnabled = true;
        public bool BackButtonEnabled
        {
            get => _backButtonEnabled;
            set { _backButtonEnabled = value; OnPropertyChanged(new PropertyChangedEventArgs(nameof(BackButtonEnabled))); }
        }

        private bool _forwardButtonEnabled = true;
        public bool ForwardButtonEnabled
        {
            get => _forwardButtonEnabled;
            set { _forwardButtonEnabled = value; OnPropertyChanged(new PropertyChangedEventArgs(nameof(ForwardButtonEnabled))); }
        }

        private string _forwardButtonText;
        public string ForwardButtonText
        {
            get => _forwardButtonText;
            set { _forwardButtonText = value; OnPropertyChanged(new PropertyChangedEventArgs(nameof(ForwardButtonText))); }
        }


        private int _selectedSegment;
        public int SelectedSegment
        {
            get => _selectedSegment;
            set
            {
                _selectedSegment = value;
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(SelectedSegment)));
            }
        }
        private IList<SegmentedControlOption> _segmentItemsSource;
        public IList<SegmentedControlOption> SegmentItemsSource
        {
            get => _segmentItemsSource;
            set { _segmentItemsSource = value; OnPropertyChanged(new PropertyChangedEventArgs(nameof(SegmentItemsSource))); }
        }


        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(this, e);
        }
    }

}
