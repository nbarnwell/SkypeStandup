using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    public partial class MainWindow : Window
    {
        private readonly Skype _skype;
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
            _skype.CallStatus += SkypeOnCallStatus;
            //this.KeyUp += OnKeyUp;
            KeyboardHook.KeyPressed += OnKeyUp;
            KeyboardHook.SetHooks();
        }
        
        private void SkypeOnCallStatus(Call pCall, TCallStatus status)
        {
            IList<Conference> activeConferences = _skype.Conferences.Cast<Conference>().ToList();
            IList<Conference> finishedConferences = new List<Conference>();
            foreach (Conference conference in ActiveConferences)
            {
                Conference c2 = conference;
                if (activeConferences.Any(c1 => c1 == c2) == false)
                {
                    finishedConferences.Add(conference);
                }
            }

            foreach (Conference conference in finishedConferences)
            {
                ActiveConferences.Remove(conference);
            }

            foreach (Conference conference in activeConferences)
            {
                
            }
        }

        private void OnKeyUp(object sender, KeyHookEventArgs keyEventArgs)
        {
            Debug.WriteLine(string.Format("OnKeyUp(sender: {0}, keyEventArgs: {1})", sender, keyEventArgs));
            KeyPresses.Insert(0, keyEventArgs.Key.ToString());

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
    }
}
