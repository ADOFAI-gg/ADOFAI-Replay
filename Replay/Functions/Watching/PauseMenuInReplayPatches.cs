using HarmonyLib;
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
        
        [HarmonyPatch(typeof(scrController), "TogglePauseGame")]
        [HarmonyPrefix]
        public static void TogglePauseGamePatch2()
        {
            if (!WatchReplay.IsPlaying) return;
            scrController.instance.paused = ReplayBasePatches._paused;
            ReplayBasePatches._paused = !ReplayBasePatches._paused;
        }
    }
}