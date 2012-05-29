param([string]$Action)


$location = (Get-Location)
$build = Join-Path $location "build.cmd"
$xunit = Join-Path  $location "\tools\xunit\xunit.console.clr4.x86.exe"
$test_assembly = Join-Path  $location  "src\MarkPad.Tests\bin\Testing\MarkPad.Tests.dll"
$assembly_info = Join-Path  $location "src\GlobalAssemblyInfo.cs"

function Update-Version
{
    Param([Parameter(Mandatory=$true)][string]$version)

    Run-Tests
    Set-Version $version
    Create-ReleaseDirectory $version
    Create-Package
    Bundle-Package  $version
    Update-Manifest
}


function Run-Tests
{
    Log-Message "Running unit tests"
    CMD /C "$build Testing x86"
    CMD /C "$xunit $test_assembly"
}
 
function Set-Version
{
    Param([Parameter(Mandatory=$true)][string]$version)

    Log-Message "Updating version to $version"
    $infile=get-content $assembly_info
    $regex = New-Object System.Text.RegularExpressions.Regex "\b\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}\b"
    $replace = $regex.Replace($infile,$version)
    set-content -Value $replace $assembly_info
}

function Create-ReleaseDirectory
{
    Param([Parameter(Mandatory=$true)][string]$version)

    Log-Message "Create directory for version $version"
    $release_dir = Join-Path (Get-Location) "tools\versions\$version"
    New-Item $release_dir -type directory -Force
}

function Create-Package
{
    Log-Message "Creating package"
    CMD /C "$build Release x86"
    $output = Join-Path (Get-Location) "src\MarkPad\bin\Release\*"
    $artifacts = Join-Path (Get-Location) "artifacts\"
    CMD /C "xcopy $output $artifacts /s /e /Y"
}

function Bundle-Package
{
    Param([Parameter(Mandatory=$true)][string]$version)
    
    Log-Message "Bundle package"
    $binary = Join-Path (Get-Location) "artifacts\MarkPad.exe"
    $dir = Join-Path (Get-Location) "tools\versions\$version\"
    Copy-Item $binary $dir
}

function Update-Manifest()
{
    Log-Message "Update manifest"
}

function Log-Message ($message)
{
    write-host $message -foregroundcolor "green"
}

switch ($Action) 
    { 
        "Run-Tests" { Run-Tests } 
        "Create-Package" { Create-Package }
        "Update-Version" { Update-Version }
        default { }
    }
