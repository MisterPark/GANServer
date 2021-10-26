set currentPath=%~dp0
set root=C:\Users\%username%\anaconda3
call %root%\Scripts\activate.bat %root%

call cd %currentPath%
call conda activate py36
call python CartoonGAN.py

pause

