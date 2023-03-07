namespace VPVC.MainInternals;

public delegate void ManagedEventListenerEmptyCallback();

// These should be used in most cases

public static class ManagedEventListeners {
    public static ManagedEventListenerEmptyCallback? partyCreateFailed;
    public static ManagedEventListenerEmptyCallback? partyJoinFailed;
    public static ManagedEventListenerEmptyCallback? partyCreateOrJoinSuccess;
    public static ManagedEventListenerEmptyCallback? partyParticipantsChanged;
    public static ManagedEventListenerEmptyCallback? partyParticipantStatesUpdate;
    public static ManagedEventListenerEmptyCallback? teamChanged;
}