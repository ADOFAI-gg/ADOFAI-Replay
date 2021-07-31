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
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using Replay.Lib;

namespace Replay.UI
{
    public class ReplaySlider : MonoBehaviour
    {

        public float hSliderValue = 0.0F;
        public float end = 10.0f;
        public ReplayData replayData;
        public bool isStart = false;
        public static bool move = false;
        public string pauseplay = "||";
        public string dropdownString = "1.0X";



        public static void PlanetReset()
        {
            GCS.playDeathSound = false;
            scrController __instance = scrController.instance;
            
            __instance.ChangeState((Enum) scrController.States.Fail);
            if ((UnityEngine.Object) scrVfxPlus.instance != (UnityEngine.Object) null)
                scrVfxPlus.instance.enabled = false;
                
            if (__instance.missesOnCurrFloor.Any<scrMissIndicator>())
                __instance.missesOnCurrFloor.Last<scrMissIndicator>().StartBlinking();

            __instance.conductor.song.Stop();
            AudioManager.Instance.StopAllSounds();
            __instance.chosenplanet.PreDie();

            DOTween.To((DOGetter<float>) (() => __instance.chosenplanet.cosmeticRadius), (DOSetter<float>) (x => __instance.chosenplanet.cosmeticRadius = x), 0.0f, 0.5f).OnComplete<TweenerCore<float, float, FloatOptions>>(new TweenCallback(
                () =>
                {
                    __instance.ChangeState((Enum) scrController.States.Fail2);
                    __instance.mistakesManager.CalculatePercentAcc();
                    __instance.camy.ZoomOut();
                    __instance.chosenplanet.Die();
                    __instance.chosenplanet.other.Die();
                    GCS.playDeathSound = true;
                    
                }));
            
            foreach (ffxPlusBase lossEffect in __instance.lossEffects)
                lossEffect.StartEffect();
            
        }
        

        public static IEnumerator ResetCustomLevel(int id)
        {
            GCS.checkpointNum = id;
            scrController scrController = scrController.instance;
            if (GCS.standaloneLevelMode)
            {
                bool complete = false;
                scrUIController.instance.WipeToBlack(WipeDirection.StartsFromRight, (Action)(() => {
                    complete = true;
                    move = false;
                }));
                while (!complete)
                    yield return (object)null;
            }
            foreach (scrFloor listFloor in scrController.lm.listFloors)
            {
                listFloor.bottomglow.enabled = false;
                listFloor.topglow.enabled = false;
            }
           
            scrController.customLevel.ResetScene();
            scrController.customLevel.Play(GCS.checkpointNum);
            if (GCS.standaloneLevelMode)
            {
                yield return (object)null;
                scrUIController.instance.WipeFromBlack();
            }
        }



        public static void LevelStart(int id)
        {
            try
            {
                GCS.playDeathSound = false;
                move = true;
                scrController __instance = scrController.instance;
                GCS.checkpointNum = id;
                PlanetReset();
                    /*
                PrivateLoad<scrController> privateLoad = new PrivateLoad<scrController>("FailAction", scrController.instance);
                if (Main.version > 71) privateLoad.Call(new object[] {false, false});
                */
                

                if (scnEditor.instance != null)
                {
                    __instance.StartCoroutine(ResetCustomLevel(id));
                    //asdf.Invoke(scrController.instance, null);
                    return;
                }
                __instance.Restart();
                //__instance.conductor.StartCoroutine("DesyncFix");
                
                

                
            } catch (Exception e)
            {
                LogError.Show(ErrorCodes.CantForcePlay, e.Message);
            }
            
        }

        private void nextTo(int sec)
        {
            long nowtime = long.Parse(getSafeInfo(WorldReplay.index).time);
            
            if (long.Parse(replayData.angles[replayData.angles.Count - 1].time) < nowtime + sec * 1000) return;
            if (sec > 0)
            {
                for (int n = WorldReplay.index; n < replayData.angles.Count; n++)
                {
                    long time = long.Parse(getSafeInfo(n).time);
                    if (time >= nowtime + sec * 1000)
                    {
                        WorldReplay.index = n;
                        LevelStart(replayData.angles[n].num);
                        break;
                    }
                }
            }
            else
            {
                if (long.Parse(replayData.angles[0].time) > nowtime + sec * 1000) return;
                for (int n = WorldReplay.index; n >-1; n--)
                {
                    long time = long.Parse(getSafeInfo(n).time);
                    if (time <= nowtime + sec * 1000)
                    {
                        WorldReplay.index = n;
                        LevelStart(replayData.angles[n].num);
                        break;
                    }
                }
            }


        }

        private TileInfo getSafeInfo(int n)
        {
            TileInfo def = replayData.angles[replayData.angles.Count - 1];
            if (replayData == null) return def;
            if (replayData.angles == null) return def;
            if (replayData.angles.Count - 1 < n) return def;
            if (replayData.angles[n] == null) return def;
            return replayData.angles[n];

        }

        void OnGUI()
        {
            if (!isStart) return;
            if (replayData == null) return;
            if (replayData.angles == null) return;
            if (scrUIController.instance.transitionPanel.IsActive()) return;
            if (scrController.instance.pauseMenu.gameObject.activeSelf) return;
            //if (replayData.angles[WorldReplay.index] == null) return;

            GUIStyle buttonStyle, textStyle;
            buttonStyle = new GUIStyle(GUI.skin.button);
            buttonStyle.font = RDString.GetFontDataForLanguage(RDString.language).font;
            textStyle = new GUIStyle(GUI.skin.label);
            textStyle.font = RDString.GetFontDataForLanguage(RDString.language).font;
            textStyle.fontSize = 30;
            
            
            /*
            buttonStyle.normal.background = Main.smallcard;
            buttonStyle.hover.background = Main.smallcard;
            buttonStyle.active.background = Main.smallcard;*/

            dropdownString = MoreGUI.Dropdown(new Rect(Screen.width / 2 - 120, Screen.height - 30, 70, 20),new []{"0.25X","0.5X","1.0X","1.5X","2.0X"},dropdownString,buttonStyle,s=>
            {
                    GCS.currentSpeedRun = WorldReplay.originalSpeed*float.Parse(s.Replace("X",""));
                    GCS.nextSpeedRun = WorldReplay.originalSpeed*float.Parse(s.Replace("X",""));
                    //AudioManager.Instance.

                    
                    scrConductor.instance.song.pitch = float.Parse(s.Replace("X",""));

                    WorldReplay.index = 0;
                    LevelStart(0);
                    scrConductor.instance.StartCoroutine("DesyncFix");
            });

            if (GUI.Button(new Rect(Screen.width / 2 +100 - 300, Screen.height - 30, 50, 20), ">>", buttonStyle))
            {
                nextTo(10);
            }

            if (GUI.Button(new Rect(Screen.width / 2 - 300, Screen.height - 30, 50, 20), "<<", buttonStyle))
            {
                nextTo(-10);
            }

            if (pauseplay == "▶" && !scrController.instance.paused)
            {
                pauseplay = "||";
                scrController.instance.paused = false;
                scrController.instance.audioPaused = false;
                scrController.instance.enabled = true;
                Time.timeScale = (scrController.instance.paused ? 0f : 1f);

           
            }

            if (GUI.Button(new Rect(Screen.width / 2 - 300 + 50, Screen.height - 30, 50, 20), pauseplay, buttonStyle))
            {
                pauseplay = pauseplay == "||" ? "▶" : "||";
                scrController.instance.paused = !scrController.instance.paused;
                scrController.instance.audioPaused = scrController.instance.paused;
                scrController.instance.enabled = !scrController.instance.paused;
                Time.timeScale = (scrController.instance.paused ? 0f : 1f);

            }


            string timeInterval = Misc.ms2time(long.Parse(getSafeInfo(WorldReplay.index).time));
            string timeInterval2 = Misc.ms2time(long.Parse(replayData.time));

            hSliderValue = getSafeInfo(WorldReplay.index).num;

            GUI.Label(new Rect((Screen.width / 2)+180, Screen.height - 85, 200, 60),$"{timeInterval} / {timeInterval2}",textStyle);
            
            float newVal = GUI.HorizontalSlider(new Rect((Screen.width/2)-300, Screen.height - 50, 600, 70), hSliderValue, replayData.start, replayData.end);
            if (newVal != hSliderValue)
            {
                hSliderValue = newVal;
                int num = (int) Math.Round((double) newVal);
                int n = 0;
                foreach (TileInfo tile in replayData.angles)
                {

                    if (tile.num == num)
                    {
                        WorldReplay.index = n;
                        LevelStart(replayData.angles[WorldReplay.index].num);
                        hSliderValue = replayData.angles[WorldReplay.index].num;
                        break;
                    }

                    n++;

                }

                
            


            //LevelStart(10);
               
            }
           
        }
    }
}
