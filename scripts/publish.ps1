param(
    [string]$Configuration = "Release"
)

dotnet publish src/CliHere.App/CliHere.App.csproj `
    --configuration $Configuration `
    --runtime win-x64 `
    --self-contained false `
    -p:PublishSingleFile=true `
    -p:DebugType=None `
    -p:DebugSymbols=false `
    -o dist/CliHere

Compress-Archive -Path dist/CliHere/* -DestinationPath dist/CliHere-win-x64.zip -Force
Write-Host "Created dist/CliHere-win-x64.zip"
