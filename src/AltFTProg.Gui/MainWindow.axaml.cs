namespace AltFTProgGui;
using System;
using System.Collections.Generic;
using System.Threading;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using Avalonia.Media;
using Avalonia.Input;
using AltFTProg;
using System.IO;

public partial class MainWindow : Window {
    public MainWindow() {
        InitializeComponent();

        bool activatedOnce = false;
        Activated += delegate {
            if (activatedOnce) { return; }
            activatedOnce = true;

        };
    }

    protected override void OnLoaded(RoutedEventArgs e) {
        base.OnLoaded(e);

        Helpers.SetToolbarIcons(this, mnu,
                               (imgRefresh, "Refresh"),
                               (imgProgram, "DataProgram"),
                               (imgLoadTemplate, "FileOpen"),
                               (imgApp, "App"));

        OnMenuRefresh(this, new RoutedEventArgs());
    }

    protected override void OnOpened(EventArgs e) {
        base.OnOpened(e);
    }


    #region Menu

    public void OnMenuDeviceChanged(object sender, SelectionChangedEventArgs e) {
        if (mnuProgram == null) { return; }

        var deviceItem = mnuDevice.SelectedItem as DeviceItem;
        UpdateDeviceItem(deviceItem);
    }

    public void OnMenuRefresh(object sender, RoutedEventArgs e) {
        mnu.IsEnabled = false;
        Cursor = new Cursor(StandardCursorType.Wait);

        var selectedItem = mnuDevice.SelectedItem as DeviceItem;
        mnuDevice.Items.Clear();
        mnuDevice.Items.Add("Detecting FTDI devices...");
        mnuDevice.SelectedIndex = 0;
        mnuDevice.IsEnabled = false;

        ThreadPool.QueueUserWorkItem((s) => {
            try {
                var devices = FtdiDevice.GetDevices();
                if (devices.Count == 0) {
                    Dispatcher.UIThread.Post(() => {
                        mnuDevice.Items.Clear();
                        mnuDevice.Items.Add("No FTDI devices found");
                        mnuDevice.SelectedIndex = 0;
                    });
                } else {
                    var deviceItems = new List<DeviceItem>();
                    DeviceItem? newSelectedItem = null;
                    foreach (var device in devices) {
                        var deviceItem = new DeviceItem(device);
                        deviceItems.Add(deviceItem);
                        if (deviceItem.Device.Equals(selectedItem?.Device)) {
                            newSelectedItem = deviceItem;
                        }
                    }
                    Dispatcher.UIThread.Post(() => {
                        mnuDevice.Items.Clear();
                        foreach (var deviceItem in deviceItems) {
                            mnuDevice.Items.Add(deviceItem);
                        }
                        mnuDevice.SelectedIndex = 0;
                        mnuDevice.IsEnabled = true;
                        if (newSelectedItem != null) {
                            mnuDevice.SelectedItem = newSelectedItem;
                        }
                    });
                }
            } catch (InvalidOperationException) {
                Dispatcher.UIThread.Post(() => {
                    mnuDevice.Items.Clear();
                    mnuDevice.Items.Add("Error accessing FTDI devices");
                    mnuDevice.SelectedIndex = 0;
                });
            }

            Dispatcher.UIThread.Post(() => {
                mnu.IsEnabled = true;
                Cursor = Cursor.Default;
            });
        });
    }

    public void OnMenuProgram(object sender, RoutedEventArgs e) {
        if (mnuDevice.SelectedItem is DeviceItem deviceItem) {
            mnu.IsEnabled = false;
            Cursor = new Cursor(StandardCursorType.Wait);

            ThreadPool.QueueUserWorkItem((s) => {
                deviceItem.Device.SaveEeprom();
                Dispatcher.UIThread.Post(() => {
                    mnu.IsEnabled = true;
                    Cursor = Cursor.Default;
                });
            });
        }
    }

    public async void OnMenuLoadTemplate(object sender, RoutedEventArgs e) {
        if (mnuProgram == null) { return; }
        var deviceItem = mnuDevice.SelectedItem as DeviceItem;
        if (deviceItem == null) { return; }

        var device = deviceItem.Device;

        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel == null) { return; }
        var files = await topLevel.StorageProvider.OpenFilePickerAsync(
            new FilePickerOpenOptions {
                Title = "Open XML Template",
                AllowMultiple = false,
                FileTypeFilter = new[] {
                    new FilePickerFileType("FTDI Template XML") {
                        Patterns = new[] { "*.xml" },
                        AppleUniformTypeIdentifiers = new[] { "public.xml-text" },
                        MimeTypes = new[] { "text/xml" }
                    }
                }
            }
        );

        if (files.Count > 0) {
            using var stream = await files[0].OpenReadAsync();
            var template = FtdiXmlTemplate.Load(stream);
            if (template.DeviceType == device.DeviceType) {
                template.Apply(device);
                UpdateDeviceItem(deviceItem);
            }  // TODO: maybe a dialog to say it doesn't match and to catch potential exceptions
        }
    }


    public void OnMenuAppOptionsClick(object sender, RoutedEventArgs e) {
        OnMenuAppAboutClick(sender, e);
    }

    public void OnMenuAppFeedbackClick(object sender, RoutedEventArgs e) {
        //TODO
    }

    public void OnMenuAppUpgradeClick(object sender, RoutedEventArgs e) {
        //TODO
    }

    public void OnMenuAppAboutClick(object sender, RoutedEventArgs e) {
        Medo.Avalonia.AboutBox.ShowDialog(this, new Uri("https://medo64.com/altftprog/"));
    }

    #endregion Menu


    private void UpdateDeviceItem(DeviceItem? deviceItem) {
        var isEnabled = (deviceItem != null);
        var hasChanged = (deviceItem?.Device?.HasEepromChanged ?? false);

        mnuProgram.IsEnabled = isEnabled && hasChanged;
        mnuLoadTemplate.IsEnabled = isEnabled;

        if (tabMain == null) { return; }
        var device = deviceItem?.Device;
        PopulateDevice(tabMain, device, delegate () {
            var hasChanged = (deviceItem?.Device?.HasEepromChanged ?? false);
            mnuProgram.IsEnabled = isEnabled && hasChanged;
        });
    }

    private static void PopulateDevice(TabControl tabs, FtdiDevice? device, Action refreshAction) {
        tabs.Items.Clear();

        if (device != null) {
            if (device is Ftdi232RDevice ft232rDevice) {
                new FT232RContent(ft232rDevice, refreshAction).Populate(tabs);
            } else if (device is FtdiXSeriesDevice xSeriesDevice) {
                new FTXSeriesContent(xSeriesDevice, refreshAction).Populate(tabs);
            } else {
                var stack = new StackPanel() {
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center
                };
                stack.Children.Add(new Label() {
                    Content = "Device not supported",
                    HorizontalAlignment = HorizontalAlignment.Center
                });
                stack.Children.Add(new Label() {
                    Content = device.DeviceType,
                    FontWeight = FontWeight.Bold,
                    HorizontalAlignment = HorizontalAlignment.Center
                });
                stack.Children.Add(new Label() {
                    Content = device.UsbVendorId.ToString("X4") + ":" + device.UsbProductId.ToString("X4"),
                    HorizontalAlignment = HorizontalAlignment.Center
                });
                stack.Children.Add(new Label() { Content = " " });
                tabs.Items.Add(
                    new TabItem() {
                        Header = "",
                        Content = stack
                    }
                );
            }
        }
    }

}
