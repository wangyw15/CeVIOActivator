# CeVIOActivator

Activate all versions of CeVIO AI

# Usage

0. Install CeVIO Editor and at least one voicebank
1. Download the latest version from [Releases](https://github.com/wangyw15/CeVIOActivator/releases/latest)
2. Unzip the file (to anywhere you want)
3. Use the executable `CeVIOActivator.Loader.exe` to start CeVIO Editor (**everytime**)

The patch should be always effective for any version.

## Installation

> No need to open the loader everytime

1. Copy all the files of the patch to the installation folder of the CeVIO Editor
   - For example: `C:\Program Files\CeVIO\CeVIO AI`
2. Rename `CeVIO AI.exe` to `CeVIO AI.orig.exe`
3. **Copy** `CeVIO AI.exe.config` to `CeVIO AI.orig.exe.config`
4. Rename `CeVIOActivator.Loader.exe` to `CeVIO AI.exe`
5. Open the editor as normal (shortcut, double click with *.ccs file, etc.)

The structure should be like this
```
C:\Program Files\CeVIO\CeVIO AI
|  0Harmony.dll
|  CeVIO AI.exe
|  CeVIO AI.exe.config
|  CeVIO AI.orig.exe
|  CeVIO AI.orig.exe.config
|  CeVIOActivator.Patcher.dll
|  FASMX64.DLL
|  ...
```

# Development

> draft

Compile `CeVIOActivator.Patcher` first (if not successful, download [DllExport](https://github.com/3F/DllExport) and try again)

Run `CeVIOActivator.Loader`

I use `StreamWriter` for debug :)

---

- [x] Export wav
- [x] Export wav when disconnected from network
- [x] No need to re-activate annually (change `CeVIO.Editor.MissionAssistant.ProductLicense.OfflineAcceptablePeriod`)
- [x] Remove the warning
- [x] Without re-running the patch after installing new voicebanks
- [ ] Add support for all CS version
  - [x] CS7 tested
  - [ ] CS6 tested
  - [ ] CSS tested
  - [ ] CS tested
