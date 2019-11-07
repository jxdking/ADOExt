nuget pack -Properties Configuration=Release
pause
nuget add MagicEastern.ADOExt.Oracle.1.1.2-beta.nupkg -source c:\WebApplications\nuget_repo
pause