using System;
using DG.Tweening;
using HarmonyLib;
using Replay.Functions.Core;
using Replay.Functions.Menu;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Replay.Functions.Watching
{
    [HarmonyPatch]
    public class FixReplayBugsPatches
    {
        // Fix Tile Glow Bug
        internal static void SetTileGlow()
        {
            // 알파 업데이트가 정식이 되면 꼭 바꿀 것!!
            // 성능 이슈!!!!!!!!
            
            var floors = scrLevelMaker.instance.listFloors;
            var nextFloor = ReplayUtils.GetSafeList(floors, GCS.checkpointNum + 1);
            if (nextFloor != null)
            {
                var topGlow = (SpriteRenderer)typeof(scrFloor)
                    .GetField(Replay.IsAlpha ? "topGlow" : "topglow", AccessTools.all)?.GetValue(nextFloor);
                if (topGlow != null)
                    if (!topGlow.enabled)
                    {
                        return;
                    }
            }

            for (var n = GCS.checkpointNum; n <= ReplayBasePatches._playingReplayInfo.EndTile; n++)
            {
                
                var listFloor = floors[n];
                var bottomGlow = (SpriteRenderer)typeof(scrFloor)
                    .GetField(Replay.IsAlpha ? "bottomGlow" : "bottomglow", AccessTools.all)?.GetValue(listFloor);
                if (bottomGlow != null)
                    bottomGlow.enabled = false;
                
                var topGlow = (SpriteRenderer)typeof(scrFloor)
                    .GetField(Replay.IsAlpha ? "topGlow" : "topglow", AccessTools.all)?.GetValue(listFloor);
                if (topGlow != null)
                    topGlow.enabled = false;
            }
        }

        // Fixed a bug where Freeroam was not visible
        private static void FixFreeroamBug()
        {
            foreach (var l in scrLevelMaker.instance.listFreeroam)
            {
                foreach (scrFloor f in l)
                {
                    f.ToggleCollider(true);
                    f.isLandable = true;
                    f.opacity = 1;
                    f.freeroam = true;
                    f.freeroamGenerated = true;
                }

                var p = l.parentFloor;
                p.enabled = true;
                p.freeroam = true;
                p.freeroamGenerated = false;
            }
        }
        
        [HarmonyPatch(typeof(scrController), "PlayerControl_Enter")]
        [HarmonyPrefix]
        public static void FixFreeroam()
        {
            if (scrConductor.instance.song != null)
            {
                if(scrConductor.instance.song.volume == 0)
                    scrConductor.instance.song.volume = 1;
            }
            
            if (WatchReplay.IsPlaying)
                FixFreeroamBug();
        }
        
        [HarmonyPatch(typeof(scrController), "Checkpoint_Enter")]
        [HarmonyPrefix]
        public static void FixTopGlow()
        {
            if (!WatchReplay.IsPlaying) return;
            SetTileGlow();
            WatchReplay.SetPlanetColor(scrController.instance.chosenplanet, ReplayBasePatches._playingReplayInfo);
            WatchReplay.SetPlanetColor(scrController.instance.chosenplanet.other, ReplayBasePatches._playingReplayInfo);
        }


        [HarmonyPatch(typeof(scrMistakesManager), "SaveCustom")]
        [HarmonyPrefix]
        public static bool FixSaveCustom(ref EndLevelType __result)
        {
            if (!WatchReplay.IsPlaying) return true;
            __result = EndLevelType.None;
            return false;
        }
        
        [HarmonyPatch(typeof(scrUIController), "WipeFromBlack")]
        [HarmonyPrefix]
        public static bool FixBlackScreen(scrUIController __instance)
        {
            if (WatchReplay.IsPlaying)
                scrUIController.wipeDirection = WipeDirection.StartsFromRight;
            
            scrSfx.instance.PlaySfx(SfxSound.ScreenWipeIn, 0.5f);
            __instance.transitionPanel.gameObject.SetActive(true);
            __instance.transitionPanel.color = Color.black;
            RectTransform rectTransform = __instance.transitionPanel.rectTransform;
            float x = (scrUIController.wipeDirection == WipeDirection.StartsFromLeft) ? 1f : 0f;
            rectTransform.pivot = new Vector2(x, 0.5f);
            rectTransform.localScale = Vector3.one;
            rectTransform.DOKill(false);
            float duration = GCS.speedTrialMode ? (0.3f / GCS.currentSpeedTrial) : 0.3f;
            var tween = rectTransform.DOScaleX(0f, duration).SetEase(Ease.InOutQuint).SetUpdate(true).OnComplete(
                delegate
                {
                    __instance.transitionPanel.gameObject.SetActive(false);
                });
            tween.OnKill(() =>
            {
                __instance.transitionPanel.gameObject.SetActive(false);
            });
            return false;
        }

        [HarmonyPatch(typeof(scrUIController), "WipeToBlack")]
        [HarmonyPrefix]
        public static bool FixBlackScreen2(scrUIController __instance, WipeDirection direction, Action onComplete,
            Tweener ___wipeToBlack)
        {
            if (WatchReplay.IsPlaying)
                ReplayBasePatches.Reset();

            if (ReplayBasePatches._progressDisplayerCancel)
            {
                ReplayBasePatches._progressDisplayerCancel = false;
                return false;
            }

            return true;
        }




        [HarmonyPatch(typeof(scrConductor), "StartMusicCo")]
        [HarmonyPostfix]
        public static void FixSongNotPlayingBugPatch()
        {
            if (scrConductor.instance.song != null)
            {
                if(scrConductor.instance.song.volume == 0)
                    scrConductor.instance.song.volume = 1;
            }
        }


        [HarmonyPatch(typeof(SoundEffect), "Awake")]
        [HarmonyPrefix]
        public static void WTFWhyBreakThis(SoundEffect __instance)
        {
            __instance.Sound = ADOBase.gc.soundEffects[(int)SfxSound.MenuSquelch];
        }
    }
}