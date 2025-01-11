namespace TextEditor.UI.Controls
{
    public class Caret
    {
        private int _position;
        public int Position { get => _position; set => _position = value; }

        // Example: blinking handled externally
        // or have IsVisible toggled by a timer

        public void MoveLeft()  => _position = Math.Max(0, _position - 1);
        public void MoveRight(int maxLength) => _position = Math.Min(_position + 1, maxLength);

        public Caret(int start = 0)
        {
            _position = start;
        }
    }
}