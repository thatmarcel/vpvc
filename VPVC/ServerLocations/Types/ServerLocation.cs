using System.Collections.Generic;
using Newtonsoft.Json;

namespace VPVC.ServerLocations.Types; 

public class ServerLocation {
    [JsonProperty(PropertyName = "id")]
    public string identifier = "";
    
    [JsonProperty(PropertyName = "displayName")]
    public string displayName = "";
    
    [JsonProperty(PropertyName = "backendHostname")]
    public string backendHostname = "";
    
    [JsonProperty(PropertyName = "partyJoinCodeLetterPrefix")]
    public string partyJoinCodeLetterPrefix = "";
    
    [JsonProperty(PropertyName = "voiceChatServerHostnames")]
    public List<string> voiceChatServerHostnames = new();
    
    public int latencyInMilliseconds = -1;
    
    // Used in XAML
    // ReSharper disable once UnusedMember.Global
    // ReSharper disable once ConvertToAutoProperty
    public string Identifier => identifier;
    
    // Used in XAML
    // ReSharper disable once UnusedMember.Global
    // ReSharper disable once ConvertToAutoProperty
    public string DisplayName => displayName;
}