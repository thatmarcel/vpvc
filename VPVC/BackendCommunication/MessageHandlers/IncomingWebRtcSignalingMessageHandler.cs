using VPVC.BackendCommunication.Interfaces;
using VPVC.BackendCommunication.Shared.ProtobufMessages;
using VPVC.BackendCommunication.Shared.ProtobufMessages.ServerToClient;

namespace VPVC.BackendCommunication.MessageHandlers; 

public class IncomingWebRtcSignalingMessageHandler: IMessageHandler {
    public void HandleMessage(SessionMessage message) {
        if (message.incomingWebRtcSignalingMessageContent == null) {
            return;
        }

        IncomingWebRtcSignalingMessageContent content = message.incomingWebRtcSignalingMessageContent;

        if (content.sendingParticipantId != null && content.signalingMessageType != null && content.sdpContent != null) {
            PartyEventListeners.incomingWebRtcSignaling?.Invoke(
                content.sendingParticipantId,
                content.signalingMessageType,
                content.sdpContent
            );
        }
    }
}