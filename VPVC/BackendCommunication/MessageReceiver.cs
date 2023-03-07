using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.UI.Dispatching;
using VPVC.BackendCommunication.Interfaces;
using VPVC.BackendCommunication.MessageHandlers;
using VPVC.BackendCommunication.Shared;
using VPVC.BackendCommunication.Shared.ProtobufMessages;

namespace VPVC.BackendCommunication; 

// Class handling the forwarding of received messages to the correct message handler
// (messages are queued and processed one after another instead of directly when received
// to prevent problems caused by two messages being processed at the same time and both making
// modifications to stored variables which can e.g. lead to duplicate entries in lists)
public static class MessageReceiver {
    private static readonly Dictionary<int, IMessageHandler> messageHandlers = new() {
        { MessageTypes.partyCreateResult, new PartyCreateResultMessageHandler() },
        { MessageTypes.partyJoinResult, new PartyJoinResultMessageHandler() },
        { MessageTypes.partyParticipantsChange, new PartyParticipantsChangeMessageHandler() },
        { MessageTypes.partyParticipantStatesUpdate, new PartyParticipantStatesUpdateMessageHandler() },
        { MessageTypes.incomingWebRtcSignalingMessage, new IncomingWebRtcSignalingMessageHandler() },
        { MessageTypes.changeTeamResult, new ChangeTeamResultMessageHandler() }
    };

    // Stores the list of messages that have yet to be processed
    private static readonly BlockingCollection<SessionMessage> messageQueue = new();

    // When a message is received, add it to the queue for processing
    public static void MessageReceived(SessionMessage message) {
        messageQueue.Add(message);
    }

    // Process the next message in the queue
    public static void ProcessNextMessage() {
        var nextMessageInfo = messageQueue.Take();

        if (messageHandlers.ContainsKey(nextMessageInfo.type)) {
            DebuggingInformationHelper.lastReceivedMessageString = JsonSerializer.Serialize(nextMessageInfo);
            App.RunInForeground(() => messageHandlers[nextMessageInfo.type].HandleMessage(nextMessageInfo));
        }
    }
}