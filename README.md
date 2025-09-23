# TCP Group Chat Application

## Deskripsi
Aplikasi group chat berbasis TCP dengan arsitektur client-server yang dibuat menggunakan C# .NET 8 dan Windows Forms. Aplikasi ini memiliki tampilan retro ala Windows 95.

## Fitur
- **Group Chat Real-time**: Multiple client dapat bergabung dalam satu grup chat
- **Join/Leave Notifications**: Notifikasi otomatis ketika ada member yang bergabung atau keluar
- **Member List**: Daftar member yang sedang online di grup
- **Private Messages**: Fitur pesan pribadi dengan command `/w username message`
- **TCP Protocol**: Menggunakan protokol TCP untuk komunikasi yang reliable
- **JSON Messages**: Format pesan menggunakan JSON untuk pertukaran data

## Arsitektur

### Server (`serverChatTCP`)
- **ChatServer.cs**: Class utama untuk menangani server logic
- **Program.cs**: Entry point untuk menjalankan server
- Dapat menangani multiple client secara bersamaan (≥5 client)
- Broadcast pesan ke semua client yang terhubung
- Tracking member list dan notifikasi join/leave

### Client (`tcp-group-chat`)
- **ChatForm.cs**: UI form untuk chat interface
- **NetworkClient.cs**: Class untuk menangani koneksi TCP ke server
- **Form1.cs**: Login form untuk input username
- **Program.cs**: Entry point aplikasi client

## Cara Menjalankan

### 🚀 Quick Start (Recommended)

**Opsi 1: Automatic Testing**
```bash
# Test otomatis dengan 1 server + 1 client
double-click test_application.bat
```

**Opsi 2: Multi-Client Testing**
```bash
# 1. Jalankan server terlebih dahulu
double-click start_server.bat

# 2. Test multiple clients
double-click test_multiple_clients.bat
```

### ⚙️ Manual Setup

**1. Menjalankan Server**
```bash
cd serverChatTCP
dotnet run
```
Atau jalankan file `start_server.bat`

Server akan berjalan di port 8888. Perintah server:
- `/list` - Tampilkan daftar client yang terhubung
- `/stop` - Matikan server  
- `/help` - Tampilkan bantuan

**2. Menjalankan Client**
```bash
cd tcp-group-chat
dotnet run
```
Atau jalankan file `start_client.bat`

**3. Testing Multiple Clients**
- Jalankan server terlebih dahulu
- Buka multiple instance client (copy folder atau run multiple times)
- Masukkan username yang berbeda untuk setiap client
- Test fitur group chat, join/leave notifications, dan private messages

### 🐛 Troubleshooting

**Error: "Failed to connect to server"**
- Pastikan server sudah running di port 8888
- Cek Windows Firewall settings
- Pastikan tidak ada aplikasi lain yang menggunakan port 8888

**Error: "Object reference not set"**  
- ✅ Issue ini sudah diperbaiki di versi terbaru
- Pastikan menggunakan code yang sudah di-update

**Error: "JSON parsing error"**
- ✅ Issue ini sudah diperbaiki dengan length-prefixed protocol
- Server sekarang menggunakan reliable message framing
- Tidak ada lagi corruption atau partial message issues

**Multiple clients tidak bisa connect**
- Server sudah mendukung multiple connections
- Gunakan username yang berbeda untuk setiap client

**Messages tidak sampai atau corrupt**
- ✅ Fixed: Protokol baru menjamin message integrity
- Length-prefixed format eliminasi semua parsing errors

## Protokol Data

Aplikasi menggunakan **length-prefixed JSON protocol** untuk pertukaran pesan yang reliable:

### Wire Format
```
[4-byte length (Int32)][JSON message (UTF-8)]
```

### JSON Message Structure
```json
{
  "Type": "msg|join|leave|pm|sys",
  "From": "username",
  "To": "target_username", // untuk private message
  "Text": "message content", 
  "Ts": 1737131234 // unix timestamp
}
```

### Keunggulan Protokol
- ✅ **Reliable**: Setiap pesan dijamin lengkap dan tidak corrupt
- ✅ **Efficient**: Minimal overhead, hanya 4 bytes header
- ✅ **Scalable**: Mendukung pesan dengan ukuran bervariasi
- ✅ **Error-Free**: Eliminasi JSON parsing errors

### Jenis Pesan
- **msg**: Pesan grup (broadcast)
- **join**: Notifikasi user bergabung
- **leave**: Notifikasi user keluar
- **pm**: Private message
- **sys**: Pesan sistem dari server

## Fitur UI
- **Retro Windows 95 Style**: Tampilan klasik dengan color scheme abu-abu
- **Responsive Layout**: Panel yang dapat di-resize
- **Real-time Updates**: Member list dan chat messages update secara real-time
- **Color-coded Messages**: 
  - Hitam: Pesan normal
  - Hijau: Join notifications
  - Orange: Leave notifications
  - Biru: Private messages
  - Merah: Error/System messages

## Private Messages
Untuk mengirim pesan pribat, gunakan format:
```
/w username pesan anda
```

Contoh:
```
/w john Halo john, bagaimana kabarmu?
```

## Teknologi
- **C# .NET 8**
- **Windows Forms** untuk GUI
- **TCP Sockets** (TcpListener, TcpClient, NetworkStream)
- **Asynchronous Programming** (async/await)
- **JSON Serialization** untuk protokol data

## Requirements
- .NET 8 SDK
- Windows OS (untuk Windows Forms)
- Port 8888 harus tersedia untuk server

## Catatan Teknis
- Server mendukung multiple client concurrent
- Automatic username disambiguation (jika ada duplicate, akan ditambah suffix)
- Error handling untuk connection loss dan invalid data
- Thread-safe operations untuk multiple client handling
- UI thread synchronization untuk update real-time
Projek Praktikum Jaringan Komputer Semester 5
