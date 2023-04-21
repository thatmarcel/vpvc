namespace VPVC.GameCommunication; 

public class ScreenInfo {
    private string deviceId;
    private string name;

    // Used in XAML
    // ReSharper disable once UnusedMember.Global
    // ReSharper disable once ConvertToAutoProperty
    public string DeviceId => deviceId;
    
    // Used in XAML
    // ReSharper disable once UnusedMember.Global
    // ReSharper disable once ConvertToAutoProperty
    public string Name => name;

    public ScreenInfo(string deviceId, string name) {
        this.deviceId = deviceId;
        this.name = name;
    }
}