namespace AltFTProgGui;
using System;
using System.Globalization;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Styling;

internal static class Helpers {

    public static void SetToolbarIcons(Window window, Grid toolbarGrid, params (Image image, string iconName)[] imagesAndIconsNames) {
        double iconSize = Math.Ceiling(16 * window.RenderScaling * 1.25) switch {  // must be double to be used in styles
            <= 16 => 16,
            <= 24 => 24,
            <= 32 => 32,
            <= 48 => 48,
            _ => 64,
        };
        double textSizePt = (iconSize * 0.75) * 0.8;  // (1pt=0.75px) and then 20% smaller
        var isDark = "Dark".Equals(window.ActualThemeVariant.ToString(), StringComparison.OrdinalIgnoreCase);
        var foregroundBrush = new SolidColorBrush(!isDark ? Color.FromRgb(45, 71, 114) : Color.FromRgb(144, 175, 225));

        {  // styles
            var buttonStyles = new Style(x => x.OfType<Button>());
            buttonStyles.Setters.Add(new Setter(Button.HeightProperty, iconSize));
            toolbarGrid.Styles.Add(buttonStyles);

            var menuItemStyles = new Style(x => x.OfType<MenuItem>());
            menuItemStyles.Setters.Add(new Setter(MenuItem.ForegroundProperty, foregroundBrush));
            toolbarGrid.Styles.Add(menuItemStyles);

            var menuItemDropDownStyles = new Style(x => x.OfType<MenuItem>().Class("DropDown"));
            menuItemDropDownStyles.Setters.Add(new Setter(MenuItem.FontSizeProperty, textSizePt));
            toolbarGrid.Styles.Add(menuItemDropDownStyles);

            var labelStyles = new Style(x => x.OfType<Label>());
            labelStyles.Setters.Add(new Setter(Label.HeightProperty, iconSize));
            labelStyles.Setters.Add(new Setter(Label.FontSizeProperty, textSizePt));
            labelStyles.Setters.Add(new Setter(Label.ForegroundProperty, foregroundBrush));
            toolbarGrid.Styles.Add(labelStyles);
        }

        foreach (var item in imagesAndIconsNames) {
            var image = item.image;
            var iconName = item.iconName;
            var url = "avares://altftprogui/Assets/Toolbar/"
                    + iconName
                    + "_"
                    + iconSize.ToString(CultureInfo.InvariantCulture)
                    + (isDark ? "D" : "")
                    + ".png";
            image.Source = new Bitmap(AssetLoader.Open(new Uri(url)));
        }
    }

}
