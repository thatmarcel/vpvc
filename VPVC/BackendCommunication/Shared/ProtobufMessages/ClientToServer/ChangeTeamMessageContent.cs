using ProtoBuf;

namespace VPVC.BackendCommunication.Shared.ProtobufMessages.ClientToServer; 

[ProtoContract]
public class ChangeTeamMessageContent {
    [ProtoMember(1)]
    public int newTeamIndex { get; set; }
}