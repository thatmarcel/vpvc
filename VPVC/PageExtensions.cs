using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace VPVC; 

public static class PageExtensions {
    public static void ShowMessageDialog(this Page window, string title, string message) {
        var dialogMessageTextBlock = new TextBlock {
            Text = message,
            TextWrapping = TextWrapping.WrapWholeWords
        };
        
        var dialog = new ContentDialog {
            Title = title,
            Content = dialogMessageTextBlock,
            CloseButtonText = "Close",
            XamlRoot = window.Content.XamlRoot
        };

        dialog.ShowAsync();
    }
}