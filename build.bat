@echo off
setlocal

title Utility GHMod Build Script

set "ROOT=%~dp0"
set "SOURCE=%ROOT%Utility"
set "BUILD=%ROOT%build"
set "OUTPUT=%ROOT%Utility.ghmod"

if not exist "%SOURCE%\modinfo.json" (
    echo [ERRO] Pasta do mod nao encontrada: "%SOURCE%"
    echo O build.bat deve ficar ao lado da pasta Utility.
    pause
    exit /b 1
)

if exist "%BUILD%" rmdir /s /q "%BUILD%"
mkdir "%BUILD%"

robocopy "%SOURCE%" "%BUILD%" /E /XF *.csproj *.ghmod /XD bin obj >nul
set "RC=%ERRORLEVEL%"
if %RC% GEQ 8 (
    echo [ERRO] Falha ao copiar os arquivos. Codigo Robocopy: %RC%
    rmdir /s /q "%BUILD%"
    pause
    exit /b %RC%
)

if exist "%OUTPUT%" del /f /q "%OUTPUT%"

powershell -NoProfile -ExecutionPolicy Bypass -Command ^
  "Add-Type -AssemblyName System.IO.Compression.FileSystem; [System.IO.Compression.ZipFile]::CreateFromDirectory('%BUILD%', '%OUTPUT%', [System.IO.Compression.CompressionLevel]::Optimal, $false)"

if errorlevel 1 (
    echo [ERRO] Nao foi possivel gerar Utility.ghmod.
    if exist "%BUILD%" rmdir /s /q "%BUILD%"
    pause
    exit /b 1
)

rmdir /s /q "%BUILD%"

echo.
echo [OK] Mod gerado com sucesso:
echo "%OUTPUT%"
for %%F in ("%OUTPUT%") do echo Tamanho: %%~zF bytes
pause
endlocal
