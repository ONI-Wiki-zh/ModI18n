using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;
using PeterHan.PLib.Options;

using KMod;
using static Localization;

namespace ModI18n
{
    [JsonObject(MemberSerialization.OptIn)]
    [ModInfo("https://github.com/ONI-Wiki-zh/ONIi18n")]
    class I18nOptions
    {
        [Option("ModI18n.STRINGS.OPTIONS.LANG.NAME", "ModI18n.STRINGS.OPTIONS.LANG.DESC")]
        [JsonProperty]
        public LANG PreferedLanguage { get; set; } = LANG.en;

        public I18nOptions()
        {
            char[] sep = { '-', '_' };
            string[] currLocaleCode = GetLocale().Code.Split(sep);
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
                default:
                    break;
            }
        }
    }
}
