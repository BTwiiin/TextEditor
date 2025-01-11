using System.Windows;
using System.Windows.Input;
using Microsoft.Win32; 
using TextEditor;

namespace TextEditor.UI
{
    public partial class MainWindow : Window
    {
        private TextEditorApp _editorApp;

        public MainWindow()
        {
            InitializeComponent();

            // Create the editor app
            _editorApp = TextEditorApp.Instance;

            // Link the app to the rope editor control
            EditorControl._textEditorApp  = _editorApp;

            Loaded += (s, e) =>
            {
                EditorControl.Focus();
            };
        }

        private void OpenFileClick(object sender, RoutedEventArgs e)
        {
            var openDlg = new OpenFileDialog
            {
                Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*"
            };
            if (openDlg.ShowDialog() == true)
            {
                _editorApp.LoadFile(openDlg.FileName);
                // Re-render the control
                EditorControl.RefreshFromApp();
            }
            EditorControl.Focus();
        }

        private void SaveFileClick(object sender, RoutedEventArgs e)
        {
            var saveDlg = new SaveFileDialog
            {
                Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*"
            };
            if (saveDlg.ShowDialog() == true)
            {
                _editorApp.SaveFile(saveDlg.FileName);
            }
            EditorControl.Focus();
        }

        private void UndoClick(object sender, RoutedEventArgs e)
        {
            _editorApp.Undo();
            EditorControl.RefreshFromApp();
            EditorControl.Focus();
        }

        private void RedoClick(object sender, RoutedEventArgs e)
        {
            _editorApp.Redo();
            EditorControl.RefreshFromApp();
            EditorControl.Focus();
        }

        private void ApplyBoldClick(object sender, RoutedEventArgs e)
        {
            // Example: apply "bold" to first 5 chars
            _editorApp.ApplyFormatting(0, 5, "BOLD");
            EditorControl.RefreshFromApp();
            EditorControl.Focus();
        }
    }
}
