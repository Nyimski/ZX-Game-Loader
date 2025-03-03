@echo off
set "TempPath=%LocalAppData%\Temp"
del "%TempPath%\current_block.txt" /F /Q
del "%TempPath%\total_blocks.txt" /F /Q
exit