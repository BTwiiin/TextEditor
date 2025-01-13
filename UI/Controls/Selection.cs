using System.Windows;
using System.Windows.Media;
using TextEditor.UI.Controls.Helpers;

namespace TextEditor.UI.Controls
{
    public class Selection
    {

        private TextHelpers _textHelper = new TextHelpers();
        public int Start { get; private set; } = -1;
        public int End { get; private set; } = -1;

        public bool HasSelection => Start >= 0 && End > Start;

        public void Clear()
        {
            Start = -1;
            End = -1;
        }

        public void SetRange(int start, int end)
        {
            if (start < 0 || end < 0)
            {
                Clear();
                return;
            }
            Start = Math.Min(start, end);
            End = Math.Max(start, end);
        }

        public (int, int) GetOrderedRange()
        {
            return (Math.Min(Start, End), Math.Max(Start, End));
        }

        // Method for building the selection geometry
        public Geometry? BuildSelectionGeometry(string fullText, Point startPoint, double actualWidth)
        {
            if (Start >= End) return null;

            // Substring from 0..selectionEnd
            string endText = fullText.Substring(0, End);
            FormattedText ftEnd = _textHelper.CreateFormattedText(endText, actualWidth);
            Geometry geomEnd = ftEnd.BuildHighlightGeometry(startPoint);

            if (geomEnd == null) return null;

            // Substring from 0..selectionStart
            string startText = fullText.Substring(0, Start); // Corrected to use startText here
            FormattedText ftStart = _textHelper.CreateFormattedText(startText, actualWidth);
            Geometry geomStart = ftStart.BuildHighlightGeometry(startPoint);

            // Subtract to get the selection geometry
            Geometry selection = geomEnd.Clone();
            if (geomStart != null)
            {
                selection = Geometry.Combine(selection, geomStart, GeometryCombineMode.Exclude, null);
            }

            return selection;
        }
    }
}
