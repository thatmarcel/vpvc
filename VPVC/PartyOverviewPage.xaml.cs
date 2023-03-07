using System;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using VPVC.BackendCommunication;
using VPVC.MainInternals;

namespace VPVC; 

public sealed partial class PartyOverviewPage: Page {
    public readonly ObservableCollection<string> teamOnePlayerNames = new();
    public readonly ObservableCollection<string> teamTwoPlayerNames = new();
    private int currentTeamIndex = 0;
    
    public PartyOverviewPage() {
        InitializeComponent();
        
        UpdateTeamListAccessoryVisibilities();

        ApplicationState.partyOverviewInformationChanged += HandlePartyOverviewInformationChanged;

        DebuggingInformationHelper.informationHasBeenUpdated += () => {
            try {
                debuggingInformationTextBlock.Text = DebuggingInformationHelper.infoText;
            } catch (Exception) {}
        };
    }

    private void HandlePartyOverviewInformationChanged() {
        var party = PartyManager.currentParty;
        
        if (party == null) {
            return;
        }

        joinCodeTextBlock.Text = party.joinCode;

        foreach (var participant in party.otherParticipants) {
            Logger.Log(participant.userDisplayName + ", team: " + participant.teamIndex);
        }

        var newTeamOnePlayerNames = party.otherParticipants
            .Where(participant => participant.teamIndex == 0)
            .Select(participant => $"{participant.userDisplayName}{(participant.isPartyLeader ? " (leader)" : "")}")
            .ToList();
        
        var newTeamTwoPlayerNames = party.otherParticipants
            .Where(participant => participant.teamIndex == 1)
            .Select(participant => $"{participant.userDisplayName}{(participant.isPartyLeader ? " (leader)" : "")}")
            .ToList();

        currentTeamIndex = party.participantSelf.teamIndex;

        if (currentTeamIndex == 0) {
            newTeamOnePlayerNames.Add(
                party.participantSelf.isPartyLeader
                    ? $"{party.participantSelf.userDisplayName} (you, leader)"
                    : $"{party.participantSelf.userDisplayName} (you)"
            );
        } else {
            newTeamTwoPlayerNames.Add(
                party.participantSelf.isPartyLeader
                    ? $"{party.participantSelf.userDisplayName} (you, leader)"
                    : $"{party.participantSelf.userDisplayName} (you)"
            );
        }

        teamOnePlayerNames.ToList().All(playerName => teamOnePlayerNames.Remove(playerName));
        teamTwoPlayerNames.ToList().All(playerName => teamTwoPlayerNames.Remove(playerName));

        newTeamOnePlayerNames.Sort();
        newTeamTwoPlayerNames.Sort();
        
        foreach (var playerName in newTeamOnePlayerNames) {
            teamOnePlayerNames.Add(playerName);
        }
        
        foreach (var playerName in newTeamTwoPlayerNames) {
            teamTwoPlayerNames.Add(playerName);
        }
        
        UpdateTeamListAccessoryVisibilities();
    }

    private void UpdateTeamListAccessoryVisibilities() {
        teamOneJoinButton.Visibility =
            (currentTeamIndex != 0 && teamOnePlayerNames.Count < Config.maxParticipantsPerTeam)
                ? Visibility.Visible
                : Visibility.Collapsed;
        
        teamTwoJoinButton.Visibility =
            (currentTeamIndex != 1 && teamTwoPlayerNames.Count < Config.maxParticipantsPerTeam)
                ? Visibility.Visible
                : Visibility.Collapsed;

        teamOneNoPlayersTextBlock.Visibility = teamOnePlayerNames.Count < 1 ? Visibility.Visible : Visibility.Collapsed;
        teamTwoNoPlayersTextBlock.Visibility = teamTwoPlayerNames.Count < 1 ? Visibility.Visible : Visibility.Collapsed;
    }

    private void HandleLeavePartyButtonClick(object sender, RoutedEventArgs e) {
        ConnectionManager.Disconnect();
    }
    
    private void HandleTeamOneJoinButtonClick(object sender, RoutedEventArgs e) {
        ManagedEventListeners.teamChanged += HandleTeamChanged;

        teamOneJoinButton.IsEnabled = false;
        teamTwoJoinButton.IsEnabled = false;
        
        PartyEventSender.SendChangeTeam(0);
    }
    
    private void HandleTeamTwoJoinButtonClick(object sender, RoutedEventArgs e) {
        ManagedEventListeners.teamChanged += HandleTeamChanged;

        teamOneJoinButton.IsEnabled = false;
        teamTwoJoinButton.IsEnabled = false;
        
        PartyEventSender.SendChangeTeam(1);
    }

    private void HandleTeamChanged() {
        ManagedEventListeners.teamChanged -= HandleTeamChanged;

        teamOneJoinButton.IsEnabled = true;
        teamTwoJoinButton.IsEnabled = true;
    }
}