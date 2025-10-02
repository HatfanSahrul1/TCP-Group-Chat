@echo off
title WhatsApp95 Chat - Username Uniqueness Test
color 0F

echo.
echo ========================================
echo  Username Uniqueness Test
echo ========================================
echo.
echo Testing improved username uniqueness handling
echo.
echo Features to test:
echo 1. Server uses readable format: username_02, username_03, etc.
echo 2. Client receives confirmation of actual username
echo 3. Window title updates to show correct username
echo 4. Users can message each other using correct usernames
echo.
echo Instructions:
echo 1. Server will start automatically
echo 2. Multiple clients will open
echo 3. Try using SAME username in different clients
echo 4. Check window titles show correct usernames
echo 5. Test messaging between users with similar names
echo.
echo Press any key to start testing...
pause >nul

echo.
echo Starting server...
start "WhatsApp95 Server" cmd /k "cd ..\server && dotnet run"

timeout /t 3 >nul

echo Starting multiple clients...
echo Please use the SAME username in all clients to test uniqueness
echo.

start "Client 1" cmd /k "dotnet run"
timeout /t 2 >nul
start "Client 2" cmd /k "dotnet run" 
timeout /t 2 >nul
start "Client 3" cmd /k "dotnet run"

echo.
echo Username Uniqueness Test Started!
echo.
echo Test Scenarios:
echo ✓ Use same username "test" in all 3 clients
echo ✓ Check window titles: "WhatsApp95 - test", "WhatsApp95 - test_02", "WhatsApp95 - test_03"
echo ✓ Send messages between different users
echo ✓ Try private messages: /w test_02 hello
echo ✓ Verify users can communicate with correct usernames
echo.
echo Expected Results:
echo - First client: username stays "test"
echo - Second client: username becomes "test_02" 
echo - Third client: username becomes "test_03"
echo - Window titles update automatically
echo - Private messaging works with new usernames
echo.
echo Press any key to exit...
pause >nul