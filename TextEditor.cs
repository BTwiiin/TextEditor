using TextEditor.Core;         // Rope, UndoRedoStack, FormatDictionary
using TextEditor.Utilities;    // FileHandler

namespace TextEditor
{
    public class TextEditorApp
    {
        public Rope _rope; // We expose rope so the RopeEditorControl can do "live" edits if needed
        private Deque _undoStack;
        private Deque _redoStack;
        private FormatDictionary _formatDictionary;
        private FileHandler _fileHandler;

        // Private static field to hold the single instance of the class
        private static TextEditorApp? _instance;

        // Lock object for thread safety
        private static readonly object _lock = new object();

        private TextEditorApp()
        {
            // Initialize data structures
            _rope = new Rope();
            _undoStack = new Deque();
            _redoStack = new Deque();
            _formatDictionary = new FormatDictionary();
            _fileHandler = new FileHandler(); 
        }

        // Public static property to get the instance
        public static TextEditorApp Instance
        {
            get
            {
                lock (_lock) // Ensuring thread safety
                {
                    if (_instance == null)
                    {
                        _instance = new TextEditorApp();
                    }
                    return _instance;
                }
            }
        }

        /// <summary>
        /// Expose the rope length if the UI needs it (e.g. caret bounds).
        /// </summary>
        public int RopeLength => _rope.Length;

        /// <summary>
        /// Load text from a file path into the rope.
        /// </summary>
        public void LoadFile(string filePath)
        {
            string content = _fileHandler.ReadFile(filePath);

            // Clear everything before loading new file
            _rope = new Rope(content);
            _undoStack.Clear();
            _redoStack.Clear();
            _formatDictionary.Clear();
        }

        /// <summary>
        /// Save current rope text to a file.
        /// </summary>
        public void SaveFile(string filePath)
        {
            string text = _rope.GetText();
            _fileHandler.WriteFile(filePath, text);
        }

        /// <summary>
        /// Return the entire text for display.
        /// </summary>
        public string GetText()
        {
            return _rope.GetText();
        }

        /// <summary>
        /// Insert text at a given position (with undo/redo).
        /// This ensures undone insert actually deletes the text, 
        /// and redo re-inserts it.
        /// </summary>
        public void InsertText(int position, string text)
        {
            if (position < 0 || position > _rope.Length)
                throw new ArgumentOutOfRangeException(nameof(position));

            var oldPosition = position;
            var oldLength = text.Length;

            // "do" action
            Action doAction = () => _rope.Insert(oldPosition, text);
            // "undo" action
            Action undoAction = () => _rope.Delete(oldPosition, oldLength);

            // Perform the action now
            doAction();

            // Push undo to _undoStack
            _undoStack.Push(() =>
            {
                // Undo means: remove the text
                undoAction();

                // When we do an undo, we push a redo action onto _redoStack
                _redoStack.Push(() =>
                {
                    doAction();
                    // After we redo, we push a new undo action onto _undoStack
                    _undoStack.Push(() => undoAction());
                });
            });

            // Clear the redo stack for a fresh action
            _redoStack.Clear();
        }

        /// <summary>
        /// Delete 'length' characters at 'position' (with undo/redo).
        /// This ensures undone delete re-inserts the text, 
        /// and redo re-deletes it.
        /// </summary>
        public void DeleteText(int position, int length)
        {
            if (position < 0 || position + length > _rope.Length)
                throw new ArgumentOutOfRangeException();

            string removedText = _rope.Substring(position, length);

            Action doAction = () => _rope.Delete(position, length);
            Action undoAction = () => _rope.Insert(position, removedText);

            // Perform the delete
            doAction();

            // Push onto undo stack
            _undoStack.Push(() =>
            {
                // On "undo", revert => re-insert text
                undoAction();

                // On "redo", re-delete
                _redoStack.Push(() =>
                {
                    doAction();
                    _undoStack.Push(() => undoAction());
                });
            });

            // Clear redo after new action
            _redoStack.Clear();
        }

        /// <summary>
        /// Apply formatting to a range, storing it in the FormatDictionary.
        /// (No undo/redo shown here, but you could follow the same pattern.)
        /// </summary>
        public void ApplyFormatting(int startIndex, int length, string formatTag)
        {
            _formatDictionary.AddFormatting(new Range(startIndex, startIndex + length), formatTag);
        }

        /// <summary>
        /// Undo last action.
        /// </summary>
        public void Undo()
        {
            if (!_undoStack.IsEmpty())
            {
                var undoCommand = _undoStack.Pop();
                undoCommand();
            }
        }

        /// <summary>
        /// Redo last undone action.
        /// </summary>
        public void Redo()
        {
            if (!_redoStack.IsEmpty())
            {
                var redoCommand = _redoStack.Pop();
                redoCommand();
            }
        }

        /// <summary>
        /// Return the FormatDictionary for usage in UI or rendering.
        /// </summary>
        public FormatDictionary GetFormatDictionary()
        {
            return _formatDictionary;
        }
    }
}
