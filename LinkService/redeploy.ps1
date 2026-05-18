param(
    [string]$Image = "linkservice:latest"
)

Write-Host "Building $Image..." -ForegroundColor Cyan
docker build -t $Image .
if ($LASTEXITCODE -ne 0) { Write-Host "Build failed." -ForegroundColor Red; exit 1 }

Write-Host "Restarting deployment..." -ForegroundColor Cyan
kubectl rollout restart deployment/linkservice

Write-Host "Waiting for rollout..." -ForegroundColor Cyan
kubectl rollout status deployment/linkservice

Write-Host "Done. API available at http://localhost:30080" -ForegroundColor Green
