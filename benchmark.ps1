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
# Escolha do Job
# -------------------------------
$jobs = @(
    "short",
    "medium",
    "long",
    "verylong",
    "dry"
)

Write-Host "`nSelecione o job:" -ForegroundColor Yellow
for ($i = 0; $i -lt $jobs.Length; $i++) {
    Write-Host "$($i + 1)) $($jobs[$i])"
}

$jobChoice = Read-Host "Digite o número (default: 1)"

if ([string]::IsNullOrWhiteSpace($jobChoice)) {
    $job = "short"
}
else {
    $index = [int]$jobChoice - 1
    if ($index -ge 0 -and $index -lt $jobs.Length) {
        $job = $jobs[$index]
    }
    else {
        Write-Host "❌ Opção inválida. Usando default short"
        $job = "short"
    }
}

# -------------------------------
# Escolha de Diagnosers
# -------------------------------
Write-Host "`nSelecione os diagnosers (separados por vírgula):" -ForegroundColor Yellow
Write-Host "1) Memory"
Write-Host "2) Disassembly"
Write-Host "3) Threading"
Write-Host "Exemplo: 1,3 ou ENTER para nenhum"

$diagChoice = Read-Host "Opção"

$diagnosers = @()

if (-not [string]::IsNullOrWhiteSpace($diagChoice)) {
    $choices = $diagChoice.Split(",")

    foreach ($c in $choices) {
        switch ($c.Trim()) {
            "1" { $diagnosers += "--memory" }
            "2" { $diagnosers += "--disasm" }
            "3" { $diagnosers += "--threading" }
        }
    }
}

# -------------------------------
# Escolha de Exporters
# -------------------------------
Write-Host "`nSelecione os exporters (separados por vírgula):" -ForegroundColor Yellow
Write-Host "1) HTML"
Write-Host "2) Markdown"
Write-Host "3) GitHub"
Write-Host "4) CSV"
Write-Host "Exemplo: 1,2 ou ENTER para nenhum"

$expChoice = Read-Host "Opção"

$exporters = @()

if (-not [string]::IsNullOrWhiteSpace($expChoice)) {
    $choices = $expChoice.Split(",")

    foreach ($c in $choices) {
        switch ($c.Trim()) {
            "1" { $exporters += "html" }
            "2" { $exporters += "markdown" }
            "3" { $exporters += "github" }
            "4" { $exporters += "csv" }
        }
    }
}

# -------------------------------
# Execução
# -------------------------------
Write-Host "`n⚙️ Executando benchmarks..." -ForegroundColor Green
Write-Host "Framework: $framework"
Write-Host "Filter: $filter`n"

$extraArgs = @()

if ($diagnosers.Count -gt 0) {
    $extraArgs += $diagnosers
}

if ($exporters.Count -gt 0) {
    $extraArgs += "--exporters"
    $extraArgs += $exporters
}

dotnet run `
    --project "tests/Hydrix.Benchmarks/Hydrix.Benchmarks.csproj" `
    -c Release `
    -f $framework `
    -- `
    --filter "$filter" `
    --job $job `
    @extraArgs
