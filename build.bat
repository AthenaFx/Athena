@echo off
"tools\nuget\nuget.exe" "install" "FAKE" "-OutputDirectory" "tools" "-ExcludeVersion" "-PreRelease"

SET FAKE_PATH=tools\FAKE\tools\Fake.exe


IF [%1]==[] (
    "%FAKE_PATH%" "build.fsx" 
) ELSE (
    "%FAKE_PATH%" "build.fsx" %* 
)