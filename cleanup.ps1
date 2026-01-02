$paths = @(
    "obj",
    "dbg",
    "bin",
    "tests/TestResults"
)

foreach ($path in $paths) {
    if (Test-Path $path) {
        Write-Host "Removing $path"
        Remove-Item $path -Recurse -Force
    }
}
