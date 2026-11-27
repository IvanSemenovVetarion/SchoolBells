@echo off
setlocal
cd /d "%~dp0"

set "CSC=%WINDIR%\Microsoft.NET\Framework64\v4.0.30319\csc.exe"
if not exist "%CSC%" set "CSC=%WINDIR%\Microsoft.NET\Framework\v4.0.30319\csc.exe"
if not exist "%CSC%" (
    echo ERROR: C# compiler not found. .NET Framework 4.x is required.
    exit /b 1
)

"%CSC%" /nologo /codepage:65001 /target:exe /out:SelfTest.exe ^
    /r:System.dll /r:System.Core.dll /r:System.Drawing.dll /r:System.Windows.Forms.dll ^
    src\AssemblyInfo.cs ^
    src\Model.cs ^
    src\Storage.cs ^
    src\Playback.cs ^
    src\Theme.cs ^
    src\Controls.cs ^
    src\Recording.cs ^
    src\LiveAudio.cs ^
    src\CopyDaysForm.cs ^
    src\MainForm.cs ^
    src\MainForm.Schedule.cs ^
    src\MainForm.Settings.cs ^
    src\MainForm.Music.cs ^
    src\MainForm.Voice.cs ^
    tests\SelfTest.cs

if errorlevel 1 (
    echo.
    echo TEST BUILD FAILED
    exit /b 1
)

SelfTest.exe %*
exit /b %errorlevel%
