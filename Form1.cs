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
            
            // Tombol Join
            btnJoin = new Button();
            btnJoin.Location = new Point(30, 85);
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
            this.Size = new Size(300, 170);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.BackColor = Color.FromArgb(192, 192, 192);
            
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