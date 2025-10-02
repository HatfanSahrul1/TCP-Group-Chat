using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading.Tasks;

namespace tcp_group_chat
{
    public class ChatForm : Form
    {
        private string username;
        private string serverIP;
        private int serverPort;
        private NetworkClient networkClient;
        private List<string> groupMembers = new List<string>();
        private string currentChatTarget = "Group Chat"; // "Group Chat" or username for private chat
        private Dictionary<string, List<string>> privateChatHistory = new Dictionary<string, List<string>>();
        
        // Controls untuk contacts panel (kiri)
        private Panel panelContacts;
        private Label lblContacts;
        private ListView listOnlineUsers;
        private TextBox txtSearch;
        private Button btnNewChat;
        private Button btnDisconnect;
        private Button btnThemeToggle;
        
        // Theme variables
        private bool isDarkMode = false;
        
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

        public ChatForm(string username, string serverIP = "127.0.0.1", int serverPort = 8888)
        {
            this.username = username;
            this.serverIP = serverIP;
            this.serverPort = serverPort;
            InitializeComponent();
            
            // Initialize network client
            networkClient = new NetworkClient();
            networkClient.MessageReceived += OnMessageReceived;
            networkClient.ConnectionStateChanged += OnConnectionStateChanged;
            networkClient.ErrorOccurred += OnErrorOccurred;
            
            // Default to group chat
            lblContactName.Text = "Group Chat";
            lblContactStatus.Text = "Connecting to server...";
            
            // Connect to server
            _ = Task.Run(async () => await ConnectToServer());
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
            lblContacts.Text = "Online Users";
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

            // Online Users ListView
            listOnlineUsers = new ListView();
            listOnlineUsers.Location = new Point(10, 65);
            listOnlineUsers.Size = new Size(200, 450);
            listOnlineUsers.Font = new Font("MS Sans Serif", 8);
            listOnlineUsers.BackColor = Color.White;
            listOnlineUsers.BorderStyle = BorderStyle.Fixed3D;
            listOnlineUsers.View = View.List;
            listOnlineUsers.FullRowSelect = true;
            listOnlineUsers.HideSelection = false;
            listOnlineUsers.SelectedIndexChanged += ListOnlineUsers_SelectedIndexChanged;
            listOnlineUsers.MouseDoubleClick += ListOnlineUsers_MouseDoubleClick;
            
            // Context menu for private chat
            var contextMenu = new ContextMenuStrip();
            var privateChateMenuItem = new ToolStripMenuItem("Private Chat");
            privateChateMenuItem.Click += PrivateChatMenuItem_Click;
            contextMenu.Items.Add(privateChateMenuItem);
            listOnlineUsers.ContextMenuStrip = contextMenu;

            // Group Chat button
            btnNewChat = new Button();
            btnNewChat.Location = new Point(10, 525);
            btnNewChat.Size = new Size(95, 25);
            btnNewChat.Text = "Group Chat";
            btnNewChat.Font = new Font("MS Sans Serif", 8);
            btnNewChat.BackColor = Color.FromArgb(192, 192, 192);
            btnNewChat.ForeColor = Color.Black;
            btnNewChat.FlatStyle = FlatStyle.Standard;
            btnNewChat.Click += BtnGroupChat_Click;

            // Disconnect button
            btnDisconnect = new Button();
            btnDisconnect.Location = new Point(115, 525);
            btnDisconnect.Size = new Size(95, 25);
            btnDisconnect.Text = "Disconnect";
            btnDisconnect.Font = new Font("MS Sans Serif", 8);
            btnDisconnect.BackColor = Color.FromArgb(192, 192, 192);
            btnDisconnect.ForeColor = Color.Black;
            btnDisconnect.FlatStyle = FlatStyle.Standard;
            btnDisconnect.Click += BtnDisconnect_Click;

            // Theme toggle button
            btnThemeToggle = new Button();
            btnThemeToggle.Location = new Point(10, 555);
            btnThemeToggle.Size = new Size(100, 25);
            btnThemeToggle.Text = "🌙 Dark Mode";
            btnThemeToggle.Font = new Font("MS Sans Serif", 8);
            btnThemeToggle.BackColor = Color.FromArgb(192, 192, 192);
            btnThemeToggle.ForeColor = Color.Black;
            btnThemeToggle.FlatStyle = FlatStyle.Standard;
            btnThemeToggle.Click += BtnThemeToggle_Click;

            panelContacts.Controls.Add(lblContacts);
            panelContacts.Controls.Add(txtSearch);
            panelContacts.Controls.Add(listOnlineUsers);
            panelContacts.Controls.Add(btnNewChat);
            panelContacts.Controls.Add(btnThemeToggle);
            panelContacts.Controls.Add(btnDisconnect);

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

        private void ListOnlineUsers_SelectedIndexChanged(object sender, EventArgs e)
        {
            // This is handled by double-click or context menu
        }

        private void ListOnlineUsers_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (listOnlineUsers.SelectedItems.Count > 0)
            {
                string selectedUser = listOnlineUsers.SelectedItems[0].Text;
                if (selectedUser != username) // Don't chat with yourself
                {
                    StartPrivateChat(selectedUser);
                }
            }
        }

        private void PrivateChatMenuItem_Click(object sender, EventArgs e)
        {
            if (listOnlineUsers.SelectedItems.Count > 0)
            {
                string selectedUser = listOnlineUsers.SelectedItems[0].Text;
                if (selectedUser != username) // Don't chat with yourself
                {
                    StartPrivateChat(selectedUser);
                }
            }
        }

        private void StartPrivateChat(string targetUser)
        {
            currentChatTarget = targetUser;
            lblContactName.Text = $"Private Chat - {targetUser}";
            lblContactStatus.Text = "online";
            
            // Enable chat controls
            txtMessage.Enabled = true;
            btnSend.Enabled = true;
            rtbMessages.Enabled = true;
            
            // Load private chat history
            LoadPrivateChatHistory(targetUser);
        }

        private async void DisconnectAndExit()
        {
            try
            {
                if (networkClient != null && networkClient.IsConnected)
                {
                    await networkClient.DisconnectAsync();
                }
            }
            catch (Exception ex)
            {
                // Log error but continue with exit
                Console.WriteLine($"Error during disconnect: {ex.Message}");
            }
            finally
            {
                Application.Exit();
            }
        }

        private void BtnGroupChat_Click(object sender, EventArgs e)
        {
            SwitchToGroupChat();
        }

        private void ShowReconnectDialog()
        {
            var result = MessageBox.Show(
                "Connection to server lost.\n\nWould you like to:\n" +
                "• Yes: Try to reconnect\n" +
                "• No: Close application\n" +
                "• Cancel: Stay offline",
                "Connection Lost",
                MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Warning);

            switch (result)
            {
                case DialogResult.Yes:
                    _ = Task.Run(async () =>
                    {
                        await Task.Delay(1000); // Brief delay
                        BeginInvoke(new Action(async () =>
                        {
                            AppendMessage("System", "🔄 Attempting to reconnect...", DateTime.Now, Color.Orange);
                            await ConnectToServer();
                        }));
                    });
                    break;
                
                case DialogResult.No:
                    DisconnectAndExit();
                    break;
                
                case DialogResult.Cancel:
                    AppendMessage("System", "📴 Staying offline. Click 'Group Chat' button when ready to reconnect.", DateTime.Now, Color.Gray);
                    break;
            }
        }

        private void LoadPrivateChatHistory(string targetUser)
        {
            rtbMessages.Clear();
            
            if (privateChatHistory.ContainsKey(targetUser))
            {
                foreach (string message in privateChatHistory[targetUser])
                {
                    rtbMessages.AppendText(message + "\n");
                }
            }
            else
            {
                rtbMessages.AppendText($"=== Private Chat with {targetUser} ===\n");
                rtbMessages.AppendText($"Start chatting with {targetUser}!\n");
            }
            
            // Auto-scroll to bottom
            rtbMessages.SelectionStart = rtbMessages.Text.Length;
            rtbMessages.ScrollToCaret();
        }

        private void SwitchToGroupChat()
        {
            currentChatTarget = "Group Chat";
            lblContactName.Text = "Group Chat";
            lblContactStatus.Text = $"{groupMembers.Count} members online";
            
            // Enable chat controls
            txtMessage.Enabled = true;
            btnSend.Enabled = true;
            rtbMessages.Enabled = true;
            
            // Load group chat messages (clear and show current session)
            // Note: In a real app, you might want to preserve group chat history
            rtbMessages.Clear();
            rtbMessages.AppendText("=== Group Chat ===\n");
            rtbMessages.AppendText("Welcome to the group chat!\n");
        }

        // Async version to prevent UI blocking
        private async void UpdateGroupMembersListAsync()
        {
            await Task.Run(() =>
            {
                if (InvokeRequired)
                {
                    BeginInvoke(new Action(UpdateGroupMembersList));
                }
                else
                {
                    UpdateGroupMembersList();
                }
            });
        }

        private void UpdateGroupMembersList()
        {
            try
            {
                // Update right panel (group members)
                listGroupMembers.BeginUpdate();
                listGroupMembers.Items.Clear();
                listGroupMembers.Items.Add("👥 Group Chat Members");
                listGroupMembers.Items.Add("");
                
                var sortedMembers = groupMembers.OrderBy(m => m).ToList();
                foreach (string member in sortedMembers)
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
                
                // Update left panel (online users ListView)
                listOnlineUsers.BeginUpdate();
                listOnlineUsers.Items.Clear();
                
                foreach (string member in sortedMembers)
                {
                    var item = new ListViewItem(member);
                    if (member == username)
                    {
                        item.ForeColor = Color.Blue; // Highlight yourself
                        item.Font = new Font(item.Font, FontStyle.Bold);
                    }
                    listOnlineUsers.Items.Add(item);
                }
                
                // Update status
                lblContactStatus.Text = $"{groupMembers.Count} members online";
            }
            finally
            {
                listGroupMembers.EndUpdate();
                listOnlineUsers.EndUpdate();
            }
        }

        private async Task ConnectToServer()
        {
            try
            {
                // Dispose old client if exists
                if (networkClient != null)
                {
                    networkClient.MessageReceived -= OnMessageReceived;
                    networkClient.ConnectionStateChanged -= OnConnectionStateChanged;
                    networkClient.ErrorOccurred -= OnErrorOccurred;
                    networkClient.Dispose();
                }
                
                // Create new client
                networkClient = new NetworkClient();
                networkClient.MessageReceived += OnMessageReceived;
                networkClient.ConnectionStateChanged += OnConnectionStateChanged;
                networkClient.ErrorOccurred += OnErrorOccurred;
                
                bool connected = await networkClient.ConnectAsync(serverIP, serverPort, username);
                if (!connected)
                {
                    AppendMessage("System", $"Failed to connect to server at {serverIP}:{serverPort}. Will retry automatically...", DateTime.Now, Color.Red);
                }
                else
                {
                    AppendMessage("System", $"Connected to server at {serverIP}:{serverPort} successfully!", DateTime.Now, Color.Green);
                }
            }
            catch (Exception ex)
            {
                AppendMessage("System", $"Connection error: {ex.Message}", DateTime.Now, Color.Red);
            }
        }

        private void OnMessageReceived(object sender, ChatMessage message)
        {
            // Use BeginInvoke for better performance instead of Invoke
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => ProcessReceivedMessage(message)));
                return;
            }
            
            ProcessReceivedMessage(message);
        }

        private void ProcessReceivedMessage(ChatMessage message)
        {
            DateTime messageTime = DateTimeOffset.FromUnixTimeSeconds(message.Ts).DateTime;
            
            switch (message.Type)
            {
                case "msg":
                    AppendMessage(message.From, message.Text, messageTime, Color.Black);
                    break;
                
                case "pm":
                    string privateMessageSender = message.From;
                    string privateMessageTarget = message.To;
                    
                    if (message.To == username)
                    {
                        // Received private message
                        // Store in private chat history
                        if (!privateChatHistory.ContainsKey(privateMessageSender))
                        {
                            privateChatHistory[privateMessageSender] = new List<string>();
                        }
                        string formattedMessage = $"[{messageTime:HH:mm:ss}] {privateMessageSender}: {message.Text}";
                        privateChatHistory[privateMessageSender].Add(formattedMessage);
                        
                        // Display only if currently in this private chat
                        if (currentChatTarget == privateMessageSender)
                        {
                            AppendMessage(privateMessageSender, message.Text, messageTime, Color.Blue);
                        }
                        else
                        {
                            // Show notification in group chat that you have a private message
                            if (currentChatTarget == "Group Chat")
                            {
                                AppendMessage("System", $"💬 Private message from {privateMessageSender} (double-click their name to view)", messageTime, Color.Purple);
                            }
                        }
                    }
                    else if (message.From == username)
                    {
                        // This is your own private message (confirmation)
                        // Don't add to history here as it's already added in SendMessage
                        if (currentChatTarget == privateMessageTarget)
                        {
                            // Message already displayed by SendMessage method
                        }
                    }
                    break;
                
                case "join":
                    AppendMessage("System", message.Text, messageTime, Color.Green);
                    if (!groupMembers.Contains(message.From))
                    {
                        groupMembers.Add(message.From);
                        UpdateGroupMembersListAsync();
                    }
                    break;
                
                case "leave":
                    AppendMessage("System", message.Text, messageTime, Color.Orange);
                    groupMembers.Remove(message.From);
                    UpdateGroupMembersListAsync();
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
                BeginInvoke(new Action(() => ProcessConnectionStateChange(state)));
                return;
            }

            ProcessConnectionStateChange(state);
        }

        private void ProcessConnectionStateChange(string state)
        {
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
                    UpdateGroupMembersListAsync();
                }
            }
            else if (state == "Disconnected")
            {
                txtMessage.Enabled = false;
                btnSend.Enabled = false;
                groupMembers.Clear();
                UpdateGroupMembersListAsync();
                
                AppendMessage("System", "❌ Disconnected from server", DateTime.Now, Color.Red);
                
                // Show reconnection options
                ShowReconnectDialog();
            }
            else if (state.Contains("Error") || state.Contains("Failed"))
            {
                AppendMessage("System", $"⚠️ Connection Error: {state}", DateTime.Now, Color.Red);
                ShowReconnectDialog();
            }
        }

        private void OnErrorOccurred(object sender, string error)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => AppendMessage("Error", error, DateTime.Now, Color.Red)));
                return;
            }

            AppendMessage("Error", error, DateTime.Now, Color.Red);
        }

        private void AppendMessage(string sender, string message, DateTime timestamp, Color color)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => AppendMessage(sender, message, timestamp, color)));
                return;
            }

            string timeString = timestamp.ToString("HH:mm:ss");
            string formattedMessage = $"[{timeString}] {sender}: {message}\n";
            
            // Prevent excessive message history to avoid performance issues
            if (rtbMessages.Lines.Length > 1000)
            {
                // Remove first 200 lines to keep history manageable
                var lines = rtbMessages.Lines;
                var newLines = lines.Skip(200).ToArray();
                rtbMessages.Lines = newLines;
            }
            
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
            if (!string.IsNullOrWhiteSpace(txtMessage.Text) && networkClient?.IsConnected == true)
            {
                string message = txtMessage.Text.Trim();
                txtMessage.Clear(); // Clear immediately to prevent double-send
                
                // Disable send button temporarily to prevent spam
                btnSend.Enabled = false;
                
                try
                {
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
                    else if (currentChatTarget == "Group Chat")
                    {
                        // Send group message
                        await networkClient.SendGroupMessageAsync(message);
                    }
                    else
                    {
                        // Send private message to current chat target
                        await networkClient.SendPrivateMessageAsync(currentChatTarget, message);
                        
                        // Add to private chat history
                        if (!privateChatHistory.ContainsKey(currentChatTarget))
                        {
                            privateChatHistory[currentChatTarget] = new List<string>();
                        }
                        string formattedMessage = $"[{DateTime.Now:HH:mm:ss}] You: {message}";
                        privateChatHistory[currentChatTarget].Add(formattedMessage);
                        
                        // Display in chat
                        AppendMessage("You", message, DateTime.Now, Color.Black);
                    }
                }
                catch (Exception ex)
                {
                    AppendMessage("Error", $"Failed to send message: {ex.Message}", DateTime.Now, Color.Red);
                }
                finally
                {
                    // Re-enable send button
                    if (InvokeRequired)
                    {
                        BeginInvoke(new Action(() => btnSend.Enabled = true));
                    }
                    else
                    {
                        btnSend.Enabled = true;
                    }
                }
            }
        }

        private async void BtnDisconnect_Click(object sender, EventArgs e)
        {
            try
            {
                if (networkClient != null && networkClient.IsConnected)
                {
                    await networkClient.DisconnectAsync();
                    AppendMessage("System", "Disconnected from server.", DateTime.Now, Color.Orange);
                    
                    // Clear group members and update UI
                    groupMembers.Clear();
                    UpdateGroupMembersListAsync();
                    
                    // Disable chat controls
                    txtMessage.Enabled = false;
                    btnSend.Enabled = false;
                    lblContactStatus.Text = "Disconnected";
                }
                else
                {
                    AppendMessage("System", "Already disconnected.", DateTime.Now, Color.Gray);
                }
            }
            catch (Exception ex)
            {
                AppendMessage("System", $"Error during disconnect: {ex.Message}", DateTime.Now, Color.Red);
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
                    // Note: Online users are managed by server, this is legacy code
                    MessageBox.Show($"Note: Online users are automatically managed by the server.\nUsers online are displayed in the left panel.");
                }
                else
                {
                    MessageBox.Show("Please enter a valid username.");
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
            try
            {
                if (networkClient != null && networkClient.IsConnected)
                {
                    await networkClient.DisconnectAsync();
                    networkClient.Dispose();
                }
            }
            catch (Exception ex)
            {
                // Log error but don't prevent closing
                System.Diagnostics.Debug.WriteLine($"Error during cleanup: {ex.Message}");
            }
            
            base.OnFormClosing(e);
            Application.Exit();
        }

        // Theme Methods
        private struct ThemeColors
        {
            // Light Mode Colors (Windows 95 style)
            public static readonly Color LightBackground = Color.FromArgb(192, 192, 192);
            public static readonly Color LightForeground = Color.Black;
            public static readonly Color LightPanelBackground = Color.FromArgb(212, 208, 200);
            public static readonly Color LightTextBoxBackground = Color.White;
            public static readonly Color LightButtonBackground = Color.FromArgb(192, 192, 192);
            
            // Dark Mode Colors
            public static readonly Color DarkBackground = Color.FromArgb(43, 43, 43);
            public static readonly Color DarkForeground = Color.White;
            public static readonly Color DarkPanelBackground = Color.FromArgb(35, 35, 35);
            public static readonly Color DarkTextBoxBackground = Color.FromArgb(60, 60, 60);
            public static readonly Color DarkButtonBackground = Color.FromArgb(70, 70, 70);
        }

        private void ApplyTheme()
        {
            if (isDarkMode)
            {
                // Apply Dark Theme
                this.BackColor = ThemeColors.DarkBackground;
                this.ForeColor = ThemeColors.DarkForeground;
                
                // Panels
                panelContacts.BackColor = ThemeColors.DarkPanelBackground;
                panelContacts.ForeColor = ThemeColors.DarkForeground;
                panelChat.BackColor = ThemeColors.DarkPanelBackground;
                panelChat.ForeColor = ThemeColors.DarkForeground;
                panelInfo.BackColor = ThemeColors.DarkPanelBackground;
                panelInfo.ForeColor = ThemeColors.DarkForeground;
                panelChatHeader.BackColor = ThemeColors.DarkPanelBackground;
                panelMessageInput.BackColor = ThemeColors.DarkPanelBackground;
                
                // Labels
                lblContacts.ForeColor = ThemeColors.DarkForeground;
                lblContactName.ForeColor = ThemeColors.DarkForeground;
                lblContactStatus.ForeColor = Color.LightGray;
                lblInfoTitle.ForeColor = ThemeColors.DarkForeground;
                
                // TextBoxes and RichTextBox
                txtSearch.BackColor = ThemeColors.DarkTextBoxBackground;
                txtSearch.ForeColor = ThemeColors.DarkForeground;
                txtMessage.BackColor = ThemeColors.DarkTextBoxBackground;
                txtMessage.ForeColor = ThemeColors.DarkForeground;
                rtbMessages.BackColor = ThemeColors.DarkTextBoxBackground;
                rtbMessages.ForeColor = ThemeColors.DarkForeground;
                
                // Buttons
                btnNewChat.BackColor = ThemeColors.DarkButtonBackground;
                btnNewChat.ForeColor = ThemeColors.DarkForeground;
                btnDisconnect.BackColor = ThemeColors.DarkButtonBackground;
                btnDisconnect.ForeColor = ThemeColors.DarkForeground;
                btnSend.BackColor = ThemeColors.DarkButtonBackground;
                btnSend.ForeColor = ThemeColors.DarkForeground;
                btnThemeToggle.BackColor = ThemeColors.DarkButtonBackground;
                btnThemeToggle.ForeColor = ThemeColors.DarkForeground;
                btnThemeToggle.Text = "☀️ Light Mode";
                
                // ListView
                listOnlineUsers.BackColor = ThemeColors.DarkTextBoxBackground;
                listOnlineUsers.ForeColor = ThemeColors.DarkForeground;
                
                // ListBox
                listGroupMembers.BackColor = ThemeColors.DarkTextBoxBackground;
                listGroupMembers.ForeColor = ThemeColors.DarkForeground;
            }
            else
            {
                // Apply Light Theme (Windows 95 style)
                this.BackColor = ThemeColors.LightBackground;
                this.ForeColor = ThemeColors.LightForeground;
                
                // Panels
                panelContacts.BackColor = ThemeColors.LightPanelBackground;
                panelContacts.ForeColor = ThemeColors.LightForeground;
                panelChat.BackColor = ThemeColors.LightPanelBackground;
                panelChat.ForeColor = ThemeColors.LightForeground;
                panelInfo.BackColor = ThemeColors.LightPanelBackground;
                panelInfo.ForeColor = ThemeColors.LightForeground;
                panelChatHeader.BackColor = ThemeColors.LightPanelBackground;
                panelMessageInput.BackColor = ThemeColors.LightPanelBackground;
                
                // Labels
                lblContacts.ForeColor = ThemeColors.LightForeground;
                lblContactName.ForeColor = ThemeColors.LightForeground;
                lblContactStatus.ForeColor = Color.Gray;
                lblInfoTitle.ForeColor = ThemeColors.LightForeground;
                
                // TextBoxes and RichTextBox
                txtSearch.BackColor = ThemeColors.LightTextBoxBackground;
                txtSearch.ForeColor = ThemeColors.LightForeground;
                txtMessage.BackColor = ThemeColors.LightTextBoxBackground;
                txtMessage.ForeColor = ThemeColors.LightForeground;
                rtbMessages.BackColor = ThemeColors.LightTextBoxBackground;
                rtbMessages.ForeColor = ThemeColors.LightForeground;
                
                // Buttons
                btnNewChat.BackColor = ThemeColors.LightButtonBackground;
                btnNewChat.ForeColor = ThemeColors.LightForeground;
                btnDisconnect.BackColor = ThemeColors.LightButtonBackground;
                btnDisconnect.ForeColor = ThemeColors.LightForeground;
                btnSend.BackColor = ThemeColors.LightButtonBackground;
                btnSend.ForeColor = ThemeColors.LightForeground;
                btnThemeToggle.BackColor = ThemeColors.LightButtonBackground;
                btnThemeToggle.ForeColor = ThemeColors.LightForeground;
                btnThemeToggle.Text = "🌙 Dark Mode";
                
                // ListView
                listOnlineUsers.BackColor = ThemeColors.LightTextBoxBackground;
                listOnlineUsers.ForeColor = ThemeColors.LightForeground;
                
                // ListBox
                listGroupMembers.BackColor = ThemeColors.LightTextBoxBackground;
                listGroupMembers.ForeColor = ThemeColors.LightForeground;
            }
        }

        private void BtnThemeToggle_Click(object sender, EventArgs e)
        {
            isDarkMode = !isDarkMode;
            ApplyTheme();
        }

        private void LoadThemePreference()
        {
            try
            {
                string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                string configPath = Path.Combine(appDataPath, "WhatsApp95Chat", "theme.txt");
                
                if (File.Exists(configPath))
                {
                    string themeData = File.ReadAllText(configPath);
                    isDarkMode = themeData.Trim().ToLower() == "dark";
                }
            }
            catch (Exception ex)
            {
                // If error loading theme preference, use default (light mode)
                Console.WriteLine($"Error loading theme preference: {ex.Message}");
                isDarkMode = false;
            }
        }

        private void SaveThemePreference()
        {
            try
            {
                string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                string configDir = Path.Combine(appDataPath, "WhatsApp95Chat");
                string configPath = Path.Combine(configDir, "theme.txt");
                
                Directory.CreateDirectory(configDir);
                File.WriteAllText(configPath, isDarkMode ? "dark" : "light");
            }
            catch (Exception ex)
            {
                // Silent fail - theme preference not critical
                Console.WriteLine($"Error saving theme preference: {ex.Message}");
            }
        }

        // Add IDisposable pattern
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                networkClient?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}