# Taken from psake https://github.com/psake/psake

<#
.SYNOPSIS
  This is a helper function that runs a scriptblock and checks the PS variable $lastexitcode
  to see if an error occurred. If an error is detected then an exception is thrown.
.EXAMPLE
  exec { dotnet build } "Error executing dotnet build"
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

if (Test-Path .\build_out) { Remove-Item .\build_out -Force -Recurse }

exec { & dotnet restore }

$suffix = $NULL
switch -Exact ($env:BUILD_TYPE)
{
    "PROD"
    {
        $suffix = "prod"; Break
    }
    Default {
        $suffix = "local"; Break
    }
}

$commitHash = $(git rev-parse --short HEAD)
$buildSuffix = @{ $true = "$($suffix)-$($commitHash)"; $false = "$($commitHash)" }[$suffix -ne ""]

Write-Host "build: Build version suffix is $buildSuffix"

exec { & dotnet build GroupDocs.Metadata.Mcp.sln -c Release --version-suffix=$buildSuffix -v q /nologo }

exec { & dotnet pack .\src\GroupDocs.Metadata.Mcp\GroupDocs.Metadata.Mcp.csproj -c Release -o .\build_out --include-symbols -p:SymbolPackageFormat=snupkg --no-build }
