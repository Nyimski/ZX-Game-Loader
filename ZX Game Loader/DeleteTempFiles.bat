@echo off
set "TempPath=%LocalAppData%\Temp"
del "%TempPath%\current_block.txt" /F /Q
del "%TempPath%\total_blocks.txt" /F /Q
del "%TempPath%\next_block.txt" /F /Q
del "%TempPath%\tzx_control.txt" /F /Q
del "%TempPath%\save_status.txt" /F /Q
exit