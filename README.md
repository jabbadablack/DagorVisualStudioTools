# Dagor Visual Studio Tools

Tools for building and using [DagorEngine](https://github.com/GaijinEntertainment/DagorEngine) with Visual Studio

## Visual Dagor

A Visual Studio Extension to transform the [prog project](https://github.com/GaijinEntertainment/DagorEngine/blob/main/prog/prog.vcxproj) into a [shared project](https://learn.microsoft.com/en-us/xamarin/cross-platform/app-fundamentals/shared-projects?tabs=windows#what-is-a-shared-project) and set the preprocessor parameters of the common engine according to the Startup Project.
Later other useful Dagor Engine specific features will be added to improve the productivy with Visual Studio

## Gaijin MSBuild Utilities

It is a [library](https://github.com/GaijinEntertainment/DagorEngine/blob/main/prog/_jBuild/msbuild/Gaijin.MSBuild.Utilities.dll) containing custom [MSBuild Tasks](https://learn.microsoft.com/en-us/visualstudio/msbuild/task-writing) which are used by the [DagorEngine's Visual Studio Solution](https://github.com/GaijinEntertainment/DagorEngine/blob/main/DagorEngine.sln) to build and provide basic intellisense functionalities.

## Building on Windows

Build with Visual Studio's full-framework MSBuild (not `dotnet build`). Install
Visual Studio 2022 or Visual Studio 2026 with the .NET Framework 4.7.2 targeting
pack and C++ development tools. VSIX build tools are restored through NuGet; the
Visual Studio extension development workload is not required just to build.

For Visual Studio 2026 Community, run in PowerShell:

```powershell
& 'C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe' .\DagorVisualStudioTools.sln /restore /p:Configuration=Release
```

Close Visual Studio and open
`VisualDagor\bin\Release\net472\Gaijin.VisualDagor.vsix` to install the extension.
The installer targets 64-bit Visual Studio versions 17 and 18.

The MSBuild tasks are built separately at
`MSBuildUtilities\bin\Release\net472\Gaijin.MSBuild.Utilities.dll`. When updating
an engine checkout's existing MSBuild integration, deploy the task DLL and its
runtime dependency DLLs from that output directory together. MSBuild assemblies
are supplied by the Visual Studio host and are not deployment dependencies.

### Engine checkout prerequisites

The extension relies on the engine's MSBuild integration defining the
`DagorProject` and `DagorSharedProject` capabilities and the `SharedProject` rule.
It does not generate that integration. Engine projects importing
`prog\_jBuild\msbuild\projectConfigurations.props` require that directory and its
supporting props/targets files from a matching engine revision.

The local `C:\Users\cisco\develop\DagorEngine` checkout inspected for this fix
contains sample projects with those imports but no `prog\_jBuild\msbuild`
directory or `prog\prog.vcxproj`. Those engine-side files must be restored or
provided before shared-project IntelliSense can be verified in that checkout.
