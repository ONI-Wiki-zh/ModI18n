﻿using Newtonsoft.Json;
using PeterHan.PLib.Options;
using static Localization;

namespace ModI18n
{
    [JsonObject(MemberSerialization.OptIn)]
    [ConfigFile(IndentOutput: true)]
    [RestartRequired()]
    [ModInfo("https://github.com/ONI-Wiki-zh/ModI18n", "preview.png")]
    class I18nOptions
    {
        [Option("ModI18n.STRINGS.OPTIONS.LANG.NAME", "ModI18n.STRINGS.OPTIONS.LANG.DESC")]
        [JsonProperty]
        public LANG PreferedLanguage { get; set; } = LANG.en;

        [Option("ModI18n.STRINGS.OPTIONS.LOCAL_ONLY.NAME", "ModI18n.STRINGS.OPTIONS.LOCAL_ONLY.DESC")]
        [JsonProperty]
        public bool LocalOnly { get; set; } = false;

        public I18nOptions()
        {
            char[] sep = { '-', '_' };
            string[] currLocaleCode = GetLocale()?.Code?.Split(sep) ?? new string[1] { "en" };
            switch (currLocaleCode[0])
            {
                case "ko":
                    this.PreferedLanguage = LANG.ko;
                    break;
                case "ru":
                    this.PreferedLanguage = LANG.ru;
                    break;
                case "zh":
                    this.PreferedLanguage = LANG.zh_hans;
                    break;
                case "fr":
                    this.PreferedLanguage = LANG.fr;
                    break;
                case "jp":
                    this.PreferedLanguage = LANG.jp;
                    break;
                default:
                    break;
            }
        }
    }
}
