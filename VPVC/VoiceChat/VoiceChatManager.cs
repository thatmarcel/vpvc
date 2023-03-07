using System.Collections.Generic;
using System.Threading.Tasks;
// using SIPSorcery.Media;
// using SIPSorceryMedia.Windows;
using VPVC.BackendCommunication;
using VPVC.MainInternals;

namespace VPVC.VoiceChat; 

public static class VoiceChatManager {
    private static Dictionary<string, VoiceChatConnection> connections = new();

    public static void Start() {
        App.RunInBackground(StartSync);
    }

    private static void StartSync() {
        ConnectionEventListeners.disconnected += Stop;

        PartyEventListeners.incomingWebRtcSignaling += HandleIncomingWebRtcSignaling;

        ManagedEventListeners.partyParticipantStatesUpdate += HandlePartyParticipantStatesUpdate;

        ConnectToPartyParticipants();
    }

    public static void Stop() {
        foreach (var connectionPair in connections) {
            connectionPair.Value.Disconnect();

            connections.Remove(connectionPair.Key);
        }
        
        ConnectionEventListeners.disconnected -= Stop;

        PartyEventListeners.incomingWebRtcSignaling -= HandleIncomingWebRtcSignaling;

        ManagedEventListeners.partyParticipantStatesUpdate -= HandlePartyParticipantStatesUpdate;
    }

    private static void ConnectToPartyParticipants() {
        var party = PartyManager.currentParty;

        if (party == null) {
            return;
        }
        
        foreach (var partyParticipant in party.otherParticipants) {
            var newConnection = new VoiceChatConnection(partyParticipant.id);

            connections[partyParticipant.id] = newConnection;
            
            newConnection.Connect(true);
        }
    }

    private static void HandleIncomingWebRtcSignaling(string sendingParticipantId, string signalingMessageType, string sdpContent) {
        var party = PartyManager.currentParty;

        if (party == null) {
            return;
        }

        if (!connections.ContainsKey(sendingParticipantId)) {
            var newConnection = new VoiceChatConnection(sendingParticipantId);

            connections[sendingParticipantId] = newConnection;
            
            newConnection.Connect(false);
        }

        var connection = connections[sendingParticipantId];
        
        connection.HandleIncomingWebRtcSignaling(signalingMessageType, sdpContent);
    }

    private static void HandlePartyParticipantStatesUpdate() {
        var party = PartyManager.currentParty;

        if (party == null) {
            return;
        }
        
        foreach (var partyParticipant in party.otherParticipants) {
            if (!connections.ContainsKey(partyParticipant.id)) {
                continue;
            }

            var connection = connections[partyParticipant.id];
            
            connection.HandlePartyParticipantStateUpdate(party, partyParticipant);
        }
    }
}