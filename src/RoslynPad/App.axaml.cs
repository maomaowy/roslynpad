using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using RoslynPad.Localization;
using RoslynPad.Resources;
using RoslynPad.UI;

namespace RoslynPad;

internal class App : Application
{
    public override void OnFrameworkInitializationCompleted()
    {
        // The editor's JoinableTaskContext must bind to the UI thread before the Roslyn host
        // composes on a background thread.
        Morgania.CodeAnalysis.Editor.HostServiceExports.InitializeMainThread();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow();
        }

        base.OnFrameworkInitializationCompleted();
    }

    public override void Initialize()
    {
        Resources.MergedDictionaries.Add(new Icons());
        AvaloniaXamlLoader.Load(this);

        // Show the system language before the settings are available; the main window applies
        // the configured language as soon as the settings are loaded.
        LocalizationManager.SetLanguage(AppLanguage.System);
        LocalizationManager.LanguageChanged += OnLanguageChanged;
        OnLanguageChanged();
    }

    private void OnLanguageChanged() => LocalizationManager.Localize(NativeMenu.GetMenu(this));

    private void OnSettingsClick(object? sender, EventArgs e)
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime { MainWindow: MainWindow mainWindow })
        {
            mainWindow.ViewModel.OpenSettingsCommand.Execute(null);
        }
    }
}
