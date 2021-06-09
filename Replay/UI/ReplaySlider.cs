using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Replay.Patch;
using System.Collections;
using System.Threading;
using Replay.Clasz;
using System.Reflection;
using Replay.Lib;

namespace Replay.UI
{
    public class ReplaySlider : MonoBehaviour
    {

        public float hSliderValue = 0.0F;
        public float end = 10.0f;
        public ReplayData replayData;
        public bool isStart = false;
        

     

        private static IEnumerator ResetCustomLevel()
        {
            scrController __instance = scrController.instance;
            scrUIController.instance.WipeToBlack(WipeDirection.StartsFromRight);

            foreach (scrFloor scrFloor in __instance.lm.listFloors)
            {
                scrFloor.bottomglow.enabled = false;
                scrFloor.topglow.enabled = false;
            }
            __instance.customLevel.ResetScene();
            __instance.customLevel.Play(GCS.checkpointNum);
            if (GCS.standaloneLevelMode)
            {
                yield return null;
                scrUIController.instance.WipeFromBlack();
            }
            yield break;
        }

     

        public static void LevelStart(int id)
        {
            try
            {
                scrController __instance = scrController.instance;
                GCS.checkpointNum = id;
                __instance.conductor.song.Stop();
                AudioManager.Instance.StopAllSounds();
                //__instance.camy.ZoomOut();
                __instance.chosenplanet.Die();
                __instance.chosenplanet.other.Die();
                scrController.instance.Awake_Rewind();
                scrCamera.instance.Rewind();

                if (scnEditor.instance != null)
                {
                    __instance.StartCoroutine(ResetCustomLevel());
                    //asdf.Invoke(scrController.instance, null);
                    return;
                }
                __instance.Restart();
                __instance.conductor.StartCoroutine("DesyncFix");

                
            } catch (Exception e)
            {
                LogError.Show(ErrorCodes.CantForcePlay, e.Message);
            }
            
        }
        void OnGUI()
        {
            if (!isStart) return;
            if (replayData == null) return;
            if (replayData.angles == null) return;
            if (replayData.angles[WorldReplay.index] == null) return;

            string timeInterval = Misc.ms2time(long.Parse(replayData.angles[WorldReplay.index].time));
            string timeInterval2 = Misc.ms2time(long.Parse(replayData.time));

            hSliderValue = replayData.angles[WorldReplay.index].num;
            GUI.Label(new Rect((Screen.height / 2)-50, Screen.height - 50, 200, 60),$"{timeInterval} / {timeInterval2}");
            float newVal = GUI.HorizontalSlider(new Rect((Screen.height/2)+80, Screen.height - 50, 400, 60), hSliderValue, replayData.start, replayData.end);
            if(newVal!=hSliderValue)
            {
                hSliderValue = newVal;
                int num = (int)Math.Round((double)newVal);
                int seq = 0;
                int n = 0;
                foreach(TileInfo tile in replayData.angles)
                {
                    
                    if(tile.num==num)
                    {
                        seq = tile.num;
                        WorldReplay.index = n;
                        hSliderValue = replayData.angles[WorldReplay.index].num;
                        break;
                    }
                    n++;

                }

               LevelStart(seq);


                //LevelStart(10);
               
            }
           
        }
    }
}
