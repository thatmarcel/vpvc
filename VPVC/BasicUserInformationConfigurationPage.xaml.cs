using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

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
        
        ApplicationState.SetUserDisplayName(userDisplayName);
    }
}