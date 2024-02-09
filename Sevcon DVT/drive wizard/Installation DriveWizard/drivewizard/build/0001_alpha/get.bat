@echo off
rem *******************************************************************************
rem (C) COPYRIGHT Sevcon Limited 2007
rem 
rem Re-written for DriveWizard
rem 
rem FILE
rem     $ProjectName:DriveWizard$
rem     $Revision:1.5$
rem     $Author:jw$
rem 
rem ORIGINAL AUTHOR
rem     Martin Cooper
rem 
rem DESCRIPTION
rem     Get batchfile to extract specific versions of required
rem     subsystems from TC.  All subsystems required by the build
rem     should be referenced in this file.
rem
rem     The versions specified for each subsystem should be updated
rem     as the build progresses in time.
rem
rem     The batch file connects to the TC database and therefore
rem     needs a valid username and password.  These can be supplied
rem     on the command line - for example...
rem
rem      get [username password]
rem
rem     ... or, if not supplied on the command line, they can be
rem     entered interactively.
rem
rem PRE-REQUISITES
rem     1. The file assumes that TC's command line client is on the
rem         path.  TC's executables are normally in:
rem            c:\program files\qsc\team coherence\client\bin.
rem
rem     2. The connection to Robin is called "TC on Robin" in the
rem         TC's Connection Manager.  This should be the case if
rem         the procedure in C6944-M-008 has been followed.
rem
rem     3. All subsystems listed in this file have had a version
rem         label assigned.  This should be of the form "v1", "v2"
rem         etc, and should increment sequentially within each
rem         subsystem.
rem
rem
rem *******************************************************************************/

if "%1"=="" goto nouser
if "%2"=="" goto usage
tc connect "TC on Robin" -Y%1,%2
rem Vain attempt to detect invalid usernames or passwords - it appears that tc
rem doesn't return meaningul error codes.
if NOT ERRORLEVEL 0 goto error
goto connectdone

:nouser
rem Expect the user to enter a username and password manually
tc connect "TC on Robin"
if NOT ERRORLEVEL 0 goto error
goto connectdone


:connectdone
rem
rem Now get the required files.  Options are:
rem
rem   -GL   Extract to the specified folder, not to the current or working folder 
rem   -R    Get files recursively
rem
rem The required syntax is:
rem   tc get //C6944/software/ss/cli      -GL"some path" -R -T -VL"v1"
rem
rem top director should NOT be recursive - it contains project files - these should be moved
if NOT ERRORLEVEL 1 tc get //DriveWizard/0001_alpha				-GL"..\.."				-W -VL"V2.3"
if NOT ERRORLEVEL 1 tc get //DriveWizard/0001_alpha/Installation/		-GL"..\..\Installation"			-W -R -VL"V2.4"
if NOT ERRORLEVEL 1 tc get //DriveWizard/0001_alpha/Source/bin/			-GL"..\..\Source/bin"			-W -R -VL"V2.1"
if NOT ERRORLEVEL 1 tc get //DriveWizard/0001_alpha/Source/customInstall	-GL"..\..\customInstall"		-W -R -VL"V2"
if NOT ERRORLEVEL 1 tc get //DriveWizard/0001_alpha/Source/DI			-GL"..\..\DI"				-W -R -VL"V2.3"
if NOT ERRORLEVEL 1 tc get //DriveWizard/0001_alpha/Source/GUI/GUICommonClasses	-GL"..\..\GUI/GUICommonClasses"		-W -R -VL"V2"
if NOT ERRORLEVEL 1 tc get //DriveWizard/0001_alpha/Source/GUI/WindowClasses	-GL"..\..\GUI/WindowClasses"		-W -R -VL"V2.4"
if NOT ERRORLEVEL 1 tc get //DriveWizard/0001_alpha/Source/GUI/XML		-GL"..\..\GUI/XML"			-W -R -VL"V2"
if NOT ERRORLEVEL 1 tc get //DriveWizard/0001_alpha/Source/HelpSystem		-GL"..\..\GUI/HelpSystem"		-W -R -VL"V2"
if NOT ERRORLEVEL 1 tc get //DriveWizard/0001_alpha/Source/SelfCharacterization	-GL"..\..\GUI/SelfCharacterization"	-W -R -VL"V2"
if NOT ERRORLEVEL 1 tc get //DriveWizard/EEPROM Conversion			-GL"..\..\EEPROM Conversion"		-W -R -VL"V2"
rem don't make EDs recurisve - we now only package minimal EDS files with DW
if NOT ERRORLEVEL 1 tc get //DriveWizard/EDS					-GL"..\..\EDS"				-W    -VL"V2" 
if NOT ERRORLEVEL 1 tc get //DriveWizard/EDS/PST				-GL"..\..\EDS"				-W    -VL"V2" 
if NOT ERRORLEVEL 1 tc get //DriveWizard/EDS/archive				-GL"..\..\EDS"				-W    -VL"V2" 
if ERRORLEVEL 1 echo Error occurred during get.

goto end

:error
echo Connect error - perhaps invalid username or password?
echo.
rem fall through...

:usage
echo Usage: get [username password]
echo.
echo If the username and password are not supplied, you will
echo be asked to enter them on the command line.
echo.
goto end


:end
