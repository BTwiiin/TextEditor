using System.Windows.Controls;

namespace TextEditor.UI
{
    public class EditorView : UserControl
    {
        private TextBox editor;

        public EditorView()
        {
            editor = new TextBox
            {
                AcceptsReturn = true,
                AcceptsTab = true,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Auto
            };
            Content = editor;
        }

        public string GetText()
        {
            return editor.Text;
        }

        public void SetText(string text)
        {
            editor.Text = text;
        }
    }
}