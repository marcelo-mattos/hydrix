# ================================
# Hydrix Benchmarks Runner
# ================================

function Select-MenuOption {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Title,

        [Parameter(Mandatory = $true)]
        [object[]]$Options,

        [Parameter(Mandatory = $true)]
        [string]$DefaultKey,

        [Parameter(Mandatory = $true)]
        [string]$Prompt
    )

    Write-Host "`n$Title" -ForegroundColor Yellow

    foreach ($option in $Options) {
        Write-Host "$($option.Key)) $($option.Label)"
    }

    $choice = Read-Host "$Prompt (default: $DefaultKey)"
    if ([string]::IsNullOrWhiteSpace($choice)) {
        $choice = $DefaultKey
    }

    $selected = $Options |
        Where-Object { $_.Key -eq $choice } |
        Select-Object -First 1

    if ($null -ne $selected) {
        return $selected
    }

    Write-Host "Opcao invalida. Usando default $DefaultKey." -ForegroundColor Red
    return $Options |
        Where-Object { $_.Key -eq $DefaultKey } |
        Select-Object -First 1
}

Write-Host "Hydrix Benchmarks Runner" -ForegroundColor Cyan

# -------------------------------
# Escolha do Framework
# -------------------------------
$frameworkOptions = @(
    [pscustomobject]@{ Key = '1'; Label = 'netcoreapp3.1'; Value = 'netcoreapp3.1' },
    [pscustomobject]@{ Key = '2'; Label = 'net6.0'; Value = 'net6.0' },
    [pscustomobject]@{ Key = '3'; Label = 'net8.0'; Value = 'net8.0' },
    [pscustomobject]@{ Key = '4'; Label = 'net10.0'; Value = 'net10.0' }
)

$frameworkSelection = Select-MenuOption `
    -Title 'Selecione o framework:' `
    -Options $frameworkOptions `
    -DefaultKey '4' `
    -Prompt 'Digite o numero'

$framework = $frameworkSelection.Value

# -------------------------------
# Escolha do Benchmark
# -------------------------------
$benchmarkOptions = @(
    [pscustomobject]@{ Key = '1'; Label = 'Todos (*)'; Filter = '*' },
    [pscustomobject]@{ Key = '2'; Label = 'FlatBenchmarks'; Filter = '*FlatBenchmarks*' },
    [pscustomobject]@{ Key = '3'; Label = 'NestedBenchmarks'; Filter = '*NestedBenchmarks*' },
    [pscustomobject]@{ Key = '4'; Label = 'MapperBenchmarks (todos)'; Filter = '*MapperBenchmarks*' },
    [pscustomobject]@{ Key = '5'; Label = 'MapperBenchmarks - flat small'; Filter = '*MapperBenchmarks*FlatSmall*' },
    [pscustomobject]@{ Key = '6'; Label = 'MapperBenchmarks - flat medium'; Filter = '*MapperBenchmarks*FlatMedium*' },
    [pscustomobject]@{ Key = '7'; Label = 'MapperBenchmarks - flat large'; Filter = '*MapperBenchmarks*FlatLarge*' },
    [pscustomobject]@{ Key = '8'; Label = 'MapperBenchmarks - with conversions'; Filter = '*MapperBenchmarks*WithConversions*' },
    [pscustomobject]@{ Key = '9'; Label = 'MapperBenchmarks - list small'; Filter = '*MapperBenchmarks*ListSmall*' },
    [pscustomobject]@{ Key = '10'; Label = 'MapperBenchmarks - list medium'; Filter = '*MapperBenchmarks*ListMedium*' },
    [pscustomobject]@{ Key = '11'; Label = 'MapperBenchmarks - list large'; Filter = '*MapperBenchmarks*ListLarge*' },
    [pscustomobject]@{ Key = '12'; Label = 'MapperBenchmarks - first hit (cold path)'; Filter = '*MapperBenchmarks*ColdPath*' }
)

$benchmarkSelection = Select-MenuOption `
    -Title 'Selecione o benchmark:' `
    -Options $benchmarkOptions `
    -DefaultKey '1' `
    -Prompt 'Digite o numero'

$filter = $benchmarkSelection.Filter

# -------------------------------
# Escolha do Job
# -------------------------------
$jobOptions = @(
    [pscustomobject]@{ Key = '1'; Label = 'short'; Value = 'short' },
    [pscustomobject]@{ Key = '2'; Label = 'medium'; Value = 'medium' },
    [pscustomobject]@{ Key = '3'; Label = 'long'; Value = 'long' },
    [pscustomobject]@{ Key = '4'; Label = 'verylong'; Value = 'verylong' },
    [pscustomobject]@{ Key = '5'; Label = 'dry'; Value = 'dry' }
)

$jobSelection = Select-MenuOption `
    -Title 'Selecione o job:' `
    -Options $jobOptions `
    -DefaultKey '1' `
    -Prompt 'Digite o numero'

$job = $jobSelection.Value

# -------------------------------
# Escolha de Diagnosers
# -------------------------------
Write-Host "`nSelecione os diagnosers (separados por virgula):" -ForegroundColor Yellow
Write-Host '1) Memory'
Write-Host '2) Disassembly'
Write-Host '3) Threading'
Write-Host 'Exemplo: 1,3 ou ENTER para nenhum'

$diagChoice = Read-Host 'Opcao'
$diagnosers = @()

if (-not [string]::IsNullOrWhiteSpace($diagChoice)) {
    foreach ($choice in $diagChoice.Split(',')) {
        switch ($choice.Trim()) {
            '1' { $diagnosers += '--memory' }
            '2' { $diagnosers += '--disasm' }
            '3' { $diagnosers += '--threading' }
        }
    }
}

# -------------------------------
# Escolha de Exporters
# -------------------------------
Write-Host "`nSelecione os exporters (separados por virgula):" -ForegroundColor Yellow
Write-Host '1) HTML'
Write-Host '2) Markdown'
Write-Host '3) GitHub'
Write-Host '4) CSV'
Write-Host 'Exemplo: 1,2 ou ENTER para nenhum'

$exporterChoice = Read-Host 'Opcao'
$exporters = @()

if (-not [string]::IsNullOrWhiteSpace($exporterChoice)) {
    foreach ($choice in $exporterChoice.Split(',')) {
        switch ($choice.Trim()) {
            '1' { $exporters += 'html' }
            '2' { $exporters += 'markdown' }
            '3' { $exporters += 'github' }
            '4' { $exporters += 'csv' }
        }
    }
}

# -------------------------------
# Execucao
# -------------------------------
Write-Host "`nExecutando benchmarks..." -ForegroundColor Green
Write-Host "Framework: $framework"
Write-Host "Benchmark: $($benchmarkSelection.Label)"
Write-Host "Filter: $filter"
Write-Host "Job: $job`n"

$extraArgs = @(
    '--filter', $filter,
    '--job', $job
)

if ($diagnosers.Count -gt 0) {
    $extraArgs += $diagnosers
}

if ($exporters.Count -gt 0) {
    $extraArgs += '--exporters'
    $extraArgs += $exporters
}

dotnet run `
    --project 'tests/Hydrix.Benchmarks/Hydrix.Benchmarks.csproj' `
    -c Release `
    -f $framework `
    -- `
    @extraArgs
