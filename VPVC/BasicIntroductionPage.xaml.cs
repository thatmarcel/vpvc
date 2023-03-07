using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace VPVC; 

public sealed partial class BasicIntroductionPage: Page {
    public BasicIntroductionPage() {
        InitializeComponent();
    }

    private void HandleContinueButtonClick(object sender, RoutedEventArgs e) {
        ApplicationState.HandleBasicIntroductionAcknowledged();
    }
}