param(
    $BuildNumber = $env:BUILD_BUILDNUMBER,
    [switch]$NoPublish = $($env:BUILD_REASON -eq 'PullRequest')
)

$ErrorActionPreference = "Stop"
Set-Location "$PSScriptRoot\.."
. .\build\Dayforce.PS.Core.Bootstrap.ps1
Use-Defaults

& $SetBuildScriptArguments -Props @{
    BuildNumber = @{ DefaultValue = Get-DefaultVersion }
}

$Properties = Get-VCSRepositoryParams
$Properties.BuildNumber = $BuildNumber

$MSBuildProperties = ConvertTo-MSBuildProperties $Properties
$BinLog = Get-BinaryLogParams

$env:MSBuildSdksPath = $null

Write-Host "Building ..."
dotnet build -v:q -nologo -c Release $BinLog.BinaryLogArg $MSBuildProperties
$ExitCode = $LastExitCode

Save-BinaryLog $BinLog

if ($ExitCode)
{
    exit $ExitCode
}

$MainAssemblyName = (Get-Item *.sln).BaseName

Write-Host "Asserting Product Version ..."
"src/bin/Release/*/$MainAssemblyName.exe" | Assert-ProductVersionEndsWithCommitId $Properties.SourceRevisionId

Write-Host "Packing ..."
$NuSpecFile = Initialize-ToolNuSpecFile $Properties "Platform" 

Remove-Item src\bin\Release\*.nupkg -ErrorAction SilentlyContinue
dotnet pack -v:q -nologo -c Release --no-build -p:NuspecFile=$NuspecFile
if ($LastExitCode)
{
    exit $LastExitCode
}

if (!$NoPublish)
{
    Write-Host "Publishing ..."
    $NuGetPkg = (Get-Item src\bin\Release\*.nupkg).FullName

    Publish-NuGetPackage -NuGetPkg $NuGetPkg (Get-FeedUrl 'dayforce') -ApiKey $TfsFeedApiKey
}