using System.Collections.ObjectModel;
using System.Linq;
using SKYPE4COMLib;

namespace SkypeStandup
{
    public class MConference
    {
        public int Id { get; private set; }
        public ObservableCollection<ConferenceParticipant> Participants { get; private set; }

        public MConference(int id)
        {
            Id = id;
            Participants = new ObservableCollection<ConferenceParticipant>();
        }

        public void AddOrUpdateParticipant(string handle, string displayName, TCallStatus status)
        {
            var participant = Participants.SingleOrDefault(p => p.Handle == handle);

            if (participant == null)
            {
                participant = new ConferenceParticipant
                                  {
                                      Handle = handle,
                                      Name = displayName
                                  };
                Participants.Add(participant);
            }

            participant.Status = status;
        }
    }
}