$root = $PSScriptRoot

$frontendPath = Join-Path $root "frontend"
$frontendPort = 4200

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

Write-Host "Reiniciando frontend Angular..." -ForegroundColor Cyan

# Encerra a instancia antiga, caso esteja rodando.
Stop-ProcessByPort $frontendPort

# Inicia o frontend em uma nova janela.
Write-Host "Iniciando frontend..." -ForegroundColor Green

Start-Process powershell.exe -ArgumentList `
    "-Command", `
    "Set-Location '$frontendPath'; npm start"

Write-Host ""
Write-Host "Frontend iniciado." -ForegroundColor Green
Write-Host "Angular: http://localhost:4200"