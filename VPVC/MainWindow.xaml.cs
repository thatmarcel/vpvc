using System;
using System.Runtime.InteropServices;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Xaml;

namespace VPVC; 

public sealed partial class MainWindow: MicaWindow {
    public MainWindow() {
        InitializeComponent();
        
        Title = "";

        titleTextBlock.Margin = new Thickness(32, 24, 32, 0);
        
        this.ConfigureWindowWithSize(800, 650);

        ApplicationState.currentFlowStepChanged = () => {
            switch (ApplicationState.currentFlowStep) {
                case ApplicationState.FlowStep.BasicIntroduction:
                    basicIntroductionPage.Visibility = Visibility.Visible;
                    basicUserInformationConfigurationPage.Visibility = Visibility.Collapsed;
                    partyJoinOrCreatePage.Visibility = Visibility.Collapsed;
                    partyOverviewPage.Visibility = Visibility.Collapsed;

                    debuggingToolsPage.Visibility = Visibility.Collapsed;
                    break;
                case ApplicationState.FlowStep.BasicUserInformationConfiguration:
                    basicIntroductionPage.Visibility = Visibility.Collapsed;
                    basicUserInformationConfigurationPage.Visibility = Visibility.Visible;
                    partyJoinOrCreatePage.Visibility = Visibility.Collapsed;
                    partyOverviewPage.Visibility = Visibility.Collapsed;

                    debuggingToolsPage.Visibility = Visibility.Collapsed;
                    break;
                case ApplicationState.FlowStep.PartyJoinOrCreate:
                    basicIntroductionPage.Visibility = Visibility.Collapsed;
                    basicUserInformationConfigurationPage.Visibility = Visibility.Collapsed;
                    partyJoinOrCreatePage.Visibility = Visibility.Visible;
                    partyOverviewPage.Visibility = Visibility.Collapsed;

                    debuggingToolsPage.Visibility = Visibility.Collapsed;
                    break;
                case ApplicationState.FlowStep.PartyOverview:
                    basicIntroductionPage.Visibility = Visibility.Collapsed;
                    basicUserInformationConfigurationPage.Visibility = Visibility.Collapsed;
                    partyJoinOrCreatePage.Visibility = Visibility.Collapsed;
                    partyOverviewPage.Visibility = Visibility.Visible;

                    debuggingToolsPage.Visibility = Visibility.Collapsed;
                    break;
                case ApplicationState.FlowStep.DebuggingToolsPage:
                    basicIntroductionPage.Visibility = Visibility.Collapsed;
                    basicUserInformationConfigurationPage.Visibility = Visibility.Collapsed;
                    partyJoinOrCreatePage.Visibility = Visibility.Collapsed;
                    partyOverviewPage.Visibility = Visibility.Collapsed;

                    debuggingToolsPage.Visibility = Visibility.Visible;
                    break;
                default:
                    basicIntroductionPage.Visibility = Visibility.Collapsed;
                    basicUserInformationConfigurationPage.Visibility = Visibility.Collapsed;
                    partyJoinOrCreatePage.Visibility = Visibility.Collapsed;
                    partyOverviewPage.Visibility = Visibility.Collapsed;

                    debuggingToolsPage.Visibility = Visibility.Collapsed;
                    break;
            }
        };
    }
}

[ComImport]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
[Guid("EECDBF0E-BAE9-4CB6-A68E-9598E1CB57BB")]
internal interface IWindowNative {
    IntPtr WindowHandle { get; }
}