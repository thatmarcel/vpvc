using System;
using VPVC.BackendCommunication.Shared.ProtobufMessages;

namespace VPVC.BackendCommunication.Interfaces; 

public interface IMessageHandler {
    public void HandleMessage(SessionMessage message);
}