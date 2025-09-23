using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;

namespace tcp_group_chat
{
    public class ChatForm : Form
    {
        private string username;
        private NetworkClient networkClient;
        private List<string> groupMembers = new List<string>();
        
        // Controls untuk contacts panel (kiri)
        private Panel panelContacts;
        private Label lblContacts;
        private ListBox listContacts;
        private TextBox txtSearch;
        private Button btnNewChat;
        
        // Controls untuk chat area (tengah)
        private Panel panelChat;
        private Panel panelChatHeader;
        private Label lblContactName;
        private Label lblContactStatus;
        private RichTextBox rtbMessages;
        private Panel panelMessageInput;
        private TextBox txtMessage;
        private Button btnSend;
        
        // Controls untuk info panel (kanan)
        private Panel panelInfo;
        private Label lblInfoTitle;
        private ListBox listGroupMembers;

        public ChatForm(string username)
        {
            this.username = username;
            InitializeComponent();
            
            // Initialize network client
            networkClient = new NetworkClient();
            networkClient.MessageReceived += OnMessageReceived;
            networkClient.ConnectionStateChanged += OnConnectionStateChanged;
            networkClient.ErrorOccurred += OnErrorOccurred;
            
            // Auto-select group chat after networkClient is initialized
            listContacts.SelectedIndex = 0;
            
            // Connect to server
            ConnectToServer();
        }

        private void InitializeComponent()
        {
            this.Text = $"WhatsApp95 - {username}";
            this.Size = new Size(1200, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.BackColor = Color.FromArgb(192, 192, 192);
            this.MinimumSize = new Size(1000, 650);

            // ===== CONTACTS PANEL (Kiri) =====
            panelContacts = new Panel();
            panelContacts.Dock = DockStyle.Left;
            panelContacts.Width = 220;
            panelContacts.BackColor = Color.FromArgb(192, 192, 192);
            panelContacts.BorderStyle = BorderStyle.Fixed3D;

            // Header Contacts
            lblContacts = new Label();
            lblContacts.Text = "WhatsApp95";
            lblContacts.Font = new Font("MS Sans Serif", 8, FontStyle.Bold);
            lblContacts.Location = new Point(10, 10);
            lblContacts.Size = new Size(200, 20);
            lblContacts.BackColor = Color.FromArgb(192, 192, 192);
            lblContacts.ForeColor = Color.Black;

            // Search box
            txtSearch = new TextBox();
            txtSearch.Location = new Point(10, 35);
            txtSearch.Size = new Size(200, 20);
            txtSearch.Font = new Font("MS Sans Serif", 8);
            txtSearch.BackColor = Color.White;
            txtSearch.BorderStyle = BorderStyle.Fixed3D;
            txtSearch.Text = "Search or start new chat";
            txtSearch.ForeColor = Color.Gray;
            txtSearch.Enter += TxtSearch_Enter;
            txtSearch.Leave += TxtSearch_Leave;

            // Contacts list - Show only Group Chat
            listContacts = new ListBox();
            listContacts.Location = new Point(10, 65);
            listContacts.Size = new Size(200, 450);
            listContacts.Font = new Font("MS Sans Serif", 8);
            listContacts.BackColor = Color.White;
            listContacts.BorderStyle = BorderStyle.Fixed3D;
            listContacts.SelectedIndexChanged += ListContacts_SelectedIndexChanged;
            listContacts.Items.Add("👨‍💻 Group Chat");
            // Auto-select will be done after networkClient is initialized

            // New Chat button (disabled for group chat only)
            btnNewChat = new Button();
            btnNewChat.Location = new Point(10, 525);
            btnNewChat.Size = new Size(200, 25);
            btnNewChat.Text = "Group Chat Only";
            btnNewChat.Font = new Font("MS Sans Serif", 8);
            btnNewChat.BackColor = Color.FromArgb(192, 192, 192);
            btnNewChat.ForeColor = Color.Black;
            btnNewChat.FlatStyle = FlatStyle.Standard;
            btnNewChat.Enabled = false;

            panelContacts.Controls.Add(lblContacts);
            panelContacts.Controls.Add(txtSearch);
            panelContacts.Controls.Add(listContacts);
            panelContacts.Controls.Add(btnNewChat);

            // ===== INFO PANEL (Kanan) =====
            panelInfo = new Panel();
            panelInfo.Dock = DockStyle.Right;
            panelInfo.Width = 180;
            panelInfo.BackColor = Color.FromArgb(192, 192, 192);
            panelInfo.BorderStyle = BorderStyle.Fixed3D;

            lblInfoTitle = new Label();
            lblInfoTitle.Text = "Chat Info";
            lblInfoTitle.Font = new Font("MS Sans Serif", 8, FontStyle.Bold);
            lblInfoTitle.Location = new Point(10, 10);
            lblInfoTitle.Size = new Size(160, 20);
            lblInfoTitle.BackColor = Color.FromArgb(192, 192, 192);
            lblInfoTitle.ForeColor = Color.Black;

            listGroupMembers = new ListBox();
            listGroupMembers.Location = new Point(10, 35);
            listGroupMembers.Size = new Size(160, 500);
            listGroupMembers.Font = new Font("MS Sans Serif", 8);
            listGroupMembers.BackColor = Color.White;
            listGroupMembers.BorderStyle = BorderStyle.Fixed3D;

            panelInfo.Controls.Add(lblInfoTitle);
            panelInfo.Controls.Add(listGroupMembers);

            // ===== CHAT AREA (Tengah) =====
            panelChat = new Panel();
            panelChat.Dock = DockStyle.Fill;
            panelChat.BackColor = Color.FromArgb(192, 192, 192);

            // Chat Header
            panelChatHeader = new Panel();
            panelChatHeader.Dock = DockStyle.Top;
            panelChatHeader.Height = 60;
            panelChatHeader.BackColor = Color.FromArgb(128, 128, 128);
            panelChatHeader.BorderStyle = BorderStyle.Fixed3D;

            lblContactName = new Label();
            lblContactName.Text = "Group Chat";
            lblContactName.Font = new Font("MS Sans Serif", 8, FontStyle.Bold);
            lblContactName.Location = new Point(10, 10);
            lblContactName.Size = new Size(300, 20);
            lblContactName.BackColor = Color.FromArgb(128, 128, 128);
            lblContactName.ForeColor = Color.White;

            lblContactStatus = new Label();
            lblContactStatus.Text = "Connecting to server...";
            lblContactStatus.Font = new Font("MS Sans Serif", 8);
            lblContactStatus.Location = new Point(10, 30);
            lblContactStatus.Size = new Size(300, 15);
            lblContactStatus.BackColor = Color.FromArgb(128, 128, 128);
            lblContactStatus.ForeColor = Color.LightGray;

            panelChatHeader.Controls.Add(lblContactName);
            panelChatHeader.Controls.Add(lblContactStatus);

            // Messages area
            rtbMessages = new RichTextBox();
            rtbMessages.Dock = DockStyle.Fill;
            rtbMessages.Font = new Font("MS Sans Serif", 8);
            rtbMessages.BackColor = Color.White;
            rtbMessages.BorderStyle = BorderStyle.Fixed3D;
            rtbMessages.Enabled = false;
            rtbMessages.ReadOnly = true;
            rtbMessages.WordWrap = true;
            rtbMessages.ScrollBars = RichTextBoxScrollBars.Vertical;

            // Message input panel
            panelMessageInput = new Panel();
            panelMessageInput.Dock = DockStyle.Bottom;
            panelMessageInput.Height = 40;
            panelMessageInput.BackColor = Color.FromArgb(192, 192, 192);
            panelMessageInput.BorderStyle = BorderStyle.Fixed3D;
            panelMessageInput.Resize += PanelMessageInput_Resize;

            txtMessage = new TextBox();
            txtMessage.Location = new Point(10, 10);
            txtMessage.Size = new Size(690, 20);
            txtMessage.Font = new Font("MS Sans Serif", 8);
            txtMessage.BackColor = Color.White;
            txtMessage.BorderStyle = BorderStyle.Fixed3D;
            txtMessage.Enabled = false;
            txtMessage.KeyPress += TxtMessage_KeyPress;
            txtMessage.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

            btnSend = new Button();
            btnSend.Location = new Point(700, 8);
            btnSend.Size = new Size(75, 23);
            btnSend.Text = "Send";
            btnSend.Font = new Font("MS Sans Serif", 8);
            btnSend.BackColor = Color.FromArgb(192, 192, 192);
            btnSend.ForeColor = Color.Black;
            btnSend.FlatStyle = FlatStyle.Standard;
            btnSend.Enabled = false;
            btnSend.Click += BtnSend_Click;
            btnSend.Anchor = AnchorStyles.Top | AnchorStyles.Right;

            panelMessageInput.Controls.Add(txtMessage);
            panelMessageInput.Controls.Add(btnSend);

            panelChat.Controls.Add(rtbMessages);
            panelChat.Controls.Add(panelMessageInput);
            panelChat.Controls.Add(panelChatHeader);

            // Add panels to form
            this.Controls.Add(panelChat);
            this.Controls.Add(panelInfo);
            this.Controls.Add(panelContacts);
            
            // Set initial responsive layout
            PanelMessageInput_Resize(panelMessageInput, EventArgs.Empty);
        }

        private void ListContacts_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listContacts.SelectedItem != null && networkClient != null && networkClient.IsConnected)
            {
                // Enable chat controls when connected
                txtMessage.Enabled = true;
                btnSend.Enabled = true;
                rtbMessages.Enabled = true;
                
                lblContactName.Text = "Group Chat";
                lblContactStatus.Text = $"{groupMembers.Count} members online";
                
                // Update group members display
                UpdateGroupMembersList();
            }
        }

        private void UpdateGroupMembersList()
        {
            listGroupMembers.Items.Clear();
            listGroupMembers.Items.Add("👥 Group Chat Members");
            listGroupMembers.Items.Add("");
            
            foreach (string member in groupMembers.OrderBy(m => m))
            {
                if (member == username)
                {
                    listGroupMembers.Items.Add($"📱 {member} (You)");
                }
                else
                {
                    listGroupMembers.Items.Add($"📱 {member}");
                }
            }
        }

        private async void ConnectToServer()
        {
            try
            {
                bool connected = await networkClient.ConnectAsync("127.0.0.1", 8888, username);
                if (!connected)
                {
                    MessageBox.Show("Failed to connect to server. Please make sure the server is running.", 
                                    "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Connection error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OnMessageReceived(object sender, ChatMessage message)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => OnMessageReceived(sender, message)));
                return;
            }

            DateTime messageTime = DateTimeOffset.FromUnixTimeSeconds(message.Ts).DateTime;
            
            switch (message.Type)
            {
                case "msg":
                    AppendMessage(message.From, message.Text, messageTime, Color.Black);
                    break;
                
                case "pm":
                    if (message.To == username)
                    {
                        AppendMessage($"{message.From} (private)", message.Text, messageTime, Color.Blue);
                    }
                    else if (message.From == username)
                    {
                        AppendMessage($"You to {message.To} (private)", message.Text, messageTime, Color.Blue);
                    }
                    break;
                
                case "join":
                    AppendMessage("System", message.Text, messageTime, Color.Green);
                    if (!groupMembers.Contains(message.From))
                    {
                        groupMembers.Add(message.From);
                        UpdateGroupMembersList();
                    }
                    break;
                
                case "leave":
                    AppendMessage("System", message.Text, messageTime, Color.Orange);
                    groupMembers.Remove(message.From);
                    UpdateGroupMembersList();
                    break;
                
                case "sys":
                    AppendMessage("Server", message.Text, messageTime, Color.Red);
                    break;
            }
        }

        private void OnConnectionStateChanged(object sender, string state)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => OnConnectionStateChanged(sender, state)));
                return;
            }

            lblContactStatus.Text = state;
            
            if (state == "Connected")
            {
                txtMessage.Enabled = true;
                btnSend.Enabled = true;
                rtbMessages.Enabled = true;
                
                // Add self to group members
                if (!groupMembers.Contains(username))
                {
                    groupMembers.Add(username);
                    UpdateGroupMembersList();
                }
            }
            else if (state == "Disconnected")
            {
                txtMessage.Enabled = false;
                btnSend.Enabled = false;
                groupMembers.Clear();
                UpdateGroupMembersList();
            }
        }

        private void OnErrorOccurred(object sender, string error)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => OnErrorOccurred(sender, error)));
                return;
            }

            AppendMessage("Error", error, DateTime.Now, Color.Red);
        }

        private void AppendMessage(string sender, string message, DateTime timestamp, Color color)
        {
            string timeString = timestamp.ToString("HH:mm:ss");
            string formattedMessage = $"[{timeString}] {sender}: {message}\n";
            
            rtbMessages.SelectionStart = rtbMessages.TextLength;
            rtbMessages.SelectionLength = 0;
            rtbMessages.SelectionColor = color;
            rtbMessages.AppendText(formattedMessage);
            rtbMessages.SelectionColor = rtbMessages.ForeColor;
            
            // Auto-scroll to latest message
            rtbMessages.SelectionStart = rtbMessages.Text.Length;
            rtbMessages.ScrollToCaret();
        }

        private void BtnSend_Click(object sender, EventArgs e)
        {
            SendMessage();
        }

        private void TxtMessage_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                SendMessage();
                e.Handled = true;
            }
        }

        private async void SendMessage()
        {
            if (!string.IsNullOrWhiteSpace(txtMessage.Text) && networkClient.IsConnected)
            {
                string message = txtMessage.Text.Trim();
                
                // Check if it's a private message command
                if (message.StartsWith("/w "))
                {
                    // Private message format: /w username message
                    string[] parts = message.Substring(3).Split(' ', 2);
                    if (parts.Length >= 2)
                    {
                        string targetUser = parts[0];
                        string privateMessage = parts[1];
                        await networkClient.SendPrivateMessageAsync(targetUser, privateMessage);
                    }
                    else
                    {
                        AppendMessage("System", "Usage: /w username message", DateTime.Now, Color.Red);
                    }
                }
                else
                {
                    // Send group message
                    await networkClient.SendGroupMessageAsync(message);
                }
                
                txtMessage.Clear();
            }
        }

        private void BtnNewChat_Click(object sender, EventArgs e)
        {
            // Simple input dialog using a form
            Form inputForm = new Form();
            inputForm.Text = "New Chat";
            inputForm.Size = new Size(300, 150);
            inputForm.StartPosition = FormStartPosition.CenterParent;
            inputForm.FormBorderStyle = FormBorderStyle.FixedDialog;
            inputForm.MaximizeBox = false;
            inputForm.BackColor = Color.FromArgb(192, 192, 192);
            
            Label lblPrompt = new Label();
            lblPrompt.Text = "Enter contact name:";
            lblPrompt.Location = new Point(20, 20);
            lblPrompt.Size = new Size(250, 20);
            lblPrompt.Font = new Font("MS Sans Serif", 8);
            
            TextBox txtInput = new TextBox();
            txtInput.Location = new Point(20, 45);
            txtInput.Size = new Size(250, 20);
            txtInput.Font = new Font("MS Sans Serif", 8);
            txtInput.BorderStyle = BorderStyle.Fixed3D;
            
            Button btnOK = new Button();
            btnOK.Text = "OK";
            btnOK.Location = new Point(115, 75);
            btnOK.Size = new Size(75, 23);
            btnOK.Font = new Font("MS Sans Serif", 8);
            btnOK.BackColor = Color.FromArgb(192, 192, 192);
            btnOK.FlatStyle = FlatStyle.Standard;
            btnOK.DialogResult = DialogResult.OK;
            
            Button btnCancel = new Button();
            btnCancel.Text = "Cancel";
            btnCancel.Location = new Point(195, 75);
            btnCancel.Size = new Size(75, 23);
            btnCancel.Font = new Font("MS Sans Serif", 8);
            btnCancel.BackColor = Color.FromArgb(192, 192, 192);
            btnCancel.FlatStyle = FlatStyle.Standard;
            btnCancel.DialogResult = DialogResult.Cancel;
            
            inputForm.Controls.Add(lblPrompt);
            inputForm.Controls.Add(txtInput);
            inputForm.Controls.Add(btnOK);
            inputForm.Controls.Add(btnCancel);
            
            if (inputForm.ShowDialog() == DialogResult.OK)
            {
                string contactName = txtInput.Text.Trim();
                if (!string.IsNullOrWhiteSpace(contactName))
                {
                    if (!listContacts.Items.Contains($"📱 {contactName}"))
                    {
                        listContacts.Items.Add($"📱 {contactName}");
                        MessageBox.Show($"Contact '{contactName}' added!");
                    }
                    else
                    {
                        MessageBox.Show("Contact already exists!");
                    }
                }
            }
            
            inputForm.Dispose();
        }

        private void TxtSearch_Enter(object sender, EventArgs e)
        {
            if (txtSearch.Text == "Search or start new chat" && txtSearch.ForeColor == Color.Gray)
            {
                txtSearch.Text = "";
                txtSearch.ForeColor = Color.Black;
            }
        }

        private void TxtSearch_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtSearch.Text))
            {
                txtSearch.Text = "Search or start new chat";
                txtSearch.ForeColor = Color.Gray;
            }
        }

        private void PanelMessageInput_Resize(object sender, EventArgs e)
        {
            if (panelMessageInput != null && txtMessage != null && btnSend != null)
            {
                // Margin dan spacing
                int leftMargin = 10;
                int spacing = 5;
                int rightMargin = 10;
                
                // Hitung lebar TextBox yang responsif
                int availableWidth = panelMessageInput.Width - leftMargin - btnSend.Width - spacing - rightMargin;
                txtMessage.Width = Math.Max(100, availableWidth); // Minimal width 100px
                
                // Posisi tombol Send di sebelah kanan TextBox
                btnSend.Left = txtMessage.Right + spacing;
            }
        }

        // Handle form closing
        protected override async void OnFormClosing(FormClosingEventArgs e)
        {
            if (networkClient != null && networkClient.IsConnected)
            {
                await networkClient.DisconnectAsync();
            }
            
            base.OnFormClosing(e);
            Application.Exit();
        }
    }
}