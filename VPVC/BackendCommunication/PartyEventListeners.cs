using System.Collections.Generic;
using VPVC.BackendCommunication.Shared;

namespace VPVC.BackendCommunication;

// These should mainly be used by the PartyManager

public delegate void PartyEventListenerPartyCreateResultCallback(bool success, string? partyJoinCode, SerializablePartyParticipant? partyParticipantSelf, byte[]? voiceChatEncryptionKey);
public delegate void PartyEventListenerPartyJoinResultCallback(bool success, SerializablePartyParticipant? partyParticipantSelf, List<SerializablePartyParticipant>? partyParticipants, byte[]? voiceChatEncryptionKey);
public delegate void PartyEventListenerPartyParticipantsChangeCallback(SerializablePartyParticipant partyParticipantSelf, List<SerializablePartyParticipant> partyParticipants);
public delegate void PartyEventListenerPartyParticipantStatesUpdateCallback(List<SerializablePartyParticipantState> partyParticipantStates);
public delegate void PartyEventListenerIncomingWebRtcSignalingCallback(string sendingParticipantId, string signalingMessageType, string sdpContent);
public delegate void PartyEventListenerChangeTeamResultCallback(bool success, int newTeamIndex);

public static class PartyEventListeners {
    public static PartyEventListenerPartyCreateResultCallback? partyCreateResult;
    public static PartyEventListenerPartyJoinResultCallback? partyJoinResult;
    public static PartyEventListenerPartyParticipantsChangeCallback? partyParticipantsChange;
    public static PartyEventListenerPartyParticipantStatesUpdateCallback? partyParticipantStatesUpdate;
    public static PartyEventListenerIncomingWebRtcSignalingCallback? incomingWebRtcSignaling;
    public static PartyEventListenerChangeTeamResultCallback? changeTeamResult;
}