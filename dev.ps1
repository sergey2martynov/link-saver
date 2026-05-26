param(
    [switch]$InitInfra,      # first-time setup: apply secrets + postgres, wait for them
    [string[]]$Only,         # redeploy specific services only, e.g. -Only gateway,linkservice
    [switch]$SkipBack        # skip backend entirely, only start the frontend dev server
)

$ErrorActionPreference = "Stop"
$root = $PSScriptRoot

$allServices = @(
    @{ Name = "linkservice";   Dir = "LinkService";   Port = 30082 },
    @{ Name = "userservice";   Dir = "UserService";   Port = 30081 },
    @{ Name = "gateway";       Dir = "Gateway";       Port = 30080 },
    @{ Name = "namingservice"; Dir = "NamingService"; Port = 30083 }
)

$services = if ($Only) {
    $allServices | Where-Object { $Only -contains $_.Name }
} else {
    $allServices
}

# ── 1. Secrets + Kafka (always, idempotent) ──────────────────────────────────
Write-Host "`nApplying secrets..." -ForegroundColor Cyan
kubectl apply -f "$root/LinkService/k8s/secret.yaml"
kubectl apply -f "$root/UserService/k8s/secret.yaml"
kubectl apply -f "$root/Gateway/k8s/secret.yaml"

if (-not $env:ANTHROPIC_API_KEY) {
    Write-Error "Set ANTHROPIC_API_KEY env var before running. Example: `$env:ANTHROPIC_API_KEY='sk-ant-...'"
    exit 1
}
# Build the NamingService secret from the env var — never stored in a file
kubectl create secret generic namingservice-secrets `
    --from-literal=anthropic-api-key="$env:ANTHROPIC_API_KEY" `
    --from-literal=kafka-bootstrap-servers="kafka:9092" `
    --dry-run=client -o yaml | kubectl apply -f -

Write-Host "`nApplying Kafka..." -ForegroundColor Cyan
kubectl apply -f "$root/infra/k8s/kafka.yaml"

Write-Host "`nWaiting for Kafka..." -ForegroundColor Cyan
kubectl rollout status deployment/kafka --timeout=120s

# ── 2. First-time Postgres (only on -InitInfra, has persistent storage) ───────
if ($InitInfra) {
    Write-Host "`nApplying Postgres..." -ForegroundColor Cyan
    kubectl apply -f "$root/LinkService/k8s/postgres.yaml"
    kubectl apply -f "$root/UserService/k8s/postgres.yaml"

    Write-Host "`nWaiting for Postgres..." -ForegroundColor Cyan
    kubectl rollout status deployment/postgres --timeout=120s
    kubectl rollout status deployment/userservice-postgres --timeout=120s
}

# ── 3. Build + deploy selected services ──────────────────────────────────────
if (-not $SkipBack) {
    Write-Host "`nBuilding images..." -ForegroundColor Cyan
    foreach ($svc in $services) {
        Write-Host "  $($svc.Name)" -ForegroundColor DarkCyan
        docker build -t "$($svc.Name):latest" -f "$root/$($svc.Dir)/Dockerfile" "$root/$($svc.Dir)/"
        if ($LASTEXITCODE -ne 0) { Write-Error "$($svc.Name) build failed"; exit 1 }
    }

    Write-Host "`nDeploying..." -ForegroundColor Cyan
    foreach ($svc in $services) {
        kubectl apply -f "$root/$($svc.Dir)/k8s/deployment.yaml"
        kubectl rollout restart deployment/$($svc.Name)
    }

    Write-Host "`nWaiting for rollouts..." -ForegroundColor Cyan
    foreach ($svc in $services) {
        Write-Host "  $($svc.Name)" -ForegroundColor DarkCyan
        kubectl rollout status deployment/$($svc.Name) --timeout=180s
        if ($LASTEXITCODE -ne 0) { Write-Error "$($svc.Name) rollout failed"; exit 1 }
    }

    Write-Host "`nBackend ready:" -ForegroundColor Green
    foreach ($svc in $services) {
        Write-Host "  $($svc.Name.PadRight(12)) -> http://localhost:$($svc.Port)" -ForegroundColor Green
    }
}

# ── 4. Frontend dev server ────────────────────────────────────────────────────
$frontendDir = Join-Path $root "frontend"

if (-not (Test-Path (Join-Path $frontendDir "node_modules"))) {
    Write-Host "`nInstalling frontend dependencies..." -ForegroundColor Cyan
    Push-Location $frontendDir
    npm install
    Pop-Location
}

Write-Host "`nStarting frontend -> http://localhost:5173" -ForegroundColor Green
Push-Location $frontendDir
npm run dev
Pop-Location
