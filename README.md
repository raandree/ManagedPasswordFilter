# ManagedPasswordFilter
Windows Password Filter that uses managed code internally

## Summary
The goal of this project is to provide password filter written mostly in managed code that can be easily extended. Realizing the same rules defined here in managed code in unmanaged C would be way more complex and requires quite some knowledge in a programming world that is not very common these days.

## Requirements
The solution is primarily designed to run on domain controllers. However, it can be used on any Windows server or clients to make sure the local passwords are according to a custom password policy.
The following components must be available:
- .net Framework 4.0+
- C++ redistributable (https://support.microsoft.com/en-us/help/2977003/the-latest-supported-visual-c-downloads)
- The solution must be compiled in release mode, otherwise the C++ DLL cannot be loaded.
To install this solution, you must be an administrator of the domain / machine.

To install this solution, you must be an administrator of the domain / machine.

## Installation
The release offers a PowerShell script that automates the steps described in https://docs.microsoft.com/en-us/windows/desktop/SecMgmt/installing-and-registering-a-password-filter-dll.
-	It copies the PwdFlt.dll, MpfProxy.dll and MpfWorker.dll to %SystemRoot%\System32
- It registers the types defined in MpfProxy.dll using RegAsm.exe
- It creates the empty file MpfBlackList.txt in %SystemRoot%
- It creates a default MpfConfig.xml in %SystemRoot%
- It adds the string “PwdFlt” to the registry value “Notification Packages” in “HKLM:\SYSTEM\CurrentControlSet\Control\Lsa”, if not already there.
After this process, a restart is required.
The output of the installation is referenced in the appendix.

## Architecture
Windows requires a DLL to be written in C / C++ for password filtering. There is no way to use a managed Dll for this. Hence one important part of the solution is the PwdFlt C project. It redirects the calls to the MpfProxy Dll, which then redirects it to the MpfWorker, which implements the password rules and does the actual work. There is some performance overhead which is neglectable running the password test rules in managed code instead of C. There is also some installation overhead: The .net proxy Dll must be registered (RegAsm.exe). The MpfWorker can be enabled / disabled and customized using the MpfConfig.xml file. This file also stores the password policy settings.

### PwdFlt
This project is written in C as required by Windows. The requirements Windows has, are documented in <TODO>. The Dll implements the methods PasswordChangeNotify and PasswordFilter. The latter one is the important one for this project. It retrieves the password in clear text and returns a Boolean.
First the Dll tries to read the file “C:\Windows\MpfConfig.xml”. If the file does not exist, the password filter always returns true. This is an easy way to disable the custom password filter without the requirement of a machine reboot in case it does not behave as expected. It is recommended to always use the standard Windows password complexity as well to agree on a minimal security standard.
The password, SamAccountName and the users FullName is converted into a BSTR and sent to the MpfProxy .net Dll by calling the IProxy interface. The MpfProxy Dll must be register first using RegAsm.exe. When using the installation script, this is taken care of.

### MpfProxy
This Dll does not have much to do but to forward the password, SamAccountName and the account’s full name to the MpfWorker. In order to allow a C application to communicate with the managed Dll, it exports the interface IProxy. The interface implementation calls the TestPasword method of the MpfWorker.
Calling a .net Dll from C requires the Dll to be strongly typed.

### MpfWorker
This project is the center of the solution. The behavior of the worker can be controlled by the MpfConfig.xml which has to be in %SystemRoot% (hard coded). If there is no MpfConfig.xml file, default settings apply. During initialization the worker tries to connect to Active Directory (RootDse) for being able to check the password against defined properties in AD. If this fails, AD checks are skipped.

The password test call coming from Windows is redirected by the PwdFlt.dll to the MpfProxy.dll to the TestPassword method of the MpfWorker. The MpfWorker processes all the defined password rules and returns true, if the password is according to the rules.

The MpfWorker uses .net reflection to find all defined password rules. Password rules must be defined in class having the PasswordRuleContainer attribute. The individual test methods are identified by the PasswordRule attribute.
The worker calls the discovered password rules in the given order until a password test method returns false. If the password has passed all test methods, the worker returns true.

A password filter rule may return true even if parts of the password are denied or part of the black list. Then the number of unwanted characters is added and checked after processing all the records against the “AllowedBlackListQuotaPercent” config setting. This and how password rules are implemented in general is documented in more detail later.

## Password Rules
### Implemented Password Rules

```Note: Password rules marked with an asterisk are configurable using the MpfConfig.xml```

The following password rules are implemented:
* DefaultPasswordRules (Order 1)
  - TestMinPasswordLength*
  - TestMaxPasswordLength*
  - TestAtLeastOneUpperCaseCharacter
  - TestAtLeastOneLowerCaseCharacter
  - TestMaxConsecutiveChars*
* BlackListPasswordRules (Order 3)
  - TestBlackList (matches the password agains all lines in the MpfBlackList.txt. Supports wildcards like *Password*)
* AdPasswordRules (Order 5)
  - TestDenyGivenName*
  - TestDenySurname*
* ZxcvbnPasswordRules (Order 10)
  - TestZxcvbn* (uses the Zxcvbn-cs project to calculate a password score. The minimal password score can be set in the config file)


### Password Rules Design
Password rules are just methods defined in a class. The class serves as a container and must have the PasswordRuleContainer attribute to be found by the MpfWorker. The attribute PasswordRuleContainer provides a parameter “Order” to control the order of execution. This is mainly for performance reasons.
The class should inherit from PasswordRulesBase if password test methods need to access the MpfConfig.

### Adding new password rules
Per the password rule design adding a new password rule is just adding a new class. The following serves as a template:

``` C#
[PasswordRuleContainer(Order = 3)]
public class BlackListPasswordRules : PasswordRulesBase
{
    [PasswordRule]
    public static bool TestRule(string password)
    {
          return true;
    }
}
```

## Security Considerations
It is a basic principle that passwords should not be stored in clear text – anywhere. The domain controller receives the password in clear text if a user or computer changes the password. There is a way in Windows to send the password hash instead of the clear text password, but then the password cannot be validated by the password filter anymore.

This Dll is designed to be used primarily on Domain Controllers. Nobody except a small number of domain administrators should be granted the right to logon and access domain controllers. If a domain controller is compromised for whatever reason, the whole enterprise environment is at risk. Therefore, this Dll dealing with the password are expected to run in a very safe environment. Passwords are not written to disk and kept only in memory.

Password filter in Windows are like a stack. The password is verified against all filters registered on the domain controller. It is recommended to leave the standard Windows password complexity enabled to always guarantee a minimal security like minimal password length.

## Technical Documentation
### Configuring the Managed Password Filter
When installing the Managed Password Filter using the provided script, a configuration file is created in %SystemRoot%.

The configuration file is read before a password change. This allows changing the configuration without a reboot.

By default, the configuration file looks like this:

``` xml
<?xml version="1.0"?>
<Config xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">
  <IsEnabled>true</IsEnabled>
  <ResultIfFailure>true</ResultIfFailure>
  <BlackListPath>C:\Windows\MpfBlackList.txt</BlackListPath>
  <PasswordPolicy>
    <MinLength>8</MinLength>
    <MaxLength>254</MaxLength>
    <MinScore>1</MinScore>
    <MaxConsecutiveRepeatingCharacters>5</MaxConsecutiveRepeatingCharacters>
    <Denysettings>DenyName</Denysettings>
    <AllowedBlackListQuotaPercent>20</AllowedBlackListQuotaPercent>
  </PasswordPolicy>
</Config>
```

#### IsEnabled
If set to “false”, the Managed Password Filter will not process the password at all. Still the default Windows integrated password filter is used.

Default is “true”.

#### ResultIfFailure
This setting defines what the Managed Password Filter returns in case one or more rules are running into exceptions. For example, if the MpfBlackList.txt does not exist and the blacklist test subsequently fails, this setting makes the password filter return “true” even if a password test did not work.

Default is “true”.

#### BlackListPath
The path to the password blacklist.

Default is “C:\Windows\MpfBlackList.txt”.

#### MinLength
The minimum allowed length of a password. As the default Windows password filter still applies, this setting does not have an effect if it is less than the Windows default setting.

Defaut is 8.

#### MaxLength
The maximum allowed length of a password. As the default Windows password filter still applies, this setting does not have an effect if it is more than the Windows default setting.

Default is 254.

#### MinScore
The Managed Password Filter integrates the [zxcvbn-cs](https://github.com/trichards57/zxcvbn-cs) password checker. The password checker returns a password score. This setting defines the minimum password score or disables the additional password checker if set to 0.

Please check out the documentation of [zxcvbn-cs](https://github.com/trichards57/zxcvbn-cs) for details about the password score.
Default is 0.

#### MaxConsecutiveRepeatingCharacters
This defines the maximum consecutive repeating characters allowed in a sequence. The default is 5. Sequences like “aaaaa” are not allowed.

Default is 5.

#### Denysettings
With this setting some dynamic strings will be counted as blacklist passwords. The following settings are possible:
- DenyName: DenyGivenName | DenySurName
- DenyGivenName:
  The user’s given name is considered as a blacklisted password. The given name is taken from Active Directory.
- DenySurname: The user’s surname is considered as a blacklisted password. The surname is taken from Active Directory.
- DenyYear: The current year is considered as a blacklisted password.
Default is "DenyName".

#### AllowedBlackListQuotaPercent
Each string that is part of the blacklist is not completely forbidden but measured to the full password length. Consider the following case:

---
- The string "*Test*" is blacklisted
- The DenySettings contains “DenyYear”
- The user’s password is “Test2018”
- As a result, 100% of the password is blacklisted and the password is not allowed.
---
- The string “*Test*” is blacklisted
- The user’s password is “Test!SomeName12”
- 26.7% of the password is blacklisted and the password is not allowed considering a default AllowedBlackListQuotaPercent of 20%
---

Default is 20 (20%).

### Blacklist
The blacklist is a file named MpfBlacklist.txt. By default, the ManagedPasswordFilter looks for the file in SystemRoot.

Before a password change is accepted, the password is tested against each entry in the blacklist. Asterisk wildcards are supported and work like in the command shell or PowerShell. Each line in the blacklist is one entry.

This happens, if the blacklist contains the following entries (values in Active Directory properties are not considered) and the AllowedBlackListQuotaPercent is 20%:
```
Test*
test*
*Somepassword*
*123456*
```

Test cases:
-	”Test123456” will not be accepted, blacklisted part is 100%
-	”Test1234” will not be accepted, blacklisted part is 50%
-	”123456Test” will not be accepted, blacklisted part is 60%
-	”PasswordTest1234” will be accepted, blacklisted part is 0%
-	”12Somepassword34” will not be accepted, blacklisted part is 25%
-	”12SomePassword34” will not be accepted, blacklisted part is 0%

### Performance Considerations
In tests in a small lab environment the password rules implemented have taken max. 200ms to process a password.

The size of the blacklist has an effect on the time it takes to process a password. 

|Entries in blacklist|Password processing time in ms|
|-------------------:|-----------------------------:|
|100 |170 |
|5000 |230 |
|10000 |280 |
|20000 |330 |
|50000 |570 |
|100000 | 1260 |

### Testing
When having installed the password filter as described in the “Installation” section, Windows forwards any new password to the MpfWorker. Windows does not tell why a password is not accepted and just refers to the password policy. To test passwords and get verbose output about the MpfWorker’s activities, use the TestApp.exe. This exe is not part of the BuildOutput\InstallVersion folder, but it is part of the full Visual Studio build folder: BuildOutput\x64\Debug | Release.

One purpose of using the TestApp.exe can reset the MpfConfig using the /InitConfig switch.

The main purpose is to test passwords against the MpfWorker. This just requires the password to test and a test platform. There are three options:
-	ManagedWorker: Calls the managed worker directly.
-	ManagedProxy: Calls the MpfWorker via the MpfProxy.
-	UnmanagedProxy: Calls the MpfWorker via the MpfProxy which gets called by the UnmanagedProxy. This is the same way the Windows password reset process uses.

The output of the TestApp looks lie this
```
PS C:\Mpf> .\TestApp.exe /platform:unmanagedproxy /password:Somepass1

-------------------------------------------------------------------------------------------
password filter testapp 0.10

...
-------------------------------------------------------------------------------------------

Password       Somepass1
AccountName    TestAccountName
FullName       TestFullName
Platform       UnmanagedProxy
InitConfig     <not set>
-------------------------------------------------------------------------------------------
Input data in unmanaged code: AccountName:'TestAccountName' FullName:'TestFullName' Password:'Somepass1'

Going through 1 test rules in class 'InitPasswordRule'
Calling password filter rule 'InitValues': True
Going through 5 test rules in class 'DefaultPasswordRules'
Calling password filter rule 'TestMinPasswordLength': True
Calling password filter rule 'TestMaxPasswordLength': True
Calling password filter rule 'TestAtLeastOneUpperCaseCharacter': True
Calling password filter rule 'TestAtLeastOneLowerCaseCharacter': True
Calling password filter rule 'TestMaxConsecutiveChars': True
Going through 1 test rules in class 'BlackListPasswordRules'
Calling password filter rule 'TestBlackList': True
Going through 2 test rules in class 'AdPasswordRules'
Could not connect to AD, skipping test
Calling password filter rule 'TestDenyGivenName': True
Could not connect to AD, skipping test
Calling password filter rule 'TestDenySurname': True
Going through 1 test rules in class 'ZxcvbnPasswordRules'
Calling password filter rule 'TestZxcvbn': True
Going through 1 test rules in class 'QuotaPasswordRules'
Calling password filter rule 'TestTotalProhibitedCharacters': True
Result in unmanaged code: -1

True
```

The last line indicates if the password is according to the policy (True).


## Appendix

### Installation Script Output
```
PS C:\MPF> .\InstallPasswordFilter.ps1
Copying file 'MpfProxy.dll' to 'C:\Windows\System32'
Copying file 'MpfWorker.dll' to 'C:\Windows\System32'
Copying file 'PwdFlt.dll' to 'C:\Windows\System32'
Copying file 'zxcvbn-core.dll' to 'C:\Windows\System32'
Registering types
Microsoft .NET Framework Assembly Registration Utility version 4.7.3190.0
for Microsoft .NET Framework version 4.7.3190.0
Copyright (C) Microsoft Corporation.  All rights reserved.

Types registered successfully

Password Blacklist does not exist, creating a new empty one in 'C:\Windows\MpfBlackList.txt'

BlackList has 0 entries

Creating default Password Filter config in 'C:\Windows\MpfConfig.xml'
Enabling password filter DLL
Current value of 'HKLM:\SYSTEM\CurrentControlSet\Control\Lsa\Notification Packages' is:
scecli

New value of 'HKLM:\SYSTEM\CurrentControlSet\Control\Lsa\Notification Packages' is:
scecli PwdFlt
Installing the password filter has succeeded
You must reboot the machine to finish the process 
```
