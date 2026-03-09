@echo off

rem Delete root folders
rd /s /q "%CD%\obj" 2>nul
rd /s /q "%CD%\dbg" 2>nul
rd /s /q "%CD%\bin" 2>nul

rem Delete all TestResults folders recursively
for /d /r "%CD%\tests" %%d in (TestResults) do (
    rd /s /q "%%d" 2>nul
)

rem Delete all BenchmarkDotNet.Artifacts folders recursively
for /d /r "%CD%\tests" %%d in (BenchmarkDotNet.Artifacts) do (
    rd /s /q "%%d" 2>nul
)
