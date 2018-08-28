# VRCModLoader
A VRChat mod loader based on [Illusion Plugin Architecture](https://github.com/Eusth/IPA)

Installation
---
Before install:
- **Tupper (from VRChat Team) said that any modification of the game can lead to a ban, as with this mod**
- You need to know that this mod has not been validated by the VRChat team, but they don't seems to care if someone use it

You can install it by downloading the installer on [the VRCTools website](https://vrchat.survival-machines.fr/) and running it using Java

You can also install it manually by putting [this UnityEngine.dll] and the built VRCModLoader.dll files to the folder `<VRChat_Install_Folder>/VRChat_Data/Managed/`.

How does it works
---
This file is loaded by the modified UnityEngine.dll assembly.<br>
On start, it will first install/update the [VRCTools mod](https://github.com/Slaynash/VRCTools) (It's a required mod for having all VRCModLoader features), copy all files located in `<VRChat_Install_Folder>/Mods` to `<VRChat_Install_Folder>/Mods_tmp`, and then load add the mods located in the directory `<VRChat_Install_Folder>/Mods_tmp`.

How to create a mod
---
To create a mod, you will need to create a new C# library, and reference the VRCModLoader.dll file.<br>
A basic mod main class is made like this:
```csharp
using VRCModLoader;

//VRCModInfo(name, version, authorname [, downloadurl])
[VRCModInfo("TestMod", "1.0", "Slaynash")]
public class TestMod : VRCMod
{
    // All the following methods are optional
    // They also works like Unity's magic methods
    void OnApplicationStart() { }
    void OnApplicationQuit() { }
    void OnLevelWasLoaded(int level) { }
    void OnLevelWasInitialized(int level) { }
    void OnUpdate() { }
    void OnFixedUpdate() { }
    void OnLateUpdate() { }
}
```
You can also reference VRCTools to use the VRCTools utils (VRCUiManagerUtils, ...) and/or use the VRCModNetwork<br>
(Please ask Slaynash on the VRCTools discord to register your packets first or they will be refused by the server)

Disclaimer:
---
'I' stand for Hugo Flores (Slaynash).

I am not affiliated with VRChat.
This content is for entertainment purpose only, and is provided "AS IS".
I am not responsible of any legal prejudice against VRChat, the VRChat team, VRChat community or legals prejudice done with an edited version of this code.

Want more infos or some help ?
---
You can [join the VRCTools discord server](https://discord.gg/E6tSYff) if you need some help, want to know when an update is released, the status of the known bugs, the upcoming features, or simply talk with others !
