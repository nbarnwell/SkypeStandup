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
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        const int SkypeProtocol = 9;

        private readonly Skype skype;
        private bool logKeyPresses;
        public bool LogKeyPresses
        {
            get { return logKeyPresses; }
            set
            {
                if (logKeyPresses) KeyPresses.Clear();

                if (logKeyPresses != value)
                {
                    logKeyPresses = value;
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

            skype = new Skype();
            skype.CallStatus += SkypeOnCallStatus;
            ((_ISkypeEvents_Event)skype).AttachmentStatus += SkypeAttachmentStatus;
            skype.Attach(SkypeProtocol, false);
            
            KeyboardHook.KeyPressed += OnKeyUp;
            KeyboardHook.SetHooks();
        }

        public void SkypeAttachmentStatus(TAttachmentStatus status)
        {
            try
            {
                if ((((ISkype)skype).AttachmentStatus != TAttachmentStatus.apiAttachSuccess))
                {
                    Debug.WriteLine(string.Format("Attachment Status - Converted Status: {0}, TAttachmentStatus: {1}", 
                        skype.Convert.AttachmentStatusToText((((ISkype)skype).AttachmentStatus)), (((ISkype)skype).AttachmentStatus)));
                }

                if (status == TAttachmentStatus.apiAttachRefused)
                {
                    Debug.WriteLine("The end-user has denied the attach request");
                }

                // End user could have changed users, try attach again.
                // Also maybe Skype was not running when we started and now is.
                if (status == TAttachmentStatus.apiAttachAvailable)
                {
                    skype.Attach(SkypeProtocol, false);
                }

                // Show are now connected to the Skype client.
                if (status == TAttachmentStatus.apiAttachSuccess)
                {
                    Debug.WriteLine("Connected. Attachment Status - Converted Status: {0}, TAttachmentStatus: {1}",
                        skype.Convert.AttachmentStatusToText((((ISkype)skype).AttachmentStatus)), (((ISkype)skype).AttachmentStatus));
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
                skype.Convert.CallTypeToText(pCall.Type), skype.Convert.CallStatusToText(status));

            if (status == TCallStatus.clsFinished)
            {
                SetTrayIcon(false);
            }

            IList<Conference> activeConferences = skype.Conferences.Cast<Conference>().ToList();
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
                var muted = ((ISkype) skype).Mute;
                muted = !muted; // Toggle muted state
                ((ISkype)skype).Mute = muted;
                SetTrayIcon(muted);
                return;
            }

            if (keyEventArgs.Key == VirtualKey.MEDIA_STOP)
            {
                foreach (Call call in skype.ActiveCalls)
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
            ((ISkype)skype).Mute = !((ISkype)skype).Mute;
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
