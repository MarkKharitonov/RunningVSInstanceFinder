# Usage
Suppose you wish to automate from Powershell the Visual Studio instance where the given solution is open. This small tool makes it easy.

Build the tool and load the Ceridian.RunningVSInstanceFinder type into the Powershell memory:
```powershell
PS C:\Dayforce\DevOps\RunningVSInstanceFinder> dotnet build -v:q -nologo

Build succeeded.
    0 Warning(s)
    0 Error(s)

Time Elapsed 00:00:01.05
PS C:\Dayforce\DevOps\RunningVSInstanceFinder> Add-Type -Path C:\dayforce\DevOps\RunningVSInstanceFinder\RunningVSInstanceFinder\bin\Debug\net472\RunningVSInstanceFinder.dll > $null
PS C:\Dayforce\DevOps\RunningVSInstanceFinder>
```

Obtain the `DTE2` object:
```powershell
PS C:\Dayforce\DevOps\RunningVSInstanceFinder> $res = [Ceridian.RunningVSInstanceFinder]::Find("C:\dayforce\CSTool\CSTool.sln")
PS C:\Dayforce\DevOps\RunningVSInstanceFinder> $res

DTE                SolutionFullName              ErrorMessage
---                ----------------              ------------
System.__ComObject C:\dayforce\CSTool\CSTool.sln


PS C:\Dayforce\DevOps\RunningVSInstanceFinder>
```

Now let us build inside the respective VS instance and get back the build output pane text: 
```powershell
PS C:\Dayforce\DevOps\RunningVSInstanceFinder> $res.DTE.Solution.SolutionBuild.Build()
PS C:\Dayforce\DevOps\RunningVSInstanceFinder> $text = [Ceridian.RunningVSInstanceFinder]::GetOutputPaneText($res.DTE, "Build")
PS C:\Dayforce\DevOps\RunningVSInstanceFinder> $text.Length
6384
PS C:\Dayforce\DevOps\RunningVSInstanceFinder> $text -split "`r`n" | Select-Object -Last 2
========== Build: 0 succeeded, 0 failed, 3 up-to-date, 0 skipped ==========

PS C:\Dayforce\DevOps\RunningVSInstanceFinder>
```

Now let us check the Error List pane:
```powershell
PS C:\Dayforce\DevOps\RunningVSInstanceFinder> [Ceridian.RunningVSInstanceFinder]::GetErrorList($res.DTE)
PS C:\Dayforce\DevOps\RunningVSInstanceFinder>
```

I am going to botch the code on purpose, build and get the build output and the error list again:
```powershell
PS C:\Dayforce\DevOps\RunningVSInstanceFinder> echo "xyz" | Out-File -Encoding ascii -Append ..\..\CSTool\CSTool\ClassifyCmd.cs
PS C:\Dayforce\DevOps\RunningVSInstanceFinder> $res.DTE.Solution.SolutionBuild.Build()
PS C:\Dayforce\DevOps\RunningVSInstanceFinder> [Ceridian.RunningVSInstanceFinder]::GetOutputPaneText($res.DTE, "Build") -split "`r`n" | Select-Object -Last 2
========== Build: 0 succeeded, 1 failed, 2 up-to-date, 0 skipped ==========

PS C:\Dayforce\DevOps\RunningVSInstanceFinder> [Ceridian.RunningVSInstanceFinder]::GetErrorList($res.DTE)


ErrorLevel  : High
Description : A namespace cannot directly contain members such as fields, methods or statements
FileName    : C:\dayforce\CSTool\CSTool\ClassifyCmd.cs
Line        : 394
Column      : 1
Project     : CSTool\CSTool.csproj



PS C:\Dayforce\DevOps\RunningVSInstanceFinder>
```

It is also possible to obtain the `DTE2` object for a "vacant" VS instance that has no solution open. Just pass `$null` as the solution path:
```powershell
PS C:\Dayforce\DevOps\RunningVSInstanceFinder> $res.DTE.Solution.Close()
PS C:\Dayforce\DevOps\RunningVSInstanceFinder> $res2 = [Ceridian.RunningVSInstanceFinder]::Find($null)
PS C:\Dayforce\DevOps\RunningVSInstanceFinder> $res2

DTE                SolutionFullName ErrorMessage
---                ---------------- ------------
System.__ComObject


PS C:\Dayforce\DevOps\RunningVSInstanceFinder> [Object]::ReferenceEquals($res2.DTE, $res.DTE)
True
PS C:\Dayforce\DevOps\RunningVSInstanceFinder>
```

Finally it is possible to obtain the `DTE2` for multiple VS instances at once:
```powershell
PS C:\Dayforce\DevOps\RunningVSInstanceFinder> $res.DTE.Solution.Open("C:\dayforce\CSTool\CSTool.sln")
PS C:\Dayforce\DevOps\RunningVSInstanceFinder> $res2 = [Ceridian.RunningVSInstanceFinder]::FindMany([string[]]@("C:\dayforce\CSTool\CSTool.sln", "C:\dayforce\DevOps\RunningVSInstanceFinder\RunningVSInstanceFinder.sln"))
PS C:\Dayforce\DevOps\RunningVSInstanceFinder> $res2|Format-List


Found         : {[C:\dayforce\CSTool\CSTool.sln, System.Collections.Generic.List`1[Ceridian.RunningVSInstanceFinder+FindResult]],
                [C:\dayforce\DevOps\RunningVSInstanceFinder\RunningVSInstanceFinder.sln,
                System.Collections.Generic.List`1[Ceridian.RunningVSInstanceFinder+FindResult]]}
ErrorMessages : {}



PS C:\Dayforce\DevOps\RunningVSInstanceFinder> $res2.Found

Key                                                                    Value
---                                                                    -----
C:\dayforce\CSTool\CSTool.sln                                          {C:\dayforce\CSTool\CSTool.sln}
C:\dayforce\DevOps\RunningVSInstanceFinder\RunningVSInstanceFinder.sln {C:\dayforce\DevOps\RunningVSInstanceFinder\RunningVSInstanceFinder.sln}


PS C:\Dayforce\DevOps\RunningVSInstanceFinder> $res2.Found['C:\dayforce\CSTool\CSTool.sln']

DTE                SolutionFullName              ErrorMessage
---                ----------------              ------------
System.__ComObject C:\dayforce\CSTool\CSTool.sln


PS C:\Dayforce\DevOps\RunningVSInstanceFinder> $res2.Found['C:\dayforce\DevOps\RunningVSInstanceFinder\RunningVSInstanceFinder.sln']

DTE                SolutionFullName                                                       ErrorMessage
---                ----------------                                                       ------------
System.__ComObject C:\dayforce\DevOps\RunningVSInstanceFinder\RunningVSInstanceFinder.sln


PS C:\Dayforce\DevOps\RunningVSInstanceFinder>
```

# Notes:
 - The code is current not available on NuGet, so to use it you need to clone the repo and build it with `dotnet build`.
 - Bear in mind that trying to automate a VS instance with an open modal dialog (there can be other busy scenarios) results in a `COMException`.
 - My original intention was to just implement the `Find` and `FindMany` methods. However, I failed to obtain the Error List and the Build Output Pane contents, because the `DTE2.ToolWindows` property is `$null` in Powershell. I have asked how to do it [in this StackOverflow question](https://stackoverflow.com/questions/72745483/how-can-i-get-the-contents-of-the-vs-2022-build-output-pane-through-the-given-dt).
