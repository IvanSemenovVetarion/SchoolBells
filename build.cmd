@echo off
setlocal
cd /d "%~dp0"

set "CSC=%WINDIR%\Microsoft.NET\Framework64\v4.0.30319\csc.exe"
if not exist "%CSC%" set "CSC=%WINDIR%\Microsoft.NET\Framework\v4.0.30319\csc.exe"
if not exist "%CSC%" (
    echo ERROR: C# compiler not found. .NET Framework 4.x is required.
    exit /b 1
)

taskkill /f /im SchoolBells.exe >nul 2>nul

"%CSC%" /nologo /codepage:65001 /target:winexe /optimize+ /out:SchoolBells.exe ^
    /win32icon:app.ico ^
    /win32manifest:app.manifest ^
    /r:System.dll /r:System.Core.dll /r:System.Drawing.dll /r:System.Windows.Forms.dll ^
    src\Program.cs ^
    src\AssemblyInfo.cs ^
    src\Model.cs ^
    src\Storage.cs ^
    src\Playback.cs ^
    src\Theme.cs ^
    src\Recording.cs ^
    src\CopyDaysForm.cs ^
    src\MainForm.cs ^
    src\MainForm.Schedule.cs ^
    src\MainForm.Settings.cs ^
    src\MainForm.Music.cs ^
    src\MainForm.Voice.cs

if errorlevel 1 (
    echo.
    echo BUILD FAILED
    exit /b 1
)

echo.
echo BUILD OK: SchoolBells.exe
exit /b 0
