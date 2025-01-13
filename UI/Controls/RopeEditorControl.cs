using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using TextEditor.UI.Controls.Helpers;

namespace TextEditor.UI.Controls
{
    public class RopeEditorControl : Control
    {
        public TextEditorApp _textEditorApp { get; set; }

        private TextHelpers _textHelper = new TextHelpers();
        private Caret _caret = new Caret(0);
        private Selection _selection = new Selection();

        // --- TYPING SESSION FIELDS ---
        private bool _typingSessionActive = false;
        private StringBuilder _typingBuffer = new StringBuilder();
        private int _typingStart = 0;
        private DateTime _lastTypingTime = DateTime.MinValue;
        private const int TYPING_TIMEOUT_MS = 1000; // 1 second
        // -----------------------------

        private DispatcherTimer _caretTimer;
        private bool _caretBlinkVisible = true;

        static RopeEditorControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(RopeEditorControl),
                new FrameworkPropertyMetadata(typeof(RopeEditorControl)));
        }

        public RopeEditorControl()
        {
            Focusable = true;
            _textEditorApp = TextEditorApp.Instance;

            // blinking caret timer
            _caretTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(500)
            };
            _caretTimer.Tick += (s, e) =>
            {
                // If there's a selection, caret typically not blinking
                if (_selection.HasSelection) 
                {
                    _caretBlinkVisible = false;
                }
                else
                {
                    _caretBlinkVisible = !_caretBlinkVisible;
                }
                InvalidateVisual();
            };
            _caretTimer.Start();
        }
        // Make sure caret is in range if control gets focus
        protected override void OnGotFocus(RoutedEventArgs e)
        {
            base.OnGotFocus(e);
            if (_textEditorApp != null)
            {
                if (_caret.Position > _textEditorApp.RopeLength)
                    _caret.Position = _textEditorApp.RopeLength;
            }
            InvalidateVisual();
        }

        // ========== TEXT INPUT ==========

        protected override void OnTextInput(TextCompositionEventArgs e)
        {
            base.OnTextInput(e);
            if (_textEditorApp == null) return;

            string typed = e.Text;
            if (string.IsNullOrEmpty(typed))
                return;

            // If there's a selection, remove it "live"
            if (_selection.HasSelection)
            {
                int start = Math.Min(_selection.Start, _selection.End);
                int length = Math.Abs(_selection.End - _selection.Start);
                // remove from rope "live"
                _textEditorApp._rope.Delete(start, length);
                _caret.Position = start;
                ClearSelection();

                // Also end any existing typing session
                FinalizeTypingSession();
            }

            // Check if we are continuing the same typing session
            bool newSessionNeeded = false;
            if (!_typingSessionActive)
            {
                newSessionNeeded = true;
            }
            else
            {
                // if user paused too long, finalize old session
                double msSinceLast = (DateTime.Now - _lastTypingTime).TotalMilliseconds;
                if (msSinceLast > TYPING_TIMEOUT_MS)
                {
                    FinalizeTypingSession();
                    newSessionNeeded = true;
                }
            }

            if (newSessionNeeded)
            {
                _typingSessionActive = true;
                _typingBuffer.Clear();
                _typingStart = _caret.Position;
            }

            // "Live" insert so user sees typed char
            _textEditorApp._rope.Insert(_caret.Position, typed);
            _caret.Position += typed.Length;

            // Accumulate into the typing buffer
            _typingBuffer.Append(typed);
            _lastTypingTime = DateTime.Now;

            e.Handled = true;
            InvalidateVisual();
        }

        // ========== KEY DOWN ==========

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (_textEditorApp == null) return;

            bool handled = false;
            bool isCtrl = (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control;
            bool isShift = (Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift;

            // If user pressed arrow, back, delete, or enter => finalize typingc
            if (e.Key == Key.Left || e.Key == Key.Right ||
                e.Key == Key.Up || e.Key == Key.Down ||
                e.Key == Key.Back || e.Key == Key.Delete ||
                e.Key == Key.Enter)
            {
                FinalizeTypingSession();
            }

            // Example: Ctrl+Z => Undo
            if (isCtrl && e.Key == Key.Z)
            {
                FinalizeTypingSession();
                _textEditorApp.Undo();
                handled = true;

                if (_caret.Position > _textEditorApp.RopeLength)
                    _caret.Position = _textEditorApp.RopeLength;

                InvalidateVisual();
                Focus(); // Keep focus
            }
            else if (isCtrl && e.Key == Key.Y)
            {
                // Optionally: Ctrl+Y => Redo
                _textEditorApp.Redo();
                handled = true;

                if (_caret.Position > _textEditorApp.RopeLength)
                    _caret.Position = _textEditorApp.RopeLength;

                InvalidateVisual();
                Focus();
            }
            else if (isCtrl && e.Key == Key.A)
            {
                // Ctrl + A => select all
                if (_textEditorApp.RopeLength > 0)
                {
                    _selection.SetRange(0, _textEditorApp.RopeLength);
                    _caret.Position = _textEditorApp.RopeLength;
                }
                handled = true;
            }
            else
            {
                switch (e.Key)
                {
                    case Key.Back:
                        if (_caret.Position > 0 || _selection.HasSelection)
                        {
                            if (_selection.HasSelection)
                            {
                                int start = Math.Min(_selection.Start, _selection.End);
                                int length = Math.Abs(_selection.End - _selection.Start);
                                // remove live
                                _textEditorApp._rope.Delete(start, length);
                                _caret.Position = start;
                                ClearSelection();
                            }
                            else
                            {
                                _textEditorApp._rope.Delete(_caret.Position - 1, 1);
                                _caret.Position--;
                            }
                            handled = true;
                        }
                        break;
                    case Key.Left:
                        _caret.MoveLeft();
                        InvalidateVisual();
                        e.Handled = true;
                        break;

                    case Key.Right:
                        _caret.MoveRight(_textEditorApp._rope.Length);
                        InvalidateVisual();
                        e.Handled = true;
                        break;
                    
                    case Key.Enter:
                        _textEditorApp._rope.Insert(_caret.Position, "\n");
                        _caret.MoveRight(_textEditorApp._rope.Length); 
                        InvalidateVisual();
                        e.Handled = true;
                        break;
                    
                }
            }

            if (handled)
            {
                e.Handled = true;
                InvalidateVisual();
            }
        }

        // ========== TYPING SESSION FINALIZE ==========

        private void FinalizeTypingSession()
        {
            if (_typingSessionActive && _typingBuffer.Length > 0)
            {
                Console.WriteLine("Finalizing typing session: " + _typingBuffer.ToString());
                // Remove the "live inserted" text
                _textEditorApp._rope.Delete(_typingStart, _typingBuffer.Length);

                // Insert it again via InsertText => pushes ONE undo
                _textEditorApp.InsertText(_typingStart, _typingBuffer.ToString());
            }

            _typingSessionActive = false;
            _typingBuffer.Clear();
        }

        private void ClearSelection() => _selection.Clear();

        protected override void OnLostKeyboardFocus(KeyboardFocusChangedEventArgs e)
        {
            base.OnLostKeyboardFocus(e);
            FinalizeTypingSession();
        }

        // ========== RENDERING ==========

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            var bgRect = new Rect(0, 0, ActualWidth, ActualHeight);
            drawingContext.DrawRectangle(Brushes.White, null, bgRect);

            if (_textEditorApp == null) return;

            string fullText = _textEditorApp._rope.GetText();
            if (string.IsNullOrEmpty(fullText))
            {
                if (_caretBlinkVisible && IsKeyboardFocusWithin)
                {
                    _caret.DrawCaret(drawingContext, 5, 5, ActualWidth, ActualHeight);
                }
                return;
            }

            // Draw entire text
            FormattedText ftAll = _textHelper.CreateFormattedText(fullText, ActualWidth);
            drawingContext.DrawText(ftAll, new Point(5, 5));

            // If selection, highlight it ...
            if (_selection.HasSelection)
            {
                Geometry? selectionGeom = _selection.BuildSelectionGeometry(fullText, new Point(5, 5), ActualWidth);

                if (selectionGeom != null)
                {
                    drawingContext.PushOpacity(0.4);
                    drawingContext.DrawGeometry(Brushes.LightBlue, null, selectionGeom);
                    drawingContext.Pop();
                }
            }

            // Draw caret if blink is on
            if (_caretBlinkVisible && IsKeyboardFocusWithin)
            {
                string textBeforeCaret = fullText.Substring(0, Math.Min(_caret.Position, fullText.Length));
                Point caretPos = _caret.ComputeCaretPosition(textBeforeCaret, ActualWidth);
                _caret.DrawCaret(drawingContext, caretPos.X, caretPos.Y, ActualWidth, ActualHeight);
            }
        }

        public void RefreshFromApp()
        {
            InvalidateVisual();
        }

    }
}