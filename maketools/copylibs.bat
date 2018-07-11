set TARGET_DIR=%1
xcopy /d /y ..\lib\External\cvextern.dll %TARGET_DIR%
xcopy /d /y ..\lib\External\libExtrack.dll %TARGET_DIR%
