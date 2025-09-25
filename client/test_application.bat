@echo off
echo =======================================
echo     Testing TCP Group Chat
echo =======================================
echo.
echo This script will test the application:
echo 1. First, it will run the server in background
echo 2. Then run a client to test connection
echo.
echo Press any key to start testing...
pause > nul

echo.
echo [1/2] Starting Server...
cd /d "c:\Users\THINKPAD\dotnet_kuliah\serverChatTCP"
start "ChatServer" cmd /k "dotnet run"

echo.
echo [2/2] Waiting 3 seconds for server to start...
timeout /t 3 > nul

echo.
echo Starting Client...
cd /d "c:\Users\THINKPAD\dotnet_kuliah\tcp-group-chat"
dotnet run

echo.
echo Testing complete!
pause