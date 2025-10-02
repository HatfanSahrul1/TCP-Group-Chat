# TCP Group Chat - WhatsApp95 Style

## 🎯 Status: **WEEK 2 COMPLETED** ✅

Aplikasi chat group TCP multi-client dengan antarmuka Windows Forms bergaya retro tahun 90-an. 
**All Target Minggu 2 requirements telah diimplementasi!**

## 🚀 Week 2 Features - COMPLETED ✅
- ✅ **Username Unik**: Server auto-handle duplicate usernames
- ✅ **Online Users ListView**: Interactive user list dengan context menu
- ✅ **Private Messaging**: `/w command`, double-click, right-click
- ✅ **Join/Leave Notifications**: Color-coded system messages
- ✅ **Dark/Light Mode Toggle**: Professional theme switching dengan persistence
- ✅ **Error Handling**: Reconnect dialogs, graceful degradation
- ✅ **Graceful Shutdown**: Proper resource disposal
- ✅ **Server Logging**: Comprehensive file-based logging system

## 🎮 Features Overview

### **Core Features**
- **Multi-client Support**: Support unlimited concurrent clients
- **Real-time Group Chat**: Instant message delivery ke semua client
- **Private Messaging**: Multiple ways untuk send private messages
- **Online Users Display**: Interactive ListView dengan user management
- **Windows 95 Theme**: Authentic retro interface experience

### **New Advanced Features**
- **Smart Username System**: Auto-generate unique usernames
- **Private Chat Mode**: Dedicated private chat interface
- **Color-coded Messages**: Different colors untuk different message types
- **Connection Recovery**: Smart reconnection dengan user confirmation
- **Comprehensive Logging**: Server-side file logging system
- **Context Menus**: Right-click actions untuk enhanced UX

## 🏗️ Architecture
- **Server**: TCP server (TcpListener) di port 8888 dengan logging
- **Client**: Windows Forms app dengan enhanced NetworkClient
- **Protocol**: Length-prefixed JSON messaging untuk reliability
- **UI**: Windows Forms dengan Windows 95 styling & modern features
- **Logging**: File-based server logging dengan timestamps

## 🔧 Requirements
- .NET 8.0 atau lebih tinggi
- Windows OS (untuk Windows Forms)
- Port 8888 tersedia (configurable di kode)

## ⚡ Quick Start

### Option 1: Using Batch Files (Recommended)
```bash
# Terminal 1: Start Server
cd server
start_server.bat

# Terminal 2+: Start Client(s) 
cd client
start_client.bat
```

### Option 2: Manual dotnet run
```bash
# Terminal 1: Server
cd server
dotnet run

# Terminal 2+: Client(s)
cd client
dotnet run
```

## 🧪 Testing New Features

### Quick Feature Test
```bash
cd client
test_new_features.bat
```

### Manual Testing Steps
1. **Start Server**: `cd server && dotnet run`
2. **Start Multiple Clients**: Run client multiple times
3. **Test Features**:
   - Try duplicate usernames → See auto-uniqueness
   - Double-click user di ListView → Private chat
   - Right-click user → Context menu
   - Use `/w username message` → Private message
   - Close server → See reconnect dialog
   - Click "Disconnect" → Clean app exit

## 🎯 Usage Guide

### **Group Chat**
1. Jalankan server terlebih dahulu
2. Jalankan client(s) - bisa multiple instances
3. Enter username (duplicates akan auto-renamed)
4. Start chatting dalam group!

### **Private Chat**
- **Method 1**: Double-click username di Online Users ListView
- **Method 2**: Right-click username → "Private Chat"  
- **Method 3**: Type `/w username your message`
- **Switch Back**: Click "Group Chat" button

### **Advanced Features**
- **Reconnection**: Jika server disconnect, pilih reconnect/close/offline
- **Error Recovery**: App tetap usable dengan graceful error handling
- **Logging**: Check `server/logs/` untuk comprehensive server logs

## 📁 Project Structure
```
tcp-group-chat/
├── WEEK2_FEATURES.md          # Detailed week 2 feature documentation
├── server/                    # Enhanced TCP Server
│   ├── ChatServer.cs         # Server dengan logging & error handling
│   ├── Program.cs            # Server entry point
│   ├── logs/                 # Auto-created log directory
│   └── start_server.bat      # Server startup script
├── client/                    # Enhanced Windows Forms Client
│   ├── ChatForm.cs           # Main UI dengan ListView & private chat
│   ├── NetworkClient.cs      # Enhanced TCP client networking
│   ├── Program.cs            # Client entry point
│   ├── start_client.bat      # Client startup script
│   └── test_new_features.bat # Feature testing script
└── README.md                 # This file
```

## 🎨 Message Color System
- 🖤 **Black**: Normal group messages
- 💙 **Blue**: Private messages you sent/received
- 🟢 **Green**: User join notifications
- 🟠 **Orange**: User leave notifications  
- 🔴 **Red**: Error messages & system alerts
- 🟣 **Purple**: Private message notifications

## 🐛 Known Limitations
- Private chat history tidak persistent (reset saat restart)
- Belum ada typing indicators
- Belum ada emoji support
- Log files bertambah tanpa rotation

## 🎉 Achievement Unlocked: Week 2 Complete! ✅

All target requirements dari "Target Minggu 2 (Reliability, UX, & Polishing)" telah berhasil diimplementasi dan tested. Ready untuk production testing dengan multiple concurrent clients!

See `WEEK2_FEATURES.md` untuk detailed technical documentation.
