using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Cache;
using System.Reflection;
using HarmonyLib;
using PeterHan.PLib.Core;
using PeterHan.PLib.Database;
using PeterHan.PLib.Options;
using static Localization;

namespace ModI18n
{
    public class Utils
    {
        public static readonly string i18nBaseUrl = "https://cdn.jsdelivr.net/gh/ONI-Wiki-zh/ONIi18n@1/dist/";
        public static readonly string modsDir = KMod.Manager.GetDirectory();
        public static readonly string stringsFolder = Path.Combine(modsDir, "i18n");
        public static readonly int maxPrintCount = 3;
        public static Dictionary<string, string> translations = null;

        public static void DownloadFile(string remoteFilename, string localFilename)
        {
            Stream remoteStream = null;
            Stream localStream = null;
            WebResponse response = null;

            try
            {
                WebRequest request = WebRequest.Create(remoteFilename);
                request.CachePolicy = new HttpRequestCachePolicy(HttpRequestCacheLevel.NoCacheNoStore);
                response = request.GetResponse();

                remoteStream = response.GetResponseStream();
                localStream = File.Create(localFilename);
                remoteStream.CopyTo(localStream);
            }
            finally
            {
                if (response != null) response.Close();
                if (remoteStream != null) remoteStream.Close();
                if (localStream != null) localStream.Close();
            }
        }


        // Download and read translation file
        public static void InitTranslations()
        {
            if (translations != null) { Debug.Log("[ModI18n] Translation is already initiated."); return; }

            I18nOptions options = POptions.ReadSettings<I18nOptions>() ?? new I18nOptions();
            string code = LangAttribute.GetAttr(options.PreferedLanguage).code;
            bool localOnly = options.LocalOnly;

            Directory.CreateDirectory(stringsFolder);
            string filename = $"{code}.po";
            string path = Path.Combine(stringsFolder, filename);
            if (localOnly) { Debug.Log("[ModI18n] LocalOnly set to true"); }
            else
                try
                {
                    DownloadFile($"{i18nBaseUrl}{filename}", path);
                    Debug.Log($"[ModI18n] downloaded strings: {filename}");
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"[ModI18n]Failed to fatch locolization file from: {i18nBaseUrl}{filename}");
                    Debug.Log(e.Message);
                }

            try
            {
                translations = LoadStringsFile(path, false);
                Debug.Log($"[ModI18n] Translation init successfully: {path}");
            }
            catch (FileNotFoundException)
            {
                Debug.LogWarning($"[ModI18n] Failed to load locolization file: {filename}");
            }
        }

        public static void LoadStrings()
        {
            string output_folder = System.IO.Path.Combine(KMod.Manager.GetDirectory(), "strings_templates");
            GenerateStringsTemplate(typeof(STRINGS), output_folder);
            RegisterForTranslation(typeof(STRINGS));

            InitTranslations();
            OverloadStrings(translations);

            // used to translate mod options created by PLib
            int printCount = maxPrintCount;
            foreach (KeyValuePair<string, string> e in translations)
            {
                if (printCount > 0)
                    Debug.Log($"[ModI18n] String.add {e.Key} {e.Value}");
                Strings.Add(e.Key, e.Value);
                printCount--;
            }
            if (printCount < 0)
                Debug.Log($"[ModI18n] ... and {-printCount} more String.add ");

        }

        public static object GetField(Type type, object instance, string fieldName)
        {
            BindingFlags bindFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
            FieldInfo field = type.GetField(fieldName, bindFlags);
            return field.GetValue(instance);
        }

        public static List<KeyValuePair<string, string>> CollectStrings()
        {
            StringTable RootTable = (StringTable)GetField(typeof(Strings), null, "RootTable");
            Dictionary<int, string> RootTableKeyNames = (Dictionary<int, string>)GetField(typeof(StringTable), RootTable, "KeyNames");
            Dictionary<int, StringEntry> RootTableEntries = (Dictionary<int, StringEntry>)Utils.GetField(typeof(StringTable), RootTable, "Entries");

            SortedSet<string> keys = new SortedSet<string>();
            foreach (var kv in RootTableKeyNames)
            {
                keys.Add(kv.Value);
            }

            List<KeyValuePair<string, string>> dict = new List<KeyValuePair<string, string>>();
            foreach (var k in keys)
            {
                if (!k.StartsWith("ModI18n.") && !k.StartsWith("PeterHan.PLib.") && !translations.ContainsKey(k))
                    dict.Add(new KeyValuePair<string, string>(k, RootTableEntries[k.GetHashCode()].String));
            }
            return dict;
        }

        public static void GenerateStringsTemplateForAll(string file_path)
        {
            using (StreamWriter streamWriter = new StreamWriter(file_path, append: false,
                new System.Text.UTF8Encoding(encoderShouldEmitUTF8Identifier: false)))
            {
                streamWriter.WriteLine("msgid \"\"");
                streamWriter.WriteLine("msgstr \"\"");
                streamWriter.WriteLine("\"Application: Oxygen Not Included\"");
                streamWriter.WriteLine("\"POT Version: 2.0\"");
                streamWriter.WriteLine("");
                foreach (var kv in CollectStrings())
                {
                    string msgctxt = kv.Key;
                    string msgid = kv.Value.Replace("\"", "\\\"").Replace("\n", "\\n");
                    streamWriter.WriteLine($"#. {msgctxt}");
                    streamWriter.WriteLine($"msgctxt \"{msgctxt}\"");
                    streamWriter.WriteLine($"msgid \"{msgid}\"");
                    streamWriter.WriteLine($"msgstr \"\"");
                    streamWriter.WriteLine("");
                }
            }
        }
    }
    public class Patches
    {
        // Seems that LegacyModMain.Load is execuated later
        [HarmonyPatch(typeof(LegacyModMain), "Load")]
        //[HarmonyPatch(typeof(Db), "Initialize")]
        public class LegacyModMain_Patch
        {
            [HarmonyPriority(int.MinValue)] // execuate last
            public static void Postfix()
            {
                Utils.LoadStrings();
                string output_folder = Path.Combine(KMod.Manager.GetDirectory(), "strings_templates");
                Utils.GenerateStringsTemplateForAll(Path.Combine(output_folder, "curr_strings.pot"));
            }
        }

        // Download and read translation file
        [HarmonyPatch(typeof(Localization), "Initialize")]
        public class LocalizationInitializePatch
        {
            [HarmonyPriority(int.MaxValue)] // execuate first
            public static void Postfix() { Utils.InitTranslations(); }
        }

        // It seems that LocString.CreateLocStringKeys won't work if no translation is loaded
        // As it is usually called after RegisterForTranslation, I add a patch here
        [HarmonyPatch(typeof(Localization), "RegisterForTranslation")]
        public class LocalizationRegisterForTranslationPatch
        {
            [HarmonyPriority(int.MinValue)] // execuate at last
            public static void Postfix()
            {
                if (Utils.translations != null)
                {
                    Debug.Log($"[ModI18n] RegisterForTranslation with patching");
                    OverloadStrings(Utils.translations);
                }
                else
                    Debug.Log($"[ModI18n] Utils.translations is null");
            }
        }

        // Override all OverloadStrings which other mods could used to load localization
        [HarmonyPatch(typeof(Localization), "OverloadStrings")]
        [HarmonyPatch(new Type[] { typeof(Dictionary<string, string>) })]
        public class OverloadStringsPatch
        {
            [HarmonyPriority(int.MinValue)] // execuate last
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
