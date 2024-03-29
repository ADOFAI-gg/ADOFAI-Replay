﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DG.Tweening;
using DG.Tweening.Plugins;
using DG.Tweening.Plugins.Options;
using HarmonyLib;
using Replay.Functions.Core;
using Replay.Functions.Saving;
using Replay.Functions.Watching;
using Replay.UI;
using ReplayLoader;
using SkyHook;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityModManagerNet;
using Debug = System.Diagnostics.Debug;
using Debugger = DG.Tweening.Core.Debugger;
using Object = UnityEngine.Object;

namespace Replay.Functions.Menu
{
    [HarmonyPatch]
    public class ReplayMenuPatches
    {
        private static bool _disableAll;
        
        internal static bool _isFirstLoading;
        internal static bool _created;
        
        public static List<UnityModManager.ModEntry> CompatKeyViewers;
        
        
        // Create a new replay tile
        public static void CreateNewFloor()
        {
            if (_created) return;
            var originalFloor = GameObject.Find("FloorCalibration");
            if (originalFloor == null) return;
            var floorParent = GameObject.Find("outer ring").transform;
            var copyFloor= Object.Instantiate(originalFloor, floorParent).GetComponent<scrFloor>();
            copyFloor.gameObject.name = "FloorReplay";
            copyFloor.transform.position = new Vector3(2, 3, 0);
            copyFloor.levelnumber = -6974;
            copyFloor.floorRenderer.enabled = true;
            copyFloor.gameObject.SetActive(true);
                
            var textParent = GameObject.Find("Canvas World").transform;
            var originalText = GameObject.Find("Calibration");
            var copyText = Object.Instantiate(originalText, textParent).GetComponent<Text>();
            Object.DestroyImmediate(copyText.GetComponent<scrTextChanger>());
            copyText.text = Replay.CurrentLang.replayModText;
            copyText.gameObject.name = "Replay";
            copyText.transform.position = new Vector3(2.7348f, 4.1518f, 72.32f);
            _created = true;
        }


        [HarmonyPatch(typeof(scrCreditsText),"Start")]
        [HarmonyPostfix]
        public static void CreditTextAddFlowerPatch(scrCreditsText __instance)
        {
            var text = $"<color=#ffffffaa><size=42>{Replay.CurrentLang.replayMod}</size></color>\n";
            text += $"<color=#ffffffaa><size=34>{Replay.CurrentLang.programming}:</size></color>Flower\n";
            text += $"<color=#ffffffaa><size=34>{Replay.CurrentLang.uiDesign}:</size></color>ppapman\n";
            text += $"<color=#ffffffaa><size=34>{Replay.CurrentLang.japaneseTranslate}:</size></color>sjk\n";
            text += "\n";
            text += __instance.text.text;
            __instance.text.text = text;
        }

        
        
        [HarmonyPatch(typeof(Debugger),"LogWarning")]
        [HarmonyPrefix]
        public static bool DisableErrorDobePatch(object message)
        {
            return false;
        }

        [HarmonyPatch(typeof(scnLevelSelect), "Awake")]
        [HarmonyPostfix]
        public static void SetCreated()
        {
            _created = false;
        }
        
        
        [HarmonyPatch(typeof(scrController),"Awake")]
        [HarmonyPostfix]
        [Obsolete("Obsolete")]
        public static void InitFirstSettingPatch()
        {

            if (_isFirstLoading) return;
            _isFirstLoading = true;
            
            
            AdofaiTweaksAPI.Init();
            YoutubeStreamAPI.Init();
            
            var keyLabels = typeof(KeyLabel).GetEnumValues();
            //Replay.Log(Enum.Parse(typeof(KeyCode), "Escape"));

            
            foreach (KeyLabel keyLabel in keyLabels)
            {
                var stringKey = keyLabel.ToString();

                stringKey = stringKey switch
                {
                    "Grave" => "BackQuote",
                    "Equal" => "Equals",
                    "LeftBrace" => "LeftBracket",
                    "RightBrace" => "RightBracket",
                    "Apostrophe" => "Quote",
                    "Enter" => "Return",
                    "LShift" => "LeftShift",
                    "RShift" => "RightShift",
                    "Dot" => "Period",
                    "LControl" => "LeftControl",
                    "Super" => "LeftWindows",
                    "LAlt" => "LeftAlt",
                    "RAlt" => "RightAlt",
                    "RControl" => "RightControl",
                    "PrintScreen" => "Print",
                    "PauseBreak" => "Pause",
                    "ArrowUp" => "UpArrow",
                    "ArrowLeft" => "LeftArrow",
                    "ArrowDown" => "DownArrow",
                    "ArrowRight" => "RightArrow",
                    "NumLock" => "Numlock",
                    "KeypadSlash" => "KeypadDivide",
                    "KeypadAsterisk" => "KeypadMultiply",
                    "KeypadDot" => "KeypadPeriod",
                    "MouseLeft" => "Mouse0",
                    "MouseRight" => "Mouse1",
                    "MouseMiddle" => "Mouse2",
                    "MouseX1" => "Mouse3",
                    "MouseX2" => "Mouse4",
                    "BackSlash" => "Backslash",
                    "Unknown" => "None",
                    _ => stringKey
                };
                

                if (Enum.TryParse(stringKey, out KeyCode k))
                {
                    AddKeyInputsPatches.KeyLabelToKeyCode[keyLabel] = k;
                }
                
            }

            Replay.IsDebug = DiscordController.currentUserID == 390747532172460033L;

            Replay.AllKeyCodes = (KeyCode[])typeof(KeyCode).GetEnumValues();


            ReplayUIUtils.SwipeStart = ADOBase.gc.soundEffects[(int)SfxSound.ScreenWipeOut];


            CompatKeyViewers = ReplayUtils.GetKeyviewers();
            foreach (var k in Replay.ReplayOption.noUsingKeyviewers)
            {
                if (Replay.KeyViewerOnOff.TryGetValue(k, out var v))
                {
                    v.Enabled = false;
                }
            }

            if (ReplayUI.Instance == null)
            {
                var r = ReplayLoader.ReplayAssets.Assets.LoadAsset<GameObject>("assets/prefs/replayui.prefab");
                var a = Object.Instantiate(r);
                Object.DontDestroyOnLoad(a);
                ReplayUI.Instance = r.GetComponent<ReplayUI>();
                ReplayUI.Instance.enabled = true;
                ReplayUI.Instance.gameObject.SetActive(true);
            }
            ReplayViewingTool.PauseImage = ReplayUI.Instance.Pause.GetComponent<Image>();

            GlobalLanguage.OK = Replay.CurrentLang.agreed;
            GlobalLanguage.No = Replay.CurrentLang.noAgreed;  
            ReplayUI.Instance.BbiBbiGameobject.SetActive(false);
            if (Replay.ReplayOption.CanICollectReplayFile == 0)
            {
                _disableAll = true;
                scrController.instance.paused = true;
                Time.timeScale = 0;
                ReplayUI.Instance.ShowNotification(Replay.CurrentLang.replayMod, "<cspace=-0.05em>"+Replay.CurrentLang.replayCollectMessage,
                    () =>
                    {
                        scrSfx.instance.PlaySfx(SfxSound.MenuSquelch);
                        _disableAll = false;
                        scrController.instance.paused = false;
                        Time.timeScale = 1;
                        Replay.ReplayOption.CanICollectReplayFile = 1;
                        return true;
                    }, () =>
                    {
                        scrSfx.instance.PlaySfx(SfxSound.MenuSquelch);
                        _disableAll = false;
                        Replay.ReplayOption.CanICollectReplayFile = 2;
                        scrController.instance.paused = false;
                        Time.timeScale = 1;
                        return true;
                    },RDString.language);
            }
            

            scnReplayIntro.scnReplayIntro.OnStart = ReplaySelectScene.Awake;
            scnReplayIntro.scnReplayIntro.OnQuit = ReplaySelectScene.OnQuit;
            scnReplayIntro.scnReplayIntro.OnLoad = ReplaySelectScene.OnLoad;

            var args = Environment.GetCommandLineArgs();
            if (args.Length > 1)
            {
                var rpl = ReplayUtils.LoadReplay(args[1]);
                WatchReplay.Play(rpl);
            }

            Loader.UpdateLayoutNextFrame();
        }
        
        

       
       
        
        [HarmonyPatch(typeof(ReplayUIUtils), "InitUI")]
        [HarmonyPostfix]
        public static void AddBackgroundScript()
        {
            ReplayUIUtils._swipeImage.parent.gameObject.AddComponent<ReplayBackground>();
            ReplayUIUtils.Audio.ignoreListenerPause = true;
        }


        [HarmonyPatch(typeof(scrController),"OnLandOnPortal")]
        [HarmonyPrefix]
        public static bool ReplayFloorPatch(int portalDestination)
        {
            if (portalDestination == -6974)
            {
                ReplayUIUtils.DoSwipe(() =>
                {
                    SceneManager.LoadScene("scnReplayIntro", LoadSceneMode.Single);
                });
                return false;
            }
            return true;
        }
        

        [HarmonyPatch(typeof(scnLevelSelect),"Update")]
        [HarmonyPrefix]
        public static void ShortcutKeysPatch()
        {
            if(!_created)
                CreateNewFloor();
            
            if (RDEditorUtils.CheckForKeyCombo(true, true, KeyCode.R))
            {
          
                ReplayUIUtils.DoSwipe(() =>
                {
                    SceneManager.LoadScene("scnReplayIntro", LoadSceneMode.Single);
                });

            }
        }
    }
}