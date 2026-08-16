# ============================================================
#  PDM Rentals — One-Click Azure Deploy Script
#  Usage: Right-click → "Run with PowerShell"
#         OR in terminal: .\deploy.ps1
# ============================================================

$ErrorActionPreference = "Stop"

# ── Configuration ────────────────────────────────────────────
$AppName      = "pdm-rentals-app-2026"
$ProjectDir   = $PSScriptRoot                      # same folder as this script
$PublishDir   = Join-Path $ProjectDir "bin\Release\net10.0\publish"
$ZipPath      = Join-Path $ProjectDir "pdm-rentals-publish.zip"
$KuduUrl      = "https://$AppName.scm.azurewebsites.net/api/zipdeploy"
# ─────────────────────────────────────────────────────────────

function Write-Step($msg) {
    Write-Host ""
    Write-Host "▶  $msg" -ForegroundColor Cyan
}

function Write-Success($msg) {
    Write-Host "✅  $msg" -ForegroundColor Green
}

function Write-Fail($msg) {
    Write-Host "❌  $msg" -ForegroundColor Red
}

# ── Banner ───────────────────────────────────────────────────
Write-Host ""
Write-Host "╔══════════════════════════════════════════════╗" -ForegroundColor DarkCyan
Write-Host "║       PDM Rentals — Azure Deploy Script      ║" -ForegroundColor DarkCyan
Write-Host "║       Target: $AppName       ║" -ForegroundColor DarkCyan
Write-Host "╚══════════════════════════════════════════════╝" -ForegroundColor DarkCyan
Write-Host ""

# ── Step 1: Publish ──────────────────────────────────────────
Write-Step "Step 1/4 — Publishing application (Release mode)..."
try {
    & dotnet publish "$ProjectDir\RentalBlazorApp.csproj" -c Release --nologo -v minimal
    if ($LASTEXITCODE -ne 0) { throw "dotnet publish failed." }
    Write-Success "Publish complete → $PublishDir"
} catch {
    Write-Fail "Publish failed: $_"
    exit 1
}

# ── Step 2: Remove old ZIP ───────────────────────────────────
Write-Step "Step 2/4 — Packaging publish output into ZIP..."
if (Test-Path $ZipPath) {
    Remove-Item $ZipPath -Force
    Write-Host "   Old zip removed." -ForegroundColor DarkGray
}

try {
    Compress-Archive -Path "$PublishDir\*" -DestinationPath $ZipPath
    $zipSize = [math]::Round((Get-Item $ZipPath).Length / 1MB, 1)
    Write-Success "ZIP created ($zipSize MB) → $ZipPath"
} catch {
    Write-Fail "Failed to create ZIP: $_"
    exit 1
}

# ── Step 3: Get Kudu credentials ─────────────────────────────
Write-Step "Step 3/4 — Azure deployment credentials"
Write-Host ""
Write-Host "   You need your Kudu deployment credentials." -ForegroundColor Yellow
Write-Host "   To find them:" -ForegroundColor Yellow
Write-Host "   Azure Portal → App Service '$AppName'" -ForegroundColor Yellow
Write-Host "   → Deployment Center → FTPS credentials tab" -ForegroundColor Yellow
Write-Host ""

$KuduUser = Read-Host "   Enter Kudu username (starts with `$)"
$KuduPassRaw = Read-Host "   Enter Kudu password" -AsSecureString
$KuduPass = [Runtime.InteropServices.Marshal]::PtrToStringAuto(
    [Runtime.InteropServices.Marshal]::SecureStringToBSTR($KuduPassRaw)
)

# ── Step 4: Deploy via Kudu REST API ─────────────────────────
Write-Step "Step 4/4 — Deploying to Azure App Service..."
Write-Host "   Uploading to: $KuduUrl" -ForegroundColor DarkGray
Write-Host "   This may take 30–90 seconds..." -ForegroundColor DarkGray

try {
    $creds   = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes("${KuduUser}:${KuduPass}"))
    $headers = @{ Authorization = "Basic $creds" }

    $response = Invoke-RestMethod `
        -Uri        $KuduUrl `
        -Method     POST `
        -Headers    $headers `
        -InFile     $ZipPath `
        -ContentType "application/octet-stream"

    Write-Success "Deployment complete!"
} catch {
    Write-Fail "Deployment failed: $($_.Exception.Message)"
    Write-Host ""
    Write-Host "   Tip: If you get a 401 error, double-check your" -ForegroundColor Yellow
    Write-Host "   username and password from the Azure Portal." -ForegroundColor Yellow
    Write-Host ""
    Write-Host "   You can also deploy manually by dragging the ZIP onto:" -ForegroundColor Yellow
    Write-Host "   https://$AppName.scm.azurewebsites.net/ZipDeployUI" -ForegroundColor Cyan
    exit 1
}

# ── Done ─────────────────────────────────────────────────────
Write-Host ""
Write-Host "════════════════════════════════════════════════" -ForegroundColor DarkGreen
Write-Host "  🚀 Deployment successful!" -ForegroundColor Green
Write-Host "  🌐 https://$AppName.azurewebsites.net" -ForegroundColor Green
Write-Host "════════════════════════════════════════════════" -ForegroundColor DarkGreen
Write-Host ""
Write-Host "  Note: The app may take 30–60 seconds to restart." -ForegroundColor DarkGray
Write-Host "  On first deploy, EF Core migrations run automatically." -ForegroundColor DarkGray
Write-Host ""
