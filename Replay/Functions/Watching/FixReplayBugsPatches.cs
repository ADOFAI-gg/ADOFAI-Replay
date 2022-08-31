using HarmonyLib;
using Replay.Functions.Core;
using Replay.Functions.Menu;
using UnityEngine;

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
            if(WatchReplay.IsPlaying)
                SetTileGlow();
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