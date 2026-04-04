$root = if ($PSScriptRoot) { $PSScriptRoot } else { (Get-Location).Path }

@("obj","dbg","bin","TestResults") | ForEach-Object {
    $p = Join-Path $root $_
    if (Test-Path $p) { Remove-Item $p -Recurse -Force -ErrorAction SilentlyContinue }
}

Get-ChildItem -Path (Join-Path $root "src"), (Join-Path $root "tests") -Directory -Recurse -ErrorAction SilentlyContinue |
    Where-Object { $_.Name -in @("bin","obj","dbg","TestResults","BenchmarkDotNet.Artifacts") } |
    ForEach-Object { Remove-Item $_.FullName -Recurse -Force -ErrorAction SilentlyContinue }
