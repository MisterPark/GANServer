set dir=%cd%
set root=C:\Users\%username%\anaconda3
call %root%\Scripts\activate.bat %root%

call conda activate artline
call python runway_model.py

pause
