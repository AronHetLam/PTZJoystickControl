using Avalonia.Threading;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using PtzJoystickControl.Gui.ViewModels;
using PtzJoystickControl.Gui.Views;
using PtzJoystickControl.Gui.TrayIcon;
using Splat;
using System;
using System.ComponentModel;
using System.Linq;

namespace PtzJoystickControl.Gui
{
    public partial class App : Avalonia.Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var camerasViewModel = Locator.Current.GetServiceOrThrow<CamerasViewModel>();

                var mainWindow = new MainWindow();
                var mainWindowViewModel = new MainWindowViewModel(
                    Locator.Current.GetServiceOrThrow<GamepadsViewModel>(),
                    camerasViewModel,
                    mainWindow);
                mainWindow.DataContext = mainWindowViewModel;

                mainWindow.Closing += (object? s, CancelEventArgs e) =>
                {
                    mainWindow.Hide();
                    e.Cancel = true;
                };

                var trayIconHandler = Locator.Current.GetServiceOrThrow<TrayIconHandler>();
                trayIconHandler.OnShowClicked += (object? s, EventArgs e) =>
                {
                    mainWindow.Show();
                    if (mainWindow.WindowState == WindowState.Minimized)
                        mainWindow.WindowState = WindowState.Normal;
                    else
                    {
                        mainWindow.Topmost = true;
                        mainWindow.Topmost = false;
                    }
                };
                trayIconHandler.OnQuitClicked += (object? s, EventArgs e) =>
                {
                    desktop.Shutdown();
                };

                camerasViewModel.PropertyChanged += (object? s, PropertyChangedEventArgs e) =>
                {
                    if (e.PropertyName == nameof(CamerasViewModel.SelectedCamera))
                    {
                        var updateIcon = () => trayIconHandler.UpdateIcon(camerasViewModel.Cameras.IndexOf(camerasViewModel.SelectedCamera!) + 1);
                        if (Dispatcher.UIThread.CheckAccess())
                            updateIcon();
                        else
                            Dispatcher.UIThread.InvokeAsync(updateIcon);
                    }
                };

                void onStartup(object? s, ControlledApplicationLifetimeStartupEventArgs e)
                {
                    if (e.Args.Contains("-m"))
                        mainWindow.WindowState = WindowState.Minimized;
                    desktop.Startup -= onStartup;
                };

                void onExit(object? s, ControlledApplicationLifetimeExitEventArgs e)
                {
                    trayIconHandler.Exit(s, e);
                }

                desktop.Startup += onStartup;
                desktop.Exit += onExit;

                desktop.MainWindow = mainWindow;
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}
