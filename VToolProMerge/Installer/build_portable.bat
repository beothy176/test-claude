@echo off
REM ====================================================================
REM  Build VToolPro Merge dang PORTABLE ZIP.
REM  Khong can Inno Setup. Khong can quyen admin.
REM  Output: VToolProMerge-Portable-x.y.z.zip — copy sang may khac, giai
REM  nen, chay VToolProMerge.exe la dung. Da self-contained .NET 8.
REM ====================================================================

setlocal
set ROOT=%~dp0..
set INST=%~dp0
set CFG=Release
set RID=win-x64
set VER=2.1.0.0
set OUTDIR=%INST%publish-portable
set ZIPNAME=VToolProMerge-Portable-%VER%-%RID%.zip

echo === [1/3] Lam sach output cu ===
if exist "%OUTDIR%" rmdir /s /q "%OUTDIR%"
if exist "%INST%%ZIPNAME%" del "%INST%%ZIPNAME%"

echo === [2/3] dotnet publish self-contained ===
pushd "%ROOT%"
dotnet publish VToolProMerge.csproj ^
    -c %CFG% ^
    -r %RID% ^
    --self-contained true ^
    /p:PublishSingleFile=false ^
    /p:PublishReadyToRun=true ^
    /p:DebugType=none /p:DebugSymbols=false ^
    -o "%OUTDIR%"
if errorlevel 1 (
    echo Publish FAILED.
    popd
    exit /b 1
)
popd

echo === [3/3] Nen ZIP ===
REM PowerShell Compress-Archive san co tu Windows 10+
powershell -NoProfile -Command ^
    "Compress-Archive -Path '%OUTDIR%\*' -DestinationPath '%INST%%ZIPNAME%' -Force"
if errorlevel 1 exit /b 1

echo.
echo ====================================================================
echo  XONG! File san sang chuyen sang may khac:
echo    %INST%%ZIPNAME%
echo.
echo  Cach dung tren may dich:
echo    1) Copy file zip sang may.
echo    2) Click chuot phai -^> Extract All.
echo    3) Chay VToolProMerge.exe trong thu muc giai nen.
echo.
echo  Khong can cai .NET vi self-contained.
echo  Neu can xuat PDF: cai Microsoft Office Word tren may dung.
echo ====================================================================

endlocal
