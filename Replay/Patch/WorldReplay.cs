using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using Replay.Clasz;
using Replay.UI;
using Replay.Lib;
using System.Reflection;
using System.Threading;
using System;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;

namespace Replay.Patch
{
    internal static class WorldReplay
    {
        public static bool isReplayStart = false, isStop = false, nostopmod = false;
        public static int index = 0;
        public static double restangle = 0;
        public static ReplayData data = new ReplayData();
        internal static ReplaySlider Slider;
        public static float originalSpeed = 1f;

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
            if (Main.gui != null) Main.gui.ReplayObject.SetActive(isReplayStart);
            index = 0;
            data = new ReplayData();
            GCS.playDeathSound = true;
        }

        
        
        [AdofaiPatch(
            "Invincible_Lag",
            "scrPlanet",
            "SwitchChosen"
            )]
        public static class Invincible_Lag
        {
            public static double originalAngle;

            public static void Prefix(scrPlanet __instance)
            {
                if (!Main.IsEnabled) return;
                if (!scrController.instance.gameworld) return;
                
                if (!isReplayStart) return;
                originalAngle = __instance.angle;
                /*
                if (data.angles.Count - 1 < index) return;
                __instance.angle = data.angles[index].angle;
                */
                
                double num = (double)((GCS.perfectOnlyMode ? 45f : GCS.HITMARGIN_COUNTED) * 0.017453292f);
               num = Math.Max(
                    scrMisc.GetAdjustedAngleBoundaryInDeg(
                        GCS.perfectOnlyMode ? HitMarginGeneral.Perfect : HitMarginGeneral.Counted,
                        __instance.conductor.bpm * __instance.controller.speed,
                        __instance.conductor.song.pitch) * 0.01745329238474369,
                    num);

                if (!scrMisc.isDiffInMargin(__instance.angle, __instance.targetExitAngle, num)
                    && (__instance.currfloor.isCCW && __instance.angle < __instance.targetExitAngle
                        || !__instance.currfloor.isCCW && __instance.angle > __instance.targetExitAngle))
                {
                   __instance.angle = __instance.targetExitAngle + (num - 0.001) * (__instance.currfloor.isCCW ? -1 : 1);
                }

            }

            public static void Postfix(scrPlanet __instance)
            {
                if (!Main.IsEnabled) return;
                if (!scrController.instance.gameworld) return;
                
                if (!isReplayStart) return;
                __instance.angle = originalAngle;
            }
        }
    

        public static void PlanetHit()
        {
            scrController.instance.Hit();
            index++;
        }

        public static bool HitNow()
        {
            
            scrController __instance = scrController.instance;
            scrPlanet planet = __instance.chosenplanet;
            
            if (__instance.currFloor.isCCW)
            {
                if (planet.angle <= data.angles[index].angle) return true;
            }
            else
            {
                if (planet.angle >= data.angles[index].angle) return true;
            }
            
            /*
            HitMargin hitMargin = scrMisc.GetHitMargin((float)planet.angle, (float)planet.targetExitAngle, __instance.isCW, (float)((double)__instance.conductor.bpm * __instance.speed), __instance.conductor.song.pitch);

            
            if (data.angles[index].margin == HitMargin.VeryEarly && hitMargin == HitMargin.TooEarly) return true;
            if (data.angles[index].margin == HitMargin.TooEarly && hitMargin == HitMargin.EarlyPerfect) return true;
            if (data.angles[index].margin == HitMargin.TooEarly && hitMargin == HitMargin.Perfect) return true;
            if (data.angles[index].margin == HitMargin.Perfect && hitMargin == HitMargin.LatePerfect) return true;
            if (data.angles[index].margin == HitMargin.LatePerfect && hitMargin == HitMargin.TooLate) return true;
            if (data.angles[index].margin == HitMargin.TooLate && hitMargin == HitMargin.VeryLate) return true;*/

            double num = __instance.chosenplanet.angle - data.angles[index].angle;
            if (__instance.controller.isCW)
            {
                num = -num;
            }
            if (__instance.gameworld && num <= 0.005&&planet.currfloor.seqID==data.angles[index].num)
            {
                return true;
            }
  
            
            double num2 = (GCS.perfectOnlyMode ? 45f : GCS.HITMARGIN_COUNTED) * 0.0174532924f;
            num2 = Math.Max(
                scrMisc.GetAdjustedAngleBoundaryInDeg(
                    GCS.perfectOnlyMode ? HitMarginGeneral.Perfect : HitMarginGeneral.Counted,
                    planet.conductor.bpm * planet.controller.speed,
                    planet.conductor.song.pitch) * 0.01745329238474369,
                num2);
            if (!__instance.paused
                && !scrMisc.isDiffInMargin(planet.angle, planet.targetExitAngle, num2)
                && (planet.currfloor.isCCW && planet.angle < planet.targetExitAngle
                    || !planet.currfloor.isCCW && planet.angle > planet.targetExitAngle))
            {
                Main.Logger.Log(__instance.currentSeqID+"   "+planet.angle+"    "+planet.targetExitAngle);
                return true;
            }
            

            return false;
            
        }

        [AdofaiPatch("test2",
            "scrPlanet",
            "ScrubToFloorNumber")]
        public static class test2
        {
            public static void Prefix(scrPlanet __instance, int floorNum)
            {
                scrFloor scrFloor = scrLevelMaker.instance.listFloors[floorNum];
                Main.Logger.Log(scrFloor.isCCW.ToString());
                double snappedLastAngle = (double)new PrivateLoad<scrPlanet>("snappedLastAngle", __instance).Get();
                string o = (snappedLastAngle + scrFloor.angleLength * -1).ToString();
                string t = (snappedLastAngle + scrFloor.angleLength * 1).ToString();
                Main.Logger.Log(o+"   "+t);
            }
        }
        
        [AdofaiPatch("test",
            "scrPlanet",
            "MoveToNextFloor")]
        public static class test
        {
            public static void Prefix(scrPlanet __instance, scrFloor floor)
            {
                double snappedLastAngle = (double)new PrivateLoad<scrPlanet>("snappedLastAngle", __instance.other).Get();
                string o = (snappedLastAngle + floor.angleLength * -1).ToString();
                string t = (snappedLastAngle + floor.angleLength * 1).ToString();
                Main.gui.text = "   " + o + "   " + t+"   "+floor.isCCW+"   "+snappedLastAngle+"   "+floor.angleLength;
                Main.Logger.Log(floor.angleLength.ToString());

            }
            

            
            
        }
        
        
        [AdofaiPatch(
            "Hide_UI2",
            "scnEditor",
            "SwitchToEditMode"
            )]
        public static class Hide_UI2
        {
            public static void Prefix(bool clsToEditor)
            {
                if (!Main.IsEnabled) return;
                if (!clsToEditor) return;
                reset();
            }
        }

        
        [AdofaiPatch(
            "Ignore_Judgment_Changes",
            "scrUIController",
            "DifficultyArrowPressed"
            )]
        public static class Ignore_Judgment_Changes
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
            int index = (int) data.difficulty;
            
            new PrivateLoad<scrUIController>("currentDifficultyIndex",uIController).Set(index);
            nostopmod = true;
            new PrivateLoad<scrController>("OnApplicationQuit", scrController.instance).Call(new object[]{});
            Difficulty difficulty = (Difficulty)data.difficulty;
            Main.Logger.Log(index+"    "+difficulty);
            GCS.difficulty = difficulty;
            uIController.difficultyText.text = RDString.Get("enum.Difficulty." + difficulty.ToString());
            Sprite[] sprites = (Sprite[])new PrivateLoad<scrUIController>("difficultyImages", uIController).Get();
            uIController.difficultyImage.sprite =
                sprites == null ? RDConstants.data.bullseyeSprites[index] : sprites[index];

            if (data.start != 0 && __instance.controller.currentSeqID == 0)
            {
                ReplaySlider.LevelStart(data.start);
            }
            if (__instance.controller.goShown) __instance.HideText();

        }


        [AdofaiPatch(
            "Ignore_Keystrokes",
            "scrController",
            "ValidInputWasTriggered"
            )]
        public static class Ignore_Keystrokes
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
            "Clear_Overload",
            "scrFailBar",
            "Update")]
        public static class Clear_Overload
        {
            public static void Postfix(scrFailBar __instance, ref bool ___failCalled)
            {
                if (!Main.IsEnabled) return;
                if(!isReplayStart) return;
                float value = 0f;
                if(Main.version>71) value = (float)new PrivateLoad<scrFailBar>("overloadCounter", __instance).Get();
                else value = (float)new PrivateLoad<scrFailBar>("value", __instance).Get();

                if (value <= 1f) ___failCalled = false;
                }
        }
        
        [AdofaiPatch(
            "Hide_UI",
            "scrUIController",
            "WipeToBlack"
            )]
        public static class Hide_UI
        {
            public static void Prefix()
            {
                if (!Main.IsEnabled) return;
                if (!isReplayStart) return;
                if (data.start != 0 && scrController.instance.controller.currFloor.seqID == 0) return;
                if (ReplaySlider.move) return;
                reset();
                GCS.playDeathSound = true;
            }
        }
        

        
        
        [AdofaiPatch(
            "Invincible_Death",
            "scrController",
            "FailAction")]
        public static class Invincible_Death
        {
            public static bool Prefix()
            {
                if (Main.IsEnabled)
                {
                    if (isReplayStart)
                    {
                        if (scrController.instance.currentSeqID - 1 >= data.end)
                        {
                            GCS.playDeathSound = true;
                            return true;
                        }
                        return false;
                    }
                }
                return true;
            }
            
        }
        
        [AdofaiPatch(
            "NoStopMod_Compatibility",
            "scrController",
            "OnApplicationQuit")]
        public static class NoStopMod_Compatibility
        {
            public static bool Prefix()
            {
                if (Main.IsEnabled)
                {
                    if (nostopmod)
                    {
                        nostopmod = true;
                        return false;
                    }
                }
                return true;
            }
            
        }
        
        


        [AdofaiPatch(
            "PlayerControl_Update",
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
                    if (!isReplayStart) return;
                    if (!__instance.goShown) return;
                    if (data.angles.Count - 1 < index) return;
                    if (__instance.paused) return;
                    if (HitNow())
                    {
                        __instance.chosenplanet.angle = data.angles[index].angle;
                        PlanetHit();
                    }

                }
            }
        }
        
    }
}
