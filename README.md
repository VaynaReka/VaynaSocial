# VaynaSocial - Live Chat, Mail & Social Networking System

[![Nexus Mods](https://img.shields.io/badge/Nexus%20Mods-Mod%2039-blue.svg)](https://www.nexusmods.com/thislandismyland/mods/39)
[![Target](https://img.shields.io/badge/.NET%20Framework-4.7.2-purple.svg)](https://dotnet.microsoft.com/)
[![Mod Loader](https://img.shields.io/badge/BepInEx-5.4.x-green.svg)](https://github.com/BepInEx/BepInEx)

Source code for **VaynaSocial**, a BepInEx plugin for the game *This Land Is My Land*.

- **Nexus Mods Page:** [https://www.nexusmods.com/thislandismyland/mods/39](https://www.nexusmods.com/thislandismyland/mods/39)
- **Author:** Vayna Reka

---

## 🛠️ Overview

VaynaSocial revitalizes the community and multiplayer aspects of *This Land Is My Land*:
- **Live In-Game Chat:** Interactive chat window (`T` / `Enter` / `F9`) supporting Global, Tribal, and Whisper messaging channels.
- **In-Game Mail System:** Send and receive in-game letters and item barter attachments between player camps.
- **Leaderboards & Events:** Clan leaderboards, social barter, and tribal challenges.
- **Save & SP Protection:** Prevents Skill Points (SP) loss and preserves player profiles across game restarts.
- **Clean Architecture:** Standard BepInEx 5.4.x / HarmonyX patch with no obfuscation, no malicious code, and standard Unity/C# networking.

---

## 🏗️ Build Instructions

### Prerequisites
1. **Windows 10 / 11**
2. **Visual Studio 2019 / 2022** (with *.NET desktop development* workload installed) OR **MSBuild / .NET Framework 4.7.2 Developer Pack**.
3. *This Land Is My Land* game files or BepInEx 5.4.x core libraries:
   - `BepInEx.dll` (from `BepInEx/core/`)
   - `0Harmony.dll` (from `BepInEx/core/`)
   - `Assembly-CSharp.dll` (from `This Land Is My Land_Data/Managed/`)
   - `UnityEngine.dll`, `UnityEngine.CoreModule.dll`, `UnityEngine.UI.dll`
   - `VaynaCore.dll` (from `BepInEx/plugins/`)

### Option 1: Building with Visual Studio (Recommended)
1. Open `VaynaSocial.csproj` (or `VaynaSocial.slnx`) in Visual Studio.
2. In the top toolbar, select **Release** configuration and **Any CPU** platform.
3. From the menu bar, select **Build > Build Solution** (or press `Ctrl + Shift + B`).
4. The compiled plugin binary will be generated at:
   ```
   bin\Release\VaynaSocial.dll
   ```

### Option 2: Building via Command Line (MSBuild)
Run Developer Command Prompt for Visual Studio and execute:
```cmd
msbuild VaynaSocial.csproj /p:Configuration=Release /p:Platform="Any CPU"
```
The compiled assembly will be placed in `bin\Release\VaynaSocial.dll`.

---

## 📦 Installation
Copy `bin\Release\VaynaSocial.dll` into your game's `BepInEx\plugins\` folder:
```
<Game Directory>\BepInEx\plugins\VaynaSocial.dll
```

---

## 📜 License
Open for modding review and personal use. All rights reserved by Vayna Reka.
