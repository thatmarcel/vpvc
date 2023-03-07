using ProtoBuf;

namespace VPVC.BackendCommunication.Shared.ProtobufMessages.ClientToServer; 

[ProtoContract]
public class PartyCreateMessageContent {
    [ProtoMember(1)]
    public string? userDisplayName { get; set; }
}