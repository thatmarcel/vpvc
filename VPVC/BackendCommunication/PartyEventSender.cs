using VPVC.BackendCommunication.Shared;
using VPVC.BackendCommunication.Shared.ProtobufMessages;
using VPVC.BackendCommunication.Shared.ProtobufMessages.ClientToServer;

namespace VPVC.BackendCommunication; 

public static class PartyEventSender {
    public static void SendPartyCreate(string userDisplayName) {
        var content = new PartyCreateMessageContent {
            userDisplayName = userDisplayName
        };
        WithMessageContent.SendPartyCreate(content);
    }
    
    public static void SendPartyJoin(string userDisplayName, string partyJoinCode) {
        var content = new PartyJoinMessageContent {
            partyJoinCode = partyJoinCode,
            userDisplayName = userDisplayName
        };
        WithMessageContent.SendPartyJoin(content);
    }
    
    public static void SendSelfStateUpdate(int gameState, int relativePositionX, int relativePositionY) {
        var content = new SelfStateUpdateMessageContent {
            gameState = gameState,
            relativePositionX = relativePositionX,
            relativePositionY = relativePositionY
        };
        WithMessageContent.SendSelfStateUpdate(content);
    }
    
    public static void SendChangeTeam(int newTeamIndex) {
        var content = new ChangeTeamMessageContent {
            newTeamIndex = newTeamIndex
        };
        WithMessageContent.SendChangeTeam(content);
    }
    
    public static class WithMessageContent {
        public static void SendPartyCreate(PartyCreateMessageContent content) {
            var message = new SessionMessage {
                type = MessageTypes.partyCreate,
                partyCreateMessageContent = content
            };
            MessageSender.SendMessage(message);
        }
    
        public static void SendPartyJoin(PartyJoinMessageContent content) {
            var message = new SessionMessage {
                type = MessageTypes.partyJoin,
                partyJoinMessageContent = content
            };
            MessageSender.SendMessage(message);
        }
    
        public static void SendSelfStateUpdate(SelfStateUpdateMessageContent content) {
            var message = new SessionMessage {
                type = MessageTypes.selfStateUpdate,
                selfStateUpdateMessageContent = content
            };
            MessageSender.SendMessage(message);
        }
    
        public static void SendChangeTeam(ChangeTeamMessageContent content) {
            var message = new SessionMessage {
                type = MessageTypes.changeTeam,
                changeTeamMessageContent = content
            };
            MessageSender.SendMessage(message);
        }
    }
}