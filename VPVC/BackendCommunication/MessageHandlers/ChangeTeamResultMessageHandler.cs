using VPVC.BackendCommunication.Interfaces;
using VPVC.BackendCommunication.Shared.ProtobufMessages;
using VPVC.BackendCommunication.Shared.ProtobufMessages.ServerToClient;

namespace VPVC.BackendCommunication.MessageHandlers; 

public class ChangeTeamResultMessageHandler: IMessageHandler {
    public void HandleMessage(SessionMessage message) {
        if (message.changeTeamResultMessageContent == null) {
            return;
        }

        ChangeTeamResultMessageContent content = message.changeTeamResultMessageContent;

        PartyEventListeners.changeTeamResult?.Invoke(
            content.success,
            content.newTeamIndex
        );
    }
}