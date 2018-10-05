Add-Type -Path $PSScriptRoot\MpfWorker.dll -ErrorAction Stop
$destination = 'C:\Windows\System32'
$fileList = dir $PSScriptRoot -Filter *.dll
$mpfConfigPath = "$destination\MpfConfig.xml"
$blacklistPath = "$destination\Blacklist.txt"

foreach ($file in $fileList)
{
    Write-Host "Copying file '$file' to '$destination'" -ErrorAction SilentlyContinue
    $file | Copy-Item -Destination $destination
}
Write-Host "Registering types"
C:\Windows\Microsoft.NET\Framework64\v4.0.30319\RegAsm.exe "$destination\MpfProxy.dll"
Write-Host

if (-not (Test-Path -Path $blacklistPath))
{
    Write-Host "Password Blacklist does not exist, creating a new empty one in '$blacklistPath'"
    New-Item -Path $blacklistPath -ItemType File | Out-Null
}
else
{
    Write-Host "Blacklist was found in '$blacklistPath'"
}
Write-Host
Write-Host "BlackList has $((Get-Content -Path $blacklistPath | Measure-Object).Count) entries"
Write-Host
Write-Host "Creating default Password Filter config in '$mpfConfigPath'"
$config = New-Object Mpf.MpfConfig
$config.BlackListPath = $blacklistPath
$config.IsEnabled = $true
$config.ResultIfFailure = $true

$policy = New-Object Mpf.PasswordPoliciy
$policy.Denysettings = 'DenyName', 'DenyYear'
$policy.MaxLength = 254
$policy.MinLength = 12
$policy.MinScore = 3
$policy.MaxConsecutiveRepeatingCharacters = 5
$config.PasswordPolicy = $policy
$config.Export($mpfConfigPath)

Write-Host 'Enabling password filter DLL'
$notificationPackagesValue = (Get-ItemProperty -Path HKLM:\SYSTEM\CurrentControlSet\Control\Lsa -Name 'Notification Packages').'Notification Packages'
Write-Host "Current value of 'HKLM:\SYSTEM\CurrentControlSet\Control\Lsa\Notification Packages' is:"
Write-Host $notificationPackagesValue

if ($notificationPackagesValue -contains 'PwdFlt')
{
    Write-Host
    Write-Host 'The password filter has been already enabled, exiting.'
    return
}

$notificationPackagesValue += 'PwdFlt'
Set-ItemProperty -Path HKLM:\SYSTEM\CurrentControlSet\Control\Lsa -Name 'Notification Packages' -Value $notificationPackagesValue

Write-Host
$notificationPackagesValue = (Get-ItemProperty -Path HKLM:\SYSTEM\CurrentControlSet\Control\Lsa -Name 'Notification Packages').'Notification Packages'
Write-Host "New value of 'HKLM:\SYSTEM\CurrentControlSet\Control\Lsa\Notification Packages' is:"
Write-Host $notificationPackagesValue

Write-Host "Installing the password filter has succeeded"
Write-Host "You must reboot the machine to finish the process"