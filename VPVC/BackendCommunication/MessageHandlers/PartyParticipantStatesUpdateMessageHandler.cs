using VPVC.BackendCommunication.Interfaces;
using VPVC.BackendCommunication.Shared.ProtobufMessages;
using VPVC.BackendCommunication.Shared.ProtobufMessages.ServerToClient;

namespace VPVC.BackendCommunication.MessageHandlers; 

public class PartyParticipantStatesUpdateMessageHandler: IMessageHandler {
    public void HandleMessage(SessionMessage message) {
        if (message.partyParticipantStatesUpdateMessageContent == null) {
            return;
        }

        PartyParticipantStatesUpdateMessageContent content = message.partyParticipantStatesUpdateMessageContent;

        if (content.partyParticipantStates != null) {
            PartyEventListeners.partyParticipantStatesUpdate?.Invoke(
                content.partyParticipantStates
            );
        }
    }
}