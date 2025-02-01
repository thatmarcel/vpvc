using ProtoBuf;

namespace VPVC.BackendCommunication.Shared.ProtobufMessages.ServerToClient; 

[ProtoContract]
public class PartyCreateResultMessageContent {
    [ProtoMember(1)]
    public bool success { get; set; }
    
    [ProtoMember(2)]
    public string? partyJoinCode { get; set; }
    
    [ProtoMember(3)]
    public SerializablePartyParticipant? partyParticipantSelf { get; set; }
    
    [ProtoMember(4)]
    public byte[]? voiceChatEncryptionKey { get; set; }
}