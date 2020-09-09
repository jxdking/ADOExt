nuget pack -Properties Configuration=Release
pause
nuget add MagicEastern.ADOExt.Oracle.2.1.1.nupkg -source c:\WebApplications\nuget_repo
pause