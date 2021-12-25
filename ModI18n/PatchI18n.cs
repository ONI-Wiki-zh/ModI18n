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

        public static void LoadRemoteStrings(string code, bool localOnly)
        {
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
                Dictionary<string, string> translated_strings = LoadStringsFile(path, false);
                OverloadStrings(translated_strings);

                // used to translate mod options created by PLib
                foreach (KeyValuePair<string, string> e in translated_strings)
                {
                    Strings.Add(e.Key, e.Value);
                }

                Debug.Log($"[ModI18n] Localization file loaded: {path}");
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

            I18nOptions options = POptions.ReadSettings<I18nOptions>() ?? new I18nOptions();
            Utils.LoadRemoteStrings(LangAttribute.GetAttr(options.PreferedLanguage).code, options.LocalOnly);
        }
    }

    public class Patches
    {
        [HarmonyPatch(typeof(Db))]
        [HarmonyPatch("Initialize")]
        [HarmonyPriority(int.MinValue)] // execuate last
        public class Db_Initialize_Patch
        {
            // this is where PLib patches
            public static void Postfix()
            {
                Utils.loadStrings();
            }
        }
    }
    public class I18nUserMod : KMod.UserMod2
    {
        // patch things after all other mods loaded
        public override void OnLoad(Harmony harmony)
        {
            base.OnLoad(harmony);
            PUtil.InitLibrary(false);
            new PLocalization().Register();
            new POptions().RegisterOptions(this, typeof(I18nOptions));
        }
    }
}
