:nuget pack -Properties Configuration=Release
:pause
nuget pack -Symbols -SymbolPackageFormat snupkg -Properties Configuration=Release
pause
nuget add MagicEastern.ADOExt.Oracle.2.1.5.nupkg -source c:\WebApplications\nuget_repo
pause