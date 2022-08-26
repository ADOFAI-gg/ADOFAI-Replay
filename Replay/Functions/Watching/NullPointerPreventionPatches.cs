using HarmonyLib;

namespace Replay.Functions.Watching
{
    [HarmonyPatch]
    public class NullPointerPreventionPatches
    {
        
        [HarmonyPatch(typeof(scrFailBar), "DidFail")]
        [HarmonyPrefix]
        public static void SafeNullPatch(ref scrController ___controller)
        {
            if (___controller == null)
                ___controller = scrController.instance;
        }


        [HarmonyPatch(typeof(scrCountdown), "Update")]
        [HarmonyPrefix]
        public static void SafeNull2Patch(ref scrController ___controller)
        {
            if (___controller == null)
                ___controller = scrController.instance;
        }
    }
}