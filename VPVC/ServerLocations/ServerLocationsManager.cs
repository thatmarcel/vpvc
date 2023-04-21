using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using Newtonsoft.Json;
using VPVC.ServerLocations.Types;

namespace VPVC.ServerLocations;

public delegate void ServerLocationsManagerPrepareCallback(bool success);
public delegate void ServerLocationsOptionalCallback(List<ServerLocation>? serverLocations);
public delegate void ServerLocationsCallback(List<ServerLocation> serverLocations);

public static class ServerLocationsManager {
    public static List<ServerLocation>? serverLocations;
    public static ServerLocationsCallback? onServerLocationsChanged;

    public static ServerLocation? selectedServerLocation;

    public static string SelectedBackendServerHostname => selectedServerLocation?.backendHostname ?? "";
    public static string SelectedVoiceChatServerHostname => selectedServerLocation?.voiceChatServerHostnames.FirstOrDefault() ?? "";
    public static string SelectedLocationPartyJoinCodeLetterPrefix => selectedServerLocation?.partyJoinCodeLetterPrefix ?? "";
    
    public static void Prepare(ServerLocationsManagerPrepareCallback completion) {
        FetchServerLocations(receivedServerLocations => {
            if (receivedServerLocations == null) {
                App.RunInForeground(() => completion(false));
                return;
            }
            
            SortLocationsByLatency(receivedServerLocations, sortedServerLocations => {
                serverLocations = sortedServerLocations;

                App.RunInForeground(() => {
                    onServerLocationsChanged?.Invoke(sortedServerLocations);
                    
                    completion(true);
                });
            });
        });
    }

    public static ServerLocation? FindServerLocationForPartyJoinCodeLetterPrefix(string prefix) {
        return serverLocations?.FirstOrDefault(s => s.partyJoinCodeLetterPrefix == prefix);
    }
    
    private static void FetchServerLocations(ServerLocationsOptionalCallback completion) {
        // ReSharper disable once AsyncVoidLambda
        App.RunInBackground(async () => {
            try {
                using var httpClient = new HttpClient(
                    new HttpClientHandler {
                        AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
                    }
                );

                httpClient.BaseAddress = new Uri("https://files.vpvc.app/");
                var response = await httpClient.GetAsync("server-locations.json");

                if (!response.IsSuccessStatusCode) {
                    completion(null);
                    return;
                }

                var responseString = await response.Content.ReadAsStringAsync();

                var responseObject = JsonConvert.DeserializeObject<ServerLocationsResponse>(responseString);

                completion(responseObject?.locations);
            } catch (Exception exception) {
                Logger.Log(exception.ToString());

                completion(null);
            }
        });
    }

    private static void SortLocationsByLatency(List<ServerLocation> serverLocationsToSort, ServerLocationsCallback completion) {
        int waitingPingCount = serverLocationsToSort.Count;
        
        foreach (var serverLocation in serverLocationsToSort) {
            App.RunInBackground(() => {
                var pingSender = new Ping();
                var pingReply = pingSender.Send(serverLocation.backendHostname, 2500 /* timeout in ms */);
                
                if (pingReply.Status == IPStatus.Success) {
                    serverLocation.latencyInMilliseconds = (int) pingReply.RoundtripTime;
                }

                waitingPingCount--;

                if (waitingPingCount < 1) {
                    completion(serverLocationsToSort.OrderBy(s => s.latencyInMilliseconds).ToList());
                }
            });
        }
    }
}