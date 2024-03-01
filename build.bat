dotnet build -c Release StudyHub.WPF/StudyHub.WPF.csproj

dotnet publish -c Release -p:PublishSingleFile=false -p:PublishReadyToRun=true --self-contained true -o StudyHub.WPF/bin/publish -f net8.0-windows --os win -a x64 StudyHub.WPF/StudyHub.WPF.csproj

cd StudyHub.Setup
"C:\Program Files (x86)\Inno Setup 6\ISCC.exe" StudyHub.Setup.iss