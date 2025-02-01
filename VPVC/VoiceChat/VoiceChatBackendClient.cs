using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using LiteNetLib;
using VPVC.Helpers;
using VPVC.MainInternals;
using VPVC.ServerLocations;

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

    public Dictionary<string, long> lastAudioTimestampsForParticipantIds = new();

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

        listener.NetworkReceiveEvent += (fromPeer, dataReader, channel, deliveryMethod) => {
            if (!isMatchedWithTarget) {
                isMatchedWithTarget = dataReader.AvailableBytes > 0;
                return;
            }

            var senderId = dataReader.GetString(4);

            var receivedEncryptedBytes = dataReader.GetRemainingBytes();

            if (senderId == null || receivedEncryptedBytes == null) {
                return;
            }
            
            var party = PartyManager.currentParty;

            if (party == null) {
                return;
            }

            var receivedBytes = EncryptionHelper.Decrypt(receivedEncryptedBytes, party.voiceChatEncryptionKey);

            lastAudioTimestampsForParticipantIds[senderId] = DateTime.Now.ToFileTime();

            onBufferReceived?.Invoke(senderId, receivedBytes);
        };
        
        client.Start();
        client.Connect(ServerLocationsManager.SelectedVoiceChatServerHostname, Config.voiceChatBackendServerPort, "VPVC-Voice-Chat");
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
            client.Connect(ServerLocationsManager.SelectedVoiceChatServerHostname, Config.voiceChatBackendServerPort, "VPVC-Voice-Chat");
        }
    }

    private void OnConnected() {
        var party = PartyManager.currentParty;

        if (party == null) {
            return;
        }
        
        // Make sure the party code isn't directly sent through
        // the potentially insecure voice chat transport channel
        var partyIdentifierConstructionBytes = Encoding.UTF8.GetBytes($"{party.joinCode}:{party.voiceChatEncryptionKey}");
        var partyIdentifierBytes = SHA256.HashData(partyIdentifierConstructionBytes);
        var partyIdentifier = Convert.ToBase64String(partyIdentifierBytes);
        
        var infoBytes = Encoding.UTF8.GetBytes($"{partyIdentifier}:{party.participantSelf.id}");
        client.SendToAll(infoBytes, DeliveryMethod.ReliableOrdered);
        
        onConnected?.Invoke();
    }
    
    public void SendAudioBuffer(byte[] bytes) {
        if (isMatchedWithTarget) {
            var party = PartyManager.currentParty;

            if (party == null) {
                return;
            }

            var encryptedBytes = EncryptionHelper.Encrypt(bytes, party.voiceChatEncryptionKey);
            
            client.SendToAll(encryptedBytes, DeliveryMethod.Sequenced);
        }
    }
}