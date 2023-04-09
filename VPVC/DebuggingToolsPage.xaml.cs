using System;
using System.Collections.Generic;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using VPVC.BackendCommunication.Shared;
using VPVC.GameCommunication;

namespace VPVC; 

public sealed partial class DebuggingToolsPage: Page {
    // Variable is referenced in XAML
    // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
    // ReSharper disable once CollectionNeverQueried.Local
    private readonly List<Tuple<string, int>> overrideGameStateOptions = new() {
        new Tuple<string, int>("Lobby", GameStates.lobby),
        new Tuple<string, int>("Agent select", GameStates.agentSelect),
        new Tuple<string, int>("In-game", GameStates.inGame)
    };
    
    public DebuggingToolsPage() {
        InitializeComponent();

        DebuggingInformationHelper.informationHasBeenUpdated += () => {
            try {
                debuggingInformationTextBlock.Text = DebuggingInformationHelper.infoText;
            } catch (Exception) {}
        };

        coordinatesOverrideGameStateSelectionComboBox.SelectedIndex = 0;

        coordinateAndGameStateOverrideCheckBox.Checked += (_, _) => UpdateCoordinatesOverride();
        coordinateAndGameStateOverrideCheckBox.Unchecked += (_, _) => UpdateCoordinatesOverride();
        coordinatesOverrideGameStateSelectionComboBox.SelectionChanged += (_, _) => UpdateCoordinatesOverride();
        coordinatesXOverrideTextBox.TextChanged += (_, _) => UpdateCoordinatesOverride();
        coordinatesYOverrideTextBox.TextChanged += (_, _) => UpdateCoordinatesOverride();
    }

    private void HandleBackToPartyOverviewButtonClick(object sender, RoutedEventArgs e) {
        ApplicationState.HandlePartyJoined();
    }

    private void UpdateCoordinatesOverride() {
        if ((coordinateAndGameStateOverrideCheckBox.IsChecked ?? false) && coordinatesOverrideGameStateSelectionComboBox.SelectedValue is int overridenGameState) {
            GameStateAndCoordinatesExtractor.overridenGameState = overridenGameState;

            try {
                GameStateAndCoordinatesExtractor.overridenRelativeCoordinatesX = Convert.ToInt32(coordinatesXOverrideTextBox.Text);
                GameStateAndCoordinatesExtractor.overridenRelativeCoordinatesY = Convert.ToInt32(coordinatesYOverrideTextBox.Text);
            } catch (Exception) { }
        } else {
            GameStateAndCoordinatesExtractor.overridenGameState = -1;
        }
    }
}