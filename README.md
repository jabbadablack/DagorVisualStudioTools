# Dagor Visual Studio Tools — personal fork

Fork of GaijinEntertainment/DagorVisualStudioTools for
[jabbadablack/DagorEngine](https://github.com/jabbadablack/DagorEngine).

## New game projects

Install `VisualDagor/bin/Release/net472/Gaijin.VisualDagor.vsix`, restart Visual
Studio, then choose **Tools > New Dagor Game...**. Select your engine checkout,
game name, project location and template:

- **Single-player**: a native Dagor window, game loop and rendering entry points.
- **Multiplayer**: the same base plus an ENet host/client handshake and connection
  feedback, ready for game-specific simulation and replication.

The engine checkout must contain `templates/singleplayer`, `templates/multiplayer`
and `prog/_jBuild/msbuild/create_game.py`. Run its Visual Studio setup script first
to deploy the MSBuild tasks. Existing directories are never overwritten.

New projects reference the engine in place and include a Jamfile, CMake workspace,
Visual Studio solution, source and runtime settings. They are created at the
chosen location, not in an engine `games` directory. Jam currently requires a
Windows project and engine to share a drive. Python and Jam come from `GDEVTOOL`
or a `devtools` directory beside the engine.

If the engine solution is open, the wizard adds the project and sets it as the
startup project; otherwise it opens the new standalone solution. Choose dev | x64,
build the project and press F5. Generated CMake files support Rider 2026.1+ and
CLion using the engine's portable Jam workspace on Windows, Linux and macOS.

## Build

Use Visual Studio's full-framework MSBuild with the .NET Framework 4.7.2 targeting
pack. VSIX build tooling is restored through NuGet:

```powershell
& 'C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe' .\DagorVisualStudioTools.sln /restore /p:Configuration=Release /p:DeployExtension=false
```

The VSIX supports 64-bit Visual Studio 2022 and 2026. The task library is built
separately under `MSBuildUtilities/bin/Release/net472`; deploy its dependency DLLs
with it. Engine setup handles deployment.

## Engine IntelliSense

Visual Dagor recognizes the `DagorProject` and `DagorSharedProject` capabilities
and the `SharedProject` property rule. It transfers startup-project compiler
settings to the common engine source browser. The fork's engine integration
provides the required rules, generated file lists and include settings.

The Visual Studio extension itself runs on Windows. Other IDEs use the generated
CMake project or `prog/_jBuild/CMakeLists.txt` directly; they do not need the VSIX.

## Editor and free camera in both templates

Both templates include the shared starter scene editor: F2 switches the ImGui
inspector; F10 switches free camera. Use WASD, E/C, mouse and Shift; hold RMB over
the editor viewport to fly. The inspector supports selection, object creation
and deletion, transform edits, and scene save/load. The normal debug/dev executable includes these modes; F2/F10 switches them
at runtime without separate targets or rebuilds. Release omits the starter
development tools. The inspector uses the engine's testGI shader bundle, which must be
installed/built with the engine's sample shaders.

The portable workspace and its presets live under `prog/_jBuild` in the engine.
Generated projects have their own CMake entry point and engine references.
