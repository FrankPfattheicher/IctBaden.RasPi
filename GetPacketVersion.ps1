Param(
    [string]$ReleaseNotesFileName = "ReleaseNotes.md"
)

Write-Output "Release notes: $ReleaseNotesFileName"

$regex = '\* +(?<semVer>\d\.\d\.\d) +.*'
$lines = Get-Content $ReleaseNotesFileName
$version = $lines | Select-String -Pattern $regex | Select-Object -First 1
$version -match $regex
$packetVersion = $Matches.semVer

Write-Output "The current version is: $packetVersion"

Write-Host ("##vso[task.setvariable variable=PACKAGE_VERSION;]$packetVersion")

