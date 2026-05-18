param(
    [string]$Image = "gateway:latest"
)

Write-Host "Building $Image..." -ForegroundColor Cyan
docker build -t $Image .
if ($LASTEXITCODE -ne 0) { Write-Host "Build failed." -ForegroundColor Red; exit 1 }

Write-Host "Restarting deployment..." -ForegroundColor Cyan
kubectl rollout restart deployment/gateway

Write-Host "Waiting for rollout..." -ForegroundColor Cyan
kubectl rollout status deployment/gateway

Write-Host "Done. Gateway available at http://localhost:30080" -ForegroundColor Green
