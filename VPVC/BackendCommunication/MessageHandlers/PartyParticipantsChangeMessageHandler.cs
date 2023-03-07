using System.Collections.Generic;
using VPVC.BackendCommunication.Interfaces;
using VPVC.BackendCommunication.Shared;
using VPVC.BackendCommunication.Shared.ProtobufMessages;
using VPVC.BackendCommunication.Shared.ProtobufMessages.ServerToClient;

namespace VPVC.BackendCommunication.MessageHandlers; 

public class PartyParticipantsChangeMessageHandler: IMessageHandler {
    public void HandleMessage(SessionMessage message) {
        if (message.partyParticipantsChangeMessageContent == null) {
            return;
        }

        PartyParticipantsChangeMessageContent content = message.partyParticipantsChangeMessageContent;

        if (content.partyParticipantSelf != null) {
            PartyEventListeners.partyParticipantsChange?.Invoke(
                content.partyParticipantSelf,
                content.partyParticipants ?? new List<SerializablePartyParticipant>()
            );
        }
    }
}