using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.Windows.Forms;

namespace MainApplication
{ 
    public sealed class MessageManager
    {
        private static readonly MessageManager instance = new MessageManager();
        private TextBox messageTextBox;
        private Queue<string> messageQueue;
        private MenuItem mnuClear = new MenuItem();
        private ContextMenu mnuContext = new ContextMenu();

        public static MessageManager Instance
        {
            get
            {
                return instance;
            }
        }

        public void EnQueueMessage(char[] message_chars)
        {
            string message_string;

            // Convert to string
            message_string = new string(message_chars);
            // Check if not initialized
            if (messageQueue == null)
            {
                // Initialize
                messageQueue = new Queue<string>();
            }
            // Add string
            messageQueue.Enqueue(message_string);            
        }

        public void EnQueueMessage(string message)
        {
            message += Environment.NewLine;
            // Check if not initialized
            if (messageQueue == null)
            {
                // Initialize
                messageQueue = new Queue<string>();
            }
            // Add string
            messageQueue.Enqueue(message);
        }

        public void UpdateDisplay()
        {
            string message;

            // Check if messageQueue exist
            if (messageQueue == null)
            {
                return;
            }
            // while queue is not empty
            while (messageQueue.Count != 0)
            {
                // Take out a message
                message = messageQueue.Dequeue();
                // Display it
                messageTextBox.AppendText(message);
            }
        }

        public TextBox MessageTextBox
        {
            set
            {
                messageTextBox = value;
            }
        }

        public void Initialize()
        {
            // Create context menue entry
            mnuClear.Index = 0;
            mnuClear.Text = "Clear";
            mnuClear.Click += new System.EventHandler(this.mnuClear_Click);
            mnuContext.MenuItems.Add(mnuClear);
            // Attach menu context
            messageTextBox.ContextMenu = mnuContext;
        }

        private void mnuClear_Click(object sender, EventArgs e)
        {
            messageTextBox.Clear();
        }
    }
}
