using System;
using System.Diagnostics.CodeAnalysis;
using System.Formats.Tar;
using System.Security.Cryptography.X509Certificates;
using System.Windows;
using System.Windows.Media;
using TextEditor.UI.Controls.Helpers;

namespace TextEditor.UI.Controls
{
    public class Caret
    {
        private TextHelpers _textHelper = new TextHelpers();
        private int _offset;

        public int Offset 
        {
            get => _offset;
            set => _offset = value;
        }

        private double _fontSize = 16;

        // Example: blinking handled externally (as you mentioned)
        public bool IsVisible { get; set; }

        public Caret(int start = 0)
        {
            _offset = start;
            IsVisible = true;
        }

        // Move the caret left, preventing it from going before the start
        public void MoveLeft() => _offset = Math.Max(0, _offset - 1);

        // Move the caret right, preventing it from going beyond the rope's length
        public void MoveRight(int maxLength) => _offset = Math.Min(_offset + 1, maxLength);

        // Move the caret up, preventing it from going beyond the start of the text
        public void MoveUp(string fullText)
        {
            if (string.IsNullOrEmpty(fullText) || _offset <= 0)
                return;

            int index = fullText.LastIndexOf('\n', _offset - 1);
            if (index > 0)
            {
                int previousLineIndex = fullText.LastIndexOf('\n', index - 1);
                int previousLineLength = (previousLineIndex >= 0) ? index - previousLineIndex - 1 : index;
                int currentLineStart = fullText.LastIndexOf('\n', _offset - 1) + 1;
                int distanceFromLineStart = _offset - currentLineStart;

                if (distanceFromLineStart < previousLineLength)
                {
                    _offset -= previousLineLength + 1;
                    return;
                }

                _offset = index;

                return;
            }

            return;
        }

        // Move the caret down, preventing it from going beyond the end of the text
        public void MoveDown(string fullText)
        {
            if (string.IsNullOrEmpty(fullText) || _offset >= fullText.Length)
                return;

            int index = fullText.IndexOf('\n', _offset - 1);
            if (index > 0)
            {
                int nextLineIndex = fullText.IndexOf('\n', index + 1);
                int nextLineLength = (nextLineIndex >= 0) ? nextLineIndex - index - 1 : fullText.Length - index - 1;
                int currentLineStart = fullText.LastIndexOf('\n', _offset - 1) + 1;
                int distanceFromLineStart = _offset - currentLineStart;

                int currentLineLength = (currentLineStart >= 0) ? index - currentLineStart : index;

                if (distanceFromLineStart <= nextLineLength)
                {
                    _offset += currentLineLength + 1;
                    return;
                }

                _offset = index + nextLineLength + 1;
                return;
            }

            return;
        }

        // Compute the offset of the caret based on the substring's width
        public Point ComputeCaretOffset(string fullText, double actualWidth)
        {
            Point startPoint = new Point(5, 5);
            if (string.IsNullOrEmpty(fullText) || _offset <= 0)
                return startPoint;
            
            return CalculateLogicalPosition(fullText, actualWidth, startPoint); 
        }


        public void DrawCaret(DrawingContext dc, double x, double y, double controlWidth, double controlHeight)
        {
            double safeX = Math.Min(x, controlWidth - 2);
            double safeY = Math.Min(y, controlHeight - _fontSize);

            Pen caretPen = new Pen(Brushes.Black, 1);
            dc.DrawLine(caretPen, new Point(safeX, safeY), new Point(safeX, safeY + _fontSize));
        }

        private Point CalculateLogicalPosition(string fullText, double actualWidth, Point startPoint)
        {
            int currentLine = 0;
            string partial = fullText.Substring(0, Math.Min(_offset, fullText.Length));
            FormattedText ft = _textHelper.CreateFormattedText(partial, actualWidth);

            double lineHeight = ft.LineHeight;

            // Build geometry to find the bounding box of the text
            Geometry geo = ft.BuildHighlightGeometry(startPoint);
            if (geo == null) return startPoint;

            Rect bounds = geo.Bounds;
            double x = bounds.Right;
            double y = bounds.Bottom - ft.Height;
            
            if (partial.Contains("\n"))
            {
                int caretLine = partial.Count(c => c == '\n');
                // We actually have caretLine+1 total lines
                int totalLines = caretLine + 1;

                for (int i = 0; i < caretLine; i++)
                {
                    int index = partial.IndexOf('\n');
                    if (_offset > index)
                    {
                        currentLine++;
                        x = 5;
                    }
                    if (index >= 0)
                    {
                        partial = partial.Substring(index + 1);
                        x = _textHelper.CreateFormattedText(partial, actualWidth).WidthIncludingTrailingWhitespace + 5;
                    }
                }

                Console.WriteLine($"totalLines: {totalLines}");
                Console.WriteLine($"currentLine: {currentLine}");
                Console.WriteLine($"ft.Height: {ft.Height}");
                Console.WriteLine($"lineHeight: {lineHeight}");
                y += currentLine * lineHeight;

                return new Point(x, y);
            }

            return new Point(x, y);
        }

    }
}
