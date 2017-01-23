@echo off

rem %1 - full path to heat.exe
rem %2 - full path to dir with files to package
rem %3 - full path to output .wxs file

IF %1.==. goto ParamMissing
IF %2.==. goto ParamMissing
IF %3.==. goto ParamMissing

del /s /q /f "%~2\*vshost*"
del /s /q /f "%~2\*.pdb"
del /s /q /f "%~2\*.xml"
del /s /q /f "%~2\Agents\AgentLoader32\*.pdb"

for /d %%i in ("%~2\*") do (
	if /i "%%i" NEQ "%~2\Agents" rd /S /Q %%i
)

for /d %%i in ("%~2\Agents\*") do (
	if /i "%%i" NEQ "%~2\Agents\AgentLoader32" rd /S /Q %%i
)

%1 dir %2 -var var.ReleaseDir -gg -cg AppGroup -scom -sreg -sfrag -srd -dr INSTALLDIR -out %3
goto ExitOk

:ParamMissing
	echo Param missing, exiting
	echo RunHeat.bat [full path to heat.exe] [full path to dir with files to package] [full path to output.wxs]
	exit /b 1
	
:ExitOk
	exit /b 0