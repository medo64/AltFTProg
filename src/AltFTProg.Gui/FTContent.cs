namespace AltFTProgGui;
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

    public static void NewTextRow(Grid grid, string caption, string value, bool isEnabled = true) {
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

        var text = new TextBox {
            Text = value,
            IsEnabled = isEnabled,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 3),
        };
        text.SetValue(Grid.ColumnProperty, 1);
        text.SetValue(Grid.RowProperty, row);
        grid.Children.Add(text);
    }

    public static void NewCheckRow(Grid grid, string caption, bool value, bool isEnabled = true) {
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

        var check = new CheckBox {
            IsChecked = value,
            IsEnabled = isEnabled,
        HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 3),
        };
        check.SetValue(Grid.ColumnProperty, 1);
        check.SetValue(Grid.RowProperty, row);
        grid.Children.Add(check);
    }

    public static void NewComboRow(Grid grid, string caption, string value) {
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

        var combo = new ComboBox {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 3),
        };
        combo.SetValue(Grid.ColumnProperty, 1);
        combo.SetValue(Grid.RowProperty, row);
        grid.Children.Add(combo);
    }

}
