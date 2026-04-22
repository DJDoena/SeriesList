set sourceDir=%1
set targetDir="N:\Fresh Downloads\!Tools\SeriesList"

md %targetDir%
xcopy /y %sourceDir%\*.dll %targetDir%
xcopy /y %sourceDir%\*.pdb %targetDir%
xcopy /y %sourceDir%\*.exe %targetDir%