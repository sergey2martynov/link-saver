param(
    [string]$Image = "userservice:latest"
)

Write-Host "Building $Image..." -ForegroundColor Cyan
docker build -t $Image .
if ($LASTEXITCODE -ne 0) { Write-Host "Build failed." -ForegroundColor Red; exit 1 }

Write-Host "Restarting deployment..." -ForegroundColor Cyan
kubectl rollout restart deployment/userservice

Write-Host "Waiting for rollout..." -ForegroundColor Cyan
kubectl rollout status deployment/userservice

Write-Host "Done. API available at http://localhost:30081" -ForegroundColor Green
