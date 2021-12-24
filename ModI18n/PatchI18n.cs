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
        public static void LoadRemoteStrings(string code)
        {
            string modsDir = KMod.Manager.GetDirectory();
            string stringsFolder = System.IO.Path.Combine(modsDir, "i18n");
            System.IO.Directory.CreateDirectory(stringsFolder);
            string filename = $"{code}.po";
            string path = System.IO.Path.Combine(stringsFolder, filename);

            try
            {
                using (var client = new System.Net.WebClient())
                    client.DownloadFile(i18nBaseUrl + filename, path);
                OverloadStrings(LoadStringsFile(path, false));
                Debug.Log($"[ModI18n] loaded strings: {filename}");
            }
            catch (System.Net.WebException)
            {
                Debug.LogWarning("[ModI18n]Failed to fatch locolization file from: " + i18nBaseUrl + filename);
            }
        }
    }



    public class PatchI18n
    {
        [HarmonyPatch(typeof(Db))]
        [HarmonyPatch("Initialize")]
        public class Db_Initialize_Patch
        {
            public static void Prefix()
            {
                Debug.Log("[ModI18n] I execute before Db.Initialize!");
            }

            public static void Postfix()
            {
                Debug.Log("[ModI18n] I execute after Db.Initialize!");
            }
        }
    }
    public class LocalizationPatch
    {
        [HarmonyPatch(typeof(Localization), "Initialize")]
        public class Localization_Initialize_Patch
        {
            public static void Postfix()
            {
                string output_folder = System.IO.Path.Combine(KMod.Manager.GetDirectory(), "strings_templates");
                GenerateStringsTemplate(typeof(STRINGS), output_folder);

                I18nOptions options = POptions.ReadSettings<I18nOptions>() ?? new I18nOptions();
                Utils.LoadRemoteStrings(LangAttribute.GetAttr(options.PreferedLanguage).code);
            }
        }
    }
    public class I18nUserMod : KMod.UserMod2
    {
        // patch things after all other mods loaded
        public override void OnLoad(Harmony harmony)
        {
            PUtil.InitLibrary(false);
            new PLocalization().Register();
            new POptions().RegisterOptions(this, typeof(I18nOptions));
        }

        public override void OnAllModsLoaded(Harmony harmony, System.Collections.Generic.IReadOnlyList<KMod.Mod> mods)
        {
            foreach (var mod in mods)
                Debug.Log($"[ModI18n] found mod: {mod.title}");

            // let the game patch everything after all other mods loaded
            base.OnLoad(harmony);
        }
    }
}
