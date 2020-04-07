nuget pack -Properties Configuration=Release
pause
nuget add MagicEastern.ADOExt.Oracle.2.0.3.nupkg -source c:\WebApplications\nuget_repo
pause