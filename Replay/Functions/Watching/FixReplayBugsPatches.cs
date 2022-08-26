using HarmonyLib;
using Replay.Functions.Menu;

namespace Replay.Functions.Watching
{
    [HarmonyPatch]
    public class FixReplayBugsPatches
    {
        // Fix Tile Glow Bug
        internal static void SetTileGlow()
        {
            for (var n = GCS.checkpointNum; n <= ReplayBasePatches._playingReplayInfo.EndTile; n++)
            {
                var listFloor = scrLevelMaker.instance.listFloors[n];
                if ((bool)listFloor.bottomglow)
                    listFloor.bottomglow.enabled = false;
                listFloor.topglow.enabled = false;
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
        public static void FixSongNotPlayingBugPatch()
        {
            if (scrConductor.instance.song != null)
                scrConductor.instance.song.volume = 1;

            if (WatchReplay.IsPlaying)
                FixFreeroamBug();
        }

        [HarmonyPatch(typeof(SoundEffect), "Awake")]
        [HarmonyPrefix]
        public static void WTFWhyBreakThis(SoundEffect __instance)
        {
            __instance.Sound = ADOBase.gc.soundEffects[(int)SfxSound.MenuSquelch];
        }
    }
}