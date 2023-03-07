using VPVC.BackendCommunication.Interfaces;
using VPVC.BackendCommunication.Shared.ProtobufMessages;
using VPVC.BackendCommunication.Shared.ProtobufMessages.ServerToClient;

namespace VPVC.BackendCommunication.MessageHandlers; 

public class PartyJoinResultMessageHandler: IMessageHandler {
    public void HandleMessage(SessionMessage message) {
        if (message.partyJoinResultMessageContent == null) {
            return;
        }

        PartyJoinResultMessageContent content = message.partyJoinResultMessageContent;

        PartyEventListeners.partyJoinResult?.Invoke(
            content.success,
            content.partyParticipantSelf,
            content.partyParticipants
        );
    }
}