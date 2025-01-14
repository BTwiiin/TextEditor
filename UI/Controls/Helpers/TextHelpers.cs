using System.Drawing;
using System.Windows;
using System.Windows.Media;

namespace TextEditor.UI.Controls.Helpers
{
    public class TextHelpers
    {
        private double _fontSize = 16;
        private Typeface _typeface = new Typeface("Consolas");

        public FormattedText CreateFormattedText(string text, double actualWidth)
        {
            var ft = new FormattedText(
                text,
                System.Globalization.CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                _typeface,
                _fontSize,
                Brushes.Black,
                VisualTreeHelper.GetDpi(Application.Current.MainWindow).PixelsPerDip
            )
            {
                MaxTextWidth = Math.Max(0, actualWidth - 10),
                Trimming = TextTrimming.None,
                MaxLineCount = 1000,
                TextAlignment = TextAlignment.Left,
                LineHeight = 16*1.2,
            };
            
            return ft;
        }
        
    }
}