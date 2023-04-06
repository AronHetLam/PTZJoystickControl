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
using PtzJoystickControl.Core.Services;

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
            if (ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop)
                return;

            if (desktop.Args.Contains("-r"))
            {
                desktop.MainWindow = new InstanceRunningWindow();
                return;
            }

            var camerasViewModel = Locator.Current.GetServiceOrThrow<CamerasViewModel>();

            var mainWindow = new MainWindow();
            var mainWindowViewModel = new MainWindowViewModel(
                Locator.Current.GetServiceOrThrow<GamepadsViewModel>(),
                camerasViewModel,
                mainWindow);
            mainWindow.DataContext = mainWindowViewModel;

            mainWindow.Closing += (object? s, CancelEventArgs e) =>
            {   // Prevent closnig to keep application running in background
                mainWindow.Hide();
                e.Cancel = true;
            };

            var trayIconHandler = Locator.Current.GetServiceOrThrow<TrayIconHandler>();
            trayIconHandler.OnShowClicked += (object? s, EventArgs e) => ShowMainWindow();
            trayIconHandler.OnUpdateCheckClicked += (object? s, EventArgs e) => CheckForUpdate(true);
            trayIconHandler.OnQuitClicked += (object? s, EventArgs e) => desktop.Shutdown();

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

                CheckForUpdate();

                desktop.Startup -= onStartup;
            };

            void onExit(object? s, ControlledApplicationLifetimeExitEventArgs e)
            {
                trayIconHandler.Exit(s, e);
            }

            desktop.Startup += onStartup;
            desktop.Exit += onExit;

            desktop.MainWindow = mainWindow;

            base.OnFrameworkInitializationCompleted();
        }

        private void ShowMainWindow()
        {
            if (ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop)
                return;

            var mainWindow = desktop.MainWindow;

            mainWindow.Show();
            if (mainWindow.WindowState == WindowState.Minimized)
                mainWindow.WindowState = WindowState.Normal;
            else
            {   // Force mainWindow to be on top.
                mainWindow.Topmost = true;
                mainWindow.Topmost = false;
            }
        }

        private void CheckForUpdate(bool showModalWhenNotAvailable = false)
        {
            if (ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop)
                return;

            var updateTask = Locator.Current.GetServiceOrThrow<IUpdateService>()
                .CheckForUpdate()
                .ContinueWith((task) =>
                {
                    var update = task.Result;
                    if (!showModalWhenNotAvailable && !update.Available)
                        return;

                    var showUpdateModal = () =>
                    {
                        var vm = new UpdateDialogViewModel(update);
                        var dialog = new UpdateDialogWindow() { DataContext = vm };
                        ShowMainWindow();
                        dialog.ShowDialog(desktop.MainWindow);
                    };
                    if (Dispatcher.UIThread.CheckAccess())
                        showUpdateModal();
                    else
                        Dispatcher.UIThread.InvokeAsync(showUpdateModal);
                });
        }
    }
}
