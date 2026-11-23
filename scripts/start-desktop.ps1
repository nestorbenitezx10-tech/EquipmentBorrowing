$ErrorActionPreference = 'Stop'

$repoRoot = Split-Path -Parent $PSScriptRoot
$projectPath = Join-Path $repoRoot 'EquipmentBorrowing\src\EquipmentBorrowing.Desktop\EquipmentBorrowing.Desktop.csproj'
$dotnet = Join-Path $env:ProgramFiles 'dotnet\dotnet.exe'

if (-not (Test-Path -LiteralPath $projectPath)) {
    throw "Desktop project not found: $projectPath"
}
if (-not (Test-Path -LiteralPath $dotnet)) {
    throw "dotnet.exe not found: $dotnet"
}

$env:EQUIPMENTBORROWING_API_URL = 'http://localhost:5090/'
& $dotnet run --configuration Release --project $projectPath
