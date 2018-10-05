param( 
    [Parameter(Mandatory)]
    [string]$AccountDn
)

function Set-ADUserPassword
{ 
    param( 
        [Parameter(Mandatory)]
        [string]$AccountDn,
        [Parameter(Mandatory)]
        [string]$Password
    )

    $user = [adsi]"LDAP://$AccountDn"
    $user.psbase.Invoke("SetPassword", $Password)
    $user.psbase.CommitChanges()
}

function Edit-ADUserPassword
{ 
    param(
        [Parameter(Mandatory)]
        [string]$AccountDn,
        [Parameter(Mandatory)]
        [string]$OldPassword,
        [Parameter(Mandatory)]
        [string]$NewPassword
    )

    $user = [adsi]"LDAP://$AccountDn"
    $user.psbase.Invoke("ChangePassword", $OldPassword, $NewPassword)
    $user.psbase.CommitChanges()
}

$iterations = 50000
$password = &"$PSScriptRoot\New-Password.ps1" -PasswordLength 30
Set-ADUserPassword -AccountDn $AccountDn -Password $password

foreach ($i in (1..$iterations))
{
    $oldPassword = $password
    $password = &"$PSScriptRoot\New-Password.ps1" -PasswordLength 30
    try
    {
        #Edit-ADUserPassword -AccountDn $dn -OldPassword $oldPassword -NewPassword $password
        Set-ADUserPassword -AccountDn $AccountDn -Password $password
        Write-Host . -NoNewline
    }
    catch
    {
        
    }
}