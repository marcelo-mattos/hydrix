# ================================
# Hydrix Benchmarks Runner
# ================================

Write-Host "🚀 Hydrix Benchmarks Runner" -ForegroundColor Cyan

# -------------------------------
# Escolha do Framework
# -------------------------------
$frameworks = @(
    "netcoreapp3.1",
    "net6.0",
    "net8.0",
    "net10.0"
)

Write-Host "`nSelecione o framework:" -ForegroundColor Yellow
for ($i = 0; $i -lt $frameworks.Length; $i++) {
    Write-Host "$($i + 1)) $($frameworks[$i])"
}

$fwChoice = Read-Host "Digite o número (default: 4)"

if ([string]::IsNullOrWhiteSpace($fwChoice)) {
    $framework = "net10.0"
}
else {
    $index = [int]$fwChoice - 1
    if ($index -ge 0 -and $index -lt $frameworks.Length) {
        $framework = $frameworks[$index]
    }
    else {
        Write-Host "❌ Opção inválida. Usando default net10.0"
        $framework = "net10.0"
    }
}

# -------------------------------
# Escolha do Benchmark
# -------------------------------
$benchmarks = @{
    "1" = "*"
    "2" = "*Flat*"
    "3" = "*Nested*"
}

Write-Host "`nSelecione o benchmark:" -ForegroundColor Yellow
Write-Host "1) Todos (*)"
Write-Host "2) Flat"
Write-Host "3) Nested"

$bmChoice = Read-Host "Digite o número (default: 1)"

if ([string]::IsNullOrWhiteSpace($bmChoice)) {
    $filter = "*"
}
elseif ($benchmarks.ContainsKey($bmChoice)) {
    $filter = $benchmarks[$bmChoice]
}
else {
    Write-Host "❌ Opção inválida. Usando todos (*)"
    $filter = "*"
}

# -------------------------------
# Execução
# -------------------------------
Write-Host "`n⚙️ Executando benchmarks..." -ForegroundColor Green
Write-Host "Framework: $framework"
Write-Host "Filter: $filter`n"

dotnet run `
    --project "tests/Hydrix.Benchmarks/Hydrix.Benchmarks.csproj" `
    -c Release `
    -f $framework `
    -- `
    --filter "$filter" `
    --job short
