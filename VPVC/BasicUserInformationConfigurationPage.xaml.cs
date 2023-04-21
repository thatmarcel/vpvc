using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using VPVC.ServerLocations;

namespace VPVC; 

public sealed partial class BasicUserInformationConfigurationPage: Page {
    public BasicUserInformationConfigurationPage() {
        InitializeComponent();
        
        userDisplayNameTextBox.TextChanged += HandleUserDisplayNameTextBoxTextChanged;
    }

    private void HandleUserDisplayNameTextBoxTextChanged(object sender, RoutedEventArgs args) {
        var userDisplayName = userDisplayNameTextBox.Text;

        continueButton.IsEnabled = userDisplayName.Length >= Config.minUserDisplayNameLength && userDisplayName.Length <= Config.maxUserDisplayNameLength;
    }
    
    private void HandleContinueButtonClick(object sender, RoutedEventArgs e) {
        var userDisplayName = userDisplayNameTextBox.Text;

        if (userDisplayName.Length < Config.minUserDisplayNameLength || userDisplayName.Length > Config.maxUserDisplayNameLength) {
            return;
        }

        continueProgressRing.IsActive = true;
        continueButton.IsEnabled = false;
        
        ServerLocationsManager.Prepare(prepareSuccess => {
            if (!prepareSuccess) {
                continueProgressRing.IsActive = false;
                continueButton.IsEnabled = true;
                
                ShowServerLocationsPrepareErrorMessage();
                
                return;
            }

            ApplicationState.SetUserDisplayName(userDisplayName);
        });
    }
    
    private void ShowServerLocationsPrepareErrorMessage() {
        this.ShowMessageDialog(
            "Retrieving available servers failed",
            "Something went wrong and we couldn't retrieve the list of available servers. Please make sure you're connected to the internet and try again."
        );
    }
}