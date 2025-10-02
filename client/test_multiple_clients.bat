@echo off
echo =======================================
echo   Multi-Client Testing for Group Chat
echo =======================================
echo.
echo This will open multiple chat clients for testing:
echo - Make sure the server is running first!
echo - Each client will open in a separate window
echo - Use different usernames for each client
echo.
echo How many clients do you want to test? (2-5): 
set /p CLIENT_COUNT=

if "%CLIENT_COUNT%"=="" set CLIENT_COUNT=2
if %CLIENT_COUNT% LSS 2 set CLIENT_COUNT=2
if %CLIENT_COUNT% GTR 5 set CLIENT_COUNT=5

echo.
echo Opening %CLIENT_COUNT% client windows...
echo.

for /L %%i in (1,1,%CLIENT_COUNT%) do (
    echo Starting Client %%i...
    start "ChatClient_%%i" cmd /k "cd /d c:\Users\THINKPAD\dotnet_kuliah\tcp-group-chat\client && dotnet run"
    timeout /t 2 > nul
)

echo.
echo All %CLIENT_COUNT% clients started!
echo Use different usernames for each client.
echo Test group messaging and private messages.
echo.
pause