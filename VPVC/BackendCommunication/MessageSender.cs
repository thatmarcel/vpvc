using System;
using System.IO;
using System.Threading;
using ProtoBuf;
using VPVC.BackendCommunication.Shared.ProtobufMessages;

namespace VPVC.BackendCommunication; 

// Class handling encoding and sending messages to clients
public static class MessageSender {
    public static void SendMessage(SessionMessage message) {
        App.RunInBackground(() => {
            byte[] serializedMessageBytes;

            using (var stream = new MemoryStream()) {
                // Serialize the message to ProtoBuf data with a length prefix
                // as without one, de-serializing the data doesn't work
                // (presumably because of the padding added by the TCP socket library?)
                Serializer.SerializeWithLengthPrefix(stream, message, PrefixStyle.Base128, 1);
                serializedMessageBytes = stream.ToArray();
            }
            
            ConnectionManager.sessionClient?.SendBinary(serializedMessageBytes);
        });
    }
}