set curDir=%~dp0
set root=C:\Users\%username%\anaconda3
call %root%\Scripts\activate.bat %root%

call cd %curDir%
call conda activate artline
call python artlineServer.py

pause
