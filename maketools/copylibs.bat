set TARGET_DIR=%1
xcopy /d /y ..\lib\Emgu.CV\cvextern.dll %TARGET_DIR%
xcopy /d /y ..\lib\Extrack\libExtrack.dll %TARGET_DIR%
