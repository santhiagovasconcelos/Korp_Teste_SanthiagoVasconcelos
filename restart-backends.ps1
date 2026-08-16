$root = $PSScriptRoot

$produtosPath = Join-Path $root "backend-produtos"
$notasPath = Join-Path $root "backend-notas"

$produtosPort = 5019
$notasPort = 5150

function Stop-ProcessByPort {
    param (
        [int]$Port
    )

    $connections = Get-NetTCPConnection `
        -LocalPort $Port `
        -State Listen `
        -ErrorAction SilentlyContinue

    if ($connections) {
        foreach ($connection in $connections) {
            Write-Host "Encerrando processo da porta $Port..."

            Stop-Process `
                -Id $connection.OwningProcess `
                -Force `
                -ErrorAction SilentlyContinue
        }

        Start-Sleep -Seconds 1
    }
}

Write-Host "Reiniciando backends do Korp..." -ForegroundColor Cyan

# Encerra as instancias antigas, caso estejam rodando.
Stop-ProcessByPort $produtosPort
Stop-ProcessByPort $notasPort

# Inicia o backend de produtos em uma nova janela.
Write-Host "Iniciando backend-produtos..." -ForegroundColor Green

Start-Process powershell.exe -ArgumentList `
    "-Command", `
    "Set-Location '$produtosPath'; dotnet run"

Start-Sleep -Seconds 2

# Inicia o backend de notas em uma nova janela.
Write-Host "Iniciando backend-notas..." -ForegroundColor Green

Start-Process powershell.exe -ArgumentList `
    "-Command", `
    "Set-Location '$notasPath'; dotnet run"

Write-Host ""
Write-Host "Backends iniciados." -ForegroundColor Green
Write-Host "Produtos: http://localhost:5019"
Write-Host "Notas:    http://localhost:5150"