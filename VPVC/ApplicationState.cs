namespace VPVC;

public delegate void CurrentFlowStepChangedCallback();
public delegate void PartyOverviewInformationChangedCallback();

public static class ApplicationState {
    public static FlowStep currentFlowStep = FlowStep.BasicUserInformationConfiguration;
    public static CurrentFlowStepChangedCallback? currentFlowStepChanged = null;
    private static void SetCurrentFlowStep(FlowStep newFlowStep) {
        currentFlowStep = newFlowStep;
        currentFlowStepChanged?.Invoke();
    }
    
    
    public static PartyOverviewInformationChangedCallback? partyOverviewInformationChanged = null;

    public static void HandleBasicIntroductionAcknowledged() {
        SetCurrentFlowStep(FlowStep.BasicUserInformationConfiguration);
    }
    
    public static void HandleDebuggingToolsPageRequested() {
        SetCurrentFlowStep(FlowStep.DebuggingToolsPage);
    }
    
    public static string? userDisplayName { get; private set; }
    public static void SetUserDisplayName(string newUserDisplayName) {
        userDisplayName = newUserDisplayName;
        SetCurrentFlowStep(FlowStep.PartyJoinOrCreate);
    }

    public static void HandlePartyJoined() {
        SetCurrentFlowStep(FlowStep.PartyOverview);
    }

    public static void HandleBackendConnectionDisconnected() {
        SetCurrentFlowStep(FlowStep.PartyJoinOrCreate);
    }

    public enum FlowStep {
        BasicIntroduction,
        BasicUserInformationConfiguration,
        PartyJoinOrCreate,
        PartyOverview,
        
        DebuggingToolsPage
    }
}