Suppose you wish to automate from Powershell the Visual Studio instance where the given solution is open. This small tool makes it easy.

For the sake of this example, let us assume that `$VSInstanceFinder` contains the path to RunningVSInstanceFinder.dll and `$SlnFilePath` is the file path of the solution in question.

In this particular example:
```powershell
C:\> $VSInstanceFinder
C:\Users\mkharitonov\AppData\Local\PackageManagement\NuGet\Packages\RunningVSInstanceFinder.1.0.20278.6-shelve-828342\tools\RunningVSInstanceFinder.dll
C:\> $SolFilePath
C:\Dayforce\DevOps\tfstool\tfstool.sln
C:\>   
```

Then to obtain the `DTE` object is easy:
```powershell
Add-Type -Path $VSInstanceFinder
$dte = [Ceridian.RunningVSInstanceFinder]::Find($SolFilePath)
```

It is also possible to obtain the `DTE` object for a "vacant" VS instance that has no solution open. Just pass `$null` as the solution path:
```powershell
$dte = [Ceridian.RunningVSInstanceFinder]::Find($null)
```

Notes:
 - The code is current not available on NuGet, so to use it you need to clone the repo and build it with `dotnet build`.
 - Bear in mind that trying to automate a VS instance with an open modal dialog results in a `COMException`.