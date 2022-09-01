using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Replay.Functions.Core;
using Replay.Functions.Core.Types;
using Replay.Functions.Menu;
using Replay.Functions.Watching;
using Replay.Languages;
using SFB;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityModManagerNet;

namespace Replay
{
    public static class Replay
    {
        private static bool _saveOptionOpend;
        private static bool _keyviewerOptionOpend;
        private static bool _registerKey;
        private static bool _registerKeyDeathcam;
        private static bool _deathCamOptionOpend;
        private static Harmony _replayHarmony;
        private static GUIStyle _title;
        private static GUIStyle _registerKeyButton;
        private static Dictionary<SystemLanguage, LocalizedText> _languages = new Dictionary<SystemLanguage, LocalizedText>
        {
            {SystemLanguage.Korean, new Korean()},
            {SystemLanguage.English, new English()},
            {SystemLanguage.Japanese, new Japanese()},
        };
        
        internal static UnityModManager.ModEntry unityModEntry;
        
        public static bool IsDebug = true;
        public static bool IsAlpha;
        public static KeyCode[] AllKeyCodes;
        public static bool IsUsingNoStopMod;
        public static ReplayOption ReplayOption = new ReplayOption();
        public static Dictionary<string, KeyviewerInput> KeyViewerOnOff = new Dictionary<string, KeyviewerInput>();
        public static LocalizedText CurrentLang => _languages.TryGetValue(RDString.language, out var v)
            ? v
            : _languages[SystemLanguage.English];

        static Replay()
        {
            var harmony = new Harmony("1.replay.first.patches");
            var overlayer = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.GetName().Name == "Overlayer");
            if (overlayer != null)
            {
                var originalOverlayer = overlayer.GetType("Overlayer.Patches.StartProgUpdater").GetMethod("Prefix", AccessTools.all);
                var prefix2 = typeof(NullPointerPreventionPatches).GetMethod("OverlayerSafeNull", AccessTools.all);
                harmony.Patch(originalOverlayer, prefix: new HarmonyMethod(prefix2));
            }
            //Debug.Log(overlayer);

        }
        
        
        public static void Setup(UnityModManager.ModEntry modEntry)
        {
            /*
            var overlayer = UnityModManager.FindMod("Overlayer");
            var index = UnityModManager.modEntries.IndexOf(overlayer);
            Log(index);*/

            
            try
            {
                SceneManager.GetSceneByName("scnLevelSelect");
                IsAlpha = true;
            }
            catch
            {
                IsAlpha = false;
            }

            ReplayOption = UnityModManager.ModSettings.Load<ReplayOption>(modEntry);
            _replayHarmony ??= new Harmony(modEntry.Info.Id);
            
            /*
            
            testGUI = new GameObject().AddComponent<TestGUI>();
            UnityEngine.Object.DontDestroyOnLoad(testGUI);*/
            
            
            modEntry.OnToggle = OnToggle;
            modEntry.OnHideGUI = OnHideGUI;
            modEntry.OnGUI = OnGUI;
            modEntry.OnSaveGUI = OnSettingGUI;
            
            unityModEntry = modEntry;
            ReplayAssets.Init();

            if (!Directory.Exists(Path.Combine(Application.dataPath, "Replays")))
                Directory.CreateDirectory(Path.Combine(Application.dataPath, "Replays"));
                
            if (!Directory.Exists(Path.Combine(Application.dataPath, "Screenshot")))
                Directory.CreateDirectory(Path.Combine(Application.dataPath, "Screenshot"));

            if (string.IsNullOrEmpty(ReplayOption.savedPath))
                ReplayOption.savedPath = Path.Combine(Application.dataPath, "Replays");
            
            ReplayUtils.RegisterRPL();

        }

        private static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            if (_title == null)
            {
                _title = new GUIStyle(GUI.skin.label)
                {
                    fontStyle = FontStyle.Bold
                };
                _registerKeyButton = new GUIStyle(GUI.skin.button);
                _registerKeyButton.normal.textColor = Color.red;
                _registerKeyButton.hover.textColor = Color.red;
                _registerKeyButton.focused.textColor = Color.red;
                _registerKeyButton.active.textColor = Color.red;
            }
            
            if (GUILayout.Button($"{(_deathCamOptionOpend ? "◢" : "▶")} {CurrentLang.deathcamOption}", _title))
                _deathCamOptionOpend = !_deathCamOptionOpend;
            
            if (_deathCamOptionOpend)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(20);
                GUILayout.Label(CurrentLang.registerSpecifiedKeyText);
                if (GUILayout.Button("   " + ((KeyCode) ReplayOption.specifiedDeathCamKeyCode) + "   ",
                        _registerKeyDeathcam ? _registerKeyButton : GUI.skin.button))
                {
                    _registerKeyDeathcam = !_registerKeyDeathcam;
                }
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                GUILayout.Label("");
                
                if (_registerKeyDeathcam)
                {
                    if (Input.GetMouseButtonDown(0)||Input.GetMouseButtonDown(1)||Input.GetKeyDown(KeyCode.Escape)) return;
                    foreach (var k in AllKeyCodes)
                    {
                        if (Input.GetKeyDown((KeyCode)k) && (int)k != ReplayOption.specifiedKeyCode)
                            ReplayOption.specifiedDeathCamKeyCode = (int)k;
                    }
                }
            }

            if (GUILayout.Button($"{(_saveOptionOpend ? "◢" : "▶")} {CurrentLang.saveOptionTitle}", _title))
                _saveOptionOpend = !_saveOptionOpend;

            if (_saveOptionOpend)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(20);
                GUILayout.Label($"{CurrentLang.currentSavePath} {ReplayOption.savedPath}");
                GUILayout.EndHorizontal();
                
                GUILayout.BeginHorizontal();
                GUILayout.Space(30);
                if (GUILayout.Button($" {CurrentLang.changePath} "))
                {
                    var path = StandaloneFileBrowser.OpenFolderPanel(CurrentLang.replayMod, ReplayOption.savedPath, false);
                    if (path.Length > 0)
                    {
                        if (!string.IsNullOrEmpty(path[0]))
                            ReplayOption.savedPath = path[0];
                    }
                }
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                GUILayout.Label("");

                GUILayout.BeginHorizontal();
                GUILayout.Space(20);
                ReplayOption.saveEverytimeDied = GUILayout.Toggle(ReplayOption.saveEverytimeDied, CurrentLang.saveEverytimeDied);
                GUILayout.EndHorizontal();
                
                GUILayout.BeginHorizontal();
                GUILayout.Space(20);
                ReplayOption.saveWhen90P = GUILayout.Toggle(ReplayOption.saveWhen90P, CurrentLang.saveWhen90P);
                GUILayout.EndHorizontal();
                
                GUILayout.BeginHorizontal();
                GUILayout.Space(20);
                ReplayOption.saveEveryLevelComplete = GUILayout.Toggle(ReplayOption.saveEveryLevelComplete, CurrentLang.saveEveryLevelComplete);
                GUILayout.EndHorizontal();
                
                GUILayout.BeginHorizontal();
                GUILayout.Space(20);
                ReplayOption.saveBySpecifiedKey = GUILayout.Toggle(ReplayOption.saveBySpecifiedKey, CurrentLang.saveBySpecifiedKey);
                GUILayout.EndHorizontal();
                
                if (ReplayOption.saveBySpecifiedKey)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(50);
                    GUILayout.Label(CurrentLang.registerSpecifiedKeyText);
                    if (GUILayout.Button("   " + ((KeyCode) ReplayOption.specifiedKeyCode) + "   ",
                        _registerKey ? _registerKeyButton : GUI.skin.button))
                    {
                        _registerKey = !_registerKey;
                    }
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();
                    if (_registerKey)
                    {
                        if (Input.GetMouseButtonDown(0)||Input.GetMouseButtonDown(1)||Input.GetKeyDown(KeyCode.Escape)) return;
                        foreach (var k in AllKeyCodes)
                        {
                            if (Input.GetKeyDown((KeyCode)k) && ReplayOption.specifiedDeathCamKeyCode != (int)k)
                            {
                                ReplayOption.specifiedKeyCode = (int)k;
                            }
                        }
                    }
                }
                
                GUILayout.BeginHorizontal();
                GUILayout.Space(20);
                ReplayOption.replayCount20delte = GUILayout.Toggle(ReplayOption.replayCount20delte, CurrentLang.replayCount20delete);
                GUILayout.EndHorizontal();
                
                GUILayout.Label("");
            }
            
            if (GUILayout.Button($"{(_keyviewerOptionOpend ? "◢" : "▶")} {CurrentLang.keyviewerShowOption}", _title))
                _keyviewerOptionOpend = !_keyviewerOptionOpend;

            if (_keyviewerOptionOpend)
            {
                if (ReplayMenuPatches.CompatKeyViewers == null)
                {
                    GUILayout.Label(CurrentLang.loading);
                }
                else
                {
                    foreach (var m in ReplayMenuPatches.CompatKeyViewers)
                    {
                        GUILayout.BeginHorizontal();
                        GUILayout.Space(20);
                        var newV = GUILayout.Toggle(KeyViewerOnOff[m.Info.DisplayName].Enabled, m.Info.DisplayName);
                        if (KeyViewerOnOff[m.Info.DisplayName].Enabled != newV)
                        {
                            if (newV)
                            {
                                ReplayOption.noUsingKeyviewers.Remove(m.Info.DisplayName);
                                if (WatchReplay.IsPlaying)
                                    KeyViewerOnOff[m.Info.DisplayName].OnStartInputs.Invoke(null, new object[]{});
                            }
                            else
                            {
                                ReplayOption.noUsingKeyviewers.Add(m.Info.DisplayName);
                                if (WatchReplay.IsPlaying)
                                    KeyViewerOnOff[m.Info.DisplayName].OnEndInputs.Invoke(null, new object[]{});
                            }
                            KeyViewerOnOff[m.Info.DisplayName].Enabled = newV;
                        }
                        
                        GUILayout.EndHorizontal();
                    }
                }
                GUILayout.Label("");
            }
            


        }

        private static bool OnToggle(UnityModManager.ModEntry modEntry, bool value)
        {
            if (value)
            {
                if(SceneManager.GetActiveScene().name != "scnSplash")
                    ReplayMenuPatches.InitFirstSettingPatch();
                
                _replayHarmony.PatchAll(Assembly.GetExecutingAssembly());
            }
            else
            {
                if(WatchReplay.IsPlaying)
                    ReplayBasePatches.Reset();
                if (SceneManager.GetActiveScene().name == "scnReplayIntro")
                    SceneManager.LoadScene(IsAlpha ? "scnLevelSelect" : "scnNewIntro");
                else
                    ADOBase.RestartScene();
                if (scrController.instance != null)
                {
                    ReplayBasePatches._progressDisplayerCancel = true;
                    scrUIController.instance.WipeToBlack(WipeDirection.StartsFromLeft);
                }

                _replayHarmony.UnpatchAll(modEntry.Info.Id);
            }
            return true;
        }

        private static void OnSettingGUI(UnityModManager.ModEntry modEntry)
        {
            ReplayOption.Save(modEntry);
        }
        
        private static void OnHideGUI(UnityModManager.ModEntry modEntry)
        {
            _registerKey = false;
        }
        

        public static void Log(params object[] obj)
        {
            unityModEntry.Logger.Log(obj==null? "null":string.Join(", ",obj).Trim());
        }
    }
}