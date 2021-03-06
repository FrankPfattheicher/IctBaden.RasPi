Param(
    [string]$ReleaseNotesFileName
)

if ($ReleaseNotesFileName -eq "") {
    $ReleaseNotesFileName = "ReleaseNotes.md"
}

Write-Output "Release notes: $ReleaseNotesFileName"

$semVer = "(?<semVer>\d+\.\d+\.\d+(\.\d+)?)"
$relNote = "\* +$semVer +.*"
$lines = Get-Content $ReleaseNotesFileName
$version = $lines | Select-String -Pattern $relNote | Select-Object -First 1
$version -match $relNote
$packageVersion = $Matches.semVer

Write-Output "The current version is: $packageVersion"
Write-Host "##vso[task.setvariable variable=PACKAGE_VERSION;]$packageVersion"

$FileName = ".\IctBaden.RasPi.Net40\AssemblyInfo.cs"
(Get-Content $FileName) -replace $semVer,$packageVersion | Set-Content $FileName
$FileName = ".\RasPiSample.Net40\AssemblyInfo.cs"
(Get-Content $FileName) -replace $semVer,$packageVersion | Set-Content $FileName

$FileName = ".\IctBaden.RasPi\IctBaden.RasPi.csproj"
(Get-Content $FileName) -replace $semVer,$packageVersion | Set-Content $FileName
$FileName = ".\RasPiSample\RasPiSample.csproj"
(Get-Content $FileName) -replace $semVer,$packageVersion | Set-Content $FileName
