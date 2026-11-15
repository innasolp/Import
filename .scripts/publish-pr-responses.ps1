# Publish PR Review Responses
# Requires: $env:GH_TOKEN

$repo = "innasolp/Import"
$prNumber = 59
$baseUrl = "https://api.github.com/repos/$repo"

$token = $env:GH_TOKEN
if (-not $token) {
    Write-Error "Set GH_TOKEN environment variable."
    exit 1
}

$headers = @{
    Authorization = "token $token"
    Accept = "application/vnd.github.v3+json"
}

$inlineComments = @(
    @{ id = "4579524580"; path = "Import.Catalog.Category.Interfaces/ICategory.cs"; line = 3; body = "Acknowledged. Will add XML documentation comments for the interface and its properties in a follow-up." },
    @{ id = "4579524642"; path = "Import.Catalog.Category.Interfaces/Import.Catalog.Category.Interfaces.csproj"; line = 12; body = "Fixed. Replaced `$(SolutionDir)\packages` with `$(MSBuildProjectDirectory)\..\packages` so the project builds correctly outside of a solution context." },
    @{ id = "4579524721"; path = "Import.Catalog.Category.Factory.Interfaces/Import.Catalog.Category.Factory.Interfaces.csproj"; line = 13; body = "Fixed. Changed `<GeneratePackageOnBuild>True</GeneratePackageOnBuild>` to lowercase `true`. Also replaced `$(SolutionDir)` with `$(MSBuildProjectDirectory)\..\packages`." },
    @{ id = "4579525109"; path = "Import.Catalog.Category.Service.Factory.Test/Import.Catalog.Category.Service.Factory.Test.csproj"; line = 10; body = "Fixed. Updated Microsoft.Extensions.Configuration, .Configuration.Binder, and .Configuration.Json from 10.0.13 to 9.0.0 to match the .NET 9 target framework." },
    @{ id = "4579525110"; path = "Import.Catalog.Category.Service.Factory.Test/Import.Catalog.Category.Service.Factory.Test.csproj"; line = 3; body = "Fixed. Changed target framework from net10.0 to net9.0 to match the SDK available in the project." },
    @{ id = "4579525148"; path = "Import.Catalog.Category.Factory.Interfaces/ICategoryItemHandlerFactory.cs"; line = 10; body = "Acknowledged. Keeping both IImportSettingsProvider and IImportSettings as they serve different purposes. Will reconsider if needed." },
    @{ id = "4579525165"; path = "Import.Catalog.Category.Interfaces/ICategoryItemHandler.cs"; line = 10; body = "Acknowledged. CategorySourceUrl is the configured source URL from settings, while SourceUrl is the actual URL of the import source. The distinction is meaningful in context." },
    @{ id = "4579525166"; path = "Import.Catalog.Category.Interfaces/ICategoryItemHandler.cs"; line = 17; body = "Acknowledged. The interface is a marker/tag interface for DI registration. Will add an XML comment in a follow-up." },
    @{ id = "4579525167"; path = "Import.Catalog.Category.Interfaces/ICategoryItemHandler.cs"; line = 14; body = "Acknowledged. CategorySourceUrl is the configured source URL from settings, while SourceUrl is the actual URL of the import source. The distinction is meaningful in context." },
    @{ id = "4579525175"; path = "Import.Catalog.Category.Interfaces/ICategoryItemHandler.cs"; line = 5; body = "Acknowledged. The name IImportCategory is intentional - it represents a category item during the import process." },
    @{ id = "4579525312"; path = "Import.Catalog.Category.Interfaces/Category.cs"; line = 8; body = "Partial fix. The required modifier is incompatible with the new() constraint used throughout the codebase. Instead, changed the property to initialize with = string.Empty." },
    @{ id = "4579525313"; path = "Import.Catalog.Category.Interfaces/Category.cs"; line = 10; body = "Partial fix. The required modifier is incompatible with the new() constraint. Changed the property to initialize with = string.Empty." },
    @{ id = "4579525731"; path = "BuildImportPackages.ps1"; line = 59; body = "Fixed. Changed -Filter *.nupkg to -Include *.nupkg, *.snupkg so symbol packages are also copied." },
    @{ id = "4579525734"; path = "BuildImportPackages.ps1"; line = 21; body = "Fixed. Replaced both occurrences of `$env:USERPROFILE` with Join-Path `$HOME '.nuget/packages'` for cross-platform compatibility." },
    @{ id = "4579525737"; path = "BuildImportPackages.ps1"; line = 24; body = "Fixed. See response for line 21 - both occurrences replaced with `$HOME` for cross-platform compatibility." },
    @{ id = "4579526001"; path = "Import.Catalog.Category.Service.Factory/CategoryServiceImpl.cs"; line = 6; body = "Acknowledged. The namespace Service.Factory is intentional - these classes are factory-layer implementations, not pure services." },
    @{ id = "4579526004"; path = "Import.Catalog.Category.Service.Factory/CategoryServiceImpl.cs"; line = 8; body = "Fixed. Aligned all constructor parameters consistently." },
    @{ id = "4579526005"; path = "Import.Catalog.Category.Service.Factory/CategoryServiceImpl.cs"; line = 22; body = "Fixed. Aligned all constructor parameters consistently." },
    @{ id = "4579526012"; path = "Import.Catalog.Category.Service.Factory/CategoryTimerServiceImpl.cs"; line = 8; body = "Acknowledged. Refactoring to parameter objects is a larger change. Deferred for now." },
    @{ id = "4579526013"; path = "Import.Catalog.Category.Service.Factory/CategoryTimerServiceImpl.cs"; line = 21; body = "Acknowledged. The empty body is intentional - this class exists solely for DI registration with concrete generic parameters." },
    @{ id = "4579526014"; path = "Import.Catalog.Category.Service.Factory/CategoryTimerServiceImpl.cs"; line = 19; body = "Acknowledged. This is a common pattern in the codebase. Will consider a using alias if readability becomes an issue." },
    @{ id = "4579526278"; path = "Import.Catalog.Category.Service.Factory/CategoryServiceFactory.cs"; line = 34; body = "Fixed. Changed nameof(categorySourceUrl) to the constant CategorySourceUrl so the error message shows the actual setting key name." },
    @{ id = "4579526279"; path = "Import.Catalog.Category.Service.Factory/CategoryServiceFactory.cs"; line = 39; body = "Fixed. Changed nameof(loadStagesSettings) to the constant LoadStages." },
    @{ id = "4579526280"; path = "Import.Catalog.Category.Service.Factory/CategoryServiceFactory.cs"; line = 48; body = "Fixed. Changed == default to == null for reference type comparison clarity." },
    @{ id = "4579526281"; path = "Import.Catalog.Category.Service.Factory/CategoryServiceFactory.cs"; line = 56; body = "Fixed. Added ?? throw new InvalidDataException(...) after the factory lookup." },
    @{ id = "4579526282"; path = "Import.Catalog.Category.Service.Factory/CategoryServiceFactory.cs"; line = 59; body = "Fixed. Added ?? throw new InvalidDataException(...) after the factory lookup." },
    @{ id = "4579526283"; path = "Import.Catalog.Category.Service.Factory/CategoryServiceFactory.cs"; line = 62; body = "Fixed. Aligned all parameters in Create method and the abstract Create signature." },
    @{ id = "4579526284"; path = "Import.Catalog.Category.Service.Factory/CategoryServiceFactory.cs"; line = 68; body = "Acknowledged. Refactoring to parameter objects is a larger change. Deferred for now." },
    @{ id = "4579526551"; path = "Import.Catalog.Category.Service.Factory/CategoryServiceFactoryImpl.cs"; line = 38; body = "Fixed. The constructor of CategoryServiceImpl does not accept importSettingsProvider and importSettings. Removed them from the constructor call. The abstract Create signature retains them for consistency with the base class." },
    @{ id = "4579526552"; path = "Import.Catalog.Category.Service.Factory/CategoryTimerServiceFactoryImpl.cs"; line = 67; body = "Verified. The argument order (interval, categoryLoadData) matches the constructor parameter order of CategoryTimerServiceImpl. No change needed." }
)

$successCount = 0
$failCount = 0

Write-Host "=== Publishing review comments ===" -ForegroundColor Cyan

foreach ($c in $inlineComments) {
    $url = "$baseUrl/pulls/$prNumber/comments/$($c.id)/comments"
    $payload = @{ body = $c.body } | ConvertTo-Json

    try {
        $response = Invoke-RestMethod -Uri $url -Method Post -Headers $headers -Body ([System.Text.Encoding]::UTF8.GetBytes($payload)) -ContentType "application/json; charset=utf-8"
        Write-Host "[OK] Commented on #$($c.id): $($c.path):$($c.line)" -ForegroundColor Green
        $successCount++
    }
    catch {
        Write-Host "[FAIL] #$($c.id): $($_.Exception.Message)" -ForegroundColor Red
        $failCount++
    }
}

Write-Host ""
Write-Host "=== Marking comments as resolved ===" -ForegroundColor Cyan

foreach ($c in $inlineComments) {
    $url = "$baseUrl/pulls/$prNumber/comments/$($c.id)/resolve"
    $payload = @{ resolved = $true } | ConvertTo-Json

    try {
        Invoke-RestMethod -Uri $url -Method Put -Headers $headers -Body ([System.Text.Encoding]::UTF8.GetBytes($payload)) -ContentType "application/json; charset=utf-8" | Out-Null
        Write-Host "[Resolved] #$($c.id)" -ForegroundColor Gray
    }
    catch {
        Write-Host "[Skip] #$($c.id) - $($_.Exception.Message)" -ForegroundColor Yellow
    }
}

Write-Host ""
Write-Host "=== Done ===" -ForegroundColor Cyan
Write-Host "Comments posted: $successCount" -ForegroundColor Green
if ($failCount -gt 0) {
    Write-Host "Comments failed: $failCount" -ForegroundColor Red
}
