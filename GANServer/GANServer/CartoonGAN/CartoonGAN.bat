set currentPath=%cd%
set root=C:\Users\%username%\anaconda3
call %root%\Scripts\activate.bat %root%

call conda activate py36
call python CartoonGAN.py

pause

