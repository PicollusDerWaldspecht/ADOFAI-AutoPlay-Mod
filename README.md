# ADOFAI AutoPlay Mod

A BepInEx plugin that lets you toggle autoplay in *A Dance of Fire and
Ice* with a hotkey. Works in every level.

## Installation

1. Install [BepInEx 5.x (x64)](https://github.com/BepInEx/BepInEx/releases)
   into your ADOFAI folder and run the game once so it creates its
   folder structure.
2. Get the mod DLL either option works:
   - **Download:** grab `AdoFai-AutoPlay-Mod.dll` from the
     [Releases page](../../releases).
   - **Build yourself:** see [Building](#building) below. The build should
     drop the DLL into the plugins folder automatically.
3. If you downloaded the DLL, put it into
   `…/A Dance of Fire and Ice/BepInEx/plugins/`.
4. Launch the game.

## Usage

Press **F8** to toggle autoplay on or off. That's it.

## Configuration

On first launch you'll find `BepInEx/config/adofai.autoplay.cfg`:

| Option             | Default | What it does                                                           |
| ------------------ | ------- | ---------------------------------------------------------------------- |
| `ToggleKey`        | `F8`    | Key that toggles autoplay. Any Unity `KeyCode` name works.             |
| `EnabledAtStart`   | `false` | Used **only when `RememberState` is `false`**. `false` = always start with autoplay OFF, `true` = always start ON. |
| `RememberState`    | `false` | `true` = on startup, resume whatever state autoplay was in when you last quit. Overrides `EnabledAtStart`. |
| `Enabled`          | `false` | Storage slot for `RememberState`, the mod writes your current state here every time you toggle. Only read on startup when `RememberState = true`. You don't need to edit this by hand! |
| `HideAutoplayText` | `true`  | Hides the green "autoplay" label in the top-left while active.         |

## Building

### Requirements

- **.NET SDK 6.0 or newer** ([download here](https://dotnet.microsoft.com/download))
- **ADOFAI installed locally.** The build references DLLs from the
  game's `Managed/` folder, so the game has to be on the same machine.
  The default Steam path is used automatically, override it with the
  `ADOFAI_PATH` environment variable if yours is somewhere else.
- You do **not** need Mono or .NET Framework installed. The project
  targets `net472` via the `Microsoft.NETFramework.ReferenceAssemblies`
  NuGet package, which ships the needed reference DLLs.

### Build

```powershell
# Optional: only needed if ADOFAI isn't at the default Steam path
$env:ADOFAI_PATH = "D:\Games\A Dance of Fire and Ice"

dotnet build -c Release
```

The post-build step automatically copies the DLL into
`<ADOFAI_PATH>/BepInEx/plugins/ADOFAI-AutoPlay-Mod/`. Just restart the
game afterwards.

## Compatibility

Tested with ADOFAI (Mono build) and BepInEx 5.4.x.
