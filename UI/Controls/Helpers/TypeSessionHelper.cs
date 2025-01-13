using System;
using System.Text;

namespace TextEditor.UI.Controls.Helpers
{
    public class TypingSessionHelper
    {
        private bool _typingSessionActive = false;
        private const int TYPING_TIMEOUT_MS = 1000;

        public bool IsTypingSessionActive => _typingSessionActive;

        // Start a new typing session or continue the previous one if not expired
        public void StartSession(int caretPosition, ref bool typingSessionActive, ref StringBuilder typingBuffer, ref int typingStart, ref DateTime lastTypingTime)
        {
            if (typingSessionActive)
            {
                // Check if the session timeout has passed
                double msSinceLast = (DateTime.Now - lastTypingTime).TotalMilliseconds;
                if (msSinceLast > TYPING_TIMEOUT_MS)
                {
                    FinalizeSession(ref typingBuffer, ref typingStart, (start, text) => 
                    {
                        // Insert text action implementation
                        // For example: Console.WriteLine($"Insert text at {start}: {text}");
                    });
                    typingSessionActive = false;
                }
            }

            if (!typingSessionActive)
            {
                typingSessionActive = true;
                typingBuffer.Clear();
                typingStart = caretPosition;
            }
        }

        // Finalize the typing session and insert the buffered text
        public void FinalizeSession(ref StringBuilder typingBuffer, ref int typingStart, Action<int, string> insertTextAction)
        {
            if (_typingSessionActive && typingBuffer.Length > 0)
            {
                // Remove the "live inserted" text
                insertTextAction(typingStart, typingBuffer.ToString());
            }

            _typingSessionActive = false;
            typingBuffer.Clear();
        }

        // Add typed text to the buffer
        public void AccumulateText(string typedText, ref StringBuilder typingBuffer)
        {
            typingBuffer.Append(typedText);
        }

        // Update the last typing time
        public void UpdateTypingTime(ref DateTime lastTypingTime)
        {
            lastTypingTime = DateTime.Now;
        }
    }
}
