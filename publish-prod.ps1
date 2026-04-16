# Taken from psake https://github.com/psake/psake

<#
.SYNOPSIS
  Publishes signed NuGet packages to NuGet.org.
  Expects packages in .\artifacts folder (output of the signing step).
.EXAMPLE
  $env:NUGET_API_KEY_PROD = "your-api-key"
  .\publish-prod.ps1
#>
function Exec {
    [CmdletBinding()]
    param(
        [Parameter(Position = 0, Mandatory = 1)][scriptblock]$cmd,
        [Parameter(Position = 1, Mandatory = 0)][string]$errorMessage = ($msgs.error_bad_command -f $cmd)
    )
    & $cmd
    if ($lastexitcode -ne 0) {
        throw ("Exec: " + $errorMessage)
    }
}

if (-Not (Test-Path .\artifacts)) { throw ("Folder '.\artifacts' does not exist.") }

Get-ChildItem .\artifacts -Filter *.nupkg |
Foreach-Object {
    $fileName = $_.Name
    $package = ".\artifacts\${fileName}"

    Write-Host "Package $package will be published."

    exec { & dotnet nuget push $package --api-key "$env:NUGET_API_KEY_PROD" --source https://api.nuget.org/v3/index.json --skip-duplicate }
}
