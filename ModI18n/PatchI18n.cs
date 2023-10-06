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
using UnityEngine;
using static Localization;
using static ModI18n.Patches;

namespace ModI18n
{
    public class Utils
    {
        public static readonly string i18nBaseUrl = "https://cdn.jsdelivr.net/gh/ONI-Wiki-zh/ONIi18n@1/dist/";
        public static readonly string modsDir = KMod.Manager.GetDirectory();
        public static readonly string stringsFolder = Path.Combine(modsDir, "i18n");
        public static readonly int maxPrintCount = 3;
        public static Dictionary<string, string> translations = null;
        public static string templatesFolder = Path.Combine(KMod.Manager.GetDirectory(), "strings_templates", "ModI18n");

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
                translations = new Dictionary<string, string>();
                Debug.LogWarning($"[ModI18n] Failed to load locolization file: {filename}");
            }
        }

        public static void LoadStrings()
        {
            GenerateStringsTemplate(typeof(STRINGS), templatesFolder);
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

        public static void LoadStringsWithPrefix(string prefix)
        {
            int printCount = maxPrintCount;
            foreach (KeyValuePair<string, string> e in translations)
            {
                if (!e.Key.StartsWith(prefix)) continue;
                if (printCount > 0)
                    Debug.Log($"[ModI18n] String.add {e.Key} {e.Value}");
                Strings.Add(e.Key, e.Value);
                printCount--;
            }
            if (printCount < 0)
                Debug.Log($"[ModI18n] ... and {-printCount} more String.add with prefix '{prefix}'");
        }

        public static object GetField(Type type, object instance, string fieldName)
        {
            BindingFlags bindFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
            FieldInfo field = type.GetField(fieldName, bindFlags);
            return field.GetValue(instance);
        }

        public static List<KeyValuePair<string, string>> CollectStrings()
        {
            string vanillaStrginsTemplate = Path.Combine(UnityEngine.Application.streamingAssetsPath, "strings", "strings_template.pot");
            Debug.Log(vanillaStrginsTemplate);
            Dictionary<string, string> vanillaStrgins = LoadStringsFile(vanillaStrginsTemplate, true);

            StringTable RootTable = (StringTable)GetField(typeof(Strings), null, "RootTable");
            Dictionary<int, string> RootTableKeyNames = (Dictionary<int, string>)GetField(typeof(StringTable), RootTable, "KeyNames");
            Dictionary<int, StringEntry> RootTableEntries = (Dictionary<int, StringEntry>)GetField(typeof(StringTable), RootTable, "Entries");

            SortedSet<string> keys = new SortedSet<string>();
            foreach (var kv in RootTableKeyNames)
                keys.Add(kv.Value);

            List<KeyValuePair<string, string>> dict = new List<KeyValuePair<string, string>>();
            foreach (var k in keys)
            {
                if (!vanillaStrgins.ContainsKey(k)
                    && !k.StartsWith("PeterHan.PLib.")
                    && !k.StartsWith("ModI18n.")
                    // some Klei's mistake
                    && !k.StartsWith("STRINGS.BUILDINGS.DAMAGESOURCESDAMAGESOURCES.")
                    && !k.StartsWith("STRINGS.BUILDINGS.DISINFECTABLEDISINFECTABLE.")
                    && !k.StartsWith("STRINGS.BUILDINGS.REPAIRABLEREPAIRABLE.")
                    // empty strings
                    && RootTableEntries[k.GetHashCode()].String?.Length > 0
                    )
                    dict.Add(new KeyValuePair<string, string>(k, RootTableEntries[k.GetHashCode()].String));
            }
            Debug.Log($"[ModI18n] Collected {dict.Count} strings");
            return dict;
        }

        public static void GenerateStringsTemplateForAll(string file_path)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(file_path));
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
                Debug.Log($"[ModI18n] Strings loaded in LegacyModMain.Load");
                Utils.GenerateStringsTemplateForAll(Path.Combine(Utils.templatesFolder, "curr_mods_templates.pot"));
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
                if (new System.Diagnostics.StackFrame(2).GetMethod().Name == "OnAllModsLoaded") { }
                else if (Utils.translations != null)
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

        [HarmonyPatch(typeof(Assets), "SubstanceListHookup")]
        public class SubstanceListHookupPatch
        {
            [HarmonyPriority(int.MinValue)] // execuate last
            public static void Prefix() => Utils.LoadStringsWithPrefix("STRINGS.ELEMENTS.");
        }

        [HarmonyPatch(typeof(ModUtil), "AddBuildingToPlanScreen")]
        [HarmonyPatch(new Type[] {
            typeof(HashedString),
            typeof(string),
            typeof(string),
            typeof(string),
            typeof(ModUtil.BuildingOrdering)
        })]
        public class AddBuildingToPlanScreenPatch
        {
            [HarmonyPriority(int.MinValue)] // execuate last
            public static void Prefix() => Utils.LoadStringsWithPrefix("STRINGS.BUILDINGS.");
        }

        [HarmonyPatch(typeof(GeneratedBuildings), "LoadGeneratedBuildings")]
        public class LoadGeneratedBuildingsPatch
        {
            [HarmonyPriority(int.MinValue)] // execuate last
            public static void Prefix() => Utils.LoadStringsWithPrefix("STRINGS.BUILDINGS.");
        }

        [HarmonyPatch(typeof(EntityConfigManager), "LoadGeneratedEntities")]
        public class LoadGeneratedEntitiesPatch
        {
            [HarmonyPriority(int.MinValue)] // execuate last
            public static void Prefix() => Utils.LoadStrings();
        }

        [HarmonyPatch(typeof(EntityTemplates), "CreateLooseEntity")]
        public class CreateLooseEntityPatch
        {
            [HarmonyPriority(int.MinValue)] // execuate last
            public static void Prefix(string id, ref string name, ref string desc)
            {
                string foodNameCode = $"STRINGS.ITEMS.FOOD.{id.ToUpperInvariant()}.NAME";
                string foodDescCode = $"STRINGS.ITEMS.FOOD.{id.ToUpperInvariant()}.DESC";
                if (Utils.translations.ContainsKey(foodNameCode))
                    name = Utils.translations[foodNameCode];
                if (Utils.translations.ContainsKey(foodDescCode))
                    desc = Utils.translations[foodDescCode];
            }
        }

        //[HarmonyPatch(typeof(EntityTemplates), "CreatePlacedEntity",
        //    new Type[] 
        //{ typeof(string),typeof(string),typeof(string),
        //    typeof(float),typeof(KAnimFile),typeof(string),
        //    typeof(Grid.SceneLayer),typeof(int),typeof(int),
        //    typeof(EffectorValues),typeof(EffectorValues),typeof(SimHashes),
        //    typeof( List<Tag>),typeof(float) 

        //})]
        public class CreatePlacedEntityPatch
        {
            [HarmonyPriority(int.MinValue)] // execuate last
            public static void Prefix(string id, ref string name, ref string desc)
            {
                string creaturesNameCode = $"STRINGS.CREATURES.SPECIES.{id.ToUpperInvariant()}.NAME";
                string creaturesDescCode = $"STRINGS.CREATURES.SPECIES.{id.ToUpperInvariant()}.DESC";
                if (Utils.translations.ContainsKey(creaturesNameCode))
                    name = Utils.translations[creaturesNameCode];
                if (Utils.translations.ContainsKey(creaturesDescCode))
                    desc = Utils.translations[creaturesDescCode];
            }
        }

        [HarmonyPatch(typeof(EntityTemplates), "CreateAndRegisterSeedForPlant")]
        public class CreateAndRegisterSeedForPlantPatch
        {
            [HarmonyPriority(int.MinValue)] // execuate last
            public static void Prefix(string id, ref string name, ref string desc)
            {
                string seedNameCode = $"STRINGS.CREATURES.SPECIES.SEEDS.{id.ToUpperInvariant()}.NAME";
                string seedDescCode = $"STRINGS.CREATURES.SPECIES.SEEDS.{id.ToUpperInvariant()}.DESC";
                if (Utils.translations.ContainsKey(seedNameCode))
                    name = Utils.translations[seedNameCode];
                if (Utils.translations.ContainsKey(seedDescCode))
                    desc = Utils.translations[seedDescCode];
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

        public override void OnAllModsLoaded(Harmony harmony, IReadOnlyList<KMod.Mod> mods)
        {
            base.OnAllModsLoaded(harmony, mods);
            var start = System.DateTime.Now;
            Directory.CreateDirectory(Path.GetDirectoryName(Utils.templatesFolder));

            foreach (KMod.Mod mod in mods)
            {
                if (mod.title == "ModI18nReborn") continue;
                if (!mod.IsActive()) continue;
                if (mod.status == KMod.Mod.Status.Installed)
                {
                    Debug.Log($"[ModI18n] Detected active mod {mod.title}");
                    foreach (Assembly assem in mod.loaded_mod_data.dlls)
                    {
                        Debug.Log($"[ModI18n] Detected mod assem: {assem.FullName}");
                        HashSet<string> nss = new HashSet<string>();
                        foreach (Type t in assem.GetTypes())
                        {
                            if (!nss.Contains(t.Namespace))
                                try
                                {
                                    nss.Add(t.Namespace);
                                    // only namesapce and assem matters for register and generating
                                    GenerateStringsTemplate(t, Utils.templatesFolder);
                                }
                                catch (Exception e)
                                {
                                    Debug.LogWarning($"[ModI18n] Error when generating template for ns {t.Namespace} of {assem.FullName}: {e.Message} {e.StackTrace}");
                                }
                            try
                            {
                                RegisterForTranslation(t);
                            }
                            catch { }
                        }
                    }
                }
            }
            var allMethod = typeof(EntityTemplates).GetMethods(BindingFlags.Static | BindingFlags.Public);
            foreach (var method in allMethod)
            {
                if (method.Name == "CreateAndRegisterSeedForPlant")
                {
                    harmony.Patch(method, prefix: new HarmonyMethod(typeof(CreateAndRegisterSeedForPlantPatch).GetMethod("Prefix")));
                }
                if (method.Name == "CreatePlacedEntity")
                {
                    harmony.Patch(method, prefix: new HarmonyMethod(typeof(CreatePlacedEntityPatch).GetMethod("Prefix")));
                }

            }
            Utils.LoadStrings();
            Debug.Log($"[ModI18n] Used {(System.DateTime.Now - start).TotalSeconds} seconds to generate string templates OnAllModsLoaded!");
        }
    }
}
