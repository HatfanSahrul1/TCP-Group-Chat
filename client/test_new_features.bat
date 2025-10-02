@echo off
echo ===============================================
echo       WhatsApp95 TCP Chat - Quick Test
echo ===============================================
echo.
echo This will start:
echo 1. Server (in background)
echo 2. Client (main window)
echo.
echo Features to test:
echo - Username uniqueness
echo - Online users list (ListView)
echo - Private messages (double-click user or right-click)
echo - Group chat
echo - Join/Leave notifications
echo - Disconnect/Reconnect handling
echo.
pause

echo.
echo [1/2] Starting Server in background...
cd /d "%~dp0\..\server"
start "TCP Chat Server" cmd /k "dotnet run"

echo.
echo [2/2] Waiting 3 seconds for server to start...
timeout /t 3 > nul

echo.
echo Starting Client...
cd /d "%~dp0"
dotnet run

echo.
echo Test complete!
pause