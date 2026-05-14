using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using System.Linq;
using Avalonia.Markup.Xaml;
using HairSalon.ViewModels;
using HairSalon.Views;

namespace HairSalon;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            ShowLogin(desktop);
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void ShowLogin(IClassicDesktopStyleApplicationLifetime desktop)
    {
        var loginVm = new LoginViewModel();
        var loginWindow = new LoginWindow { DataContext = loginVm };

        loginVm.LoginSucceeded += () =>
        {
            var mainVm = new MainWindowViewModel();
            var mainWindow = new MainWindow { DataContext = mainVm };

            mainVm.LogoutRequested += () =>
            {
                mainWindow.Close();
                ShowLogin(desktop);
            };

            desktop.MainWindow = mainWindow;
            mainWindow.Show();
            loginWindow.Close();
        };

        desktop.MainWindow = loginWindow;
        loginWindow.Show();
    }
}
