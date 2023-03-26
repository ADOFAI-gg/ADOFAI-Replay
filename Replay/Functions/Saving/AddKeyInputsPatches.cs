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

        private static KeyCode GetInput()
        {
            var keyCode = KeyCode.None;
            if (RDC.auto || scrController.instance.midspinInfiniteMargin ||
                scrController.instance.noFailInfiniteMargin) return keyCode;
            if (_lastFrame == Time.unscaledTime)
            {
                if (_pressedKeys.Count > 0)
                    keyCode = _pressedKeys.Dequeue();
            }
            else
            {
                _pressedKeys.Clear();
                if(Replay.AllKeyCodes == null)
                    Replay.AllKeyCodes = (KeyCode[])typeof(KeyCode).GetEnumValues();
                
                var keyCodes = SaveReplayPatches.LimitedKeys == null
                    ? Replay.AllKeyCodes
                    : SaveReplayPatches.LimitedKeys;
                
                foreach (var k in keyCodes)
                {
                    if(SaveReplayPatches.LimitedKeys == null && IgnoreKeys.TryGetValue(k, out var v)) continue;
                    if(_heldPressInfo.TryGetValue(k, out var v3)) continue;
                    if(Input.GetKeyDown(k))
                        _pressedKeys.Enqueue(k);
                }

                if (_pressedKeys.Count > 0)
                    keyCode = _pressedKeys.Dequeue();
            }

            return keyCode;
        }

   
        
        
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
                    if (obj.value is KeyCode value)
                        code = value;
                    else if (obj.value is AsyncKeyCode value2)
                        code = KeyLabelToKeyCode[value2.label];
                    if (!pressed.Contains(code))
                    {
                        if (AdofaiTweaksAPI.IsEnabled)
                        {
                            if (AdofaiTweaksAPI.ActiveKeys.Contains(code))
                            {
                                pressed.Add(code);
                                n++;
                            }
                        }
                        else
                            pressed.Add(code);
                    }
                }
            }

            if (AdofaiTweaksAPI.IsEnabled && n < 4)
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
            var isFreeroam = controller.currFloor.freeroam && !scrController.isGameWorld;
            
            if (!scrController.isGameWorld && !isFreeroam) return;
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

 
/*
        [HarmonyPatch(typeof(scrController), "PlayerControl_Update")]
        [HarmonyPrefix]
        public static void KeyInputDetectPatch()
        {
            try
            {
                if (WatchReplay.IsPlaying) return;
                if (!scrController.instance.goShown) return;
                if (SaveReplayPatches._states == CustomControllerStates.Fail ||
                    SaveReplayPatches._states == CustomControllerStates.Won) return;

                foreach (var keyCode in Replay.AllKeyCodes)
                {
                    if (Input.GetKeyDown(keyCode))
                    {
    
                        var k = new PressInfo()
                        {
                            Key = keyCode,
                            PressTime = (scrConductor.instance.dspTime - scrConductor.instance.dspTimeSongPosZero)
                        };
                        SaveReplayPatches._keyboardInfos.Add(k);
                        _pressHeldInfo[keyCode] = k;
                    }
                    if (Input.GetKey(keyCode))
                    {
                        if (_pressHeldInfo.TryGetValue(keyCode, out var v))
                            v.HeldTime += Time.unscaledDeltaTime;
                    }

                    if (Input.GetKeyUp(keyCode))
                        _pressHeldInfo.Remove(keyCode);
                }
            }
            catch
            {

            }
        }*/

        [HarmonyPatch(typeof(scrController), "PlayerControl_Update")]
        [HarmonyPrefix]
        public static void KeyInputDetectPatch()
        {
            try
            {
                if (WatchReplay.IsPlaying) return;
                if (!scrController.instance.goShown) return;

                var keyCodes = SaveReplayPatches.LimitedKeys == null
                    ? Replay.AllKeyCodes
                    : SaveReplayPatches.LimitedKeys;
                
                foreach (var keyCode in keyCodes)
                {
                    if(SaveReplayPatches.LimitedKeys == null && IgnoreKeys.TryGetValue(keyCode, out var v)) continue;
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