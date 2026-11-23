param(
    [switch]$SimulateT08DatabaseError
)

$ErrorActionPreference = 'Stop'

$repoRoot = Split-Path -Parent $PSScriptRoot
$projectPath = Join-Path $repoRoot 'EquipmentBorrowing\src\EquipmentBorrowing.Api\EquipmentBorrowing.Api.csproj'
$dotnet = Join-Path $env:ProgramFiles 'dotnet\dotnet.exe'

if (-not (Test-Path -LiteralPath $projectPath)) {
    throw "API project not found: $projectPath"
}
if (-not (Test-Path -LiteralPath $dotnet)) {
    throw "dotnet.exe not found: $dotnet"
}

$databaseName = if ($SimulateT08DatabaseError) { 'campus_equipment_t08_missing' } else { 'campus_equipment_lab5' }
$securePassword = Read-Host "PostgreSQL password for $databaseName" -AsSecureString
$plainPassword = [System.Net.NetworkCredential]::new('', $securePassword).Password
$env:ConnectionStrings__CampusDb = "Host=localhost;Port=5432;Database=$databaseName;Username=postgres;Password=$plainPassword"
$env:ASPNETCORE_URLS = 'http://localhost:5090'

try {
    & $dotnet run --project $projectPath
    if ($LASTEXITCODE -ne 0) {
        exit $LASTEXITCODE
    }
}
finally {
    Remove-Item Env:ConnectionStrings__CampusDb -ErrorAction SilentlyContinue
    Remove-Item Env:ASPNETCORE_URLS -ErrorAction SilentlyContinue
    Remove-Variable plainPassword, securePassword -ErrorAction SilentlyContinue
}
