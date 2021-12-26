using System.Collections.Generic;
using HarmonyLib;
using PeterHan.PLib.Core;
using PeterHan.PLib.Database;
using PeterHan.PLib.Options;
using static Localization;

namespace ModI18n
{
    public class Utils
    {
        public static string i18nBaseUrl = "https://cdn.jsdelivr.net/gh/ONI-Wiki-zh/ONIi18n@v1/dist/";
        public static string modsDir = KMod.Manager.GetDirectory();
        public static string stringsFolder = System.IO.Path.Combine(modsDir, "i18n");
        public static int numToLast = 2;
        public static Dictionary<string, string> translations = null;

        // Download and read translation file
        public static void InitTranslations()
        {
            if (translations != null) { Debug.Log("[ModI18n] Translation is already initiated."); return; }

            I18nOptions options = POptions.ReadSettings<I18nOptions>() ?? new I18nOptions();
            string code = LangAttribute.GetAttr(options.PreferedLanguage).code;
            bool localOnly = options.LocalOnly;

            System.IO.Directory.CreateDirectory(stringsFolder);
            string filename = $"{code}.po";
            string path = System.IO.Path.Combine(stringsFolder, filename);
            if (localOnly) { Debug.Log("[ModI18n] LocalOnly set to true"); }
            else
                try
                {
                    using (var client = new System.Net.WebClient())
                        client.DownloadFile(i18nBaseUrl + filename, path);
                    Debug.Log($"[ModI18n] downloaded strings: {filename}");
                }
                catch (System.Net.WebException)
                {
                    Debug.LogWarning($"[ModI18n]Failed to fatch locolization file from: {i18nBaseUrl}{filename}");
                }

            try
            {
                translations = LoadStringsFile(path, false);
                Debug.Log($"[ModI18n] Translation init successfully: {path}");
            }
            catch (System.IO.FileNotFoundException)
            {
                Debug.LogWarning($"[ModI18n] Failed to load locolization file: {filename}");
            }
        }

        public static void loadStrings()
        {
            string output_folder = System.IO.Path.Combine(KMod.Manager.GetDirectory(), "strings_templates");
            GenerateStringsTemplate(typeof(STRINGS), output_folder);
            RegisterForTranslation(typeof(STRINGS));

            InitTranslations();
            OverloadStrings(translations);

            // used to translate mod options created by PLib
            foreach (KeyValuePair<string, string> e in translations)
            {
                Debug.Log($"[ModI18n] String.add {e.Key} {e.Value}");
                Strings.Add(e.Key, e.Value);
            }
        }
    }
    public class Patches
    {
        // Seems that LegacyModMain.Load is execuated later
        [HarmonyPatch(typeof(LegacyModMain), "Load")]
        //[HarmonyPatch(typeof(Db), "Initialize")]
        [HarmonyPriority(int.MinValue)] // execuate last
        public class LegacyModMain_Patch
        {
            public static void Postfix() { Utils.loadStrings(); }
        }

        // Download and read translation file
        [HarmonyPatch(typeof(Localization), "Initialize")]
        [HarmonyPriority(int.MaxValue)] // execuate first
        public class LocalizationInitializePatch
        {
            public static void Postfix() { Utils.InitTranslations(); }
        }

        // It seems that LocString.CreateLocStringKeys won't work if no translation is loaded
        // As it is usually called after RegisterForTranslation, I add a patch here
        [HarmonyPatch(typeof(Localization), "RegisterForTranslation")]
        [HarmonyPriority(int.MinValue)] // execuate at last
        public class LocalizationRegisterForTranslationPatch
        {
            public static void Postfix()
            {
                if (Utils.translations != null)
                    OverloadStrings(Utils.translations);
            }
        }

        // Override all OverloadStrings which other mods could used to load localization
        [HarmonyPatch(typeof(Localization), "OverloadStrings")]
        [HarmonyPatch(new System.Type[] { typeof(Dictionary<string, string>) })]
        [HarmonyPriority(int.MinValue)] // execuate last
        public class OverloadStringsPatch
        {
            public static void Prefix(ref Dictionary<string, string> translated_strings)
            {
                if (Utils.translations == null) { Debug.Log($"[ModI18n] Utils.translations not loaded"); return; }
                if (Utils.translations == translated_strings) return; // don't change calls of myself
                foreach (var kv in Utils.translations)
                    translated_strings[kv.Key] = kv.Value;
            }
        }
    }
    public class I18nUserMod : KMod.UserMod2
    {
        public override void OnLoad(Harmony harmony)
        {
            base.OnLoad(harmony);
            PUtil.InitLibrary(false);
            new PLocalization().Register();
            new POptions().RegisterOptions(this, typeof(I18nOptions));
        }
    }
}
