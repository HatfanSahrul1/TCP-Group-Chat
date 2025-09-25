@echo off
echo =======================================
echo   JSON Protocol Fix Verification
echo =======================================
echo.
echo This test will verify that the JSON parsing error is fixed.
echo.
echo What this test does:
echo 1. Starts the server
echo 2. Connects a client rapidly
echo 3. Sends multiple rapid messages to stress test the protocol
echo 4. Verifies no JSON parsing errors occur
echo.
echo Press any key to start the test...
pause > nul

echo.
echo [1/3] Starting Server...
cd /d "c:\Users\THINKPAD\dotnet_kuliah\serverChatTCP"
start "ChatServer-Test" cmd /k "title ChatServer-Test && dotnet run"

echo.
echo [2/3] Waiting 5 seconds for server to fully start...
timeout /t 5 > nul

echo.
echo [3/3] Starting Test Client...
echo.
echo Instructions for testing:
echo 1. Enter a username when prompted
echo 2. Send multiple rapid messages (press Enter quickly several times)
echo 3. Check server window for any JSON parsing errors
echo 4. If no errors appear, the fix is working!
echo.
cd /d "c:\Users\THINKPAD\dotnet_kuliah\tcp-group-chat"
dotnet run

echo.
echo Test completed!
echo Check the server window - you should see NO JSON parsing errors.
echo.
pause