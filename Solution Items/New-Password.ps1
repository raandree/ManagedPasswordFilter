#requires -version 4
<#
.SYNOPSIS
    This script Generates a random strong password.
.DESCRIPTION
    The New-RandomPassword generates a random strong password from mix of capital letters, small letters, symbols (Special Characters) and numbers, All the parameter only accept numeric values.
.PARAMETER PasswordLength
    PasswordLength parameter prompts for password length, If this parameter is not defined by default it creates 12 digit mix characters. This parameter cannot be used with other SmallLetter, Capitalletter, Symbol and Number.
.PARAMETER SmallLetter
    This syntax is optional and cannot be used with PasswordLength. If you use, for example 5 value, Password will include 5 random small letter characters (a-z) in the password.
.PARAMETER CapitalLetter
    This syntax is optional and cannot be used with PasswordLength. If you use for example 5 value, you will find 5 random capital letters (A-Z) in the password.
.PARAMETER Number
    By Default value is 0, and do not require to mention, If you use for example 2 value for this syntax, you will find 2 random numbers (0-9) in the password.
.PARAMETER Symbol
    By Default value is 0, and do not require to mention, If you use for example 2 value for this syntax, you will find 2 random numbers (! " # $ % & ' ( ) * + , - . / : ; < = > ? @ [ \ ] ^ _ { | } ~) in the password.
.INPUTS
    [System.int]
.OUTPUTS
    [System.String]
    [System.Management.Automation.PSCustomObject]
.NOTES
    Script Version:        2.0
    Author:                Kunal Udapi
    Creation Date:         20 September 2017
    Purpose/Change:        Get windows office and OS licensing information.
    Useful URLs:           http://kunaludapi.blogspot.in/2013/11/generate-random-password-powershell.html
                           http://vcloud-lab.com/entries/powershell/microsoft-powershell-generate-random-anything-filename--temppath--guid--password-
    OS Version:            Windows 8.1
    Powershell Version:    Powershell V5.1 Desktop
.EXAMPLE
    PS C:\>.\New-RandomPassword.ps1

    It generated strong random 12 digit password with defination Character, Phonetic and character type
.EXAMPLE
    PS C:\>.\New-RandomPassword.ps1 -PasswordLength 6
    
    Character Phonetic        Type          
    --------- --------        ----          
    3         Three           Number        
    [         Opening bracket Symbol        
    6         Six             Number        
    4         Four            Number        
    w         Whiskey         Small Letter  
    D         Delta           Capital Letter


    3[64wD

    With the parameter passwordLength, it generates 6 digit random password as shown results.
.EXAMPLE
    PS C:\>.\New-RandomPassword.ps1 -SmallLetter 4 -CapitalLetter 4 -Number 2 -Symbol 2

    As per syntax and provided value, there will be 4 small letters, 4 capital letter , 2 numbers and 2 symbols special characters in the newly generated password.
#>

[CmdletBinding(SupportsShouldProcess=$True,
ConfirmImpact='Medium',
HelpURI='http://vcloud-lab.com',
DefaultParameterSetName='Default')]
Param (
    [parameter(ParameterSetName = 'Default', Position=0, Mandatory=$false, ValueFromPipelineByPropertyName=$True, ValueFromPipeline=$True)]
    [alias('length')]
    [int]$PasswordLength = 12,

    [parameter(ParameterSetName = 'Option', Position=0, ValueFromPipelineByPropertyName=$true)]
    [alias('LowerCase','Small')]
    [int]$SmallLetter = 0,
    [parameter(ParameterSetName = 'Option', Position=1, ValueFromPipelineByPropertyName=$true)]
    [alias('UpperCase','Capital')]
    [int]$CapitalLetter = 0,
    [parameter(ParameterSetName = 'Option', Position=2, ValueFromPipelineByPropertyName=$true)]
    [int]$Number = 0,
    [parameter(ParameterSetName = 'Option', Position=3, ValueFromPipelineByPropertyName=$true)]
    [alias('SpecialLetter')]
    [int]$Symbol = 0
)
Begin {    
    $RandomOption = @()
    $CompletePassword = @()
        
    $CompleteSmallPassword = @()
    $CompleteCapitalPassword = @()
    $CompleteSymbolPassword = @()
    $CompleteNumberPassword = @()
    #table
    $JSon = @"
    [
        {"SrNo":  "1","Number":  "33","Character":  "!","Phonetic":  "Exclamation point","Type":  "Symbol"},
        {"SrNo":  "2","Number":  "34","Character":  "\"","Phonetic":  "Double quotes","Type":  "Symbol"},
        {"SrNo":  "3","Number":  "35","Character":  "#","Phonetic":  "Hash sign","Type":  "Symbol"},
        {"SrNo":  "4","Number":  "36","Character":  "$","Phonetic":  "Dollar sign","Type":  "Symbol"},
        {"SrNo":  "5","Number":  "37","Character":  "%","Phonetic":  "Percent sign","Type":  "Symbol"},
        {"SrNo":  "6","Number":  "38","Character":  "&","Phonetic":  "Ampersand","Type":  "Symbol"},
        {"SrNo":  "7","Number":  "39","Character":  "'","Phonetic":  "Single quote","Type":  "Symbol"},
        {"SrNo":  "8","Number":  "40","Character":  "(","Phonetic":  "Opening parenthesis","Type":  "Symbol"},
        {"SrNo":  "9","Number":  "41","Character":  ")","Phonetic":  "Closing parenthesis","Type":  "Symbol"},
        {"SrNo":  "10","Number":  "42","Character":  "*","Phonetic":  "Asterisk","Type":  "Symbol"},
        {"SrNo":  "11","Number":  "43","Character":  "+","Phonetic":  "Plus sign","Type":  "Symbol"},
        {"SrNo":  "12","Number":  "44","Character":  ",","Phonetic":  "Comma","Type":  "Symbol"},
        {"SrNo":  "13","Number":  "45","Character":  "-","Phonetic":  "Minus sign -Hyphen","Type":  "Symbol"},
        {"SrNo":  "14","Number":  "46","Character":  ".","Phonetic":  "Period","Type":  "Symbol"},
        {"SrNo":  "15","Number":  "47","Character":  "/","Phonetic":  "Slash","Type":  "Symbol"},
        {"SrNo":  "16","Number":  "58","Character":  ":","Phonetic":  "Colon","Type":  "Symbol"},
        {"SrNo":  "17","Number":  "59","Character":  ";","Phonetic":  "SemiColon","Type":  "Symbol"},
        {"SrNo":  "18","Number":  "60","Character":  "<","Phonetic":  "Less than sign","Type":  "Symbol"},
        {"SrNo":  "19","Number":  "61","Character":  "=","Phonetic":  "Equal sign","Type":  "Symbol"},
        {"SrNo":  "20","Number":  "62","Character":  ">","Phonetic":  "Greater than sign","Type":  "Symbol"},
        {"SrNo":  "21","Number":  "63","Character":  "?","Phonetic":  "Question mark","Type":  "Symbol"},
        {"SrNo":  "22","Number":  "64","Character":  "@","Phonetic":  "At symbol","Type":  "Symbol"},
        {"SrNo":  "23","Number":  "91","Character":  "[","Phonetic":  "Opening bracket","Type":  "Symbol"},
        {"SrNo":  "24","Number":  "92","Character":  "\\","Phonetic":  "Backslash","Type":  "Symbol"},
        {"SrNo":  "25","Number":  "93","Character":  "]","Phonetic":  "Closing bracket","Type":  "Symbol"},
        {"SrNo":  "26","Number":  "94","Character":  "^","Phonetic":  "Caret - circumflex","Type":  "Symbol"},
        {"SrNo":  "27","Number":  "95","Character":  "_","Phonetic":  "Underscore","Type":  "Symbol"},
        {"SrNo":  "29","Number":  "123","Character":  "{","Phonetic":  "Opening brace","Type":  "Symbol"},
        {"SrNo":  "30","Number":  "124","Character":  "|","Phonetic":  "Vertical bar","Type":  "Symbol"},
        {"SrNo":  "31","Number":  "125","Character":  "}","Phonetic":  "Closing brace","Type":  "Symbol"},
        {"SrNo":  "32","Number":  "126","Character":  "~","Phonetic":  "Equivalency sign - Tilde","Type":  "Symbol"},
        {"SrNo":  "33","Number":  "65","Character":  "A","Phonetic":  "Alpha ","Type":  "Capital Letter"},
        {"SrNo":  "34","Number":  "66","Character":  "B","Phonetic":  "Bravo ","Type":  "Capital Letter"},
        {"SrNo":  "35","Number":  "67","Character":  "C","Phonetic":  "Charlie ","Type":  "Capital Letter"},
        {"SrNo":  "36","Number":  "68","Character":  "D","Phonetic":  "Delta ","Type":  "Capital Letter"},
        {"SrNo":  "37","Number":  "69","Character":  "E","Phonetic":  "Echo ","Type":  "Capital Letter"},
        {"SrNo":  "38","Number":  "70","Character":  "F","Phonetic":  "Foxtrot ","Type":  "Capital Letter"},
        {"SrNo":  "39","Number":  "71","Character":  "G","Phonetic":  "Golf ","Type":  "Capital Letter"},
        {"SrNo":  "40","Number":  "72","Character":  "H","Phonetic":  "Hotel ","Type":  "Capital Letter"},
        {"SrNo":  "41","Number":  "73","Character":  "I","Phonetic":  "India ","Type":  "Capital Letter"},
        {"SrNo":  "42","Number":  "74","Character":  "J","Phonetic":  "Juliet ","Type":  "Capital Letter"},
        {"SrNo":  "43","Number":  "75","Character":  "K","Phonetic":  "Kilo ","Type":  "Capital Letter"},
        {"SrNo":  "44","Number":  "76","Character":  "L","Phonetic":  "Lima ","Type":  "Capital Letter"},
        {"SrNo":  "45","Number":  "77","Character":  "M","Phonetic":  "Mike ","Type":  "Capital Letter"},
        {"SrNo":  "46","Number":  "78","Character":  "N","Phonetic":  "November ","Type":  "Capital Letter"},
        {"SrNo":  "47","Number":  "79","Character":  "O","Phonetic":  "Oscar ","Type":  "Capital Letter"},
        {"SrNo":  "48","Number":  "80","Character":  "P","Phonetic":  "Papa ","Type":  "Capital Letter"},
        {"SrNo":  "49","Number":  "81","Character":  "Q","Phonetic":  "Quebec ","Type":  "Capital Letter"},
        {"SrNo":  "50","Number":  "82","Character":  "R","Phonetic":  "Romeo ","Type":  "Capital Letter"},
        {"SrNo":  "51","Number":  "83","Character":  "S","Phonetic":  "Sierra ","Type":  "Capital Letter"},
        {"SrNo":  "52","Number":  "84","Character":  "T","Phonetic":  "Tango ","Type":  "Capital Letter"},
        {"SrNo":  "53","Number":  "85","Character":  "U","Phonetic":  "Uniform ","Type":  "Capital Letter"},
        {"SrNo":  "54","Number":  "86","Character":  "V","Phonetic":  "Victor ","Type":  "Capital Letter"},
        {"SrNo":  "55","Number":  "87","Character":  "W","Phonetic":  "Whiskey ","Type":  "Capital Letter"},
        {"SrNo":  "56","Number":  "88","Character":  "X","Phonetic":  "X-Ray ","Type":  "Capital Letter"},
        {"SrNo":  "57","Number":  "89","Character":  "Y","Phonetic":  "Yankee ","Type":  "Capital Letter"},
        {"SrNo":  "58","Number":  "90","Character":  "Z","Phonetic":  "Zulu ","Type":  "Capital Letter"},
        {"SrNo":  "59","Number":  "97","Character":  "a","Phonetic":  "Alpha ","Type":  "Small Letter"},
        {"SrNo":  "60","Number":  "98","Character":  "b","Phonetic":  "Bravo ","Type":  "Small Letter"},
        {"SrNo":  "61","Number":  "99","Character":  "c","Phonetic":  "Charlie ","Type":  "Small Letter"},
        {"SrNo":  "62","Number":  "100","Character":  "d","Phonetic":  "Delta ","Type":  "Small Letter"},
        {"SrNo":  "63","Number":  "101","Character":  "e","Phonetic":  "Echo ","Type":  "Small Letter"},
        {"SrNo":  "64","Number":  "102","Character":  "f","Phonetic":  "Foxtrot ","Type":  "Small Letter"},
        {"SrNo":  "65","Number":  "103","Character":  "g","Phonetic":  "Golf ","Type":  "Small Letter"},
        {"SrNo":  "66","Number":  "104","Character":  "h","Phonetic":  "Hotel ","Type":  "Small Letter"},
        {"SrNo":  "67","Number":  "105","Character":  "i","Phonetic":  "India ","Type":  "Small Letter"},
        {"SrNo":  "68","Number":  "106","Character":  "j","Phonetic":  "Juliet ","Type":  "Small Letter"},
        {"SrNo":  "69","Number":  "107","Character":  "k","Phonetic":  "Kilo ","Type":  "Small Letter"},
        {"SrNo":  "70","Number":  "108","Character":  "l","Phonetic":  "Lima ","Type":  "Small Letter"},
        {"SrNo":  "71","Number":  "109","Character":  "m","Phonetic":  "Mike ","Type":  "Small Letter"},
        {"SrNo":  "72","Number":  "110","Character":  "n","Phonetic":  "November ","Type":  "Small Letter"},
        {"SrNo":  "73","Number":  "111","Character":  "o","Phonetic":  "Oscar ","Type":  "Small Letter"},
        {"SrNo":  "74","Number":  "112","Character":  "p","Phonetic":  "Papa ","Type":  "Small Letter"},
        {"SrNo":  "75","Number":  "113","Character":  "q","Phonetic":  "Quebec ","Type":  "Small Letter"},
        {"SrNo":  "76","Number":  "114","Character":  "r","Phonetic":  "Romeo ","Type":  "Small Letter"},
        {"SrNo":  "77","Number":  "115","Character":  "s","Phonetic":  "Sierra ","Type":  "Small Letter"},
        {"SrNo":  "78","Number":  "116","Character":  "t","Phonetic":  "Tango ","Type":  "Small Letter"},
        {"SrNo":  "79","Number":  "117","Character":  "u","Phonetic":  "Uniform ","Type":  "Small Letter"},
        {"SrNo":  "80","Number":  "118","Character":  "v","Phonetic":  "Victor ","Type":  "Small Letter"},
        {"SrNo":  "81","Number":  "119","Character":  "w","Phonetic":  "Whiskey ","Type":  "Small Letter"},
        {"SrNo":  "82","Number":  "120","Character":  "x","Phonetic":  "X-Ray ","Type":  "Small Letter"},
        {"SrNo":  "83","Number":  "121","Character":  "y","Phonetic":  "Yankee ","Type":  "Small Letter"},
        {"SrNo":  "84","Number":  "122","Character":  "z","Phonetic":  "Zulu ","Type":  "Small Letter"},
        {"SrNo":  "85","Number":  "48","Character":  "0","Phonetic":  "Zero","Type":  "Number"},
        {"SrNo":  "86","Number":  "49","Character":  "1","Phonetic":  "One","Type":  "Number"},
        {"SrNo":  "87","Number":  "50","Character":  "2","Phonetic":  "Two","Type":  "Number"},
        {"SrNo":  "88","Number":  "51","Character":  "3","Phonetic":  "Three","Type":  "Number"},
        {"SrNo":  "89","Number":  "52","Character":  "4","Phonetic":  "Four","Type":  "Number"},
        {"SrNo":  "90","Number":  "53","Character":  "5","Phonetic":  "Five","Type":  "Number"},
        {"SrNo":  "91","Number":  "54","Character":  "6","Phonetic":  "Six","Type":  "Number"},
        {"SrNo":  "92","Number":  "55","Character":  "7","Phonetic":  "Seven","Type":  "Number"},
        {"SrNo":  "93","Number":  "56","Character":  "8","Phonetic":  "Eight","Type":  "Number"},
        {"SrNo":  "94","Number":  "57","Character":  "9","Phonetic":  "Nine","Type":  "Number"}
    ]
"@
    #Excluded Characters
    #{"SrNo":  "28","Number":  "96","Character":  "`","Phonetic":  "Grave accent","Type":  "Symbol"},

    #System.Security.Cryptography.RNGCryptoServiceProvider
    function Get-Rng {
        $RandomBytes = New-Object -TypeName "System.Byte[]" 4
        $Random = New-Object -TypeName "System.Security.Cryptography.RNGCryptoServiceProvider"
        $Random.GetBytes($RandomBytes)
        [BitConverter]::ToInt32($RandomBytes, 0)
    } #function Get-Rng
} #Begin
Process {    
    #tables
    $AlphbatesTable = $JSon | ConvertFrom-Json
    $SymbolTable = $AlphbatesTable | Where-Object {$_.Type -eq 'Symbol'}
    $CapitalLetterTable = $AlphbatesTable | Where-Object {$_.Type -eq 'Capital Letter'}
    $SmallLetterTable = $AlphbatesTable | Where-Object {$_.Type -eq 'Small Letter'}
    $NumberTable = $AlphbatesTable | Where-Object {$_.Type -eq 'Number'}
        
    switch ($PsCmdlet.ParameterSetName) {
        'Default' {
            for ($i = 1; $i -le $PasswordLength; $i++) {
                $DefaultUniqueNumber = Get-Rng
                $PasswordHash = Get-Random -InputObject $AlphbatesTable -SetSeed $DefaultUniqueNumber
                $CompletePassword += $PasswordHash
            } #for ($i = 1; $i -le $PasswordLength; $i++)
        } #'Default'
        'Option' {
            if ($SmallLetter -ne 0) {
                for ($sm = 1; $sm -le $SmallLetter; $sm++) {
                    $SmallUniqueNumber = Get-Rng
                    $CompleteSmallPassword += Get-Random -InputObject $SmallLetterTable -SetSeed $SmallUniqueNumber
                } #for ($sm = 1; $sm -le $SmallLetter; $sm++)
            } #if ($SmallLetter -ne 0)
                
            if ($CapitalLetter -ne 0) {
                for ($c = 1; $c -le $CapitalLetter; $c++) {
                    $CapitalUniqueNumber = Get-Rng
                    $CompleteCapitalPassword += Get-Random -InputObject $CapitalLetterTable -SetSeed $CapitalUniqueNumber
                } #for ($s = 1; $s -le $CapitalLetter; $s++)
            } #if ($CapitalLetter -ne 0)

            if ($Number -ne 0) {
                for ($N = 1; $N -le $Number; $N++) {
                    $NumberUniqueNumber = Get-Rng
                    $CompleteNumberPassword += Get-Random -InputObject $NumberTable -SetSeed $NumberUniqueNumber
                } #for ($s = 1; $s -le $Number; $s++)
            } #if ($Number -ne 0)

            if ($Symbol -ne 0) {
                for ($sy = 1; $sy -le $Symbol; $sy++) {
                    $SymbolUniqueNumber = Get-Rng
                    $CompleteSymbolPassword += Get-Random -InputObject $SymbolTable -SetSeed $SymbolUniqueNumber
                } #for ($sy = 1; $sy -le $Symbol; $sy++)
            } #if ($Symbol -ne 0)
                
            $RandomOption += $CompleteSmallPassword 
            $RandomOption += $CompleteCapitalPassword 
            $RandomOption += $CompleteNumberPassword
            $RandomOption += $CompleteSymbolPassword
            #$CompletePassword = $RandomOption | Sort-Object {Get-Random (Get-Rng)}
            $CompletePassword = $RandomOption | Select-Object *, @{N='Sort'; E={1..500 | Get-Random (Get-Rng)}} | Sort-Object -Property Sort
        } #'Option'
    } #switch ($PsCmdlet.ParameterSetName)

} #Process
End {
    $FinalPassword = $CompletePassword.Character -Join ""
    $Info = $CompletePassword | Select-Object Character, Phonetic, Type
    #Write-Verbose ($Info | Out-String) -ForegroundColor Yellow -NoNewline
    $FinalPassword
} #End