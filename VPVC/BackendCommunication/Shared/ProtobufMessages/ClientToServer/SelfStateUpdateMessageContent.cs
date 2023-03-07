using ProtoBuf;

namespace VPVC.BackendCommunication.Shared.ProtobufMessages.ClientToServer; 

[ProtoContract]
public class SelfStateUpdateMessageContent {
    [ProtoMember(1)]
    public int gameState { get; set; }
    
    [ProtoMember(2)]
    public int relativePositionX { get; set; }
    
    [ProtoMember(3)]
    public int relativePositionY { get; set; }
}