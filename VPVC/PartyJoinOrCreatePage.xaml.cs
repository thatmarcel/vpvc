using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using VPVC.BackendCommunication;
using VPVC.MainInternals;
using VPVC.ServerLocations;
using VPVC.ServerLocations.Types;

namespace VPVC; 

public sealed partial class PartyJoinOrCreatePage: Page {
    // Variable is referenced in XAML
    // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
    // ReSharper disable once FieldCanBeMadeReadOnly.Local
    // ReSharper disable once MemberInitializerValueIgnored
    private List<ServerLocation> availableServerLocations = new();
    
    public PartyJoinOrCreatePage() {
        InitializeComponent();

        partyJoinCodeTextBox.TextChanged += HandlePartyJoinCodeTextBoxTextChanged;

        ServerLocationsManager.onServerLocationsChanged = updatedServerLocations => {
            availableServerLocations = updatedServerLocations;

            if (updatedServerLocations.Count > 0) {
                serverLocationSelectionComboBox.SelectedIndex = 0;
            }
        };
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

        var partyServerLocation = ServerLocationsManager.FindServerLocationForPartyJoinCodeLetterPrefix(partyJoinCode.First().ToString());

        if (partyServerLocation == null) {
            ShowPartyJoinFailedErrorMessage();
            return;
        }

        ServerLocationsManager.selectedServerLocation = partyServerLocation;

        partyJoinCode = partyJoinCode.Substring(1);
        
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

        ServerLocationsManager.selectedServerLocation = availableServerLocations.First(s =>
            ReferenceEquals(s.identifier, serverLocationSelectionComboBox.SelectedValue)
        );
        
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
        this.ShowMessageDialog(
            "Joining party failed",
            "Something went wrong and joining the party failed. Please make sure the join code is correct and try again."
        );
    }
}