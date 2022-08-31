using System;
using System.Collections.Generic;
using HarmonyLib;
using Replay.Functions.Core.Types;
using Replay.Functions.Watching;
using UnityEngine;

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
        
        // All key inputs
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
                foreach (var k in Replay.AllKeyCodes)
                {
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
        
        [HarmonyPatch(typeof(scrController), "ShowHitText")]
        [HarmonyPrefix]
        public static void SetRealHitMargin(HitMargin hitMargin)
        {
            if (WatchReplay.IsPlaying) return;
            _heldPressInfo[_lastKeyCode].RealHitMargin = hitMargin;
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
            _lastKeyCode = GetInput();

            var t = new TileInfo
            {
                HitAngleRatio = planet.angle - planet.targetExitAngle,
                SeqID = controller.currentSeqID,
                Key = _lastKeyCode,
                NoFailHit = scrController.instance.noFailInfiniteMargin,
                HeldTime = Time.unscaledDeltaTime,
                Hitmargin = scrMisc.GetHitMargin((float)planet.angle, (float)planet.targetExitAngle,
                    planet.controller.isCW, (float)(planet.conductor.bpm * planet.controller.speed),
                    planet.conductor.song.pitch),
            };
            _heldPressInfo[_lastKeyCode] = t;
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


        [HarmonyPatch(typeof(scrController), "PlayerControl_Update")]
        [HarmonyPrefix]
        public static void KeyInputDetectPatch()
        {
            try
            {
                if (WatchReplay.IsPlaying) return;
                if (!scrController.instance.goShown) return;

                foreach (var keyCode in Replay.AllKeyCodes)
                {
                    if (Input.GetKey(keyCode))
                    {
                        if (_heldPressInfo.TryGetValue(keyCode, out var v))
                            _heldPressInfo[keyCode].HeldTime += Time.unscaledDeltaTime;
                    }
                }
            }
            catch
            {

            }
        }
    }
}