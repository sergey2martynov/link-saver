param(
    [string]$Image = "gateway:latest"
)

$ErrorActionPreference = "Stop"

Write-Host "Building $Image..." -ForegroundColor Cyan
docker build -t $Image .
if ($LASTEXITCODE -ne 0) { Write-Error "Build failed"; exit 1 }

Write-Host "Applying deployment manifest..." -ForegroundColor Cyan
kubectl apply -f "$PSScriptRoot/k8s/deployment.yaml"

Write-Host "Restarting deployment..." -ForegroundColor Cyan
kubectl rollout restart deployment/gateway

Write-Host "Waiting for rollout..." -ForegroundColor Cyan
kubectl rollout status deployment/gateway --timeout=180s

Write-Host "Done. Gateway available at http://localhost:30080" -ForegroundColor Green
