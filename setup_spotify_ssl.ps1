# ============================================================
# Spotify HTTPS Localhost SSL Setup Script
# Run this script as Administrator in PowerShell
# ============================================================

param(
    [int]$Port = 8080
)

Write-Host "============================================" -ForegroundColor Cyan
Write-Host " Spotify HTTPS SSL Setup for Port $Port" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""

# Check if running as Administrator
$isAdmin = ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
if (-not $isAdmin) {
    Write-Host "[ERROR] This script must be run as Administrator!" -ForegroundColor Red
    Write-Host "Right-click PowerShell and select 'Run as Administrator'" -ForegroundColor Yellow
    exit 1
}

# Step 1: Create self-signed certificate for localhost
Write-Host "[1/4] Creating self-signed certificate for localhost..." -ForegroundColor Yellow

# Check if certificate already exists
$existingCert = Get-ChildItem -Path "Cert:\LocalMachine\My" | Where-Object { $_.DnsNameList.Unicode -contains "localhost" -and $_.NotAfter -gt (Get-Date) }

if ($existingCert) {
    Write-Host "  -> Found existing valid certificate: $($existingCert.Thumbprint)" -ForegroundColor Green
    $cert = $existingCert | Select-Object -First 1
} else {
    $cert = New-SelfSignedCertificate `
        -DnsName "localhost" `
        -CertStoreLocation "cert:\LocalMachine\My" `
        -FriendlyName "Spotify Unity HTTPS Localhost" `
        -NotAfter (Get-Date).AddYears(5)
    Write-Host "  -> Created new certificate: $($cert.Thumbprint)" -ForegroundColor Green
}

$certHash = $cert.Thumbprint

# Step 2: Remove existing SSL binding if any
Write-Host "[2/4] Checking existing SSL bindings on port $Port..." -ForegroundColor Yellow
$existingBinding = netsh http show sslcert ipport=0.0.0.0:$Port 2>&1
if ($existingBinding -notmatch "The system cannot find") {
    Write-Host "  -> Removing existing SSL binding..." -ForegroundColor Yellow
    netsh http delete sslcert ipport=0.0.0.0:$Port | Out-Null
}

# Step 3: Bind certificate to port
Write-Host "[3/4] Binding certificate to port $Port..." -ForegroundColor Yellow
$appId = "{00000000-0000-0000-0000-000000000001}"
$result = netsh http add sslcert ipport=0.0.0.0:$Port certhash=$certHash appid=$appId 2>&1
if ($LASTEXITCODE -eq 0) {
    Write-Host "  -> SSL certificate bound successfully!" -ForegroundColor Green
} else {
    Write-Host "  -> Warning: $result" -ForegroundColor Yellow
}

# Step 4: Add URL ACL reservation
Write-Host "[4/4] Adding URL ACL reservation..." -ForegroundColor Yellow
$existingAcl = netsh http show urlacl url=https://+:$Port/ 2>&1
if ($existingAcl -match "Reserved URL") {
    Write-Host "  -> URL ACL already exists, removing old one..." -ForegroundColor Yellow
    netsh http delete urlacl url=https://+:$Port/ | Out-Null
}

$result = netsh http add urlacl url=https://+:$Port/ user=Everyone 2>&1
if ($LASTEXITCODE -eq 0) {
    Write-Host "  -> URL ACL reservation added successfully!" -ForegroundColor Green
} else {
    Write-Host "  -> Warning: $result" -ForegroundColor Yellow
}

# Also trust the certificate (add to Trusted Root)
Write-Host ""
Write-Host "Adding certificate to Trusted Root store (to avoid browser warnings)..." -ForegroundColor Yellow
$rootStore = New-Object System.Security.Cryptography.X509Certificates.X509Store("Root", "LocalMachine")
$rootStore.Open("ReadWrite")
$alreadyTrusted = $rootStore.Certificates | Where-Object { $_.Thumbprint -eq $certHash }
if (-not $alreadyTrusted) {
    $rootStore.Add($cert)
    Write-Host "  -> Certificate added to Trusted Root store!" -ForegroundColor Green
} else {
    Write-Host "  -> Certificate already in Trusted Root store." -ForegroundColor Green
}
$rootStore.Close()

Write-Host ""
Write-Host "============================================" -ForegroundColor Green
Write-Host " Setup Complete!" -ForegroundColor Green
Write-Host "============================================" -ForegroundColor Green
Write-Host ""
Write-Host "Configuration:" -ForegroundColor White
Write-Host "  Redirect URI : https://localhost:$Port/callback" -ForegroundColor White
Write-Host "  Certificate  : $certHash" -ForegroundColor White
Write-Host ""
Write-Host "IMPORTANT: Update your Spotify Developer Dashboard!" -ForegroundColor Yellow
Write-Host "  1. Go to https://developer.spotify.com/dashboard" -ForegroundColor White
Write-Host "  2. Select your app" -ForegroundColor White
Write-Host "  3. Go to Settings -> Redirect URIs" -ForegroundColor White
Write-Host "  4. Remove: http://localhost:$Port/callback" -ForegroundColor Red
Write-Host "  5. Add:    https://localhost:$Port/callback" -ForegroundColor Green
Write-Host "  6. Click Save" -ForegroundColor White
Write-Host ""
