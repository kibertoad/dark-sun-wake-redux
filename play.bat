@echo off
setlocal
pushd "%~dp0"

where dotnet >nul 2>nul
if errorlevel 1 (
  echo .NET 10 SDK was not found. Install it and run play.bat again.
  popd
  exit /b 1
)

call dotnet build DarkSunWakeRedux.slnx
if errorlevel 1 (
  popd
  exit /b 1
)

rem Smoke modes run without the licensed source or extracted pack.
for %%A in (%*) do (
  if /i "%%~A"=="--smoke-test" goto run_game
  if /i "%%~A"=="--platform-smoke-test" goto run_game
)

set "GAME_SOURCE=%DARK_SUN_WAKE_PATH%"
if not defined GAME_SOURCE set "GAME_SOURCE=C:\GOG Games\Dark Sun 2"

call dotnet run --no-build --project src\DarkSunWakeRedux.Extractor -- verify-pack >nul 2>nul
if errorlevel 1 (
  if not exist "%GAME_SOURCE%\RESOURCE.GFF" (
    echo Original Dark Sun assets were not found.
    echo Set DARK_SUN_WAKE_PATH to your legal installation directory, then run play.bat again.
    popd
    exit /b 1
  )

  echo Creating the local asset pack from "%GAME_SOURCE%"...
  call dotnet run --no-build --project src\DarkSunWakeRedux.Extractor -- extract --source "%GAME_SOURCE%"
  if errorlevel 1 (
    popd
    exit /b 1
  )
)

:run_game
call dotnet run --no-build --project src\DarkSunWakeRedux.Game -- %*
set "GAME_EXIT=%ERRORLEVEL%"
popd
exit /b %GAME_EXIT%
