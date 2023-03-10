using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using NetCoreServer;
using ProtoBuf;
using VPVC.BackendCommunication.Shared.ProtobufMessages;
using VPVC.MainInternals;

namespace VPVC.BackendCommunication; 

public class SessionClient: WssClient {
    public SessionClient(SslContext context, DnsEndPoint endPoint) : base(context, endPoint) { }
    public SessionClient(SslContext context, IPAddress ipAddress, int port) : base(context, ipAddress, port) { }

    public override void OnWsConnecting(HttpRequest request) {
        request.SetBegin("GET", "/");
        request.SetHeader("Host", Config.backendServerHostname);
        request.SetHeader("Upgrade", "websocket");
        request.SetHeader("Origin", "http://localhost");
        request.SetHeader("Connection", "Upgrade");
        request.SetHeader("Cache-Control", "no-cache");
        request.SetHeader("Pragma", "no-cache");
        request.SetHeader("Sec-WebSocket-Key", Convert.ToBase64String(WsNonce));
        request.SetHeader("Sec-WebSocket-Version", "13");
        request.SetHeader("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/109.0.0.0 Safari/537.36");
        request.SetBody();
    }

    public override void OnWsConnected(HttpResponse response) {
        Logger.Log("Connected to backend.");
        
        ConnectionManager.isConnected = true;
        
        App.RunInForeground(() => ConnectionEventListeners.connected?.Invoke());
    }

    protected override void OnDisconnected() {
        Logger.Log("Disconnected from backend.");
        
        ConnectionManager.isConnected = false;
        
        App.RunInForeground(() => {
            ConnectionEventListeners.disconnected?.Invoke();
            
            PartyEventListeners.partyCreateResult = null;
            PartyEventListeners.partyJoinResult = null;
            PartyEventListeners.partyParticipantsChange = null;
            PartyEventListeners.partyParticipantStatesUpdate = null;
            PartyEventListeners.incomingWebRtcSignaling = null;
            PartyEventListeners.changeTeamResult = null;
        
            ManagedEventListeners.partyCreateFailed = null;
            ManagedEventListeners.partyJoinFailed = null;
            ManagedEventListeners.partyCreateOrJoinSuccess = null;
            ManagedEventListeners.partyParticipantsChanged = null;
            ManagedEventListeners.partyParticipantStatesUpdate = null;
            ManagedEventListeners.teamChanged = null;
        
            ConnectionEventListeners.connected = null;
            ConnectionEventListeners.disconnected = null;
        });
    }

    protected override void OnError(SocketError error) {
        Logger.Log($"Socket error: {error}");
    }

    public override void OnWsReceived(byte[] buffer, long offset, long size) {
        try {
            SessionMessage message = Serializer.DeserializeWithLengthPrefix<SessionMessage>(new MemoryStream(buffer), PrefixStyle.Base128, 1);

            MessageReceiver.MessageReceived(message);
        } catch (Exception exception) {
            Logger.Log(exception.ToString());
        }
    }
}