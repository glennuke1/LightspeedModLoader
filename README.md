# Lightspeed Mod Loader

Lightspeed Mod Loader (or LML) is a new mod loader for My Summer Car built from scratch

LML is built with optimizations and compatability in mind for low end hardware users

Note: LML is an early beta currently made for personal use (because i like using my own stuff so i can modify it to my needs), but feel free to use it if you'd like

# Wiki

[Lightspeed Mod Loader wiki](https://github.com/glennuke1/LightspeedModLoader/wiki)

# Installation

1. Download and run the [installer](https://github.com/glennuke1/LightspeedModLoader/raw/refs/heads/master/LML_Installer/LML_Installer.exe)
2. Select your MSC Executable file
3. Click install

There is an option to install official mods such as the console and mod list menu, although these are not required to use LML by itself, some mods will still require them. This option is on by default

# MSCLoader compatability

LML is compatible with most, if not all MSCLoader mods and is constantly improving in compatability and performance

LML comes installed with a modified version of MSCLoader.dll for loading MSCLoader mods and redirecting methods called by mods to LML

As of now MSCLoader mods' settings need to be modified through their respective files e.g. mods/Config/somemscloadermod/settings.json

If you can't figure out how to do that, you can either stop using LML and/or wait for an update

# Mod Loader Pro compatability

Mod Loader Pro is abandoned and mods made for it are ***NOT*** supported by LML

Users using Mod Loader Pro will not recieve any support

# Is LML a virus?

LML is FOSS (Free open source software)

It is written in C#, which means you can freely decompile it and look at all of the code (including the official mods and the installer)

Even though the mod loader source code is available on github, you can still look through the code of the installer and official mods using [DnSpy](https://github.com/dnSpy/dnSpy)

I have ***NEVER*** written anything with malicious intent and posted it online to purposfully cause harm
