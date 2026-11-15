$token = $env:GH_TOKEN
$body = Get-Content "D:\Projects\MyDevelop\Import\.scripts\pr-comment-body.md" -Raw -Encoding UTF8
$body = [string]$body

# Build JSON string
$escapedBody = $body -replace '\\','\\' -replace '"','\"' -replace "`r`n","\n" -replace "`n","\n" -replace "`r","\n"
$jsonStr = '{"body":"' + $escapedBody + '"}'

# Write WITHOUT BOM
$tempJson = [System.IO.Path]::GetTempFileName()
$utf8NoBom = New-Object System.Text.UTF8Encoding $false
[System.IO.File]::WriteAllText($tempJson, $jsonStr, $utf8NoBom)

$headers = @{
    Authorization = "token $token"
    Accept = "application/vnd.github.v3+json"
}

try {
    $response = Invoke-RestMethod `
        -Uri "https://api.github.com/repos/innasolp/Import/issues/59/comments" `
        -Method Post `
        -Headers $headers `
        -InFile $tempJson `
        -ContentType "application/json; charset=utf-8"

    Write-Host "Comment posted successfully!" -ForegroundColor Green
    Write-Host "URL: $($response.html_url)" -ForegroundColor Cyan
}
catch {
    $errorResponse = $_.Exception.Response.GetResponseStream()
    $reader = New-Object System.IO.StreamReader($errorResponse)
    $reader.BaseStream.Position = 0
    $reader.DiscardBufferedData()
    $errorContent = $reader.ReadToEnd()
    Write-Host "Error: $errorContent" -ForegroundColor Red
}

Remove-Item $tempJson -Force
