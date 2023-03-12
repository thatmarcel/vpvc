using System.Text;
using System.Threading;
using LiteNetLib;
using VPVC.MainInternals;

namespace VPVC.VoiceChat;

public delegate void VoiceChatBackendClientEmptyCallback();
public delegate void VoiceChatBackendClientBufferCallback(string senderId, byte[] buffer);

public class VoiceChatBackendClient {
    private EventBasedNetListener listener;
    private NetManager client;

    private bool shouldStop = false;
    private bool isMatchedWithTarget = false;

    public VoiceChatBackendClientEmptyCallback? onConnected;
    public VoiceChatBackendClientEmptyCallback? onDisconnected;
    public VoiceChatBackendClientBufferCallback? onBufferReceived;

    public VoiceChatBackendClient() {
        listener = new EventBasedNetListener();
        client = new NetManager(listener) {
            UnsyncedEvents = true,
            UnsyncedReceiveEvent = true,
            AutoRecycle = true
        };
    }

    public void Connect() {
        listener.PeerConnectedEvent += peer => OnConnected();
        listener.PeerDisconnectedEvent += (peer, disconnectInfo) => OnDisconnected();

        listener.NetworkReceiveEvent += (fromPeer, dataReader, deliveryMethod) => {
            if (!isMatchedWithTarget) {
                isMatchedWithTarget = dataReader.AvailableBytes > 0;
                return;
            }

            var senderId = dataReader.GetString(4);

            var receivedBytes = dataReader.GetRemainingBytes();

            if (receivedBytes == null) {
                return;
            }

            onBufferReceived?.Invoke(senderId, receivedBytes);
        };
        
        client.Start();
        client.Connect(Config.voiceChatBackendServerHostname, Config.voiceChatBackendServerPort, "VPVC-Voice-Chat");
    }

    public void DisconnectAndStop() {
        shouldStop = true;

        client.Stop();
    }

    private void OnDisconnected() {
        isMatchedWithTarget = false;
        
        if (shouldStop) {
            onDisconnected?.Invoke();
        } else {
            client.Stop();
            Thread.Sleep(1);
            client.Start();
            client.Connect(Config.voiceChatBackendServerHostname, Config.voiceChatBackendServerPort, "VPVC-Voice-Chat");
        }
    }

    private void OnConnected() {
        var party = PartyManager.currentParty;

        if (party == null) {
            return;
        }
        
        var infoBytes = Encoding.UTF8.GetBytes($"{party.joinCode}:{party.participantSelf.id}");
        client.SendToAll(infoBytes, DeliveryMethod.ReliableOrdered);
        
        onConnected?.Invoke();
    }
    
    public void SendAudioBuffer(byte[] bytes) {
        if (isMatchedWithTarget) {
            client.SendToAll(bytes, DeliveryMethod.Sequenced);
        }
    }
}