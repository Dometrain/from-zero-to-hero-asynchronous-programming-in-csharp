# Define the current folder as the search folder
$SearchFolder = "$(pwd)"

# Recursively find all `.sln` files
Write-Host "Searching for solution files (*.sln) in '$SearchFolder'..." -ForegroundColor Cyan
$slnFiles = Get-ChildItem -Path $SearchFolder -Recurse -Filter *.sln -ErrorAction SilentlyContinue

if ($slnFiles.Count -eq 0) {
    Write-Host "No solution files (*.sln) found in '$SearchFolder'." -ForegroundColor Yellow
    exit 0
}

# Define a flag to track errors
$global:hasError = $false

# Build `.sln` files in parallel
$slnFiles | ForEach-Object -Parallel {
    if ($global:hasError) { throw "Stopping due to previous error" }

    $slnFile = $_

    Write-Host "Format $($slnFile)"
    dotnet format $slnFile

    try {
        Write-Host "Building solution: $($slnFile)"
        $buildOutput = dotnet build $slnFile 2>&1
        if ($LASTEXITCODE -eq 0) {
            Write-Host "Build succeeded for: $($slnFile)" -ForegroundColor Green
        } else {
            Write-Host "Build failed for: $($slnFile)" -ForegroundColor Red
            Write-Host "Build output:" -ForegroundColor Yellow
            Write-Output $buildOutput -ForegroundColor Yellow
            $global:hasError = $true
            throw "Build failed for $slnFile"
        }
    }
    catch {
        $global:hasError = $true
        Write-Host "Error building: $slnFile" -ForegroundColor Red
    }
}

# Check if any builds failed and fail the workflow if necessary
if ($global:hasError) {
    Write-Host "One or more solutions failed to build." -ForegroundColor Red
    exit 1
}

Write-Host "Build process completed." -ForegroundColor Cyan