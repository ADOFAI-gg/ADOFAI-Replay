using HarmonyLib;
using UnityEngine.UI;
using Replay.Clasz;
using Replay.UI;
using Replay.Lib;
using System.Reflection;
using System.Threading;

namespace Replay.Patch
{
    internal static class WorldReplay
    {
        public static bool isReplayStart = false;
        public static int index = 0;
        public static double restangle = 0;
        public static ReplayData data = new ReplayData();
        internal static ReplaySlider Slider;

        public static void Start(ReplayData _data)
        {
            reset();
            isReplayStart = true;
            data = _data;

        }
    
        public static void reset()
        {
    
            if (Slider != null)
            {
                Slider.isStart = false;
                UnityEngine.Object.DestroyImmediate(Slider);
                Slider = null;
            }
            isReplayStart = false;
            if (Main.gui!=null) Main.gui.ReplayObject.SetActive(isReplayStart);
            index = 0;
            data = new ReplayData();
        }



        public static void Hit(scrController __instance)
        {
            if (__instance.currentSeqID==data.start) restangle = __instance.chosenplanet.angle - data.angles[index].angle;
        }
        


        public static void PlanetHit()
        {
            scrController.instance.Hit();
            index++;
        }

        [AdofaiPatch(
            "WorldReplay.SwitchToEditMode",
            "scnEditor",
            "SwitchToEditMode"
            )]
        public static class SwitchToEditMode
        {
            public static void Prefix(bool clsToEditor)
            {
                if (!Main.IsEnabled) return;
                if (!clsToEditor) return;
                reset();
            }
        }

        
        [AdofaiPatch(
            "WorldReplay.DifficultyArrowPressed",
            "scrUIController",
            "DifficultyArrowPressed"
            )]
        public static class DifficultyArrowPressed
        {
            public static bool Prefix()
            {
                if (Main.IsEnabled)
                {
                    if(isReplayStart) return false;
                }
                return true;
            }

        }



        public static void ShowText(scrPressToStart __instance)
        {
            PrivateLoad<scrPressToStart> privateLoad = new PrivateLoad<scrPressToStart>("text",__instance);
            Text text = (Text)privateLoad.Get();
            text.text = Main.language.press2start;
            privateLoad.Set(text);

            scrUIController uIController = scrUIController.instance;
            GCS.difficulty = (Difficulty)data.difficulty;
            uIController.difficultyText.text = RDString.Get("enum.Difficulty." + GCS.difficulty.ToString());
            uIController.difficultyImage.sprite = uIController.difficultyImages[data.difficulty];

            if (data.start != 0 && __instance.controller.currentSeqID == 0)
            {
                ReplaySlider.LevelStart(data.start);
            }
            if (__instance.controller.goShown) __instance.HideText();

        }
        

        [AdofaiPatch(
            "WorldReplay.ValidInputWasTriggered",
            "scrController",
            "ValidInputWasTriggered"
            )]
        public static class CountValidKeysPressed
        {
            public static bool Prefix()
            {
                if (Main.IsEnabled)
                {
                    if (isReplayStart && scrController.instance.goShown) return false;
                }
                return true;
            }
        }
        
        [AdofaiPatch(
            "WorldReplay.WipeToBlack",
            "scrUIController",
            "WipeToBlack"
            )]
        public static class WipeToBlack
        {
            public static void Prefix()
            {
                if (!Main.IsEnabled) return;
                if (!isReplayStart) return;
                if (data.start != 0 && scrController.instance.controller.currFloor.seqID == 0) return;
                reset();
            }
        }

        [AdofaiPatch(
            "WorldReplay.PlayerControl_Update",
            "scrController",
            "PlayerControl_Update"
            )]
        public static class PlayerControl_Update
        {
            public static void Prefix()
            {
                if (!Main.IsEnabled) return;
                scrController __instance = scrController.instance;
                if (GCS.customLevelPaths != null && !__instance.cls && !__instance.isEditingLevel)
                {
                    if (!WorldReplay.isReplayStart) return;
                    if (!__instance.goShown) return;
                    if (data.angles.Count - 1 < index) return;

                    if (__instance.currFloor.isCCW)
                    {
                        if ((__instance.chosenplanet.angle <= data.angles[index].angle+restangle && __instance.currentSeqID == data.angles[index].num))
                        {
                            PlanetHit();
                            return;
                        }
                    }
                    else
                    {
                        if ((__instance.chosenplanet.angle >= data.angles[index].angle-restangle && __instance.currentSeqID == data.angles[index].num))
                        {
                            PlanetHit();
                            return;
                        }
                    }
                }
            }
        }


    }
}
