$root = if ($PSScriptRoot) { $PSScriptRoot } else { (Get-Location).Path }

$fixedFolders = @("obj", "dbg", "bin", "TestResults", "BenchmarkDotNet.Artifacts")
$searchRoots = @($root, (Join-Path $root "src"), (Join-Path $root "tests")) |
    Where-Object { Test-Path $_ }

Get-ChildItem -Path $searchRoots -Directory -Recurse -Force -ErrorAction SilentlyContinue |
    Where-Object {
        $_.Name -in $fixedFolders -or $_.Name.StartsWith("coverage", [System.StringComparison]::OrdinalIgnoreCase)
    } |
    Sort-Object FullName -Descending |
    ForEach-Object {
        Remove-Item $_.FullName -Recurse -Force -ErrorAction SilentlyContinue
    }

$fixedFolders | ForEach-Object {
    $p = Join-Path $root $_
    if (Test-Path $p) {
        Remove-Item $p -Recurse -Force -ErrorAction SilentlyContinue
    }
}

Get-ChildItem -Path $root -Directory -Force -ErrorAction SilentlyContinue |
    Where-Object {
        $_.Name.StartsWith("coverage", [System.StringComparison]::OrdinalIgnoreCase)
    } |
    ForEach-Object {
        Remove-Item $_.FullName -Recurse -Force -ErrorAction SilentlyContinue
    }
