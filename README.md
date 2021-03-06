# ModI18n
ModI18n is a mod for mod localization. It fetches translation files when starting to ensure you have the latest translation available.

I have the idea of this project when I tried to translate a mod. I noticed that the author of my target mod is inactive: the last steam workshop comment from its author was months ago and its corresponding GitHub repo had open issues. That really discouraged from translating that mod. I also notice that some gamers are sharing the dll files of mods with their translations because they thought it was hard to contact the original modders. Translating mods in this way not only requires the codding skills of the translator, but also is risky in terms of copyright... So I have the idea of a translation platform and discussed it in the Discord server with other modders, where Aki and Romen added that this mod will also prevent problems of updated mods on steam. Many implementation details are also discussed there. Finally it comes out and I hope this project can help both modders and non-English gamers (it can also help English gamers if the mod is natively non-English).  

This repository is for the mod code itself. To contribute translations managed by this mod, please go to [ONIi18n](https://github.com/ONI-Wiki-zh/ONIi18n) there translated texts are hosted.

*Read this in other languages: [简体中文](README.zh-hans.md)*

# Install
You can [subscribe this mod](https://steamcommunity.com/sharedfiles/filedetails/?id=2692663069) in the steam workshop。

To install locally, please download the latest zip file on the [release page](https://github.com/ONI-Wiki-zh/ModI18n/releases). Then extract mod files inside and put them under your local mod directory. Note that a network connection is always required to fetch the newest translation files.
- Local mod directory (Create the `Local` folder if it does not exist)
  - Windows: `%USERPROFILE%\Documents\Klei\OxygenNotIncluded\mods\Local`
  - Linux: `~/.config/unity3d/Klei/OxygenNotIncluded/mods/Local`
  - Mac: `~/Library/Application Support/unity.Klei.Oxygen Not Included/mods/Local`

# Useage
1. If your language is one of the officially supported language, ModI18n should be able to detect your language and load translations for you. The language that ModI18n loads can be configured in the in-game mod options. If you change it, please restart the game to load new translations.
2. ModI18n will download latest translations files for you. The translation files are downloaded to `mods/i18n`. To suppress this behavior (e.g. for testing your own translations), you can set "Use Local Translations Only" to true in the in-game mod options.
3. ModI18n should try to override translations loaded by other mods. If it fails (which is unlikely), please make an issue.

# For Modders
Mod Translation is designed for modders and translators and helps mods to be international easily and timely. It worked by downloading translations from a Github repo and overriding game strings with them, which means your mods don't need to update and force everyone to restart their game for a new language to appear, and Steam failing to update is not an issue either. Similarly, it also makes the translator's life easier when adding or updating mod text in terms of updating translations accordingly. Modders are not expected to change anything in their code for basic support from Mod Translation nor do they expect to keep their code stable. With that said, adding localization support to your mods will still help to make your mod more translatable, as the string templates generated by the original mods are more complete and accurate than those generated by Mod Translations.

Mod Translation only provides translation strings. Therefore, it will never be able to steal your work as users must subscribe to the original mod (or manually install it locally). Besides, both the Mod Translation itself and the [repository that holds translations](https://github.com/ONI-Wiki-zh/ONIi18n) are with MIT license and are free for everyone to use and contribute. 

However, if you do want to prevent Mod Translation from translating your mods, you can opt-out and list your name here, so Mod Translation won't provide translation for your mods:
* Cairath

# Build
Please check Klei's forum for base mod development knowledge if you want to build this project on your own.
