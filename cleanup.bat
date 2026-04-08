@echo off
setlocal EnableExtensions EnableDelayedExpansion

rem raiz do repositório = pasta onde este .bat está
set "ROOT=%~dp0"
if "%ROOT:~-1%"=="\" set "ROOT=%ROOT:~0,-1%"

echo Root: "%ROOT%"
pushd "%ROOT%" || exit /b 1

rem =========================
rem limpa pastas de topo fixas
rem =========================
for %%F in (obj dbg bin TestResults BenchmarkDotNet.Artifacts) do (
    call :DeleteIfExists "%ROOT%\%%F"
)

rem =========================
rem limpa coverage* na raiz
rem =========================
for /d %%D in ("%ROOT%\coverage*") do (
    call :DeleteIfExists "%%~fD"
)

rem =========================
rem limpa recursivamente em src e tests
rem =========================
for %%B in (bin dbg obj TestResults BenchmarkDotNet.Artifacts) do (
    for /f "delims=" %%D in ('dir /ad /b /s "%ROOT%\src\%%B" 2^>nul') do call :DeleteIfExists "%%~fD"
    for /f "delims=" %%D in ('dir /ad /b /s "%ROOT%\tests\%%B" 2^>nul') do call :DeleteIfExists "%%~fD"
)

rem =========================
rem limpa coverage* recursivamente (QUALQUER LUGAR)
rem =========================
for /f "delims=" %%D in ('dir /ad /b /s "%ROOT%\coverage*" 2^>nul') do (
    call :DeleteIfExists "%%~fD"
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
