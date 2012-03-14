using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using SKYPE4COMLib;

namespace SkypeStandup
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private readonly Skype _skype;
        private bool _logKeyPresses;
        public bool LogKeyPresses
        {
            get { return _logKeyPresses; }
            set
            {
                if (_logKeyPresses) KeyPresses.Clear();

                if (_logKeyPresses != value)
                {
                    _logKeyPresses = value;
                    PropertyChanged(this, new PropertyChangedEventArgs("LogKeyPresses"));
                }
            }
        }

        public ObservableCollection<Conference> ActiveConferences { get; private set; }
        public ObservableCollection<Call> ActiveCalls { get; private set; }
        public ObservableCollection<string> KeyPresses { get; private set; }
        
        public MainWindow()
        {
            InitializeComponent();

            ActiveConferences = new ObservableCollection<Conference>();
            ActiveCalls = new ObservableCollection<Call>();
            KeyPresses = new ObservableCollection<string>();

            DataContext = this;

            _skype = new Skype();
            _skype.Attach(7, true);
            UpdateConferenceList();
            _skype.CallStatus += SkypeOnCallStatus;
            KeyboardHook.KeyPressed += OnKeyUp;
            KeyboardHook.SetHooks();
        }
        
        private void SkypeOnCallStatus(Call pCall, TCallStatus status)
        {
            Debug.WriteLine(string.Format("Call from {0} with status {1}", pCall.PartnerDisplayName, status));
            UpdateConferenceList();
        }

        private void UpdateConferenceList()
        {
            IList<Conference> activeConferences = _skype.Conferences.Cast<Conference>().Where(c => c.ActiveCalls.Count > 0).ToList();

            foreach (Conference conference in ActiveConferences.Where(c => !activeConferences.Contains(c)).ToList())
            {
                ActiveConferences.Remove(conference);
            }

            foreach (Conference conference in activeConferences.Where(c => !ActiveConferences.Contains(c)).ToList())
            {
                ActiveConferences.Add(conference);
            }
        }

        private void OnKeyUp(object sender, KeyHookEventArgs keyEventArgs)
        {
            Debug.WriteLine(string.Format("OnKeyUp(sender: {0}, keyEventArgs: {1})", sender, keyEventArgs));
            if (LogKeyPresses) KeyPresses.Insert(0, keyEventArgs.Key.ToString());

            if (keyEventArgs.Key == VirtualKey.MEDIA_PLAY_PAUSE)
            {
                ((ISkype)_skype).Mute = !((ISkype)_skype).Mute;
                return;
            }

            if (keyEventArgs.Key == VirtualKey.MEDIA_STOP)
            {
                foreach (Call call in _skype.ActiveCalls)
                {
                    call.Finish();
                }
            }
        }

        private void muteButton_Click(object sender, RoutedEventArgs e)
        {
            ((ISkype)_skype).Mute = !((ISkype)_skype).Mute;
        }

        public event PropertyChangedEventHandler PropertyChanged = delegate {};
    }
}
