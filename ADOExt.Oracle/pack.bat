:nuget pack -Properties Configuration=Release
:pause
nuget pack -Symbols -SymbolPackageFormat snupkg -Properties Configuration=Release
pause
nuget add MagicEastern.ADOExt.Oracle.3.0.0.nupkg -source .\
pause