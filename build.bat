@echo off
"tools\nuget\nuget.exe" "install" "FAKE" "-OutputDirectory" "tools" "-ExcludeVersion" "-PreRelease"
"tools\FAKE\tools\Fake.exe" build.fsx