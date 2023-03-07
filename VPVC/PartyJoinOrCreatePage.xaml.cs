using System.Threading;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using VPVC.BackendCommunication;
using VPVC.MainInternals;

namespace VPVC; 

public sealed partial class PartyJoinOrCreatePage: Page {
    public PartyJoinOrCreatePage() {
        InitializeComponent();

        partyJoinCodeTextBox.TextChanged += HandlePartyJoinCodeTextBoxTextChanged;
    }

    private void HandlePartyJoinCodeTextBoxTextChanged(object sender, RoutedEventArgs args) {
        var partyJoinCode = partyJoinCodeTextBox.Text;

        partyJoinButton.IsEnabled = !partyJoinProgressRing.IsActive && partyJoinCode.Length >= Config.minPartyJoinCodeLength && partyJoinCodeTextBox.Text.Length <= Config.maxPartyJoinCodeLength;
    }

    private void HandlePartyJoinButtonClick(object sender, RoutedEventArgs e) {
        var partyJoinCode = partyJoinCodeTextBox.Text;

        if (partyJoinCode.Length < Config.minPartyJoinCodeLength || partyJoinCode.Length > Config.maxPartyJoinCodeLength) {
            return;
        }
        
        SetInputsEnabled(false);
        partyJoinProgressRing.IsActive = true;

        var hadBeenConnected = false;

        ConnectionEventListeners.connected += () => {
            hadBeenConnected = true;
            
            SetInputsEnabled(true);
            
            ManagedEventListeners.partyJoinFailed += ShowPartyJoinFailedErrorMessage;
            ManagedEventListeners.partyCreateOrJoinSuccess += () => {
                partyJoinCodeTextBox.Text = "";
            };
            
            partyJoinProgressRing.IsActive = false;
        };
        
        ConnectionEventListeners.disconnected += () => {
            SetInputsEnabled(true);
            partyJoinProgressRing.IsActive = false;

            if (!hadBeenConnected) {
                ShowConnectionFailedErrorMessage();
            }
        };

        // Wait a bit before connecting to make sure the loading spinner shows for a bit
        // as connecting usually happens in a fraction of a second
        App.RunInBackground(() => {
            Thread.Sleep(1000);
            App.RunInForeground(() => PartyManager.ConnectAndJoinParty(partyJoinCode));
        });
    }
    
    private void HandlePartyCreateButtonClick(object sender, RoutedEventArgs e) {
        SetInputsEnabled(false);
        partyCreateProgressRing.IsActive = true;
        
        var hadBeenConnected = false;

        ConnectionEventListeners.connected += () => {
            hadBeenConnected = true;
            
            SetInputsEnabled(true);
            
            ManagedEventListeners.partyCreateFailed += ShowPartyCreateFailedErrorMessage;
            ManagedEventListeners.partyCreateOrJoinSuccess += () => {
                partyJoinCodeTextBox.Text = "";
            };
            
            partyCreateProgressRing.IsActive = false;
        };
        
        ConnectionEventListeners.disconnected += () => {
            SetInputsEnabled(true);
            partyCreateProgressRing.IsActive = false;

            if (!hadBeenConnected) {
                ShowConnectionFailedErrorMessage();
            }
        };
        
        // Wait a bit before connecting to make sure the loading spinner shows for a bit
        // as connecting usually happens in a fraction of a second
        App.RunInBackground(() => {
            Thread.Sleep(1000);
            App.RunInForeground(PartyManager.ConnectAndCreateParty);
        });
    }

    private void SetInputsEnabled(bool shouldBeEnabled) {
        partyCreateButton.IsEnabled = shouldBeEnabled;
        partyJoinCodeTextBox.IsEnabled = shouldBeEnabled;
        partyJoinButton.IsEnabled = shouldBeEnabled && partyJoinCodeTextBox.Text.Length >= Config.minPartyJoinCodeLength && partyJoinCodeTextBox.Text.Length <= Config.maxPartyJoinCodeLength;
    }

    private void ShowConnectionFailedErrorMessage() {
        this.ShowMessageDialog(
            "Connection failed",
            "Something went wrong and we couldn't connect to the backend server. Please make sure you're connected to the internet and try again."
        );
    }
    
    private void ShowPartyCreateFailedErrorMessage() {
        this.ShowMessageDialog(
            "Creating party failed",
            "Something went wrong and creating a new party failed. Please try again."
        );
    }
    
    private void ShowPartyJoinFailedErrorMessage() {
        Logger.Log("Page informed about join failed event.");
        this.ShowMessageDialog(
            "Joining party failed",
            "Something went wrong and joining the party failed. Please make sure the join code is correct and try again."
        );
    }
}