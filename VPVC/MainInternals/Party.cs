using System.Collections.Generic;

namespace VPVC.MainInternals; 

public class Party {
    public string joinCode;

    public readonly PartyParticipant participantSelf;
    public readonly List<PartyParticipant> otherParticipants;

    public Party(string joinCode, PartyParticipant participantSelf, List<PartyParticipant>? otherParticipants = null) {
        this.joinCode = joinCode;
        this.participantSelf = participantSelf;
        this.otherParticipants = otherParticipants ?? new List<PartyParticipant>();
    }
}