using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins;
using DG.Tweening.Plugins.Options;
using HarmonyLib;
using Replay.Functions.Core;
using Replay.Functions.Watching;
using Replay.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityModManagerNet;
using Object = UnityEngine.Object;

namespace Replay.Functions.Menu
{
    [HarmonyPatch]
    public class ReplayMenuPatches
    {
        private static bool _isFirstLoading;
        private static bool _disableAll; 
        
        public static List<UnityModManager.ModEntry> CompatKeyViewers;
        
        
        // Create a new replay tile
        public static void CreateNewFloor()
        {
            var originalFloor = GameObject.Find("FloorCalibration");
            if (originalFloor == null) return;
            var floorParent = GameObject.Find("outer ring").transform;
            var copyFloor= Object.Instantiate(originalFloor, floorParent).GetComponent<scrFloor>();
            copyFloor.gameObject.name = "FloorReplay";
            copyFloor.transform.position = new Vector3(2, 3, 0);
            copyFloor.levelnumber = -6974;
                
            var textParent = GameObject.Find("Canvas World").transform;
            var originalText = GameObject.Find("Calibration");
            var copyText = Object.Instantiate(originalText, textParent).GetComponent<Text>();
            Object.DestroyImmediate(copyText.GetComponent<scrTextChanger>());
            copyText.text = Replay.CurrentLang.replayModText;
            copyText.gameObject.name = "Replay";
            copyText.transform.position = new Vector3(2.7348f, 4.1518f, 72.32f);
        }


        [HarmonyPatch(typeof(scrCreditsText),"Awake")]
        [HarmonyPostfix]
        public static void CreditTextAddFlowerPatch(scrCreditsText __instance)
        {
            var text = $"<color=#ffffffaa><size=42>{Replay.CurrentLang.replayMod}</size></color>\n";
            text += $"<color=#ffffffaa><size=34>{Replay.CurrentLang.programming}:</size></color>Flower\n";
            text += $"<color=#ffffffaa><size=34>{Replay.CurrentLang.uiDesign}:</size></color>ppapman\n";
            text += "\n";
            text += __instance.text.text;
            __instance.text.text = text;
        }
        
        [HarmonyPatch(typeof(Debugger),"LogWarning")]
        [HarmonyPrefix]
        public static bool DisableErrorDobePatch(object message)
        {
            return !message.ToString().Contains("Target or field is missing");
        }
        
        
        [HarmonyPatch(typeof(scnLevelSelect),"Start")]
        [HarmonyPostfix]
        public static void InitFirstSettingPatch()
        {
            CreateNewFloor();
            
            if (_isFirstLoading) return;
            _isFirstLoading = true;

            Replay.IsDebug = DiscordController.currentUserID == 390747532172460033L;

            Replay.AllKeyCodes = (KeyCode[])typeof(KeyCode).GetEnumValues();
            Replay.IsUsingNoStopMod = UnityModManager.FindMod("NoStopMod") != null;
            
            ReplayUIUtils.SwipeStart = ReplayAssets.SwipeIn;
            
            CompatKeyViewers = ReplayUtils.GetKeyviewers();
            foreach (var k in Replay.ReplayOption.noUsingKeyviewers)
            {
                if (Replay.KeyViewerOnOff.TryGetValue(k, out var v))
                {
                    v.Enabled = false;
                }
            }
            
            var r = ReplayAssets.Assets.LoadAsset<GameObject>("assets/prefs/replayui.prefab");
            var a = Object.Instantiate(r);
            Object.DontDestroyOnLoad(a);
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
                        _disableAll = false;
                        scrController.instance.paused = false;
                        Time.timeScale = 1;
                        Replay.ReplayOption.CanICollectReplayFile = 1;
                    }, () =>
                    {
                        _disableAll = false;
                        Replay.ReplayOption.CanICollectReplayFile = 2;
                        scrController.instance.paused = false;
                        Time.timeScale = 1;
                    },RDString.language);
            }
            ReplayViewingTool.UpdateLayout();
            scnReplayIntro.scnReplayIntro.OnStart = ReplaySelectScene.Awake;

            var args = Environment.GetCommandLineArgs();
            if (args.Length > 1)
            {
                var rpl = ReplayUtils.LoadReplay(args[1]);
                WatchReplay.Play(rpl);
            }
        }


        [HarmonyPatch(typeof(scrController),"OnLandOnPortal")]
        [HarmonyPrefix]
        public static bool ReplayFloorPatch(int portalDestination)
        {
            if (portalDestination == -6974)
            {
                ReplayUIUtils.DoSwipe(() =>
                {
                    SceneManager.LoadScene("scnReplayIntro");
                });
                return false;
            }
            return true;
        }


        [HarmonyPatch(typeof(scnLevelSelect),"Update")]
        [HarmonyPrefix]
        public static void ShortcutKeysPatch()
        {
            
            if (RDEditorUtils.CheckForKeyCombo(true, true, KeyCode.R))
            {
          
                ReplayUIUtils.DoSwipe(() =>
                {
                    SceneManager.LoadScene("scnReplayIntro");
                });

            }
        }
    }
}