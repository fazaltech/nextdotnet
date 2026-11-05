$ErrorActionPreference = 'Stop'
# Creates an isolated database. Never runs fixture writes against the configured restaurant.
$repo = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
$cfg = Get-Content (Join-Path $repo 'backend/src/Backend.Api/appsettings.Development.json') -Raw | ConvertFrom-Json
$builder = New-Object System.Data.SqlClient.SqlConnectionStringBuilder($cfg.ConnectionStrings.DefaultConnection.Replace('Trust Server Certificate','TrustServerCertificate'))
$testDatabase = 'RmsVerification_' + [Guid]::NewGuid().ToString('N')
$builder['Initial Catalog'] = 'master'
$connection = New-Object System.Data.SqlClient.SqlConnection($builder.ConnectionString)
try {
    $connection.Open()
    $command = $connection.CreateCommand()
    $command.CommandText = "CREATE DATABASE [$testDatabase]"
    [void]$command.ExecuteNonQuery()
} finally { $connection.Dispose() }
$builder['Initial Catalog'] = $testDatabase
$env:ConnectionStrings__DefaultConnection = $builder.ConnectionString
$env:Bootstrap__AdminPassword = 'Verify!' + [Guid]::NewGuid().ToString('N') + 'Aa1'
$env:ASPNETCORE_ENVIRONMENT = 'Development'
try {
    python (Join-Path $PSScriptRoot 'verify_service.py')
    if ($LASTEXITCODE -ne 0) { throw 'Service verification failed.' }
} finally {
    Remove-Item Env:ConnectionStrings__DefaultConnection
    Remove-Item Env:Bootstrap__AdminPassword
    Write-Output "Isolated verification database retained: $testDatabase"
}

