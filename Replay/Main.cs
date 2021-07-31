using System;
using HarmonyLib;
using System.Reflection;
using UnityModManagerNet;
using UnityEngine;
using System.IO;
using Replay.UI;
using Replay.Patch;
using Replay.Clasz;
using Steamworks;
using TinyJson;
using System.Collections.Generic;
using System.Linq;
using Replay.Lib;

namespace Replay
{
    public static class Main
    {
        public static bool IsEnabled { get; private set; }
        private static ReplayUI thisGUIComponent;
   
        public static string path { get; set; }
        public static Language language = new Language();
        public static int version = (int)AccessTools.Field(typeof(GCNS), "releaseNumber").GetValue(null);
        public static bool isAlpha = false;
        public static AudioClip mainSong;
        internal static ReplayUI gui { get => thisGUIComponent; set => thisGUIComponent = value; }
        //internal static ReplaySlider gui3 { get => thisGUIComponent2; set => thisGUIComponent2 = value; }

        private static List<Type> allTweakTypes;


        public static UnityModManager.ModEntry.ModLogger Logger { get; private set; }

        public static Harmony harmony;
        public static Texture2D background = new Texture2D(912, 512);
        public static Texture2D card = new Texture2D(1586, 263);
        public static Texture2D smallcard = new Texture2D(100, 50);


        internal static void Setup(UnityModManager.ModEntry modEntry)
        {
            Logger = modEntry.Logger;
            modEntry.OnToggle = OnToggle;
            path = modEntry.Path;
            
            allTweakTypes =
                AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(a => a.GetTypes())
                    .Where(t => t.GetCustomAttribute<AdofaiPatchAttribute>() != null)
                    .OrderBy(t => t.Name)
                    .ToList();
           
            path = path.Split(new string[] { "common" }, StringSplitOptions.None)[0] + "workshop/content/977950/";

            background.LoadImage(File.ReadAllBytes("Mods\\Replay\\background.png"));
            card.LoadImage(File.ReadAllBytes("Mods\\Replay\\card.png"));
            smallcard.LoadImage(File.ReadAllBytes("Mods\\Replay\\smallcard.png"));
            SetLanguage(Persistence.GetLanguage());
            
            //mainSong = scrConductor.instance.song.clip;
        }

        private static bool OnToggle(UnityModManager.ModEntry modEntry, bool value)
        {
            IsEnabled = value;
            WorldReplay.reset();
            if(value)
            {
                Start(modEntry);
                Main.gui= new GameObject().AddComponent<ReplayUI>();
                UnityEngine.Object.DontDestroyOnLoad(Main.gui);

                //Main.gui3 = new GameObject().AddComponent<ReplaySlider>();
                //UnityEngine.Object.DontDestroyOnLoad(Main.gui3);


            } else
            {
                UnityEngine.Object.DestroyImmediate(Main.gui);
                if(PlayHistory.Menu!=null)
                {
                    UnityEngine.Object.DestroyImmediate(PlayHistory.Menu);
                }

                PlayHistory.isMenuOpening = false;
                PlayHistory.Menu = null;
                PlayHistory.isSave = false;

                Stop(modEntry);
            }
            return true;
        }

      

        private static void Start(UnityModManager.ModEntry modEntry)
        {
            harmony = new Harmony(modEntry.Info.Id);
            //harmony.PatchAll(Assembly.GetExecutingAssembly());
            
            
            foreach (Type tweakType in allTweakTypes)
            {
                AdofaiPatch.Patch(harmony, tweakType);
            }




        }

        private static void Stop(UnityModManager.ModEntry modEntry)
        {
            foreach (Type tweakType in allTweakTypes)
            {
                AdofaiPatch.Unpatch(harmony, tweakType);
            }
        }

        public static void SetLanguage(string language)
        {
            switch (language)
            {
                case "Korean":
                    Main.language = JSONParser.FromJson<Language>(File.ReadAllText("./Mods/Replay/language.json").Split(new string[] { $"\"Korean\":" }, StringSplitOptions.None)[1].Split(new string[] { "}" }, StringSplitOptions.None)[0] + "}");
                    break;

                case "English":
                    Main.language = JSONParser.FromJson<Language>(File.ReadAllText("./Mods/Replay/language.json").Split(new string[] { $"\"English\":" }, StringSplitOptions.None)[1].Split(new string[] { "}" }, StringSplitOptions.None)[0] + "}");
                    break;

                default:
                    Main.language = JSONParser.FromJson<Language>(File.ReadAllText("./Mods/Replay/language.json").Split(new string[] { $"\"English\":" }, StringSplitOptions.None)[1].Split(new string[] { "}" }, StringSplitOptions.None)[0] + "}");
                    break;
            }
        }


        [AdofaiPatch(
            "Main.ChangeLanguage",
            "RDString",
            "ChangeLanguage"
            )]
        internal static class ChangeLanguage
        {
            public static void Prefix(SystemLanguage language)
            {
                SetLanguage(language.ToString());
            }
        }
    }
}
