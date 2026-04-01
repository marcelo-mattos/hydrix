@echo off
setlocal EnableExtensions

rem raiz do repositório = pasta onde este .bat está
set "ROOT=%~dp0"
if "%ROOT:~-1%"=="\" set "ROOT=%ROOT:~0,-1%"

echo Root: "%ROOT%"
pushd "%ROOT%" || exit /b 1

rem limpa pastas de topo
call :DeleteIfExists "%ROOT%\obj"
call :DeleteIfExists "%ROOT%\dbg"
call :DeleteIfExists "%ROOT%\bin"

rem procura recursivamente em src\** e tests\**
for %%B in (bin dbg obj TestResults BenchmarkDotNet.Artifacts) do (
    for /f "delims=" %%D in ('dir /ad /b /s "%ROOT%\src\%%B" 2^>nul') do call :DeleteIfExists "%%~fD"
    for /f "delims=" %%D in ('dir /ad /b /s "%ROOT%\tests\%%B" 2^>nul') do call :DeleteIfExists "%%~fD"
)

popd
endlocal
exit /b 0

:DeleteIfExists
set "TARGET=%~1"
if exist "%TARGET%" (
    echo Removing "%TARGET%"
    attrib -r -h -s "%TARGET%\*" /s /d >nul 2>&1
    rd /s /q "%TARGET%" >nul 2>&1
    if exist "%TARGET%" echo [WARN] Could not remove "%TARGET%" (in use/locked)
)
exit /b 0
