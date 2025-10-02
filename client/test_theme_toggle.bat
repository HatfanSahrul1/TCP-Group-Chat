@echo off
title WhatsApp95 Chat - Theme Toggle Test
color 0F

echo.
echo ========================================
echo  WhatsApp95 Chat - Theme Toggle Test
echo ========================================
echo.
echo Testing Dark/Light Mode Toggle Feature
echo.
echo Instructions:
echo 1. Server will start automatically
echo 2. Client will launch - enter any username
echo 3. Test the theme toggle button:
echo    - Bottom-left: "🌙 Dark Mode" button
echo    - Click to switch to dark theme
echo    - Button changes to "☀️ Light Mode"  
echo    - Click again to switch back
echo 4. Theme preference saves automatically
echo 5. Close and reopen to test persistence
echo.
echo Press any key to start testing...
pause >nul

echo.
echo Starting server...
start "WhatsApp95 Server" cmd /c "cd ..\server && dotnet run"

timeout /t 3 >nul

echo Starting client for theme testing...
start "WhatsApp95 Client - Theme Test" cmd /c "dotnet run"

echo.
echo Theme Toggle Test Started!
echo.
echo Test scenarios:
echo ✓ Click "🌙 Dark Mode" - should switch to dark theme
echo ✓ All UI elements should change to dark colors
echo ✓ Button text changes to "☀️ Light Mode"
echo ✓ Click "☀️ Light Mode" - should switch back to light
echo ✓ Close app and reopen - should remember theme choice
echo.
echo Press any key to exit...
pause >nul