param(
    [string]$Root = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
)
$outDir = Join-Path $Root "docs/extracted"
New-Item -ItemType Directory -Force -Path $outDir | Out-Null
$files = @(
    (Join-Path $Root "LearningSources/AP2_Vorbereitungskurs_FIAE.docx"),
    (Join-Path $Root "LearningSources/AP2_Lernzettel 1.docx")
)
foreach ($f in $files) {
    if (-not (Test-Path $f)) {
        Write-Host "MISSING $f"
        continue
    }
    $tmp = Join-Path $env:TEMP ("ap2docx-" + [guid]::NewGuid().ToString("N"))
    New-Item -ItemType Directory -Path $tmp | Out-Null
    Copy-Item $f (Join-Path $tmp "src.zip")
    Expand-Archive (Join-Path $tmp "src.zip") -DestinationPath (Join-Path $tmp "unz")
    $xml = Get-Content (Join-Path $tmp "unz/word/document.xml") -Raw
    $text = [regex]::Replace($xml, "<w:tab[^/]*/>", "`t")
    $text = [regex]::Replace($text, "</w:p>", "`r`n")
    $text = [regex]::Replace($text, "<[^>]+>", "")
    $text = [System.Net.WebUtility]::HtmlDecode($text)
    $dest = Join-Path $outDir ([IO.Path]::GetFileNameWithoutExtension($f) + ".txt")
    Set-Content -Path $dest -Value $text -Encoding UTF8
    Write-Host "WROTE $dest"
    Remove-Item $tmp -Recurse -Force
}
