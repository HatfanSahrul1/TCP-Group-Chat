using System;
using System.Drawing;
using System.Windows.Forms;

namespace tcp_group_chat
{
    public class Form1 : Form
    {
        private TextBox txtNama;
        private Button btnJoin;
        private Label lblNama;

        public Form1()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            // Label
            lblNama = new Label();
            lblNama.Location = new Point(50, 30);
            lblNama.Size = new Size(100, 20);
            lblNama.Text = "Nama:";
            lblNama.Font = new Font("Arial", 10, FontStyle.Bold);
            
            // TextBox untuk nama
            txtNama = new TextBox();
            txtNama.Location = new Point(50, 50);
            txtNama.Size = new Size(200, 23);
            txtNama.PlaceholderText = "Masukkan nama Anda";
            
            // Tombol Join
            btnJoin = new Button();
            btnJoin.Location = new Point(50, 90);
            btnJoin.Size = new Size(200, 30);
            btnJoin.Text = "Join Chat";
            btnJoin.BackColor = Color.LightBlue;
            btnJoin.Click += BtnJoin_Click;
            
            // Form properties
            this.Text = "Login - Group Chat App";
            this.Size = new Size(300, 200);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.BackColor = Color.White;
            
            // Tambahkan controls ke form
            this.Controls.Add(lblNama);
            this.Controls.Add(txtNama);
            this.Controls.Add(btnJoin);
        }

        private void BtnJoin_Click(object sender, EventArgs e)
        {
            string nama = txtNama.Text.Trim();
            
            if (string.IsNullOrWhiteSpace(nama))
            {
                MessageBox.Show("Silakan masukkan nama terlebih dahulu!");
                return;
            }
            
            // Buat dan tampilkan form chat
            ChatForm chatForm = new ChatForm(nama);
            chatForm.Show();
            
            // Tutup form login
            this.Hide();
        }
    }
}