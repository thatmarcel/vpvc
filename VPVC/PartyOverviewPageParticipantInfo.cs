using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.UI.Xaml;
using VPVC.VoiceChat;

namespace VPVC; 

public class PartyOverviewPageParticipantInfo: INotifyPropertyChanged {
    public string id;
    public string displayName;
    public bool isSelf;
    public double volume;
    private bool __isTalking = false;
    public bool isTalking {
        get => __isTalking;
        set {
            __isTalking = value;
            OnPropertyChanged(nameof(TalkingLabelText));
        }
    }

    public string DisplayName => displayName;
    public int VolumeInPercent {
        get => (int) (volume * 100);
        set {
            volume = value / 100d;
            OnPropertyChanged();
            HandleVolumeChanged();
        }
    }
    public string TalkingLabelText => isTalking ? "Talking" : "Not talking";

    public PartyOverviewPageParticipantInfo(string id, string displayName, bool isSelf, double volume) {
        this.id = id;
        this.displayName = displayName;
        this.isSelf = isSelf;
        this.volume = volume;
    }
    
    public void HandleVolumeChanged() {
        VoiceChatManager.SetMaxVolumeForParticipantWithId(id, volume);
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null) {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}