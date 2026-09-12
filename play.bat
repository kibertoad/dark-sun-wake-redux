@echo off
setlocal
pushd "%~dp0"

call dotnet build DarkSunWakeRedux.slnx
if errorlevel 1 (
  popd
  exit /b 1
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

call dotnet run --no-build --project src\DarkSunWakeRedux.Game
set "GAME_EXIT=%ERRORLEVEL%"
popd
exit /b %GAME_EXIT%
