# ==========================================
# НАСТРОЙКИ (Укажите ваши пути)
# ==========================================
$SolutionPath = Join-Path $PSScriptRoot "./Import.slnx"
$SourcePackagesDir = Join-Path $PSScriptRoot "./packages"
$TargetPackagesDir = Join-Path $PSScriptRoot "../ShopImport/packages"
# Resolve NuGet global packages directory: honor NUGET_PACKAGES > dotnet nuget locals > USERPROFILE default
if ($null -ne $env:NUGET_PACKAGES -and (Test-Path $env:NUGET_PACKAGES)) {
    $NugetCacheDir = $env:NUGET_PACKAGES
} else {
    try {
        $result = dotnet nuget locals global-packages -l 2>$null
        if ($LASTEXITCODE -eq 0 -and $result.Count -gt 0) {
            $line = ($result | Where-Object { $_.Trim() -ne '' } | Select-Object -Last 1).Trim()
            if ($line -match 'global-packages:\s*(.+)') {
                $NugetCacheDir = $matches[1].Trim()
            } else {
                $NugetCacheDir = $line
            }
        } else {
            $NugetCacheDir = "$env:USERPROFILE\.nuget\packages"
        }
    } catch {
        $NugetCacheDir = "$env:USERPROFILE\.nuget\packages"
    }
}

# Имя вашей библиотеки/пакетов, которые нужно удалить из кэша NuGet
# (Поддерживает подстановочные знаки, например, "MyCompany.*")
$PackageFilter = "Import.*" 

# ==========================================
# 1. СБОРКА ПРОЕКТА (Rebuild)
# ==========================================
Write-Host ">>> Шаг 1: Очистка и сборка решения $SolutionPath..." -ForegroundColor Cyan

# dotnet build с флагом --no-incremental выполняет полную пересборку (аналог rebuild)
dotnet build $SolutionPath --configuration Release --no-incremental
if ($LASTEXITCODE -ne 0) {
    Write-Error "Ошибка сборки решения. Скрипт остановлен."
    exit $LASTEXITCODE
}
Write-Host "✔ Сборка успешно завершена.`n" -ForegroundColor Green

# ==========================================
# 2. КОПИРОВАНИЕ ПАКЕТОВ
# ==========================================
Write-Host ">>> Шаг 2: Копирование пакетов из $SourcePackagesDir в $TargetPackagesDir..." -ForegroundColor Cyan

if (-not (Test-Path $SourcePackagesDir)) {
    Write-Warning "Исходная папка пакетов не найдена: $SourcePackagesDir. Проверьте путь генерации артефактов."
} else {
    # Создаем целевую папку, если её нет
    if (-not (Test-Path $TargetPackagesDir)) {
        New-Item -ItemType Directory -Path $TargetPackagesDir | Out-Null
    }

    # Копируем все .nupkg и .snupkg файлы
    Get-ChildItem -Path $SourcePackagesDir -Filter *.nupkg -Recurse | ForEach-Object {
        $destFile = Join-Path $TargetPackagesDir $_.Name
        Copy-Item -Path $_.FullName -Destination $destFile -Force -ErrorAction Stop
        Write-Host "  [Скопирован] $_.Name" -ForegroundColor Gray
    }
    Write-Host "✔ Копирование пакетов завершено.`n" -ForegroundColor Green
}

# ==========================================
# 3. ОЧИСТКА ГЛОБАЛЬНОГО КЭША NUGET
# ==========================================
Write-Host ">>> Шаг 3: Удаление старых пакетов из глобального кэша NuGet ($NugetCacheDir)..." -ForegroundColor Cyan

if (Test-Path $NugetCacheDir) {
    # Ищем папки пакетов, соответствующие фильтру
    $CachedPackages = Get-ChildItem -Path $NugetCacheDir -Directory -Filter $PackageFilter

    if ($CachedPackages.Count -eq 0) {
        Write-Host "  Пакетов по фильтру '$PackageFilter' в кэше не найдено." -ForegroundColor Yellow
    } else {
        foreach ($pkg in $CachedPackages) {
            Write-Host "  [Удаление из кэша] $($pkg.FullName)" -ForegroundColor Yellow
            Remove-Item -Path $pkg.FullName -Recurse -Force
        }
        Write-Host "✔ Кэш NuGet успешно очищен от целевых пакетов.`n" -ForegroundColor Green
    }
} else {
    Write-Warning "Глобальный кэш NuGet не найден по пути: $NugetCacheDir"
}

Write-Host "=== Все операции успешно выполнены! ===" -ForegroundColor Green
