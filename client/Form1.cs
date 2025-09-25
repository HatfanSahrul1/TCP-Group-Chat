using System;
using System.Drawing;
using System.Windows.Forms;

namespace tcp_group_chat
{
    public class Form1 : Form
    {
        private TextBox txtNama;
        private TextBox txtServerIP;
        private TextBox txtServerPort;
        private Button btnJoin;
        private Label lblNama;
        private Label lblServerIP;
        private Label lblServerPort;

        public Form1()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            // Label Username
            lblNama = new Label();
            lblNama.Location = new Point(30, 25);
            lblNama.Size = new Size(80, 20);
            lblNama.Text = "User Name:";
            lblNama.Font = new Font("MS Sans Serif", 8, FontStyle.Regular);
            lblNama.BackColor = Color.FromArgb(192, 192, 192);
            lblNama.ForeColor = Color.Black;
            
            // TextBox untuk nama
            txtNama = new TextBox();
            txtNama.Location = new Point(30, 50);
            txtNama.Size = new Size(220, 20);
            txtNama.Font = new Font("MS Sans Serif", 8, FontStyle.Regular);
            txtNama.BackColor = Color.White;
            txtNama.BorderStyle = BorderStyle.Fixed3D;
            txtNama.KeyPress += TxtNama_KeyPress;

            // Label Server IP
            lblServerIP = new Label();
            lblServerIP.Location = new Point(30, 85);
            lblServerIP.Size = new Size(80, 20);
            lblServerIP.Text = "Server IP:";
            lblServerIP.Font = new Font("MS Sans Serif", 8, FontStyle.Regular);
            lblServerIP.BackColor = Color.FromArgb(192, 192, 192);
            lblServerIP.ForeColor = Color.Black;
            
            // TextBox untuk Server IP
            txtServerIP = new TextBox();
            txtServerIP.Location = new Point(30, 110);
            txtServerIP.Size = new Size(140, 20);
            txtServerIP.Font = new Font("MS Sans Serif", 8, FontStyle.Regular);
            txtServerIP.BackColor = Color.White;
            txtServerIP.BorderStyle = BorderStyle.Fixed3D;
            txtServerIP.Text = "127.0.0.1"; // Default localhost

            // Label Server Port
            lblServerPort = new Label();
            lblServerPort.Location = new Point(180, 85);
            lblServerPort.Size = new Size(70, 20);
            lblServerPort.Text = "Port:";
            lblServerPort.Font = new Font("MS Sans Serif", 8, FontStyle.Regular);
            lblServerPort.BackColor = Color.FromArgb(192, 192, 192);
            lblServerPort.ForeColor = Color.Black;
            
            // TextBox untuk Server Port
            txtServerPort = new TextBox();
            txtServerPort.Location = new Point(180, 110);
            txtServerPort.Size = new Size(70, 20);
            txtServerPort.Font = new Font("MS Sans Serif", 8, FontStyle.Regular);
            txtServerPort.BackColor = Color.White;
            txtServerPort.BorderStyle = BorderStyle.Fixed3D;
            txtServerPort.Text = "8888"; // Default port
            
            // Tombol Join
            btnJoin = new Button();
            btnJoin.Location = new Point(30, 145);
            btnJoin.Size = new Size(220, 25);
            btnJoin.Text = "Connect to Chat";
            btnJoin.Font = new Font("MS Sans Serif", 8, FontStyle.Regular);
            btnJoin.BackColor = Color.FromArgb(192, 192, 192);
            btnJoin.ForeColor = Color.Black;
            btnJoin.FlatStyle = FlatStyle.Standard;
            btnJoin.UseVisualStyleBackColor = false;
            btnJoin.Click += BtnJoin_Click;
            
            // Form properties
            this.Text = "WhatsApp95 - Login";
            this.Size = new Size(300, 220);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.BackColor = Color.FromArgb(192, 192, 192);
            
            // Tambahkan controls ke form
            this.Controls.Add(lblNama);
            this.Controls.Add(txtNama);
            this.Controls.Add(lblServerIP);
            this.Controls.Add(txtServerIP);
            this.Controls.Add(lblServerPort);
            this.Controls.Add(txtServerPort);
            this.Controls.Add(btnJoin);
        }

        private void BtnJoin_Click(object sender, EventArgs e)
        {
            string nama = txtNama.Text.Trim();
            string serverIP = txtServerIP.Text.Trim();
            string serverPort = txtServerPort.Text.Trim();
            
            if (string.IsNullOrWhiteSpace(nama))
            {
                MessageBox.Show("Silakan masukkan nama terlebih dahulu!");
                return;
            }
            
            if (string.IsNullOrWhiteSpace(serverIP))
            {
                MessageBox.Show("Silakan masukkan Server IP!");
                return;
            }
            
            if (!int.TryParse(serverPort, out int port) || port <= 0 || port > 65535)
            {
                MessageBox.Show("Port harus berupa angka antara 1-65535!");
                return;
            }
            
            // Buat dan tampilkan form chat dengan server info
            ChatForm chatForm = new ChatForm(nama, serverIP, port);
            chatForm.Show();
            
            // Tutup form login
            this.Hide();
        }

        private void TxtNama_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                BtnJoin_Click(sender, e);
            }
        }
    }
}