using HarmonyLib;
using UnityEngine;

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
        
        [HarmonyPatch(typeof(scrBarMaker), "Start")]
        [HarmonyPrefix]
        public static bool SafeNullPatch3(scrBarMaker __instance)
        {
            __instance.conductoraudio = scrConductor.instance.GetComponent<AudioSource>();
            __instance.controller = scrController.instance;
            __instance.staticcam = scrCamera.instance.Bgcamstatic;
            float num = __instance.staticcam.orthographicSize * __instance.staticcam.aspect * 2f;
            float num2 = -num / 2f + __instance.barwidth / 2f;
            int num3 = Mathf.FloorToInt(num / __instance.barwidth) + 1;
            for (int i = 0; i < num3; i++)
            {
                //new Vector3(num2 + (float)i * __instance.barwidth, 0f, 0f);
                GameObject gameObject = Object.Instantiate(__instance.objBar);
                scrBgbarnew component = gameObject.GetComponent<scrBgbarnew>();
                __instance.listBgbars.Add(component);
                __instance.listObjbars.Add(gameObject);
                gameObject.SetActive(true);
                float x = num2 + (float)i * __instance.barwidth;
                gameObject.transform.position = scrMisc.setX(gameObject.transform.position, x);
                gameObject.transform.position = scrMisc.setY(gameObject.transform.position, __instance.transform.position.y);
                gameObject.GetComponent<scrParallax>().SetNewStartPosition(gameObject.transform.position);
                gameObject.transform.localScale = scrMisc.setX(gameObject.transform.localScale, __instance.barwidth - __instance.barborder);
                gameObject.GetComponent<scrParallax>().enabled = true;
                gameObject.transform.parent = __instance.gameObject.transform;
            }
            for (int j = 0; j < num3; j++)
            {
                __instance.listBgbars[j].freq = Mathf.RoundToInt((float)j / (float)num3 * (float)(1000 - 50) + (float)50 + 20f);
                __instance.listBgbars[j].barmaker = __instance;
            }
            __instance.objBar.SetActive(false);
            return false;
        }

        
        [HarmonyPatch(typeof(scrCountdown), "Update")]
        [HarmonyPrefix]
        public static void SafeNull2Patch(ref scrController ___controller)
        {
            if (___controller == null)
                ___controller = scrController.instance;
        }
        
        public static void OverlayerSafeNull()
        {
            var __instance = scrUIController.instance.txtCountdown.GetComponent<scrCountdown>();
            
            if(__instance.controller == null)
                __instance.controller = scrController.instance;
            
            if(__instance.conductor == null)
                __instance.conductor = scrConductor.instance;
        }
    }
}