namespace TextEditor.Core
{
    public class UndoRedoStack
    {
        private Stack<Action> stack;

        public UndoRedoStack()
        {
            stack = new Stack<Action>();
        }
        public void Clear()
        {
            stack.Clear();
        }
        public void Push(Action action)
        {
            stack.Push(action);
        }

        public Action Pop()
        {
            if (IsEmpty())
                throw new InvalidOperationException("Stack is empty.");

            return stack.Pop();
        }

        public bool IsEmpty()
        {
            return stack.Count == 0;
        }
    }
}
