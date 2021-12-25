# ModI18n
ModI18n is a mod for mod localization. It fetches translation files when starting to ensure you have the latest translation available.

This repository is for the mod code itself. To contribute translations managed by this mod, please go to [ONIi18n](https://github.com/ONI-Wiki-zh/ONIi18n).

*Read this in other languages: [简体中文](README.zh-hans.md)*

# Install
You can [substribe this mod](https://steamcommunity.com/sharedfiles/filedetails/?id=2692663069) in the steam workshop。

To install locally, please download the latest zip file on the [release page](https://github.com/ONI-Wiki-zh/ModI18n/releases). Then extract mod files inside and put them under your local mod directory. Note that a network connection is always required to fetch the newest translation files.
- Local mod directory (Create the `Local` folder if it does not exist)
  - Windows: `%USERPROFILE%\Documents\Klei\OxygenNotIncluded\mods\Local`
  - Linux: `~/.config/unity3d/Klei/OxygenNotIncluded/mods/Local`
  - Mac: `~/Library/Application Support/unity.Klei.Oxygen Not Included/mods/Local`

# Useage
1. If your language is one of the officially supported language, ModI18n should be able to detect your language and load translations for you. The language that ModI18n loads can be configured in the in-game mod options. If you change it, please restart the game to load new translations.
2. ModI18n will download latest translations files for you. The translation files are downloaded to `mods/i18n`. To suppress this behavior (e.g. for testing your own translations), you can set "Use Local Translations Only" to true in the in-game mod options.
3. ModI18n should try to override translations loaded by other mods. If it fails (which is unlikely), you can try to reorder your mod list so that this mod is at the end.

# Modder opt-out
The following modders don't want their mods to be translated without thier supervision. Therefore this repo can't provide translation for their mods.
* Cairath

# Build
Please read [Cairath's ONI Modding guide](https://github.com/Cairath/Oxygen-Not-Included-Modding/wiki) for mod development knowledge.
