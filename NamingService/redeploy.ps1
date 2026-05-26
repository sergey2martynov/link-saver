param(
    [string]$Image = "namingservice:latest"
)

$ErrorActionPreference = "Stop"

Write-Host "Building $Image..." -ForegroundColor Cyan
docker build -t $Image .
if ($LASTEXITCODE -ne 0) { Write-Error "Build failed"; exit 1 }

Write-Host "Applying secret..." -ForegroundColor Cyan
kubectl apply -f "$PSScriptRoot/k8s/secret.yaml"

Write-Host "Applying deployment manifest..." -ForegroundColor Cyan
kubectl apply -f "$PSScriptRoot/k8s/deployment.yaml"

Write-Host "Restarting deployment..." -ForegroundColor Cyan
kubectl rollout restart deployment/namingservice

Write-Host "Waiting for rollout..." -ForegroundColor Cyan
kubectl rollout status deployment/namingservice --timeout=180s

Write-Host "Done." -ForegroundColor Green
