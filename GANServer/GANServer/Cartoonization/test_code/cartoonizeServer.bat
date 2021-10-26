set currentDirectory=%~dp0
set root=C:\Users\%username%\anaconda3
call %root%\Scripts\activate.bat %root%

call cd %currentDirectory%
call conda activate test
call python cartoonizeServer.py

pause

