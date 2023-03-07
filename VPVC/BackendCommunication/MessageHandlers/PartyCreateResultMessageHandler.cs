using VPVC.BackendCommunication.Interfaces;
using VPVC.BackendCommunication.Shared.ProtobufMessages;
using VPVC.BackendCommunication.Shared.ProtobufMessages.ServerToClient;

namespace VPVC.BackendCommunication.MessageHandlers; 

public class PartyCreateResultMessageHandler: IMessageHandler {
    public void HandleMessage(SessionMessage message) {
        if (message.partyCreateResultMessageContent == null) {
            return;
        }

        PartyCreateResultMessageContent content = message.partyCreateResultMessageContent;

        PartyEventListeners.partyCreateResult?.Invoke(
            content.success,
            content.partyJoinCode,
            content.partyParticipantSelf
        );
    }
}