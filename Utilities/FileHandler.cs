using System.IO;

namespace TextEditor.Utilities
{
    public class FileHandler
    {
        public string ReadFile(string filePath)
        {
            return File.ReadAllText(filePath);
        }

        public void WriteFile(string filePath, string content)
        {
            File.WriteAllText(filePath, content);
        }
    }
}