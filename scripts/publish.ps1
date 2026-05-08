param(
    [string]$Configuration = "Release"
)

dotnet publish src/CliHere.App/CliHere.App.csproj `
    --configuration $Configuration `
    --runtime win-x64 `
    --self-contained true `
    -p:PublishSingleFile=true `
    -o dist/CliHere

Compress-Archive -Path dist/CliHere/* -DestinationPath dist/CliHere-win-x64.zip -Force
Write-Host "Created dist/CliHere-win-x64.zip"
