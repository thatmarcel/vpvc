using System.Collections.Generic;
using Newtonsoft.Json;

namespace VPVC.ServerLocations.Types; 

public class ServerLocationsResponse {
    [JsonProperty(PropertyName = "locations")]
    public List<ServerLocation> locations = new();
}