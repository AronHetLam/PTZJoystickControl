﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Avalonia;
using Avalonia.Platform;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Controls;
using System.Reflection;

namespace PtzJoystickControl.Gui.TrayIcon
{
    internal class TrayIconHandler
    {
        private readonly IAssetLoader _assetLoader;
        private readonly string _assemblyName;
        private readonly Avalonia.Controls.TrayIcon _trayIcon;

        public TrayIconHandler(IAssetLoader assetLoader)
        {
            _assetLoader = assetLoader ?? throw new ArgumentNullException(nameof(assetLoader));
            _assemblyName = Assembly.GetExecutingAssembly().GetName().Name!;

            // Initialize Tray Icon
            _trayIcon = new Avalonia.Controls.TrayIcon()
            {
                IsVisible = true,
                Menu = buildNativeMenu(),
                ToolTipText = "PTZ Joystick Control"
            };

            _trayIcon.Clicked += (object? s , EventArgs e) => OnShowClicked?.Invoke(s, e);

            UpdateIcon(0);
        }

        public void UpdateIcon(int number)
        {
            var assets = AvaloniaLocator.Current.GetServiceOrThrow<IAssetLoader>();

            int[] sizes = { 16, 24, 32 };
            string text = number > 0 ? number < 100 ? number.ToString() : "##" : "-";
            List<Bitmap> bitmaps = new(sizes.Length);

            for (int i = 0; i < sizes.Length; i++)
            {
                Bitmap bitmap = new(assets.Open(new Uri($"avares://{_assemblyName}/Assets/bg_{sizes[i]}.png")));

                using var rtbb = new RenderTargetBitmap(new PixelSize(sizes[i], sizes[i]));
                using var ctx = rtbb.CreateDrawingContext(null);
                
                var brush = new SolidColorBrush(Colors.White);
                var formattedTextt = new FormattedText(
                    text,
                    new Typeface("Microsoft Sans Serif", FontStyle.Normal, FontWeight.SemiBold),
                    sizes[i] - 4,
                    TextAlignment.Center,
                    TextWrapping.NoWrap,
                    new Size(sizes[i], sizes[i])
                );

                ctx.DrawBitmap(bitmap.PlatformImpl, 1, new Rect(0, 0, sizes[i], sizes[i]), new Rect(0, 0, sizes[i], sizes[i]));
                ctx.DrawText(brush, new Point(0, 0), formattedTextt.PlatformImpl);

                using var ms = new MemoryStream();
                rtbb.Save(ms);
                ms.Position = 0;

                bitmaps.Add(new Bitmap(ms));
            }

            try
            {
                _trayIcon.Icon = IconFactory.PngsToMultiSizeIcon(bitmaps);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
        }

        public void Exit(object? s, EventArgs e)
        {
            _trayIcon.Dispose();
        }

        private NativeMenu buildNativeMenu() {
            var headerItem = new NativeMenuItem("PTZ Joystick Control");
            headerItem.Click += (object? s , EventArgs e) => OnShowClicked?.Invoke(s, e);

            var upadteCheckItem = new NativeMenuItem("Check for updates");
            upadteCheckItem.Click += (object? s, EventArgs e) => OnUpdateCheckClicked?.Invoke(s, e);

            var quitItem = new NativeMenuItem("Quit");
            quitItem.Click += (object? s , EventArgs e) => OnQuitClicked?.Invoke(s, e);

            return new NativeMenu{
                headerItem,
                upadteCheckItem,
                new NativeMenuItemSeparator(),
                quitItem
            };
        }

        public event EventHandler? OnShowClicked;
        public event EventHandler? OnUpdateCheckClicked;
        public event EventHandler? OnQuitClicked;
    }
}
