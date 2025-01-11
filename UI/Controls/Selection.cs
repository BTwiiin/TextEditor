namespace TextEditor.UI.Controls
{
    public class Selection
    {
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
    }
}