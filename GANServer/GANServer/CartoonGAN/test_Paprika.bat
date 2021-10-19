set currentPath=%cd%
set root=C:\Users\%username%\anaconda3
call %root%\Scripts\activate.bat %root%

call conda activate py36
call python test.py --input_dir %currentPath%\test_img --style Paprika --gpu -2

exit

