Param(
    [string]$ReleaseNotesFileName
)

if ($ReleaseNotesFileName -eq "") {
    $ReleaseNotesFileName = "ReleaseNotes.md"
}

Write-Output "Release notes: $ReleaseNotesFileName"

$regex = '\* +(?<semVer>\d\.\d\.\d) +.*'
$lines = Get-Content $ReleaseNotesFileName
$version = $lines | Select-String -Pattern $regex | Select-Object -First 1
$version -match $regex
$packetVersion3 = $Matches.semVer
$packetVersion4 = ($packetVersion3 + ".0")

Write-Output "The current version is: $packetVersion3"

Write-Host ("##vso[task.setvariable variable=PACKAGE_VERSION;]$packetVersion3")

$FileName = ".\IctBaden.RasPi.Net40\AssemblyInfo.cs"
(Get-Content $FileName) -replace "0.0.0.0", $packetVersion4 | Set-Content $FileName
$FileName = ".\RasPiSample.Net40\AssemblyInfo.cs"
(Get-Content $FileName) -replace "0.0.0.0", $packetVersion4 | Set-Content $FileName

$FileName = ".\IctBaden.RasPi\IctBaden.RasPi.csproj"
(Get-Content $FileName) -replace "0.0.0.0", $packetVersion4 | Set-Content $FileName
$FileName = ".\RasPiSample\RasPiSample.csproj"
(Get-Content $FileName) -replace "0.0.0.0", $packetVersion4 | Set-Content $FileName
