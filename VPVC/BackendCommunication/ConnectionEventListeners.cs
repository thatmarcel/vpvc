namespace VPVC.BackendCommunication;

public delegate void ConnectionEventListenerEmptyCallback();

public class ConnectionEventListeners {
    public static ConnectionEventListenerEmptyCallback? connected;
    public static ConnectionEventListenerEmptyCallback? disconnected;
}