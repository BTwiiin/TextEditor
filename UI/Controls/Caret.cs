using System;
using System.Windows;
using System.Windows.Media;
using TextEditor.UI.Controls.Helpers;

namespace TextEditor.UI.Controls
{
    public class Caret
    {
        private TextHelpers _textHelper = new TextHelpers();
        private int _position;
        private int _caretLine = 0;

        private int _currentLine = 0;

        public int CaretLine
        {
            get => _caretLine;
            set => _caretLine = value;
        }

        public int CurrentLine
        {
            get => _currentLine;
            set => _currentLine = value;
        }

        private double _lineHeight = 18.733333079;

        public int Position 
        {
            get => _position;
            set => _position = value;
        }

        private double _fontSize = 16;

        // Example: blinking handled externally (as you mentioned)
        public bool IsVisible { get; set; }

        public Caret(int start = 0)
        {
            _position = start;
            IsVisible = true;
        }

        // Move the caret left, preventing it from going before the start
        public void MoveLeft() => _position = Math.Max(0, _position - 1);

        // Move the caret right, preventing it from going beyond the rope's length
        public void MoveRight(int maxLength) => _position = Math.Min(_position + 1, maxLength);

        // Compute the position of the caret based on the substring's width
        public Point ComputeCaretPosition(string fullText, double actualWidth)
        {
            // Start at a small left margin (5,5)
            Point startPoint = new Point(5, 5);
            if (string.IsNullOrEmpty(fullText) || _position <= 0)
                return startPoint;

            // Measure text from the beginning up to the caret position
            string partial = fullText.Substring(0, Math.Min(_position, fullText.Length));
            FormattedText ft = _textHelper.CreateFormattedText(partial, actualWidth);

            // Build geometry to find the bounding box of the text
            Geometry geo = ft.BuildHighlightGeometry(startPoint);
            if (geo == null) return startPoint;

            Rect bounds = geo.Bounds;
            double x = bounds.Right;
            double y = bounds.Bottom - ft.Height;

            // Check if the last character was a newline
            char lastChar = partial[^1];
            if (lastChar == '\n')
            {
                x = 5;
                y = bounds.Bottom; 
            }
            
            if (_currentLine > 0)
            {
                y += _currentLine * _lineHeight;
            }

            Console.WriteLine($"Caret position: {x}, {y}.");

            return new Point(x, y);
        }


        public void DrawCaret(DrawingContext dc, double x, double y, double controlWidth, double controlHeight)
        {
            double safeX = Math.Min(x, controlWidth - 2);
            double safeY = Math.Min(y, controlHeight - _fontSize);

            Pen caretPen = new Pen(Brushes.Black, 1);
            dc.DrawLine(caretPen, new Point(safeX, safeY), new Point(safeX, safeY + _fontSize));
        }
    }
}
