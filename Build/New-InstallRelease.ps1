param(
	[Parameter(Mandatory)]
	[string]$TargetDir,
	[Parameter(Mandatory)]
	[string]$SolutionDir
)
$PSBoundParameters | Out-String | Write-Host

$installDir = mkdir -Path $SolutionDir\BuildOutput -Name InstallVersion -Force
Remove-Item -Path "$installDir\*" -Force
dir -Path $TargetDir -Filter *.dll | Copy-Item -Destination $installDir
Copy-Item "$SolutionDir\Solution Items\InstallPasswordFilter.ps1" -Destination $installDir