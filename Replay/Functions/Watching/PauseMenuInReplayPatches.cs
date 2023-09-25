using System.Linq;
using HarmonyLib;
using Replay.Functions.Core;
using Replay.Functions.Menu;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Replay.Functions.Watching
{
    [HarmonyPatch]
    public class PauseMenuInReplayPatches
    {
        [HarmonyPatch(typeof(scrUIController), "DifficultyArrowPressed")]
        [HarmonyPrefix]
        public static bool DisableDifficultSelectSound()
        {
            return !WatchReplay.IsPlaying;
        }
        
        
        [HarmonyPatch(typeof(PauseMenu), "Update")]
        [HarmonyPrefix]
        public static void SibalBug()
        {
            if (!WatchReplay.IsPlaying) return;
            if (scrController.instance.paused != ReplayBasePatches._paused)
                scrController.instance.paused = ReplayBasePatches._paused;
            
        }

        

        [HarmonyPatch(typeof(scrController), "TogglePauseGame")]
        [HarmonyPostfix]
        public static void TogglePauseGamePatch()
        {
            if (!WatchReplay.IsPlaying) return;
            if (!ReplayBasePatches._paused)
            {
                if (WatchReplay.IsPaused || WatchReplay.IsPlanetDied)
                {
                    scrController.instance.audioPaused = true;
                    Time.timeScale = 0;
                }
            }

        }
        
        
        [HarmonyPatch(typeof(PauseMedals), "Init")]
        [HarmonyPostfix]
        public static void ShowMedals(PauseMedals __instance)
        {
            __instance.gameObject.SetActive(!WatchReplay.IsPlaying);
        }
        
        
        
        [HarmonyPatch(typeof(scrController), "TogglePauseGame")]
        [HarmonyPrefix]
        public static void TogglePauseGamePatch2()
        {
            if (!WatchReplay.IsPlaying) return;
            scrController.instance.paused = ReplayBasePatches._paused;
            ReplayBasePatches._paused = !ReplayBasePatches._paused;
            
        }
        
        [HarmonyPatch(typeof(ADOBase), "GoToCalibration")]
        [HarmonyPrefix]
        public static bool CancelCalibrationInReplaying()
        {
            if (!WatchReplay.IsPlaying) return true;
            return false;
        }

        
        
        [HarmonyPatch(typeof(PauseMenu), "RefreshLayout")]
        [HarmonyPostfix]
        public static void DisableOtherButtonsPatch(ref GeneralPauseButton[] ___pauseButtons)
        {
            if (!WatchReplay.IsPlaying) return;

            ___pauseButtons = ___pauseButtons.Where(b =>
            {
                if (b is PauseButton button)
                {
                    if (button.rdString == "pauseMenu.levelEditor" &&
                        !string.IsNullOrEmpty((string)scnGame.instance.levelData.songSettings["songURL"]))
                        return false;
                    return !(button.rdString == "pauseMenu.restart" || button.rdString == "pauseMenu.practice" ||
                             button.rdString == "pauseMenu.settings" || button.rdString == "pauseMenu.next" ||
                             button.rdString == "pauseMenu.previous");
                }

                return false;
            }).ToArray();

        }
        
    }
}