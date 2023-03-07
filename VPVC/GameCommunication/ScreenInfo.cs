namespace VPVC.GameCommunication; 

public class ScreenInfo {
    private string deviceId;
    private string name;

    public string DeviceId => deviceId;
    public string Name => name;

    public ScreenInfo(string deviceId, string name) {
        this.deviceId = deviceId;
        this.name = name;
    }
}