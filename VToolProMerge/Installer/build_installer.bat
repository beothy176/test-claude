@echo off
REM ====================================================================
REM  Build VToolPro Merge installer.
REM  Yeu cau:
REM    - .NET 8 SDK (dotnet --version)
REM    - Inno Setup 6+ (ISCC.exe trong PATH hoac mac dinh)
REM ====================================================================

set ROOT=%~dp0..
set INST=%~dp0
set CFG=Release
set RID=win-x64
set ISCC="%ProgramFiles(x86)%\Inno Setup 6\ISCC.exe"

echo [1/3] dotnet publish self-contained...
pushd "%ROOT%"
dotnet publish VToolProMerge.csproj -c %CFG% -r %RID% --self-contained true ^
    /p:PublishSingleFile=true ^
    /p:IncludeNativeLibrariesForSelfExtract=true ^
    -o "%INST%publish"
if errorlevel 1 (
    echo Publish FAILED.
    popd
    exit /b 1
)
popd

echo [2/3] Compile Inno Setup...
if not exist %ISCC% (
    echo Khong tim thay %ISCC%.
    echo Cai Inno Setup tu: https://jrsoftware.org/isinfo.php
    exit /b 1
)
%ISCC% "%INST%VToolProMerge.iss"
if errorlevel 1 exit /b 1

echo [3/3] Done. Installer trong %INST%Output
exit /b 0
