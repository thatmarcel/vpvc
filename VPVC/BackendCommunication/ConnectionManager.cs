using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using NetCoreServer;

namespace VPVC.BackendCommunication; 

public class ConnectionManager {
    public static SessionClient? sessionClient;
    
    public static bool isConnected = false;
    
    public static void Connect() {
        Logger.Log("Connecting to backend...");
        
        var sslContext = new SslContext();

        var dnsEndPoint = new DnsEndPoint(Config.backendServerHostname, Config.backendServerPort, AddressFamily.InterNetwork);
        
        sessionClient = new(sslContext, dnsEndPoint);
        sessionClient.ConnectAsync();
    }

    public static void Disconnect() {
        if (sessionClient != null && sessionClient.IsConnected) {
            sessionClient.Disconnect();
        }
    }
}