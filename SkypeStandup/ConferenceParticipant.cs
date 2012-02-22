using System.ComponentModel;
using SKYPE4COMLib;

namespace SkypeStandup
{
    public class ConferenceParticipant : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        public string Handle { get; set; }
        public string Name { get; set; }
        
        private TCallStatus _status;
        public TCallStatus Status
        {
            get { return _status; }
            set 
            { 
                _status = value;
                PropertyChanged(this, new PropertyChangedEventArgs("Status"));
            }
        }
    }
}