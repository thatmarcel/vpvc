using VPVC.BackendCommunication.Shared;
using VPVC.Helpers;

namespace VPVC.MainInternals; 

public class PartyParticipant {
    public string id;
    public string userDisplayName;
    public int teamIndex;
    public bool isPartyLeader;
    
    public int gameState = GameStates.lobby;
    public int relativePositionX= -1;
    public int relativePositionY = -1;

    public PartyParticipant(string id, string userDisplayName, int teamIndex, bool isPartyLeader) {
        this.id = id;
        this.userDisplayName = userDisplayName;
        this.teamIndex = teamIndex;
        this.isPartyLeader = isPartyLeader;
    }

    public void UpdateFromSerializable(SerializablePartyParticipant serializablePartyParticipant) {
        id = serializablePartyParticipant.id;
        userDisplayName = serializablePartyParticipant.userDisplayName;
        teamIndex = serializablePartyParticipant.teamIndex;
        isPartyLeader = serializablePartyParticipant.isPartyLeader;
    }
    
    public void UpdateStateFromSerializable(SerializablePartyParticipantState serializablePartyParticipantState) {
        gameState = serializablePartyParticipantState.gameState;
        relativePositionX = serializablePartyParticipantState.relativePositionX;
        relativePositionY = serializablePartyParticipantState.relativePositionY;
    }

    public static PartyParticipant FromSerializable(SerializablePartyParticipant serializablePartyParticipant) {
        return new PartyParticipant(
            serializablePartyParticipant.id,
            serializablePartyParticipant.userDisplayName,
            serializablePartyParticipant.teamIndex,
            serializablePartyParticipant.isPartyLeader
        );
    }

    public double CalculateDistanceToOtherParticipant(PartyParticipant otherParticipant) {
        var otherRelativePositionX = otherParticipant.relativePositionX;
        var otherRelativePositionY = otherParticipant.relativePositionY;
        
        Logger.Log($"Calculating distance (self x: {relativePositionX}, y: {relativePositionY}, other x: {otherRelativePositionX}, other y: {otherRelativePositionX})");

        if (
            relativePositionX < 0 ||
            relativePositionY < 0 ||
            otherRelativePositionX < 0 ||
            otherRelativePositionY < 0
        ) {
            return -1;
        }
            
        return PointDistance.Calculate(
            relativePositionX,
            relativePositionY,
            otherRelativePositionX,
            otherRelativePositionY
        );
    }
}