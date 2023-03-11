using System.Collections.Generic;
using System.Linq;
using VPVC.BackendCommunication;
using VPVC.BackendCommunication.Shared;
using VPVC.MainInternals;

namespace VPVC.VoiceChat; 

public static class VoiceChatManager {
    private static readonly Dictionary<string, NewWindowsAudioEndpoint> audioEndpoints = new();

    private static VoiceChatBackendClient? voiceChatBackendClient;

    private static NewWindowsAudioEndpoint? microphoneAudioEndpoint;

    public static void Start() {
        App.RunInBackground(StartSync);
    }

    private static void StartSync() {
        microphoneAudioEndpoint = new NewWindowsAudioEndpoint(true, false);
        
        voiceChatBackendClient = new VoiceChatBackendClient();

        voiceChatBackendClient.onConnected += () => {
            microphoneAudioEndpoint?.StartAudio();
        };
        voiceChatBackendClient.onDisconnected += Stop;

        voiceChatBackendClient.onBufferReceived += (senderId, buffer) => {
            NewWindowsAudioEndpoint? senderAudioEndpoint;
            
            if (!audioEndpoints.ContainsKey(senderId)) {
                senderAudioEndpoint = new NewWindowsAudioEndpoint(false, true);
                
                senderAudioEndpoint.StartAudio();

                audioEndpoints[senderId] = senderAudioEndpoint;
            } else {
                senderAudioEndpoint = audioEndpoints[senderId];
            }
            
            senderAudioEndpoint.GotAudioRtp(buffer);
        };

        microphoneAudioEndpoint.hasNewSamples += samples => voiceChatBackendClient?.SendAudioBuffer(samples);

        ConnectionEventListeners.disconnected += Stop;

        ManagedEventListeners.partyParticipantStatesUpdate += HandlePartyParticipantStatesUpdate;
        ManagedEventListeners.partyParticipantsChanged += HandlePartyParticipantsChanged;

        voiceChatBackendClient?.Connect();
    }

    public static void Stop() {
        foreach (var audioEndpointPair in audioEndpoints) {
            audioEndpointPair.Value.CloseAudio();
            audioEndpoints.Remove(audioEndpointPair.Key);
        }
        
        ConnectionEventListeners.disconnected -= Stop;

        ManagedEventListeners.partyParticipantStatesUpdate -= HandlePartyParticipantStatesUpdate;
        ManagedEventListeners.partyParticipantsChanged -= HandlePartyParticipantsChanged;
        
        microphoneAudioEndpoint?.CloseAudio();

        voiceChatBackendClient?.DisconnectAndStop();
        voiceChatBackendClient = null;
    }
    
    private static void HandlePartyParticipantsChanged() {
        var updatedParticipants = PartyManager.currentParty?.otherParticipants;

        if (updatedParticipants == null) {
            return;
        }

        foreach (var audioEndpointPair in audioEndpoints) {
            var partyParticipant = updatedParticipants.FirstOrDefault(participant => participant.id == audioEndpointPair.Key);

            if (partyParticipant == null) {
                audioEndpointPair.Value.CloseAudio();
                audioEndpoints.Remove(audioEndpointPair.Key);
            }
        }
    }

    private static void HandlePartyParticipantStatesUpdate() {
        var party = PartyManager.currentParty;

        if (party == null) {
            return;
        }
        
        foreach (var partyParticipant in party.otherParticipants) {
            if (!audioEndpoints.ContainsKey(partyParticipant.id)) {
                continue;
            }

            var participantAudioEndpoint = audioEndpoints[partyParticipant.id];
            
            HandlePartyParticipantStateUpdate(party, partyParticipant, participantAudioEndpoint);
        }
    }
    
    private static void HandlePartyParticipantStateUpdate(Party party, PartyParticipant partyParticipant, NewWindowsAudioEndpoint participantAudioEndpoint) {
        if (partyParticipant.gameState == GameStates.lobby) {
            SetParticipantAudioVolume(1f, participantAudioEndpoint);
        } else if (partyParticipant.gameState == GameStates.agentSelect) {
            SetParticipantAudioVolume(
                party.participantSelf.teamIndex == partyParticipant.teamIndex
                    ? 1f
                    : 0f,
                participantAudioEndpoint
            );
        } else if (partyParticipant.gameState == GameStates.inGame) {
            var distance = party.participantSelf.CalculateDistanceToOtherParticipant(partyParticipant);

            if (distance < 0) {
                Logger.Log($"Participant distance under 0 (name: {distance}).");
                return;
            }

            if (distance < Config.fullVolumeHearingRadius) {
                SetParticipantAudioVolume(1f, participantAudioEndpoint);
                return;
            }

            if (distance > Config.maxHearingRadius) {
                SetParticipantAudioVolume(0f, participantAudioEndpoint);
                return;
            }

            var distanceOutsideOfFullVolumeRadius = distance - Config.fullVolumeHearingRadius;
            var radiusOutsideOfFullVolumeRadius = Config.maxHearingRadius - Config.fullVolumeHearingRadius;

            var volume = (radiusOutsideOfFullVolumeRadius - distanceOutsideOfFullVolumeRadius) / radiusOutsideOfFullVolumeRadius;
            
            SetParticipantAudioVolume((float) volume, participantAudioEndpoint);
        }
    }
    
    private static void SetParticipantAudioVolume(float volumeFraction, NewWindowsAudioEndpoint participantAudioEndpoint) {
        participantAudioEndpoint.SetOutputVolume(volumeFraction);

        /* if (volumeFraction == 0f) {
            participantAudioEndpoint.PauseAudio();
        } else {
            participantAudioEndpoint.ResumeAudio();
        } */
    }
}