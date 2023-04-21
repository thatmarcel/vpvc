using System;
using System.Net;
using System.Net.Sockets;
using NetCoreServer;
using VPVC.ServerLocations;

namespace VPVC.BackendCommunication; 

public class ConnectionManager {
    public static SessionClient? sessionClient;
    
    public static bool isConnected = false;
    
    public static void Connect() {
        Logger.Log("Connecting to backend...");

        try {
            var sslContext = new SslContext();

            var dnsEndPoint = new DnsEndPoint(
                ServerLocationsManager.SelectedBackendServerHostname,
                Config.backendServerPort,
                AddressFamily.InterNetwork
            );

            sessionClient = new(sslContext, dnsEndPoint);
            sessionClient.ConnectAsync();
        } catch (Exception exception) {
            Logger.Log(exception.ToString());
            
            SessionClient.ResetListeners();
        }
    }

    public static void Disconnect() {
        if (sessionClient is { IsConnected: true }) {
            sessionClient.Disconnect();
        }
    }
}