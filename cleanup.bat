@echo off

rem Current Directories
set objPath=%CD%\obj
set dbgPath=%CD%\dbg
set binPath=%CD%\bin
set tddPath=%CD%\tests\TestResults

rem Delete Unnecessary Folders and Files
rd "%objPath%" /s /q
rd "%dbgPath%" /s /q
rd "%binPath%" /s /q
rd "%tddPath%" /s /q
