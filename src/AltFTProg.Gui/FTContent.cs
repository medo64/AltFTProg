namespace AltFTProgGui;
using System;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;

internal static class FTContent {

    public static TabItem NewTab(string text, out Grid grid) {
        var tabGrid = new Grid();
        tabGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(0.33, GridUnitType.Star) });
        tabGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(0.67, GridUnitType.Star) });

        var tab = new TabItem() {
            Header = text,
            Content = new ScrollViewer() { Content = tabGrid },
        };

        grid = tabGrid;
        return tab;
    }

    public static void NewSeparatorRow(Grid grid) {
        var row = grid.RowDefinitions.Count;
        grid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Auto) });
        var separator = new Separator() {
            Height = 3,
            Background = Brushes.Transparent,
        };
        separator.SetValue(Grid.ColumnProperty, 1);
        separator.SetValue(Grid.RowProperty, row);
        grid.Children.Add(separator);
    }

    public static TextBox NewStringRow(Action refreshAction, Grid grid, string caption, Func<string> value, Func<string, bool>? validate = null, Action<string>? apply = null, Func<string>? button = null, bool isEnabled = true) {
        var row = grid.RowDefinitions.Count;
        grid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Auto) });

        var label = new Label() {
            Content = caption + ":",
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 7, 0),
        };
        label.SetValue(Grid.ColumnProperty, 0);
        label.SetValue(Grid.RowProperty, row);
        grid.Children.Add(label);

        var dock = new DockPanel() { };

        var box = new TextBox {
            Text = value.Invoke(),
            IsEnabled = isEnabled,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 3),
        };
        box.TextChanged += delegate {
            var oldValue = value.Invoke();
            var newValue = box.Text;
            var isOk = validate?.Invoke(newValue) ?? true;
            if (isOk) {
                if (oldValue.Equals(newValue)) { return; }
                try {
                    if (oldValue.Equals(newValue)) { return; }
                    apply?.Invoke(newValue);
                    refreshAction.Invoke();
                    box.Foreground = GetForegroundBrush();
                } catch (ArgumentException ex) {
                    box.Foreground = GetForegroundBrush(isError: true);
                    ToolTip.SetTip(box, ex.Message);
                }
            } else {
                box.Foreground = GetForegroundBrush(isError: true);
                ToolTip.SetTip(box, "Validation error");
            }
        };

        if (button != null) {
            var buttonBox = new Button() {
                Content = "Regenerate",
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(3, 0),
            };
            buttonBox.Click += delegate {
                var value = button.Invoke();
                box.Text = value;
            };
            buttonBox.SetValue(DockPanel.DockProperty, Dock.Right);
            dock.Children.Add(buttonBox);
        }
        dock.Children.Add(box);  // to fill dock panel

        dock.SetValue(Grid.ColumnProperty, 1);
        dock.SetValue(Grid.RowProperty, row);
        grid.Children.Add(dock);

        return box;
    }

    public static TextBox NewHexRow(Action refreshAction, Grid grid, string caption, Func<int> value, Func<int, bool>? validate = null, Action<int>? apply = null, bool isEnabled = true) {
        var row = grid.RowDefinitions.Count;
        grid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Auto) });

        var label = new Label() {
            Content = caption + ":",
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 7, 0),
        };
        label.SetValue(Grid.ColumnProperty, 0);
        label.SetValue(Grid.RowProperty, row);
        grid.Children.Add(label);

        var dock = new DockPanel() { };
        dock.Children.Add(new Label() {
            Content = "0x",
            VerticalAlignment = VerticalAlignment.Center
        });

        var box = new TextBox {
            Text = value.Invoke().ToString("X4", CultureInfo.InvariantCulture),
            IsEnabled = isEnabled,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 3),
        };
        box.TextChanged += delegate {
            var oldValue = value.Invoke();
            var isOk = int.TryParse(box.Text, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var newValue) && (validate?.Invoke(newValue) ?? true);
            if (isOk) {
                if (oldValue.Equals(newValue)) { return; }
                try {
                    if (oldValue.Equals(newValue)) { return; }
                    apply?.Invoke(newValue);
                    refreshAction.Invoke();
                    box.Foreground = GetForegroundBrush();
                } catch (ArgumentException ex) {
                    box.Foreground = GetForegroundBrush(isError: true);
                    ToolTip.SetTip(box, ex.Message);
                }
            } else {
                box.Foreground = GetForegroundBrush(isError: true);
                ToolTip.SetTip(box, "Validation error (must be a hexadecimal number)");
            }
        };
        dock.Children.Add(box);  // to fill dock panel

        dock.SetValue(Grid.ColumnProperty, 1);
        dock.SetValue(Grid.RowProperty, row);
        grid.Children.Add(dock);

        return box;
    }

    public static TextBox NewIntegerRow(Action refreshAction, Grid grid, string caption, Func<int> value, Func<int, bool>? validate = null, Action<int>? apply = null, string? unit = null, bool isEnabled = true) {
        var row = grid.RowDefinitions.Count;
        grid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Auto) });

        var label = new Label() {
            Content = caption + ":",
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 7, 0),
        };
        label.SetValue(Grid.ColumnProperty, 0);
        label.SetValue(Grid.RowProperty, row);
        grid.Children.Add(label);

        var dock = new DockPanel() { };
        if (unit != null) {
            var unitLabel = new Label() {
                Content = unit,
                VerticalAlignment = VerticalAlignment.Center
            };
            unitLabel.SetValue(DockPanel.DockProperty, Dock.Right);
            dock.Children.Add(unitLabel);
        }

        var box = new TextBox {
            Text = value.Invoke().ToString(CultureInfo.InvariantCulture),
            IsEnabled = isEnabled,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 3),
        };
        box.TextChanged += delegate {
            var oldValue = value.Invoke();
            var isOk = int.TryParse(box.Text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var newValue) && (validate?.Invoke(newValue) ?? true);
            if (isOk) {
                try {
                    if (oldValue.Equals(newValue)) { return; }
                    apply?.Invoke(newValue);
                    refreshAction.Invoke();
                    box.Foreground = GetForegroundBrush();
                } catch (ArgumentException ex) {
                    box.Foreground = GetForegroundBrush(isError: true);
                    ToolTip.SetTip(box, ex.Message);
                }
            } else {
                box.Foreground = GetForegroundBrush(isError: true);
                ToolTip.SetTip(box, "Validation error (must be a number)");
            }
        };
        dock.Children.Add(box);  // to fill dock panel

        dock.SetValue(Grid.ColumnProperty, 1);
        dock.SetValue(Grid.RowProperty, row);
        grid.Children.Add(dock);

        return box;
    }

    public static CheckBox NewBooleanRow(Action refreshAction, Grid grid, string caption, Func<bool> value, Func<bool, bool>? validate = null, Action<bool>? apply = null, bool isEnabled = true) {
        var row = grid.RowDefinitions.Count;
        grid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Auto) });

        var label = new Label() {
            Content = caption + ":",
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 7, 0),
        };
        label.SetValue(Grid.ColumnProperty, 0);
        label.SetValue(Grid.RowProperty, row);
        grid.Children.Add(label);

        var box = new CheckBox {
            IsChecked = value.Invoke(),
            IsEnabled = isEnabled,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 3),
        };
        box.IsCheckedChanged += delegate {
            var oldValue = value.Invoke();
            var newValue = box.IsChecked ?? false;
            if (oldValue.Equals(newValue)) { return; }
            apply?.Invoke(newValue);
            refreshAction.Invoke();
        };
        box.SetValue(Grid.ColumnProperty, 1);
        box.SetValue(Grid.RowProperty, row);
        grid.Children.Add(box);

        return box;
    }

    public static ComboBox NewEnumRow<T>(Action refreshAction, Grid grid, string caption, Func<T> value, Action<T>? apply = null, bool isEnabled = true) where T : struct, Enum {
        var row = grid.RowDefinitions.Count;
        grid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Auto) });

        var label = new Label() {
            Content = caption + ":",
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 7, 0),
        };
        label.SetValue(Grid.ColumnProperty, 0);
        label.SetValue(Grid.RowProperty, row);
        grid.Children.Add(label);

        var box = new ComboBox {
            IsEnabled = isEnabled,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 3),
        };

        var selectedValue = value.Invoke();
        ComboEnumItem<T>? selectedItem = null;
        var enumValues = Enum.GetValues<T>();
        foreach (var enumValue in enumValues) {
            var newItem = new ComboEnumItem<T>(enumValue);
            if (enumValue.Equals(selectedValue)) { selectedItem = newItem; }
            box.Items.Add(newItem);
        }
        if (selectedItem != null) { box.SelectedItem = selectedItem; }

        box.SelectionChanged += delegate {
            if (box.SelectedItem is ComboEnumItem<T> selectedItem) {
                try {
                    var oldValue = value.Invoke();
                    var newValue = selectedItem.Value;
                    if (oldValue.Equals(newValue)) { return; }
                    apply?.Invoke(newValue);
                    refreshAction.Invoke();
                    box.Foreground = GetForegroundBrush();
                } catch (ArgumentException ex) {
                    box.Foreground = GetForegroundBrush(isError: true);
                    ToolTip.SetTip(box, ex.Message);
                }
            } else {
                box.Foreground = GetForegroundBrush(isError: true);
                ToolTip.SetTip(box, "");
            }
        };
        box.SetValue(Grid.ColumnProperty, 1);
        box.SetValue(Grid.RowProperty, row);
        grid.Children.Add(box);

        return box;
    }

    public static ComboBox NewTupleRow<T>(Action refreshAction, Grid grid, string caption, (T value, string title)[] items, Func<T> value, Action<T>? apply = null, bool isEnabled = true) where T : struct {
        var row = grid.RowDefinitions.Count;
        grid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Auto) });

        var label = new Label() {
            Content = caption + ":",
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 7, 0),
        };
        label.SetValue(Grid.ColumnProperty, 0);
        label.SetValue(Grid.RowProperty, row);
        grid.Children.Add(label);

        var box = new ComboBox {
            IsEnabled = isEnabled,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 3),
        };

        var selectedValue = value.Invoke();
        TupleItem<T>? selectedItem = null;
        foreach (var item in items) {
            var newItem = new TupleItem<T>(item.title, item.value);
            if (item.value.Equals(selectedValue)) { selectedItem = newItem; }
            box.Items.Add(newItem);
        }
        if (selectedItem != null) { box.SelectedItem = selectedItem; }

        box.SelectionChanged += delegate {
            if (box.SelectedItem is TupleItem<T> selectedItem) {
                try {
                    var oldValue = value.Invoke();
                    var newValue = selectedItem.Value;
                    if (oldValue.Equals(newValue)) { return; }
                    apply?.Invoke(newValue);
                    refreshAction.Invoke();
                    box.Foreground = GetForegroundBrush();
                } catch (ArgumentException ex) {
                    box.Foreground = GetForegroundBrush(isError: true);
                    ToolTip.SetTip(box, ex.Message);
                }
            } else {
                box.Foreground = GetForegroundBrush(isError: true);
                ToolTip.SetTip(box, "");
            }
        };
        box.SetValue(Grid.ColumnProperty, 1);
        box.SetValue(Grid.RowProperty, row);
        grid.Children.Add(box);

        return box;
    }

    private static ISolidColorBrush GetForegroundBrush(bool isError = false) {
        if (isError) { return Brushes.Red; }

        if (Application.Current?.Styles[0] is IResourceProvider provider) {
            if (provider.TryGetResource("TextControlForeground", Application.Current?.ActualThemeVariant!, out var resourceObject)) {
                if (resourceObject is SolidColorBrush brush) {
                    return brush;
                }
            }
        }
        return Brushes.Black;
    }

}
