/* Josip Medved <jmedved@jmedved.com> * www.medo64.com * MIT License */

//2024-01-01: Initial version

namespace Medo.Avalonia;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using global::Avalonia;
using global::Avalonia.Controls;
using global::Avalonia.Input;
using global::Avalonia.Interactivity;
using global::Avalonia.Layout;
using global::Avalonia.Media;
using global::Avalonia.Threading;

/// <summary>
/// Feedback report dialog.
/// </summary>
public static class FeedbackBox {

    /// <summary>
    /// Shows dialog and sends feedback to support if user chooses so.
    /// Returns true if feedback was successfully sent.
    /// </summary>
    /// <param name="owner">Window that owns this window.</param>
    /// <param name="serviceUrl">Service URL which will receive data..</param>
    public static void ShowDialog(Window owner, Uri serviceUrl) {
        ShowDialog(owner, serviceUrl, null);
    }

    /// <summary>
    /// Shows dialog and sends feedback to support if user chooses so.
    /// Returns true if feedback was successfully sent.
    /// </summary>
    /// <param name="owner">Window that owns this window.</param>
    /// <param name="serviceUrl">Service URL which will receive data..</param>
    /// <param name="exception">Exception to include.</param>
    public static void ShowDialog(Window owner, Uri serviceUrl, Exception? exception) {
        var window = new Window() {
            Title = (exception != null) ? "Error report" : "Feedback",
            Width = 600,
            CanResize = false,
            SizeToContent = SizeToContent.Height,
        };
        if (owner != null) {
            window.Icon = owner.Icon;
            window.ShowInTaskbar = false;
            if (owner.Topmost) { window.Topmost = true; }
            window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
        } else {  // just in case null is passed, not ideal
            window.ShowInTaskbar = true;
            window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
        }
        window.ShowActivated = true;
        window.SystemDecorations = SystemDecorations.BorderOnly;
        window.ExtendClientAreaToDecorationsHint = true;

        var mainStack = new StackPanel() { Margin = new Thickness(11) };

        var captionLabel = new Label() {
            Content = (exception != null)
                    ? "What were you doing when the exception occurred?"
                    : "What do you wish to give feedback on?",
        };
        mainStack.Children.Add(captionLabel);

        var reportText = new TextBox() {
            AcceptsReturn = true,
            TextWrapping = TextWrapping.Wrap,
            MinHeight = 80,
            MaxHeight = 240,
        };
        mainStack.Children.Add(reportText);


        var userGrid = new Grid();
        userGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Auto) });
        userGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
        userGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Auto) });
        userGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Auto) });

        var emailLabel = new Label() {
            Content = "E-mail (optional):",
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 11, 11, 0),
        };
        emailLabel.SetValue(Grid.ColumnProperty, 0);
        emailLabel.SetValue(Grid.RowProperty, 0);
        userGrid.Children.Add(emailLabel);

        var emailTextBox = new TextBox() {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 11, 0, 0),
        };
        emailTextBox.SetValue(Grid.ColumnProperty, 1);
        emailTextBox.SetValue(Grid.RowProperty, 0);
        userGrid.Children.Add(emailTextBox);

        var nameLabel = new Label() {
            Content = "Name (optional):",
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 11, 11, 0),
        };
        nameLabel.SetValue(Grid.ColumnProperty, 0);
        nameLabel.SetValue(Grid.RowProperty, 1);

        userGrid.Children.Add(nameLabel);
        var nameTextBox = new TextBox() {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 11, 0, 0),
        };
        nameTextBox.SetValue(Grid.ColumnProperty, 1);
        nameTextBox.SetValue(Grid.RowProperty, 1);
        userGrid.Children.Add(nameTextBox);

        mainStack.Children.Add(userGrid);

        var additionalInfo = new List<KeyValuePair<string, string>>();
        additionalInfo.Add(new KeyValuePair<string, string>("Environment", GetEnvironmentText()));
        if (exception != null) {
            additionalInfo.Add(new KeyValuePair<string, string>("Exception", GetExceptionText(exception)));
        }

        var additionalInfoLabel = new Label() {
            Content = "Additional information that will be sent:",
            Margin = new Thickness(0, 11, 0, 0),
        };
        mainStack.Children.Add(additionalInfoLabel);

        var topSpacing = 0;
        foreach (var info in additionalInfo) {
            var infoExpander = new Expander() {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Padding = new Thickness(0),
                Header = info.Key,
                Content = new TextBox() {
                    Text = info.Value,
                    AcceptsReturn = true,
                    TextWrapping = TextWrapping.NoWrap,
                    IsReadOnly = true,
                    BorderThickness = new Thickness(0),
                    MaxHeight = 240,
                },
                Margin = new Thickness(0, topSpacing, 0, 0),
            };
            mainStack.Children.Add(infoExpander);
            topSpacing = 7;
        }

        var windowStack = new StackPanel();
        var buttonDockPanel = new DockPanel() { Margin = new Thickness(11) };

        var statusLabel = new Label() {
            Content = "",
            HorizontalAlignment = HorizontalAlignment.Left,
            HorizontalContentAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(11, 0),
    };

        var sendButton = new Button() {
            Content = "Send",
            IsDefault = true,
            HorizontalAlignment = HorizontalAlignment.Right,
            HorizontalContentAlignment = HorizontalAlignment.Center,
            Tag = (serviceUrl, window, windowStack, statusLabel, reportText, emailTextBox, nameTextBox, additionalInfo.ToArray()),
        };
        sendButton.Click += SendClick;
        sendButton.SetValue(DockPanel.DockProperty, Dock.Left);
        buttonDockPanel.Children.Add(sendButton);

        var closeButton = new Button() {
            Content = "Close",
            IsCancel = true,
            HorizontalAlignment = HorizontalAlignment.Right,
            HorizontalContentAlignment = HorizontalAlignment.Center,
            Tag = window,
        };
        closeButton.Click += CloseClick;
        closeButton.SetValue(DockPanel.DockProperty, Dock.Right);
        buttonDockPanel.Children.Add(closeButton);
        buttonDockPanel.Children.Add(statusLabel);

        windowStack.Children.Add(new Border() { Child = mainStack });
        windowStack.Children.Add(buttonDockPanel);

        var windowBorder = new Border {
            BorderThickness = new Thickness(1),
            BorderBrush = (exception != null)
                        ? Brushes.Red
                        : GetWindowBorderBrush(),
            Child = windowStack
        };

        window.Content = windowBorder;
        window.Activated += delegate { reportText.Focus(); };

        if (owner != null) {
            window.ShowDialog(owner);
        } else {
            window.Show();
        }
    }


    private static void SendClick(object? sender, RoutedEventArgs e) {
        var button = (Button)sender!;
        (Uri serviceUrl, Window window, StackPanel windowStack, Label statusLabel, TextBox reportBox, TextBox emailBox, TextBox nameBox, KeyValuePair<string, string>[] additionalInfo)
            = ((Uri, Window, StackPanel, Label, TextBox, TextBox, TextBox, KeyValuePair<string, string>[]))button.Tag!;

        button.IsEnabled = false;
        window.Cursor = new Cursor(StandardCursorType.Wait);

        var reportText = reportBox?.Text?.Trim() ?? "";
        var emailText = emailBox?.Text?.Trim() ?? "";
        var nameText = nameBox?.Text?.Trim() ?? "";
        if (string.IsNullOrEmpty(reportText)) { reportText = "-"; }

        var assembly = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
        var product = GetAppProduct(assembly);
        var version = GetAppVersion(assembly);

        var sbMessage = new StringBuilder();
        sbMessage.Append(!string.IsNullOrEmpty(reportText) ? reportText : "-");
        sbMessage.AppendLine();
        sbMessage.AppendLine();
        foreach (var info in additionalInfo) {
            sbMessage.AppendLine();
            sbMessage.AppendLine("[" + info.Key + "]");
            sbMessage.AppendLine(info.Value);
        }
        var message = sbMessage.ToString().Trim();

        var contentList = new List<KeyValuePair<string, string>>();
        contentList.Add(new KeyValuePair<string, string>("Product", product));
        contentList.Add(new KeyValuePair<string, string>("Version", version));
        contentList.Add(new KeyValuePair<string, string>("Message", message));
        if (!string.IsNullOrEmpty(emailText)) { contentList.Add(new KeyValuePair<string, string>("Email", emailText)); }
        if (!string.IsNullOrEmpty(nameText)) { contentList.Add(new KeyValuePair<string, string>("DisplayName", nameText)); }

        foreach (var child in windowStack.Children) {
            child.IsEnabled = false;
        }
        statusLabel.Content = "Sending...";

        ThreadPool.QueueUserWorkItem((s) => {
            var errorResponse = "Unknown error.";
            try {
                using var client = new HttpClient();
                var content = new FormUrlEncodedContent(contentList);
                using var response = client.PostAsync(serviceUrl, content).Result;

                if (response.StatusCode == HttpStatusCode.OK) {
                    using var reader = new StreamReader(response.Content.ReadAsStream());
                    var responseFromServer = reader.ReadToEnd();
                    if (responseFromServer.Length == 0) { //no data is outputed in case of real 200 response (instead of 500 wrapped in generic 200 page)
                        Dispatcher.UIThread.Post(() => {
                            statusLabel.Content = "Sent.";
                            window.Cursor = Cursor.Default;
                        });
                        Thread.Sleep(1000);
                        Dispatcher.UIThread.Post(() => {
                            window.Close();
                        });
                        return;
                    } else {
                        errorResponse = "Unexpected server response.";
                    }
                } else {
                    errorResponse = "Unexpected server response (" + ((int)response.StatusCode).ToString(CultureInfo.InvariantCulture) + ").";
                }
            } catch (Exception ex) {
                errorResponse = "Unexpected error: " + ex.Message;
            }

            Dispatcher.UIThread.Post(() => {
                statusLabel.Content = errorResponse;
            });
            Dispatcher.UIThread.Post(() => {
                foreach (var child in windowStack.Children) {
                    child.IsEnabled = true;
                }
                button.IsEnabled = true;
                window.Cursor = Cursor.Default;
            });
        });
    }

    private static void CloseClick(object? sender, RoutedEventArgs e) {
        if (sender is Control control && control.Tag is Window window) {
            window.Close();
        }
    }


    private static string GetAppProduct(Assembly assembly) {
        var productAttributes = assembly.GetCustomAttributes(typeof(AssemblyProductAttribute), true);
        if ((productAttributes != null) && (productAttributes.Length >= 1)) {
            return ((AssemblyProductAttribute)productAttributes[^1]).Product;
        } else {
            return GetAppTitle(assembly);
        }
    }

    private static string GetAppTitle(Assembly assembly) {
        var titleAttributes = assembly.GetCustomAttributes(typeof(AssemblyTitleAttribute), true);
        if ((titleAttributes != null) && (titleAttributes.Length >= 1)) {
            return ((AssemblyTitleAttribute)titleAttributes[^1]).Title;
        } else {
            return assembly.GetName().Name ?? "";
        }
    }

    private static string GetAppVersion(Assembly assembly) {
        var version = assembly.GetName().Version;
        if (version != null) {
            return $"{version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
        }
        return "";
    }

    private static readonly string[] NewLineArray = new string[] { "\n\r", "\n", "\r" };

    private static string? GetOSPrettyName() {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) {
            try {
                var osReleaseLines = File.ReadAllText("/etc/os-release").Split(NewLineArray, StringSplitOptions.RemoveEmptyEntries);
                foreach (var line in osReleaseLines) {
                    if (line.StartsWith("PRETTY_NAME=", StringComparison.OrdinalIgnoreCase)) {
                        var text = line[12..].Trim();
                        if (text.StartsWith('"') && text.EndsWith('"')) {
                            return text[1..^1];
                        }
                    }
                }
            } catch (SystemException) { }
        }
        return null;
    }


    private static ISolidColorBrush GetWindowBorderBrush() {
        if (Application.Current?.Styles[0] is IResourceProvider provider) {
            if (provider.TryGetResource("TextControlBorderBrush", Application.Current?.ActualThemeVariant!, out var resourceObject)
             || provider.TryGetResource("ThemeBorderLowBrush", Application.Current?.ActualThemeVariant!, out resourceObject)) {
                if (resourceObject is SolidColorBrush brush) {
                    return brush;
                }
            }
        }
        return Brushes.Black;
    }

    private static string GetEnvironmentText() {
        var text = new StringBuilder();

        var assembly = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
#if DEBUG
        text.AppendLine(GetAppTitle(assembly) + " " + GetAppVersion(assembly) + " DEBUG");
#else
        text.AppendLine(GetAppTitle(assembly) + " " + GetAppVersion(assembly));
#endif
        text.AppendLine(".NET Framework " + Environment.Version.ToString());
        text.AppendLine(Environment.OSVersion.ToString());
        text.AppendLine(GetOSPrettyName());
        var xdgSessionType = Environment.GetEnvironmentVariable("XDG_SESSION_TYPE");
        if (!string.IsNullOrEmpty(xdgSessionType)) {
            text.AppendLine("XDG_SESSION_TYPE=" + xdgSessionType);
        }

        return text.ToString().TrimEnd();
    }

    private static string GetExceptionText(Exception exception) {
        var text = new StringBuilder();

        var ex = exception;
        var exLevel = 0;
        while (ex != null) {
            if (exLevel == 0) {
                text.AppendLine(ex.GetType().FullName + ":");
            } else if (exLevel == 1) {
                text.AppendLine(ex.GetType().FullName + " (1):");
            } else if (exLevel == 2) {
                text.AppendLine(ex.GetType().FullName + " (2):");
            } else {
                text.AppendLine(ex.GetType().FullName + " (...):");
            }
            text.AppendLine("  \"" + ex.Message + "\"");
            if (!string.IsNullOrEmpty(ex.StackTrace)) {
                var stackLines = ex.StackTrace.Split(NewLineArray, StringSplitOptions.RemoveEmptyEntries);
                foreach (var stackLine in stackLines) {
                    text.AppendLine("  " + stackLine.Trim());
                }
            }

            ex = ex.InnerException;
            exLevel += 1;
        }

        var assembly = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
        if (assembly != null) {
            text.AppendLine("Referenced assemblies:");
            var referencedAssemblies = assembly.GetReferencedAssemblies();
            if (referencedAssemblies.Length > 0) {
                foreach (var referencedAssembly in assembly.GetReferencedAssemblies()) {
                    text.AppendLine("  " + referencedAssembly.FullName);
                }
            } else {
                text.AppendLine("(none)");
            }
        }

        return text.ToString().TrimEnd();
    }

}
