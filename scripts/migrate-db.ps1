# ============================================================================
# MediQueue Database Migration Runner (Windows PowerShell)
# Usage: .\migrate-db.ps1 -Environment Production -Verbose
# ============================================================================

param(
    [string]$Environment = "Development",
    [switch]$Verbose = $false
)

$ErrorActionPreference = "Stop"

Write-Host "`n======================================================================" -ForegroundColor Cyan
Write-Host "🔧 MediQueue Database Migration Runner" -ForegroundColor Cyan
Write-Host "======================================================================" -ForegroundColor Cyan
Write-Host "Environment: $Environment" -ForegroundColor Yellow
Write-Host ""

# Get paths
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$ProjectDir = Split-Path -Parent $ScriptDir
$APIProject = Join-Path $ProjectDir "MediQueue.API"
$InfraProject = Join-Path $ProjectDir "MediQueue.Infrastructure"

Write-Host "Project Directory: $ProjectDir" -ForegroundColor White
Write-Host "API Project: $APIProject" -ForegroundColor White
Write-Host "Infrastructure Project: $InfraProject" -ForegroundColor White
Write-Host ""

# Validate
if (-not (Test-Path $InfraProject)) {
    Write-Host "❌ Infrastructure project not found: $InfraProject" -ForegroundColor Red
    exit 1
}

if (-not (Test-Path $APIProject)) {
    Write-Host "❌ API project not found: $APIProject" -ForegroundColor Red
    exit 1
}

try {
    $dotnetVersion = dotnet --version
    Write-Host "✅ .NET SDK: $dotnetVersion" -ForegroundColor Green
} catch {
    Write-Host "❌ dotnet CLI not found" -ForegroundColor Red
    exit 1
}

Write-Host "✅ Validation passed. Starting migrations..." -ForegroundColor Green
Write-Host ""

# Build migration command
$efArgs = @(
    "ef", "database", "update"
    "--project", $InfraProject
    "--startup-project", $APIProject
    "--context", "ClinicDbContext"
)

if ($Verbose) {
    $efArgs += "--verbose"
}

if ($Environment -eq "Production") {
    Write-Host "⚠️  PRODUCTION MODE: Using Release configuration" -ForegroundColor Yellow
    $efArgs += "--configuration", "Release"
} else {
    Write-Host "🔧 Development mode" -ForegroundColor Cyan
}

Write-Host "📦 Executing: dotnet $($efArgs -join ' ')" -ForegroundColor Gray
Write-Host ""

try {
    & dotnet @efArgs
    $exitCode = $LASTEXITCODE
} catch {
    Write-Host "❌ Migration failed: $_" -ForegroundColor Red
    exit 1
}

if ($exitCode -eq 0) {
    Write-Host ""
    Write-Host "======================================================================" -ForegroundColor Green
    Write-Host "✅ Database migration completed successfully!" -ForegroundColor Green
    Write-Host "======================================================================" -ForegroundColor Green
    exit 0
} else {
    Write-Host ""
    Write-Host "======================================================================" -ForegroundColor Red
    Write-Host "❌ Migration failed with exit code: $exitCode" -ForegroundColor Red
    Write-Host "======================================================================" -ForegroundColor Red
    exit 1
}