using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Timers;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using VPVC.BackendCommunication;
using VPVC.MainInternals;
using VPVC.VoiceChat;

namespace VPVC; 

public sealed partial class PartyOverviewPage: Page {
    public readonly ObservableCollection<PartyOverviewPageParticipantInfo> teamOneParticipantInfos = new();
    public readonly ObservableCollection<PartyOverviewPageParticipantInfo> teamTwoParticipantInfos = new();

    private const int maxMillisecondsSinceLastAudioForTalking = 400;
    private const int partyParticipantsTalkingStatesUpdateTimerInterval = 200;

    private Timer? partyParticipantsTalkingStatesUpdateTimer;
    
    private int currentTeamIndex = 0;
    
    public PartyOverviewPage() {
        InitializeComponent();
        
        UpdateTeamListAccessoryVisibilities();

        ApplicationState.partyOverviewInformationChanged += HandlePartyOverviewInformationChanged;

        SetupPartyParticipantsTalkingStatesUpdateTimer();

        selfMuteCheckBox.Checked += (_, _) => { NewWindowsAudioEndpoint.isMicrophoneMuted = true; };
        selfMuteCheckBox.Unchecked += (_, _) => { NewWindowsAudioEndpoint.isMicrophoneMuted = false; };
    }

    private void SetupPartyParticipantsTalkingStatesUpdateTimer() {
        partyParticipantsTalkingStatesUpdateTimer = new Timer();
        partyParticipantsTalkingStatesUpdateTimer.Elapsed += (_, _) => App.RunInForeground(UpdatePartyParticipantsTalkingStates);
        partyParticipantsTalkingStatesUpdateTimer.Interval = partyParticipantsTalkingStatesUpdateTimerInterval;
        partyParticipantsTalkingStatesUpdateTimer.Start();
    }

    private void UpdatePartyParticipantsTalkingStates() {
        var lastAudioTimestampsForParticipantIds = VoiceChatManager.voiceChatBackendClient?.lastAudioTimestampsForParticipantIds;

        if (lastAudioTimestampsForParticipantIds == null) {
            return;
        }

        var currentTimestamp = DateTime.Now.ToFileTime();
        
        var partyParticipants = new List<PartyOverviewPageParticipantInfo>();
        partyParticipants.AddRange(teamOneParticipantInfos);
        partyParticipants.AddRange(teamTwoParticipantInfos);

        foreach (var partyParticipant in partyParticipants) {
            if (lastAudioTimestampsForParticipantIds.TryGetValue(partyParticipant.id, out long lastAudioTimestampForParticipant)) {
                var timeDifference = currentTimestamp - lastAudioTimestampForParticipant;

                partyParticipant.isTalking = timeDifference <= (maxMillisecondsSinceLastAudioForTalking * 10000 /* to 100 nanoseconds */);
            } else {
                partyParticipant.isTalking = false;
            }
        }
    }

    private void HandlePartyOverviewInformationChanged() {
        var party = PartyManager.currentParty;
        
        if (party == null) {
            return;
        }

        joinCodeTextBlock.Text = party.joinCode;

        var newTeamOneParticipantInfos = party.otherParticipants
            .Where(participant => participant.teamIndex == 0)
            .Select(participant => new PartyOverviewPageParticipantInfo(
                participant.id,
                $"{participant.userDisplayName}{(participant.isPartyLeader ? " (leader)" : "")}",
                false,
                1d
            ))
            .ToList();
        
        var newTeamTwoParticipantInfos = party.otherParticipants
            .Where(participant => participant.teamIndex == 1)
            .Select(participant => new PartyOverviewPageParticipantInfo(
                participant.id,
                $"{participant.userDisplayName}{(participant.isPartyLeader ? " (leader)" : "")}",
                false,
                1d
            ))
            .ToList();

        currentTeamIndex = party.participantSelf.teamIndex;

        if (currentTeamIndex == 0) {
            newTeamOneParticipantInfos.Add(
                new PartyOverviewPageParticipantInfo(
                    party.participantSelf.id, 
                    party.participantSelf.isPartyLeader
                        ? $"{party.participantSelf.userDisplayName} (you, leader)"
                        : $"{party.participantSelf.userDisplayName} (you)", 
                    true,
                    1d
                )
            );
        } else {
            newTeamTwoParticipantInfos.Add(
                new PartyOverviewPageParticipantInfo(
                    party.participantSelf.id, 
                    party.participantSelf.isPartyLeader
                        ? $"{party.participantSelf.userDisplayName} (you, leader)"
                        : $"{party.participantSelf.userDisplayName} (you)", 
                    true,
                    1d
                )
            );
        }
        
        teamOneParticipantInfos.ToList().ForEach(pi => teamOneParticipantInfos.Remove(pi));
        teamTwoParticipantInfos.ToList().ForEach(pi => teamTwoParticipantInfos.Remove(pi));

        newTeamOneParticipantInfos = newTeamOneParticipantInfos.OrderBy(pi => pi.displayName).ToList();
        newTeamTwoParticipantInfos = newTeamTwoParticipantInfos.OrderBy(pi => pi.displayName).ToList();
        
        foreach (var participantInfo in newTeamOneParticipantInfos) {
            teamOneParticipantInfos.Add(participantInfo);
        }
        
        foreach (var participantInfo in newTeamTwoParticipantInfos) {
            teamTwoParticipantInfos.Add(participantInfo);
        }
        
        UpdateTeamListAccessoryVisibilities();
    }

    private void UpdateTeamListAccessoryVisibilities() {
        teamOneJoinButton.Visibility =
            (currentTeamIndex != 0 && teamOneParticipantInfos.Count < Config.maxParticipantsPerTeam)
                ? Visibility.Visible
                : Visibility.Collapsed;
        
        teamTwoJoinButton.Visibility =
            (currentTeamIndex != 1 && teamTwoParticipantInfos.Count < Config.maxParticipantsPerTeam)
                ? Visibility.Visible
                : Visibility.Collapsed;

        teamOneNoPlayersTextBlock.Visibility = teamOneParticipantInfos.Count < 1 ? Visibility.Visible : Visibility.Collapsed;
        teamTwoNoPlayersTextBlock.Visibility = teamTwoParticipantInfos.Count < 1 ? Visibility.Visible : Visibility.Collapsed;
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
    
    private void HandleDebuggingToolsButtonClick(object sender, RoutedEventArgs e) {
        ApplicationState.HandleDebuggingToolsPageRequested();
    }

    private void HandlePlayerNameTapped(object sender, RoutedEventArgs e) {
        var senderElement = (FrameworkElement) sender;
        if (senderElement.DataContext is PartyOverviewPageParticipantInfo participantInfo) {
            if (!participantInfo.isSelf) {
                FlyoutBase.ShowAttachedFlyout((FrameworkElement) sender);
            }
        }
    }
}