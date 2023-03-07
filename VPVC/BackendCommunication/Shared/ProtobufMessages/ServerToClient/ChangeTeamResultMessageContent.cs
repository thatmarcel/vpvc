using ProtoBuf;

namespace VPVC.BackendCommunication.Shared.ProtobufMessages.ServerToClient; 

[ProtoContract]
public class ChangeTeamResultMessageContent {
    [ProtoMember(1)]
    public bool success { get; set; }
    
    [ProtoMember(2)]
    public int newTeamIndex { get; set; }
}