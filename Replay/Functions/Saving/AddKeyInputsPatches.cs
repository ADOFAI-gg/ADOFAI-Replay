using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Newgrounds;
using Replay.Functions.Core;
using Replay.Functions.Core.Types;
using Replay.Functions.Watching;
using SkyHook;
using UnityEngine;
using UnityModManagerNet;

namespace Replay.Functions.Saving
{
    [HarmonyPatch]
    public class AddKeyInputsPatches
    {
        private static Queue<KeyCode> _pressedKeys = new Queue<KeyCode>();
        private static Dictionary<KeyCode, TileInfo> _heldPressInfo = new Dictionary<KeyCode, TileInfo>();
        private static float _lastFrame;
        private static float _startTime;
        private static KeyCode _lastKeyCode;

        private static Dictionary<KeyCode, bool> IgnoreKeys = new Dictionary<KeyCode, bool>()
        {
            { KeyCode.End, true },
            { KeyCode.Tab, true },
            { KeyCode.Print, true },
            { KeyCode.LeftAlt, true },
            { KeyCode.LeftWindows, true },
            { KeyCode.RightWindows, true },
            { KeyCode.RightAlt, true },
            { KeyCode.RightControl, true },
            { KeyCode.F1, true },
            { KeyCode.F2, true },
            { KeyCode.F3, true },
            { KeyCode.F4, true },
            { KeyCode.F5, true },
            { KeyCode.F6, true },
            { KeyCode.F7, true },
            { KeyCode.F8, true },
            { KeyCode.F9, true },
            { KeyCode.F10, true },
            { KeyCode.F11, true },
            { KeyCode.F12, true },
        };

        public static Dictionary<KeyLabel, KeyCode> KeyLabelToKeyCode = new Dictionary<KeyLabel, KeyCode>();
        
        
        [HarmonyPatch(typeof(scrConductor), "StartMusicCo")]
        [HarmonyPostfix]
        public static void SetStartTimePatch()
        {
            if (WatchReplay.IsPlaying) return;
            _startTime = Time.time;

        }

        
        [HarmonyPatch(typeof(scrController), "PlayerControl_Enter")]
        [HarmonyPrefix]
        public static void WipeToBlackPatch()
        {
            if (WatchReplay.IsPlaying) return;
            pressed.Clear();
        }

        public static List<KeyCode> pressed = new List<KeyCode>();
        public static List<KeyCode> pressed2 = new List<KeyCode>();
        [HarmonyPatch(typeof(scrController), "CountValidKeysPressed")]
        [HarmonyPrefix]
        public static void CountValidKeysPressed()
        {
            var n = 0;
            foreach (var obj in RDInput.GetMainPressKeys())
            {
                if (n == 4) break;
                if (obj.value is KeyCode || obj.value is AsyncKeyCode)
                {
                    var code = KeyCode.None;
                    ushort asyncCode = 0;
                    if (obj.value is KeyCode value)
                        code = value;
                    else if (obj.value is AsyncKeyCode value2)
                    {
                        asyncCode = value2.key;
                        code = value2.key switch
                        {
  
                            21 => KeyCode.RightAlt,
                            92 => KeyCode.RightWindows,
                            93 => KeyCode.Menu,
                            25 => KeyCode.RightControl,
                            _ => KeyLabelToKeyCode[value2.label]
                        };
                    }

                    if (!pressed.Contains(code))
                    {
                        if (AdofaiTweaksAPI.IsEnabled)
                        {
                            if (AdofaiTweaksAPI.ActiveKeys.Contains(code) && !AsyncInputManager.isActive)
                            {
                                pressed.Add(code);
                                n++;
                            }  else if (AdofaiTweaksAPI.ActiveAsyncKeys.Contains(asyncCode) && AsyncInputManager.isActive)
                            {
                                pressed.Add(code);
                                n++;
                            }
                        }
                        else
                        {
                            pressed.Add(code);
                        }
                        if (!pressed2.Contains(code))
                            pressed2.Add(code);
                    }
                }
            }

            if (AdofaiTweaksAPI.IsEnabled && n < 4 && !AsyncInputManager.isActive)
            {
                foreach (var code in IgnoreKeys.Keys)
                {
                    if (n == 4) break;
                    if (Input.GetKeyDown(code))
                    {
                        if (!pressed.Contains(code))
                        {
                            if (AdofaiTweaksAPI.ActiveKeys.Contains(code))
                            {
                                pressed.Add(code);
                                if (!pressed2.Contains(code))
                                    pressed2.Add(code);
                                n++;
                            }

                        }
                    }
                }
            }
        }

        [HarmonyPatch(typeof(scrController), "Hit")]
        [HarmonyPrefix]
        public static void HitPatch()
        {
            if (WatchReplay.IsPlaying) return;
            var controller = scrController.instance;
            var planet = controller.chosenplanet;
            var isFreeroam = controller.currFloor.freeroam && !scrController.instance.gameworld;
            
            if (!scrController.instance.gameworld && !isFreeroam) return;
            if (scrController.instance.currFloor.midSpin) return;
            //if (scrController.instance.noFailInfiniteMargin) return; 

            //var keyCode = GetInput();
            var keyCode = pressed.Count == 0 || controller.currFloor.midSpin ? KeyCode.None : pressed[0];
            if(keyCode != KeyCode.None) pressed.RemoveAt(0);
            var flag = planet.currfloor.nextfloor != null && planet.currfloor.nextfloor.auto;
            var t = new TileInfo
            {
                HitAngleRatio = planet.angle - planet.targetExitAngle,
                SeqID = controller.currentSeqID,
                NoFailHit = scrController.instance.noFailInfiniteMargin,
                Hitmargin = ReplayUtils.GetHitMargin((float)planet.angle, (float)planet.targetExitAngle,
                    planet.controller.isCW, (float)(planet.conductor.bpm * planet.controller.speed),
                    planet.conductor.song.pitch),
                AutoHit = RDC.auto || flag,
                HeldTime = Time.unscaledDeltaTime,
                Key = keyCode,
                HitTimingPosition = ReplayUtils.UnityVector2MiniVector(planet.other.transform.position)
            };

            if (scrController.instance.planetList.Count == 3)
            {
                t.HitTimingPosition = ReplayUtils.UnityVector2MiniVector(planet.next.transform.position);
            }
            
            
            if(keyCode != KeyCode.None) _heldPressInfo[keyCode] = t;
            if (Replay.ReplayOption.CanICollectReplayFile == 1)
            {
                /*
                t.HitTime = Time.timeAsDouble - _startTime;
                t.RealHitAngle = planet.angle;
                t.TargetAngle = Math.Abs(planet.targetExitAngle) > 0.001? planet.targetExitAngle:0;*/
                t.IsFreeroam = controller.currFloor.freeroam;
                t.RelativeFloorAngle = Mathf.RoundToInt((float)(scrController.instance.currentSeqID == 0
                    ? (controller.currFloor.exitangle * (180 / Math.PI) + 90)
                    : ((controller.currFloor.angleLength * (180 / Math.PI)) % 360)));
                t.Speed = scrController.instance.speed;
            }
            
            
            SaveReplayPatches._pressInfos.Add(t);
            _lastFrame = Time.unscaledTime;
        }
        

        [HarmonyPatch(typeof(scrController), "PlayerControl_Update")]
        [HarmonyPrefix]
        public static void KeyInputDetectPatch()
        {
            try
            {
                if (WatchReplay.IsPlaying) return;
                if (!scrController.instance.goShown) return;

              

                    foreach (var keyCode in pressed2)
                    {
                        if (Input.GetKey(keyCode))
                        {
                            if (_heldPressInfo.TryGetValue(keyCode, out var held))
                                held.HeldTime += Time.unscaledDeltaTime;
                        }

                        if (Input.GetKeyUp(keyCode))
                        {
                            if (_heldPressInfo.TryGetValue(keyCode, out var held))
                                _heldPressInfo.Remove(keyCode);
                        }

                    }
                
               
                


            }
            catch
            {

            }
        }


    }
}