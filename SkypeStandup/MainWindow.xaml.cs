using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using SKYPE4COMLib;

namespace SkypeStandup
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : INotifyPropertyChanged
    {
        const int SkypeProtocol = 9;

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
            HideOnMinimize.Enable(this);            

            _skype = new Skype();
            _skype.CallStatus += SkypeOnCallStatus;
            ((_ISkypeEvents_Event)_skype).AttachmentStatus += SkypeAttachmentStatus;
            _skype.Attach(SkypeProtocol, false);
            
            KeyboardHook.KeyPressed += OnKeyUp;
            KeyboardHook.SetHooks();
        }

        private void SkypeAttachmentStatus(TAttachmentStatus status)
        {
            try
            {
                if ((((ISkype)_skype).AttachmentStatus != TAttachmentStatus.apiAttachSuccess))
                {
                    Debug.WriteLine(string.Format("Attachment Status - Calaunchyonverted Status: {0}, TAttachmentStatus: {1}", 
                        _skype.Convert.AttachmentStatusToText((((ISkype)_skype).AttachmentStatus)), (((ISkype)_skype).AttachmentStatus)));
                }

                if (status == TAttachmentStatus.apiAttachRefused)
                {
                    Debug.WriteLine("The end-user has denied the attach request");
                }

                // End user could have changed users, try attach again.
                // Also maybe Skype was not running when we started and now is.
                if (status == TAttachmentStatus.apiAttachAvailable)
                {
                    _skype.Attach(SkypeProtocol, false);
                }

                // Show are now connected to the Skype client.
                if (status == TAttachmentStatus.apiAttachSuccess)
                {
                    Debug.WriteLine("Connected. Attachment Status - Converted Status: {0}, TAttachmentStatus: {1}",
                        _skype.Convert.AttachmentStatusToText((((ISkype)_skype).AttachmentStatus)), (((ISkype)_skype).AttachmentStatus));
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("Attachment status event exception. Source: {0}, Message: {1}",
                    e.Source, e.Message);
            }
        }

        private void SkypeOnCallStatus(Call pCall, TCallStatus status)
        {
            Debug.WriteLine("SkypeOnCallStatus event fired. Call: {0}, Status: {1}", 
                _skype.Convert.CallTypeToText(pCall.Type), _skype.Convert.CallStatusToText(status));

            if (status == TCallStatus.clsFinished)
            {
                SetTrayIcon(false);
            }

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
                var muted = ((ISkype) _skype).Mute;
                muted = !muted; // Toggle muted state
                ((ISkype)_skype).Mute = muted;
                SetTrayIcon(muted);
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

        private void SetTrayIcon(bool muted)
        {
            TrayNotifyIcon.IconSource = GetIconSource(muted);
        }

        private ImageSource GetIconSource(bool muted)
        {
            if (muted)
                return (ImageSource) FindResource("IconMuted");

            return (ImageSource)FindResource("IconUnmuted");
        }

        private void muteButton_Click(object sender, RoutedEventArgs e)
        {
            ((ISkype)_skype).Mute = !((ISkype)_skype).Mute;
        }

        public event PropertyChangedEventHandler PropertyChanged = delegate {};

        // Old school event handling
        private void showMenuItem_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Normal;
        }

        private void exitMenuItem_Click(object sender, RoutedEventArgs e)
        {
            App.Current.Shutdown();
        }
    }
}
