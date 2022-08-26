using System;
using DG.Tweening;
using HarmonyLib;
using Replay.Functions.Core;
using Replay.Functions.Menu;
using UnityEngine;

namespace Replay.Functions.Watching
{
    [HarmonyPatch]
    public class MouseAndUIMovementPatches
    {
        private static float _showingCursorTime;
        private static Tween tween1, tween2;
        private static float y = -999, y2;
        private static RectTransform _replayToolUI;
        private static RectTransform _errorMeter;
        private static bool _hided;

        private static void HideUI()
        {
            if (_hided) return;
            
            tween1?.Kill();
            tween2?.Kill();
            
            Cursor.visible = false;

            tween1 = ReplayUtils.DOAnchorPos(_replayToolUI, new Vector2(_replayToolUI.anchoredPosition.x, y - 70), 0.5f).SetUpdate(true);
            tween2 = ReplayUtils.DOAnchorPos(_errorMeter, new Vector2(_errorMeter.anchoredPosition.x, y2 - 100), 0.5f).SetUpdate(true);
            _hided = true;
        }
        
        private static void ShowUI()
        {
            if (!_hided) return;
            
            tween1?.Kill();
            tween2?.Kill();
            
            Cursor.visible = true;

            tween1 = ReplayUtils.DOAnchorPos(_replayToolUI, new Vector2(_replayToolUI.anchoredPosition.x, y), 0.5f).SetUpdate(true);
            tween2 = ReplayUtils.DOAnchorPos(_errorMeter, new Vector2(_errorMeter.anchoredPosition.x, y2), 0.5f).SetUpdate(true);
            _hided = false;
        }
        
        [HarmonyPatch(typeof(scrHitErrorMeter), "UpdateLayout")]
        [HarmonyPostfix]
        public static void SetHitErrorMeterYPatch(scrHitErrorMeter __instance)
        {
            if (!WatchReplay.IsPlaying) return;
            var p = __instance.wrapperRectTransform.anchoredPosition;
            __instance.wrapperRectTransform.anchoredPosition = new Vector2(0, p.y + 100);
            _errorMeter = __instance.wrapperRectTransform;
            y2 = _errorMeter.anchoredPosition.y;
        }

        [HarmonyPatch(typeof(PauseMenu), "UpdateCursorVisibility")]
        [HarmonyPostfix]
        public static void ShowCursor()
        {
            if (!WatchReplay.IsPlaying) return;
            Cursor.visible = true;
        }
        
        [HarmonyPatch(typeof(PauseMenu), "OnEnable")]
        [HarmonyPostfix]
        public static void ShowCursor2()
        {
            if (!WatchReplay.IsPlaying) return;
            Cursor.visible = true;
        }

        [HarmonyPatch(typeof(scrConductor), "Update")]
        [HarmonyPrefix]
        public static void CheckMouseCursorMovement()
        {
            if (!WatchReplay.IsPlaying) return;
            
            if (_replayToolUI == null)
            {
                _replayToolUI = ReplayUI.Instance.Back10s.transform.parent.GetComponent<RectTransform>();
                y = _replayToolUI.anchoredPosition.y;
            }

       
            if (Math.Abs(Input.GetAxis("Mouse X")) > 0.5f || Math.Abs(Input.GetAxis("Mouse Y")) > 0.5f || WatchReplay.IsPaused ||(ReplayBasePatches._playingReplayInfo.EndTile - 1 < scrController.instance.currentSeqID))
            {
                ShowUI();
                _showingCursorTime = 0;
            }
            else
            {
                _showingCursorTime += Time.unscaledDeltaTime;
                if(_showingCursorTime > 3f)
                    HideUI();
            }
        }
        
        
    }
}