using System;
using System.Runtime.CompilerServices;
using VPVC.BackendCommunication;
using VPVC.BackendCommunication.Shared;
using VPVC.MainInternals;

namespace VPVC.GameCommunication;

public static class GameStateAndCoordinatesExtractor {
    private static System.Timers.Timer? extractionTimer;

    public static int overridenGameState = -1;
    public static int overridenRelativeCoordinatesX = -1;
    public static int overridenRelativeCoordinatesY = -1;
    
    public static void StartRepeatedExtraction() {
        extractionTimer = new System.Timers.Timer();
        extractionTimer.Elapsed += (_, _) => {
            try {
                Execute();
            } catch (Exception exception) {
                Logger.Log(exception.ToString());
            }
        };
        extractionTimer.Interval = Config.gameCoordinateExtractionIntervalInMilliseconds;
        extractionTimer.Start();
    }
    
    private static void Execute() {
        if (PartyManager.currentParty == null) {
            return;
        }

        if (
            overridenGameState is >= 0 and <= 2 &&
            overridenRelativeCoordinatesX is >= 0 and <= 100 &&
            overridenRelativeCoordinatesY is >= 0 and <= 100
        ) {
            PartyManager.currentParty.participantSelf.gameState = overridenGameState;
            PartyManager.currentParty.participantSelf.relativePositionX = overridenRelativeCoordinatesX;
            PartyManager.currentParty.participantSelf.relativePositionY = overridenRelativeCoordinatesY;
            
            if (overridenGameState == GameStates.inGame) {
                SendUpdate(overridenGameState, new Tuple<int, int>(overridenRelativeCoordinatesX, overridenRelativeCoordinatesY));
            } else {
                SendUpdate(overridenGameState, null);
            }
            
            return;
        }
        
        var screenBitmap = ScreenHelper.TakeScreenshot();

        if (screenBitmap == null) {
            DebuggingInformationHelper.didLastScreenshotTakingCompletetlyFail = true;
            return;
        }
        
        DebuggingInformationHelper.didLastScreenshotTakingCompletetlyFail = false;

        var extractedGameStateAndRelativePlayerPosition = ScreenshotProcessing.ExtractGameStateAndRelativePlayerPosition(screenBitmap);
        
        screenBitmap.Dispose();

        if (extractedGameStateAndRelativePlayerPosition == null) {
            DebuggingInformationHelper.didLastScreenshotExtractionCompletetlyFail = true;
            // Logger.Log("Extraction completely failed.");
            return;
        }
        
        DebuggingInformationHelper.didLastScreenshotExtractionCompletetlyFail = false;

        var extractedGameState = extractedGameStateAndRelativePlayerPosition.Item1;

        var extractedRelativePlayerPosition = extractedGameStateAndRelativePlayerPosition.Item2;
        
        var party = PartyManager.currentParty;
        
        // The party may have been modified by another thread during
        // screenshot processing?
        // ReSharper disable once ConditionIsAlwaysTrueOrFalse
        if (party == null) {
            Logger.Log($"Got game state: {extractedGameState} but party is null.");
            return;
        }
        
        party.participantSelf.gameState = extractedGameState;

        if (extractedRelativePlayerPosition == null) {
            SendUpdate(extractedGameState, null);
            
            // Logger.Log($"Got game state: {extractedGameState} without positions.");
            
            return;
        }

        // Positions from enemy teams need to be inverted as the map is rotated (on all maps afaik, incl. Fracture) so the
        // player's team's spawn is at the bottom.
        if (party.participantSelf.teamIndex != 0) {
            extractedRelativePlayerPosition = new Tuple<int, int>(
                100 - extractedRelativePlayerPosition.Item1,
                100 - extractedRelativePlayerPosition.Item2
            );
        }
        
        // Logger.Log($"Got game state: {extractedGameState} with positions ({extractedRelativePlayerPosition}) and sending update.");
        
        party.participantSelf.relativePositionX = extractedRelativePlayerPosition.Item1;
        party.participantSelf.relativePositionY = extractedRelativePlayerPosition.Item2;
        
        SendUpdate(extractedGameState, extractedRelativePlayerPosition);
    }
    
    private static void SendUpdate(int gameState, Tuple<int, int>? relativePlayerPosition) {
        if (relativePlayerPosition != null) {
            PartyEventSender.SendSelfStateUpdate(gameState, relativePlayerPosition.Item1, relativePlayerPosition.Item2);
        } else {
            if (gameState == GameStates.inGame) {
                return;
            }
            
            PartyEventSender.SendSelfStateUpdate(gameState, -1, -1);
        }
    }
}