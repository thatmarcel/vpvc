using System.Runtime.CompilerServices;
using System.Timers;
using VPVC.BackendCommunication.Shared;
using VPVC.MainInternals;

namespace VPVC;

public delegate void DebuggingInformationHelperEmptyCallback();

public static class DebuggingInformationHelper {
    public static DebuggingInformationHelperEmptyCallback? informationHasBeenUpdated;

    public static bool didLastScreenshotTakingCompletetlyFail = false;
    public static bool didLastScreenshotExtractionCompletetlyFail = false;
    
    public static bool hasEverEncounteredExceptionWhenRunningInBackground = false;
    public static bool hasEverEncounteredExceptionWhenRunningInForeground = false;
    public static bool hasEnqueuingInForegroundEverFailed = false;

    public static double lastScreenshotPossibleLobbyBackgroundPixelFraction = -1;
    public static double lastScreenshotPossibleAgentSelectBackgroundPixelFraction = -1;
    public static double lastScreenshotPossibleAgentSelectTimerPixelFraction = -1;

    public static string lastReceivedMessageString = "";

    public static string infoText { get; private set; } = "";

    private static Timer? updateTimer;

    private static void SetInfoText(string newInfoText) {
        infoText = newInfoText;

        App.RunInForeground(() => informationHasBeenUpdated?.Invoke());
    }

    public static void StartUpdating() {
        updateTimer = new Timer();
        updateTimer.Elapsed += (_, _) => UpdateInfoText();
        updateTimer.Interval = 500;
        updateTimer.Start();
    }

    [MethodImpl(MethodImplOptions.NoOptimization | MethodImplOptions.NoInlining)]
    private static void UpdateInfoText() {
        var party = PartyManager.currentParty;
        
        var newInfoText = "";
        
        newInfoText += $"\nDetected game state: {(
            party?.participantSelf.gameState == GameStates.lobby
                ? "Lobby"
                : (
                    party?.participantSelf.gameState == GameStates.agentSelect
                        ? "Agent select"
                        : "In-game"
                )
        )}";

        newInfoText += $"; Last screenshot possible fractions: lobby background ({lastScreenshotPossibleLobbyBackgroundPixelFraction:0.####}), agent select background ({lastScreenshotPossibleAgentSelectBackgroundPixelFraction:0.####}), agent select timer ({lastScreenshotPossibleAgentSelectTimerPixelFraction:0.####})";

        newInfoText += $"; Last received message: {lastReceivedMessageString}";
        
        newInfoText += $"; Last known relative player position X: {party?.participantSelf.relativePositionX}, Y: {party?.participantSelf.relativePositionY}";
        
        newInfoText += $"; Last screenshot taking failed: {didLastScreenshotTakingCompletetlyFail}, extraction failed: {didLastScreenshotExtractionCompletetlyFail}";
        
        newInfoText += $"; Has ever encountered errors in foreground: {hasEverEncounteredExceptionWhenRunningInForeground}, in background: {hasEverEncounteredExceptionWhenRunningInBackground}, enqueuing in foreground failed: {hasEnqueuingInForegroundEverFailed}";
        
        newInfoText += $"\nKnown states and positions of other players:";

        if (party != null) {
            foreach (var partyParticipant in party.otherParticipants) {
                newInfoText += $" {partyParticipant.userDisplayName} (state: {(
                    partyParticipant.gameState == GameStates.lobby
                        ? "Lobby"
                        : (
                            partyParticipant.gameState == GameStates.agentSelect
                                ? "Agent select"
                                : "In-game"
                        )
                )}, X: {partyParticipant.relativePositionX}, Y: {partyParticipant.relativePositionY});";
            }
        }
        
        SetInfoText(newInfoText);
    }
}