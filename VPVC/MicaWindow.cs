using Windows.UI;
using Microsoft.UI;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using WinRT;

namespace VPVC; 

public class MicaWindow: Window {
    private WindowsSystemDispatcherQueueHelper? wdsqHelper;
    private SystemBackdropConfiguration? systemBackdropConfiguration;
    private MicaController? backdropMicaController;

    private bool hasTriedActivatingMicaBackdrop = false;
    
    public MicaWindow() {
        Activated += HandleWindowActivated;
        Closed += HandleWindowClosed;
    }

    private void ActivateMicaIfAvailable() {
        if (!MicaController.IsSupported()) {
            if (Content is Panel) {
                (Content as Panel)!.Background = new SolidColorBrush(Color.FromArgb(
                    (byte) (0.1 * 255),
                    Colors.LightGray.R,
                    Colors.LightGray.G,
                    Colors.LightGray.B
                ));
            }
            
            return;
        }

        wdsqHelper = new();
        systemBackdropConfiguration = new();
        backdropMicaController = new();
        
        wdsqHelper.EnsureWindowsSystemDispatcherQueueController();

        systemBackdropConfiguration.IsInputActive = true;
        UpdateBackdropConfigurationTheme();
        
        backdropMicaController.AddSystemBackdropTarget(this.As<Microsoft.UI.Composition.ICompositionSupportsSystemBackdrop>());
        backdropMicaController.SetSystemBackdropConfiguration(systemBackdropConfiguration);
    }

    private void HandleWindowClosed(object sender, WindowEventArgs args) {
        if (backdropMicaController != null) {
            backdropMicaController.Dispose();
            backdropMicaController = null;
        }

        Activated -= HandleWindowActivated;

        systemBackdropConfiguration = null;
    }

    private void HandleWindowActivated(object sender, WindowActivatedEventArgs args) {
        if (!hasTriedActivatingMicaBackdrop) {
            ((FrameworkElement)Content).ActualThemeChanged += HandleWindowThemeChanged;
            
            ActivateMicaIfAvailable();
            
            hasTriedActivatingMicaBackdrop = true;
        }
        
        if (systemBackdropConfiguration != null) {
            systemBackdropConfiguration.IsInputActive = args.WindowActivationState != WindowActivationState.Deactivated;
        }
    }

    private void HandleWindowThemeChanged(FrameworkElement sender, object args) {
        UpdateBackdropConfigurationTheme();
    }

    private void UpdateBackdropConfigurationTheme() {
        if (systemBackdropConfiguration == null) {
            return;
        }
        
        switch (((FrameworkElement)Content).ActualTheme) {
            case ElementTheme.Dark: systemBackdropConfiguration.Theme = SystemBackdropTheme.Dark; break;
            case ElementTheme.Light: systemBackdropConfiguration.Theme = SystemBackdropTheme.Light; break;
            case ElementTheme.Default: systemBackdropConfiguration.Theme = SystemBackdropTheme.Default; break;
        }
    }
}