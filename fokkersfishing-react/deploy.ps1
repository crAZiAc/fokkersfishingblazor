<#
.SYNOPSIS
  Build the React client + .NET API and deploy them together to an Azure Web App.

.DESCRIPTION
  1. Builds the Vite client -> emits into server/wwwroot.
  2. `dotnet publish` the API (Release) -> server/publish (includes wwwroot).
  3. Zips the publish output and deploys it to the Web App via `az webapp deploy`.

  The client and server ship as ONE artifact: the API serves the SPA from wwwroot,
  so this deploys both at once to a single Web App.

.PREREQUISITES
  - Azure CLI (`az`) installed and logged in: run `az login` first.
  - .NET 8 SDK and Node.js/npm on PATH.

.EXAMPLE
  ./deploy.ps1 -ResourceGroup rg-fokkers -AppName fokkersfishing

.EXAMPLE
  ./deploy.ps1 -ResourceGroup rg-fokkers -AppName fokkersfishing -Slot staging -SkipClient
#>
[CmdletBinding()]
param(
  [Parameter(Mandatory = $true)] [string] $ResourceGroup,
  [Parameter(Mandatory = $true)] [string] $AppName,
  [string] $Slot,
  [string] $Subscription,
  [ValidateSet('Release', 'Debug')] [string] $Configuration = 'Release',
  [switch] $SkipClient,
  [switch] $SkipServer
)

$ErrorActionPreference = 'Stop'
$root      = $PSScriptRoot
$clientDir = Join-Path $root 'client'
$serverDir = Join-Path $root 'server'
$publishDir = Join-Path $serverDir 'publish'
$zipPath   = Join-Path $root 'deploy.zip'

function Assert-Command($name) {
  if (-not (Get-Command $name -ErrorAction SilentlyContinue)) {
    throw "Required command '$name' was not found on PATH."
  }
}

Write-Host '==> Checking prerequisites' -ForegroundColor Cyan
Assert-Command 'az'
Assert-Command 'dotnet'
if (-not $SkipClient) { Assert-Command 'npm' }

if ($Subscription) {
  Write-Host "==> Selecting subscription $Subscription" -ForegroundColor Cyan
  az account set --subscription $Subscription | Out-Null
}

# Confirm we are logged in (fails fast with a clear message otherwise).
az account show --only-show-errors 1>$null 2>$null
if ($LASTEXITCODE -ne 0) { throw "Not logged in to Azure. Run 'az login' first." }

# 1) Build the client into server/wwwroot
if (-not $SkipClient) {
  Write-Host '==> Building React client' -ForegroundColor Cyan
  Push-Location $clientDir
  try {
    if (Test-Path (Join-Path $clientDir 'package-lock.json')) { npm ci } else { npm install }
    if ($LASTEXITCODE -ne 0) { throw 'npm install/ci failed.' }
    npm run build
    if ($LASTEXITCODE -ne 0) { throw 'Client build failed.' }
  }
  finally { Pop-Location }
}
else {
  Write-Host '==> Skipping client build (-SkipClient)' -ForegroundColor Yellow
}

# 2) Publish the API (includes wwwroot produced above)
Write-Host '==> Publishing .NET API' -ForegroundColor Cyan
if (Test-Path $publishDir) { Remove-Item $publishDir -Recurse -Force }
dotnet publish (Join-Path $serverDir 'FokkersFishing.Api.csproj') -c $Configuration -o $publishDir
if ($LASTEXITCODE -ne 0) { throw 'dotnet publish failed.' }

# 3) Zip and deploy
Write-Host '==> Creating deployment package' -ForegroundColor Cyan
if (Test-Path $zipPath) { Remove-Item $zipPath -Force }
Compress-Archive -Path (Join-Path $publishDir '*') -DestinationPath $zipPath -Force

if ($SkipServer) {
  Write-Host "==> Skipping deploy (-SkipServer). Package ready at $zipPath" -ForegroundColor Yellow
  return
}

Write-Host "==> Deploying to Web App '$AppName'$(if ($Slot) { " (slot: $Slot)" })" -ForegroundColor Cyan
$deployArgs = @(
  'webapp', 'deploy',
  '--resource-group', $ResourceGroup,
  '--name', $AppName,
  '--src-path', $zipPath,
  '--type', 'zip',
  '--only-show-errors'
)
if ($Slot) { $deployArgs += @('--slot', $Slot) }

az @deployArgs
if ($LASTEXITCODE -ne 0) { throw 'az webapp deploy failed.' }

Write-Host '==> Done. Deployment complete.' -ForegroundColor Green
$hostName = az webapp show --resource-group $ResourceGroup --name $AppName --query defaultHostName -o tsv --only-show-errors 2>$null
if ($hostName) { Write-Host "    https://$hostName" -ForegroundColor Green }
