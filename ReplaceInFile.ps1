Param(
    [string]$FileName,
    [string]$search,
    [string]$replace
)

Write-Output "In file: $FileName"
Write-Output "Replace: $search => $replace"

(Get-Content $FileName) -replace $search, $replace | Set-Content $FileName
