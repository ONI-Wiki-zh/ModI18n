# ModI18n
一个翻译模组的模组。这个模组会在启动时下载翻译文件从而保证您始终能获取最新的可用翻译。

本仓库只含有模组代码。如您想要帮助翻译其他模组，请到[ONIi18n](https://github.com/ONI-Wiki-zh/ONIi18n)。

其他语言版本：[English](https://github.com/ONI-Wiki-zh/ModI18n)

# 安装
您可以直接在 steam 创意工坊[订阅本模组](https://steamcommunity.com/sharedfiles/filedetails/?id=2692663069)。

若要本地安装，请到 [release 页面](https://github.com/ONI-Wiki-zh/ModI18n/releases)下载最新的 zip 文件，解压并放置在您的本地模组目录下。请注意，本模组仍需要在使用时保持网络连接（但只需要与 jsDelivr 的连接而不是与 steam 社区的连接）以下载翻译文件。
- 本地模组目录（如果 Local 文件夹不存在请手动创建）
  - Windows: `%USERPROFILE%\Documents\Klei\OxygenNotIncluded\mods\Local`
  - Linux: `~/.config/unity3d/Klei/OxygenNotIncluded/mods/Local`
  - Mac: `~/Library/Application Support/unity.Klei.Oxygen Not Included/mods/Local`


# 用法
1. 如果您使用的是官方支持的语言中的一种（包括中文），ModI18n 会自动为您的模组加载合适的语言。你可以在游戏内的本模组的设置选项中改变你的语言偏好。改动后，请重启游戏以加载新的翻译文件。
2. 默认情况下本模组会在每次启动时为您下载最新的翻译文件。要阻止这种行为（例如在测试您自己的翻译），请在游戏内设置中启用“仅使用本地翻译”。
3. 本模组会尝试覆盖其他模组的翻译。如果您发现它没有成功，可以试试改变模组的先后顺序，使本模组位于列表最后。

# 构建
如果你想试着从源代码构建模组，请参阅 [Cairath 的缺氧模组教程](https://github.com/Cairath/Oxygen-Not-Included-Modding/wiki)以获取基本的模组编程知识。
