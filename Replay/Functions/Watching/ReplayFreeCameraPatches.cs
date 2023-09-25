using DG.Tweening;
using HarmonyLib;
using UnityEngine;

namespace Replay.Functions.Watching
{
    [HarmonyPatch]
    public class ReplayFreeCameraPatches
    {
        private static float _cameraScale;
        private static Vector3 _lastRotate;
        private static float _lastZoom;
        
        internal static bool _freeCameraMode = true;
        
        [HarmonyPatch(typeof(ffxCameraPlus), "StartEffect")]
        [HarmonyPrefix]
        public static bool StopCameraMovingPatch(ffxCameraPlus __instance)
        {
            if (_freeCameraMode) return true;
            _lastRotate = new Vector3(0, 0, __instance.targetRot + __instance.vfx.camAngle);
            _lastZoom = __instance.targetZoom;
            return false;
        }

        
        [HarmonyPatch(typeof(scnGame), "Update")]
        [HarmonyPrefix]
        public static void CameraMovePatch()
        {
            if (WatchReplay.IsPlaying)
            {
                scrCamera.instance.camobj.enabled = true;
                if (Input.GetKeyDown(KeyCode.B))
                {
                    _freeCameraMode = !_freeCameraMode;
                    if (_freeCameraMode)
                    {
                        scrCamera.instance.timer = 10000f;
                        scrCamera.instance.transform.localEulerAngles = _lastRotate;
                        scrCamera.instance.zoomSize = _lastZoom;
                    }
                    else
                    {
                        _lastRotate = scrCamera.instance.transform.localEulerAngles;
                        _lastZoom = scrCamera.instance.zoomSize;
                        var moveTween = (Tween)typeof(ffxCameraPlus).GetField("moveTween", AccessTools.all)
                            ?.GetValue(null);
                        var rotationTween = (Tween)typeof(ffxCameraPlus).GetField("rotationTween", AccessTools.all)
                            ?.GetValue(null);
                        var zoomTween = (Tween)typeof(ffxCameraPlus).GetField("zoomTween", AccessTools.all)
                            ?.GetValue(null);
                        moveTween?.Kill();
                        rotationTween?.Kill();
                        zoomTween?.Kill();
                        scrCamera.instance.transform.localEulerAngles = new Vector3(0, 0, 0);
                    }

                    scrCamera.instance.enabled = _freeCameraMode;
                }
            }

            if (_freeCameraMode) return;
            if (scrCamera.instance.camobj.transform.localEulerAngles != Vector3.zero)
                scrCamera.instance.camobj.transform.localEulerAngles = new Vector3(0, 0, 0);

            if (scrCamera.instance.transform.parent.localEulerAngles != Vector3.zero)
                scrCamera.instance.transform.parent.localEulerAngles = new Vector3(0, 0, 0);

            var pos = scrCamera.instance.transform.position;

            _cameraScale = scrCamera.instance.camobj.orthographicSize;

            if (Input.GetMouseButton(0))
                scrCamera.instance.transform.position = new Vector3(
                    pos.x - (Input.GetAxis("Mouse X") * 10f * _cameraScale / 250f),
                    pos.y - (Input.GetAxis("Mouse Y") * 5f * _cameraScale / 250f), pos.z);

            scrCamera.instance.camobj.orthographicSize -= Input.GetAxisRaw("Mouse ScrollWheel") * 10f;
            if (scrCamera.instance.camobj.orthographicSize < 1)
                scrCamera.instance.camobj.orthographicSize = 1;

        }
    }
}