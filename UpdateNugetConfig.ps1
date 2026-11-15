param (
    # Global user nuget.config path
    [string]$NugetConfigPath = "$env:APPDATA\NuGet\nuget.config",
    
    # Name of the package source
    [string]$SourceName = "localsource",
    
    # Directory where the .nupkg files are built and stored
    [string]$PackagesDirectory = ".\packages"
)

# 1. Check if the packages directory exists
if (-not (Test-Path $PackagesDirectory)) {
    Write-Host "Packages directory '$PackagesDirectory' not found!" -ForegroundColor Red
    exit
}

# 2. Find all .nupkg files in the directory
$nupkgFiles = Get-ChildItem -Path $PackagesDirectory -Filter "*.nupkg" -Recurse
$packagesToAdd = @()

foreach ($file in $nupkgFiles) {
    # Skip symbols packages if they exist (e.g., Import.Html.1.0.0.symbols.nupkg)
    if ($file.Name -match '\.symbols\.nupkg$') {
        continue
    }

    # Extract the Package ID by removing the version suffix
    # This regex removes everything starting from the first dot followed by a digit (e.g., .1.0.0 or .2.1-preview)
    $packageId = $file.BaseName -replace '\.\d+(\.\d+)*(-.*)?$', ''
    
    $packagesToAdd +=$packageId
}

# Remove duplicates
$packagesToAdd =$packagesToAdd | Select-Object -Unique

if ($packagesToAdd.Count -eq 0) {
    Write-Host "No valid .nupkg files found in '$PackagesDirectory'." -ForegroundColor Yellow
    exit
}

# 3. Load the nuget.config XML file
if (-not (Test-Path $NugetConfigPath)) {
    Write-Host "File $NugetConfigPath not found!" -ForegroundColor Red
    exit
}

$xml = New-Object System.Xml.XmlDocument
$xml.PreserveWhitespace =$true # Preserve original formatting
$xml.Load($NugetConfigPath)

# 4. Find or create the <packageSourceMapping> section
$configuration =$xml.SelectSingleNode("configuration")
if ($null -eq$configuration) {
    Write-Host "Invalid nuget.config: missing root <configuration> element." -ForegroundColor Red
    exit
}

$mappingNode =$configuration.SelectSingleNode("packageSourceMapping")
if ($null -eq$mappingNode) {
    $mappingNode =$xml.CreateElement("packageSourceMapping")
    $configuration.AppendChild($mappingNode) | Out-Null
}

# 5. Find or create <packageSource key="localsource">
$sourceNode = $mappingNode.SelectSingleNode("packageSource[@key='$SourceName']")
if ($null -eq$sourceNode) {
    $sourceNode =$xml.CreateElement("packageSource")
    $sourceNode.SetAttribute("key", $SourceName)
    $mappingNode.AppendChild($sourceNode) | Out-Null
}

# 6. Insert packages that are not yet in the list
$addedCount = 0
foreach ($pkg in $packagesToAdd) {$existingNode = $sourceNode.SelectSingleNode("package[@pattern='$pkg']")
    
    if ($null -eq$existingNode) {
        $packageNode =$xml.CreateElement("package")
        $packageNode.SetAttribute("pattern", $pkg)
		$sourceNode.AppendChild($packageNode) | Out-Null
		$addedCount++
        Write-Host "Added mapping for package: $pkg" -ForegroundColor Green
    }
}

# 7. Save changes
if ($addedCount -gt 0) {
    $xml.Save($NugetConfigPath)
    Write-Host "Successfully updated global nuget.config. Added $addedCount new patterns to '$SourceName'." -ForegroundColor Cyan
} else {
    Write-Host "All packable packages from '$PackagesDirectory' are already mapped. No changes required." -ForegroundColor Yellow
}