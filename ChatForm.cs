using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;

namespace tcp_group_chat
{
    public class ChatForm : Form
    {
        private string username;
        
        // Controls untuk sidebar grup
        private Panel panelSidebar;
        private ListBox listGroups;
        private Button btnCreateGroup;
        private TextBox txtNewGroup;
        
        // Controls untuk area chat
        private Panel panelChat;
        private ListBox listMessages;
        private TextBox txtMessage;
        private Button btnSend;
        private Label lblCurrentGroup;

        public ChatForm(string username)
        {
            this.username = username;
            InitializeComponent();
            LoadSampleGroups();
        }

        private void InitializeComponent()
        {
            this.Text = $"Group Chat - {username}";
            this.Size = new Size(800, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.BackColor = Color.White;

            // ===== SIDEBAR (Kiri) =====
            panelSidebar = new Panel();
            panelSidebar.Dock = DockStyle.Left;
            panelSidebar.Width = 200;
            panelSidebar.BackColor = Color.LightGray;
            panelSidebar.BorderStyle = BorderStyle.FixedSingle;

            // Label Grup
            Label lblGroups = new Label();
            lblGroups.Text = "Grup Tersedia";
            lblGroups.Font = new Font("Arial", 10, FontStyle.Bold);
            lblGroups.Location = new Point(10, 10);
            lblGroups.Size = new Size(180, 20);

            // ListBox untuk grup
            listGroups = new ListBox();
            listGroups.Location = new Point(10, 40);
            listGroups.Size = new Size(180, 400);
            listGroups.SelectedIndexChanged += ListGroups_SelectedIndexChanged;

            // TextBox untuk buat grup baru
            txtNewGroup = new TextBox();
            txtNewGroup.Location = new Point(10, 450);
            txtNewGroup.Size = new Size(120, 23);
            txtNewGroup.PlaceholderText = "Nama grup baru";

            // Button untuk buat grup
            btnCreateGroup = new Button();
            btnCreateGroup.Location = new Point(135, 450);
            btnCreateGroup.Size = new Size(55, 23);
            btnCreateGroup.Text = "Buat";
            btnCreateGroup.Click += BtnCreateGroup_Click;

            // Tambahkan controls ke sidebar
            panelSidebar.Controls.Add(lblGroups);
            panelSidebar.Controls.Add(listGroups);
            panelSidebar.Controls.Add(txtNewGroup);
            panelSidebar.Controls.Add(btnCreateGroup);

            // ===== AREA CHAT (Kanan) =====
            panelChat = new Panel();
            panelChat.Dock = DockStyle.Fill;
            panelChat.BackColor = Color.White;

            // Label grup saat ini
            lblCurrentGroup = new Label();
            lblCurrentGroup.Text = "Pilih grup untuk mulai chat";
            lblCurrentGroup.Font = new Font("Arial", 12, FontStyle.Bold);
            lblCurrentGroup.Location = new Point(20, 10);
            lblCurrentGroup.Size = new Size(500, 25);

            // ListBox untuk pesan
            listMessages = new ListBox();
            listMessages.Location = new Point(20, 40);
            listMessages.Size = new Size(530, 400);
            listMessages.Font = new Font("Arial", 10);

            // TextBox untuk mengetik pesan
            txtMessage = new TextBox();
            txtMessage.Location = new Point(20, 450);
            txtMessage.Size = new Size(450, 23);
            txtMessage.PlaceholderText = "Ketik pesan di sini...";
            txtMessage.Enabled = false;

            // Button untuk mengirim pesan
            btnSend = new Button();
            btnSend.Location = new Point(475, 450);
            btnSend.Size = new Size(75, 23);
            btnSend.Text = "Kirim";
            btnSend.Enabled = false;
            btnSend.Click += BtnSend_Click;

            // Tambahkan controls ke area chat
            panelChat.Controls.Add(lblCurrentGroup);
            panelChat.Controls.Add(listMessages);
            panelChat.Controls.Add(txtMessage);
            panelChat.Controls.Add(btnSend);

            // Tambahkan panels ke form
            this.Controls.Add(panelChat);
            this.Controls.Add(panelSidebar);
        }

        private void LoadSampleGroups()
        {
            // Grup sample
            listGroups.Items.Add("General");
            listGroups.Items.Add("Programming");
            listGroups.Items.Add("Gaming");
            listGroups.Items.Add("Music");
            listGroups.Items.Add("Movies");
        }

        private void ListGroups_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listGroups.SelectedItem != null)
            {
                string selectedGroup = listGroups.SelectedItem.ToString();
                lblCurrentGroup.Text = $"Grup: {selectedGroup}";
                
                // Enable chat controls
                txtMessage.Enabled = true;
                btnSend.Enabled = true;
                
                // Load messages (sample data)
                LoadSampleMessages(selectedGroup);
            }
        }

        private void LoadSampleMessages(string groupName)
        {
            listMessages.Items.Clear();
            
            // Sample messages
            listMessages.Items.Add($"Selamat datang di grup {groupName}!");
            listMessages.Items.Add("Admin: Halo semua! 👋");
            listMessages.Items.Add("User1: Hi, apa kabar?");
            listMessages.Items.Add("User2: Baik, terima kasih!");
            
            // Auto-scroll ke bawah
            listMessages.TopIndex = listMessages.Items.Count - 1;
        }

        private void BtnSend_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(txtMessage.Text) && listGroups.SelectedItem != null)
            {
                string message = $"{username}: {txtMessage.Text}";
                listMessages.Items.Add(message);
                txtMessage.Clear();
                
                // Auto-scroll ke pesan terbaru
                listMessages.TopIndex = listMessages.Items.Count - 1;
            }
        }

        private void BtnCreateGroup_Click(object sender, EventArgs e)
        {
            string newGroup = txtNewGroup.Text.Trim();
            
            if (!string.IsNullOrWhiteSpace(newGroup))
            {
                if (!listGroups.Items.Contains(newGroup))
                {
                    listGroups.Items.Add(newGroup);
                    txtNewGroup.Clear();
                    MessageBox.Show($"Grup '{newGroup}' berhasil dibuat!");
                }
                else
                {
                    MessageBox.Show("Grup sudah ada!");
                }
            }
        }

        // Handle form closing
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            Application.Exit();
        }
    }
}