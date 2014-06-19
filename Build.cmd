@echo off
cls
Tools\nuget.exe install FAKE -ExcludeVersion -OutputDirectory "Tools"
Tools\FAKE\tools\Fake.exe Build.fsx "%*"