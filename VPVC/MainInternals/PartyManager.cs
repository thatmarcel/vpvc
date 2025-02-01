using System.Collections.Generic;
using System.Linq;
using VPVC.BackendCommunication;
using VPVC.BackendCommunication.Shared;
using VPVC.VoiceChat;

namespace VPVC.MainInternals; 

public static class PartyManager {
    private static string? joinCodeOfPartyAttemptingToJoin;

    public static Party? currentParty;

    public static void ConnectAndCreateParty() {
        if (ApplicationState.userDisplayName == null) {
            return;
        }
        
        if (ConnectionManager.isConnected) {
            CreateParty();
        } else {
            ConnectionEventListeners.connected += CreateParty;
            
            ConnectionManager.Connect();
        }
    }
    
    public static void ConnectAndJoinParty(string partyJoinCode) {
        if (ApplicationState.userDisplayName == null) {
            return;
        }
        
        if (ConnectionManager.isConnected) {
            JoinParty(partyJoinCode);
        } else {
            ConnectionEventListeners.connected += () => JoinParty(partyJoinCode);
            
            ConnectionManager.Connect();
        }
    }

    public static void CreateParty() {
        if (ApplicationState.userDisplayName == null) {
            return;
        }
        
        PartyEventListeners.partyCreateResult += HandlePartyCreateResult;
        PartyEventSender.SendPartyCreate(ApplicationState.userDisplayName);
    }
    
    public static void JoinParty(string partyJoinCode) {
        if (ApplicationState.userDisplayName == null) {
            return;
        }
        
        PartyEventListeners.partyJoinResult += HandlePartyJoinResult;
        joinCodeOfPartyAttemptingToJoin = partyJoinCode;
        PartyEventSender.SendPartyJoin(ApplicationState.userDisplayName, partyJoinCode);
    }

    private static void HandlePartyCreateResult(bool success, string? partyJoinCode, SerializablePartyParticipant? partyParticipantSelf, byte[]? voiceChatEncryptionKey) {
        PartyEventListeners.partyCreateResult -= HandlePartyCreateResult;

        if (!success || partyJoinCode == null || partyParticipantSelf == null || voiceChatEncryptionKey == null) {
            ManagedEventListeners.partyCreateFailed?.Invoke();
            ConnectionManager.Disconnect();
            return;
        }

        currentParty = new Party(
            partyJoinCode,
            PartyParticipant.FromSerializable(partyParticipantSelf),
            null,
            voiceChatEncryptionKey
        );

        HandleSuccessfulPartyJoin();
    }

    private static void HandlePartyJoinResult(bool success, SerializablePartyParticipant? partyParticipantSelf, List<SerializablePartyParticipant>? partyParticipants, byte[]? voiceChatEncryptionKey) {
        PartyEventListeners.partyJoinResult -= HandlePartyJoinResult;

        if (!success || partyParticipantSelf == null || partyParticipants == null || joinCodeOfPartyAttemptingToJoin == null || voiceChatEncryptionKey == null) {
            ManagedEventListeners.partyJoinFailed?.Invoke();
            ConnectionManager.Disconnect();
            return;
        }

        currentParty = new Party(
            joinCodeOfPartyAttemptingToJoin,
            PartyParticipant.FromSerializable(partyParticipantSelf),
            partyParticipants.Select(PartyParticipant.FromSerializable).ToList(),
            voiceChatEncryptionKey
        );

        joinCodeOfPartyAttemptingToJoin = null;

        HandleSuccessfulPartyJoin();
    }

    private static void HandleSuccessfulPartyJoin() {
        PartyEventListeners.partyParticipantsChange += HandlePartyParticipantsChange;
        PartyEventListeners.partyParticipantStatesUpdate += HandlePartyParticipantStatesUpdate;
        PartyEventListeners.changeTeamResult += HandleChangeTeamResult;

        ConnectionEventListeners.disconnected += HandleBackendConnectionDisconnected;
        
        ApplicationState.HandlePartyJoined();
        
        ManagedEventListeners.partyCreateOrJoinSuccess?.Invoke();
        
        ApplicationState.partyOverviewInformationChanged?.Invoke();
        
        VoiceChatManager.Start();
    }

    private static void HandleBackendConnectionDisconnected() {
        currentParty = null;
        
        ApplicationState.HandleBackendConnectionDisconnected();
    }

    private static void HandlePartyParticipantsChange(SerializablePartyParticipant partyParticipantSelf, List<SerializablePartyParticipant> partyParticipants) {
        if (currentParty == null) {
            return;
        }
        
        currentParty.participantSelf.UpdateFromSerializable(partyParticipantSelf);
        
        foreach (var updatedParticipant in partyParticipants) {
            var partyParticipant = currentParty.otherParticipants.FirstOrDefault(participant => participant.id == updatedParticipant.id);

            if (partyParticipant == null) {
                currentParty.otherParticipants.Add(PartyParticipant.FromSerializable(updatedParticipant));
                continue;
            }
            
            partyParticipant.UpdateFromSerializable(updatedParticipant);
        }

        foreach (var partyParticipant in new List<PartyParticipant>(currentParty.otherParticipants)) {
            var updatedParticipant = partyParticipants.FirstOrDefault(participant => participant.id == partyParticipant.id);

            if (updatedParticipant == null) {
                currentParty.otherParticipants.Remove(partyParticipant);
            }
        }
        
        ApplicationState.partyOverviewInformationChanged?.Invoke();
        ManagedEventListeners.partyParticipantsChanged?.Invoke();
    }

    private static void HandlePartyParticipantStatesUpdate(List<SerializablePartyParticipantState> partyParticipantStates) {
        if (currentParty == null) {
            return;
        }
        
        foreach (var partyParticipant in currentParty.otherParticipants) {
            var participantState = partyParticipantStates.FirstOrDefault(participantState => participantState.participantId == partyParticipant.id);

            if (participantState == null) {
                continue;
            }
            
            partyParticipant.UpdateStateFromSerializable(participantState);
        }
        
        ManagedEventListeners.partyParticipantStatesUpdate?.Invoke();
    }

    private static void HandleChangeTeamResult(bool success, int newTeamIndex) {
        if (success && currentParty != null) {
            currentParty.participantSelf.teamIndex = newTeamIndex;
        }
        
        ManagedEventListeners.teamChanged?.Invoke();
        
        ApplicationState.partyOverviewInformationChanged?.Invoke();
    }
}