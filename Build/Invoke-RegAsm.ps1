param(
	[Parameter(Mandatory)]
	[string]$ProjectName,
	[Parameter(Mandatory)]
	[string]$FrameworkDir,
	[Parameter(Mandatory)]
	[string]$Platform,
	[Parameter(Mandatory)]
	[string]$TargetPath,
	[Parameter(Mandatory)]
	[string]$TargetDir
)

#The tlb file is required in order to build the c++ project. The tlb is imported there:
#ifdef _DEBUG
#import "..\BuildOutput\x64\Debug\MpfProxy.tlb" raw_interfaces_only
#else
#import "..\BuildOutput\x64\Release\MpfProxy.tlb" raw_interfaces_only
#endif

$PSBoundParameters | Out-String | Write-Host
$frameworkPath = $FrameworkDir -replace 'Framework', 'Framework64'
'Calling RegAsm:'
"$frameworkPath\$($FrameworkVersion)RegAsm.exe $TargetPath /tlb:$($TargetDir)$($ProjectName).tlb /codebase"

&"$frameworkPath\$($FrameworkVersion)RegAsm.exe" $TargetPath /tlb:$($TargetDir)$($ProjectName).tlb /codebase