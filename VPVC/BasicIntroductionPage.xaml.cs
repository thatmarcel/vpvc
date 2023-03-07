using System.Collections.Generic;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using VPVC.GameCommunication;

namespace VPVC; 

public sealed partial class BasicIntroductionPage: Page {
    // Variable is referenced in XAML
    // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
    private List<ScreenInfo> availableScreens = new();
        
    public BasicIntroductionPage() {
        InitializeComponent();

        availableScreens = ScreenHelper.GetScreens();

        if (availableScreens.Count < 1) {
            availableScreens.Add(new ScreenInfo("noScreensAvailable", "No screens available"));

            screenSelectionComboBox.IsEnabled = false;
            continueButton.IsEnabled = false;
        }
        
        screenSelectionComboBox.SelectedIndex = 0;
    }

    private void HandleContinueButtonClick(object sender, RoutedEventArgs e) {
        var selectedScreenDeviceId = screenSelectionComboBox.SelectedValue as string;

        if (selectedScreenDeviceId == null) {
            return;
        }
        
        ScreenHelper.SelectScreenWithDeviceId(selectedScreenDeviceId);
        
        ApplicationState.HandleBasicIntroductionAcknowledged();
    }
}