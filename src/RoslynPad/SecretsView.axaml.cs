using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using DialogHostAvalonia;
using RoslynPad.Localization;
using RoslynPad.UI;

namespace RoslynPad;

public partial class SecretsView : UserControl
{
    public SecretsView()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    private void OnCopyingToClipboard(object? sender, RoutedEventArgs e)
    {
        e.Handled = true;
    }

    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        if (DataContext is SecretsViewModel vm)
        {
            vm.ConfirmRemoveSecret = ConfirmRemoveSecretAsync;
        }
    }

    private async ValueTask<bool> ConfirmRemoveSecretAsync(string name)
    {
        var removeButton = new Button
        {
            Content = LocalizationManager.Get("Secrets_RemoveButton"),
            Margin = new Avalonia.Thickness(0, 0, 8, 0),
            Classes = { "danger" },
        };
        var cancelButton = new Button
        {
            Content = LocalizationManager.Get("Secrets_DontRemoveButton"),
            Classes = { "secondary" },
        };

        removeButton.Click += (_, _) => DialogHost.Close(MainWindow.DialogHostIdentifier, true);
        cancelButton.Click += (_, _) => DialogHost.Close(MainWindow.DialogHostIdentifier, false);

        var content = new StackPanel
        {
            Margin = new Avalonia.Thickness(20),
            Children =
            {
                new TextBlock
                {
                    Text = LocalizationManager.Format("Secrets_RemoveConfirmFormat", name),
                    TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                    Margin = new Avalonia.Thickness(0, 0, 0, 16)
                },
                new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    HorizontalAlignment = HorizontalAlignment.Right,
                    Spacing = 8,
                    Children = { cancelButton, removeButton }
                }
            }
        };

        var result = await DialogHost.Show(content, MainWindow.DialogHostIdentifier).ConfigureAwait(true);
        return result is true;
    }
}
