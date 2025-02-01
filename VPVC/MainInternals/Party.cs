using System.Collections.Generic;

namespace VPVC.MainInternals; 

public class Party {
    public string joinCode;

    public readonly PartyParticipant participantSelf;
    public readonly List<PartyParticipant> otherParticipants;

    public readonly byte[] voiceChatEncryptionKey;

    public Party(string joinCode, PartyParticipant participantSelf, List<PartyParticipant>? otherParticipants, byte[] voiceChatEncryptionKey) {
        this.joinCode = joinCode;
        this.participantSelf = participantSelf;
        this.otherParticipants = otherParticipants ?? new List<PartyParticipant>();
        this.voiceChatEncryptionKey = voiceChatEncryptionKey;
    }
}