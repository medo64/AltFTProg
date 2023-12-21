namespace AltFTProgGui;
using System;
using System.Collections.Generic;
using System.Threading;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Styling;
using Avalonia.Threading;
using AltFTProg;

public partial class MainWindow : Window {
    public MainWindow() {
        InitializeComponent();
    }

    protected override void OnLoaded(RoutedEventArgs e) {
        base.OnLoaded(e);
        Helpers.SetToolbarIcons(this, grid,
                               (imgRefresh, "Refresh"),
                               (imgProgram, "DataProgram"),
                               (imgLoadTemplate, "FileOpen"),
                               (imgApp, "App"));
    }

    protected override void OnOpened(EventArgs e) {
        base.OnOpened(e);
        OnMenuRefresh(this, new RoutedEventArgs());
    }


    #region Menu

    public void OnMenuDeviceChanged(object sender, SelectionChangedEventArgs e) {
        if (mnuProgram == null) { return; }
        var isEnabled = (cmbDevice.SelectedItem is DeviceItem);
        mnuProgram.IsEnabled = isEnabled;
        mnuLoadTemplate.IsEnabled = isEnabled;
    }

    public void OnMenuRefresh(object sender, RoutedEventArgs e) {
        cmbDevice.Items.Clear();
        cmbDevice.Items.Add("Detecting FTDI devices...");
        cmbDevice.SelectedIndex = 0;
        cmbDevice.IsEnabled = false;

        ThreadPool.QueueUserWorkItem((s) => {
            try {
                var devices = FtdiDevice.GetDevices();
                if (devices.Count == 0) {
                    cmbDevice.Items.Clear();
                    cmbDevice.Items.Add("No FTDI devices found");
                    cmbDevice.SelectedIndex = 0;
                } else {
                    var deviceItems = new List<DeviceItem>();
                    foreach (var device in devices) {
                        deviceItems.Add(new DeviceItem(device));
                    }
                    Dispatcher.UIThread.Post(() => {
                        cmbDevice.Items.Clear();
                        foreach (var deviceItem in deviceItems) {
                            cmbDevice.Items.Add(deviceItem);
                        }
                        cmbDevice.SelectedIndex = 0;
                        cmbDevice.IsEnabled = true;
                    });
                }
            } catch (InvalidOperationException) {
                Dispatcher.UIThread.Post(() => {
                    cmbDevice.Items.Clear();
                    cmbDevice.Items.Add("Error accessing FTDI devices");
                    cmbDevice.SelectedIndex = 0;
                });
            }
        });
    }

    public void OnMenuProgram(object sender, RoutedEventArgs e) {
        Console.WriteLine(DesktopScaling);
    }

    public void OnMenuLoadTemplate(object sender, RoutedEventArgs e) {
        //TODO
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
        Medo.Avalonia.AboutBox.ShowDialog(this, new Uri("https://medo64.com/ftprog2/"));
    }

    #endregion Menu

}