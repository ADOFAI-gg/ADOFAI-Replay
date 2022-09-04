using System;
using System.Collections.Generic;
using HarmonyLib;
using Replay.Functions.Core;
using Replay.Functions.Core.Types;
using Replay.Functions.Watching;
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
            { KeyCode.Escape, true },
            { KeyCode.End, true },
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

        private static KeyCode GetInput()
        {
            var keyCode = KeyCode.None;
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
                    if (Input.GetKeyDown((KeyCode)k))
                        _pressedKeys.Enqueue((KeyCode)k);
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
        
        /*
        [HarmonyPatch(typeof(scrController), "ShowHitText")]
        [HarmonyPrefix]
        public static void SetRealHitMargin(HitMargin hitMargin)
        {
            if (WatchReplay.IsPlaying) return; 
            _lastTileInfo[_lastKeyCode].RealHitMargin = hitMargin;
        }*/
        
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

            var keyCode = GetInput();
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
                Key = keyCode,
            };
            _heldPressInfo[keyCode] = t;
            if (Replay.ReplayOption.CanICollectReplayFile == 1)
            {
                t.HitTime = Time.timeAsDouble - _startTime;
                t.RealHitAngle = planet.angle;
                t.TargetAngle = Math.Abs(planet.targetExitAngle) > 0.001? planet.targetExitAngle:0;
                t.IsFreeroam = controller.currFloor.freeroam;
                t.RelativeFloorAngle = Mathf.RoundToInt((float)(scrController.instance.currentSeqID == 0
                    ? (controller.currFloor.exitangle * (180 / Math.PI) + 90)
                    : ((controller.currFloor.angleLength * (180 / Math.PI)) % 360)));
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
                }
            }
            catch
            {

            }
        }


    }
}