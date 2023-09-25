using System;
using System.Collections;
using System.IO;
using System.Threading.Tasks;
using DG.Tweening;
using HarmonyLib;
using Replay.Functions.Core;
using Replay.Functions.Core.Types;
using Replay.Functions.Menu;
using Replay.UI;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;

namespace Replay.Functions.Watching
{
    public static class WatchReplay
    {
        public static bool IsPlaying;
        public static bool IsPaused;
        public static bool IsPlanetDied;
        public static bool IsDeathCam;
        public static bool IsLoading;
        public static bool IsResetLevel;
        public static float PatchedPitch;
        public static int OfficialStartAt;

        public static bool Reloaded;

        public static void RestartLevelAt(int seqID)
        {
            IsLoading = true;
            GCS.checkpointNum = seqID;
            if (scnGame.instance != null)
            {
                Time.timeScale = 1;
                scrController.instance.enabled = true;
                IsPlanetDied = false;
                scrConductor.instance.song.Stop();
                AudioManager.Instance.StopAllSounds();
                scrController.instance.instantExplode = false;
                IsResetLevel = true;
                scrController.instance.StartCoroutine(ResetCustomLevel());
            }
            else
            {
                OfficialStartAt = seqID;
                Reloaded = true;
                scrController.instance.Restart();
                
            }
        }
        
        public static double GetPlayTime()
        {
            var countdown = scrConductor.instance.crotchetAtStart * scrConductor.instance.adjustedCountdownTicks;
            var offset = scrConductor.currentPreset.inputOffset * 0.001;
            return ((scrConductor.instance.dspTime - scrConductor.instance.dspTimeSongPosZero) * PatchedPitch) - offset - (GCS.checkpointNum == 0? countdown:0);
        }
        
        
        public static IEnumerator ResetCustomLevel()
        {
            var controller = scrController.instance;
            if (ADOBase.isScnGame)
            {
                bool complete = false;
                scrUIController.instance.WipeToBlack(WipeDirection.StartsFromRight,  (() => complete = true));
                while (!complete)
                    yield return null;
            }
            else
                GC.Collect();
            
            scnGame.instance.ResetScene();
            scnGame.instance.Play(GCS.checkpointNum);
            typeof(scrController).GetField("transitioningLevel",AccessTools.all).SetValue(controller, false);
            
            if (ADOBase.isScnGame)
            {
                yield return null;
                scrUIController.instance.WipeFromBlack();
                IsResetLevel = false;
            }
        }
        
        // A method to solve the hall of mirror room bug
        public static void DisableAllEffects(bool kill = false){
            
            if(scrVfx.instance != null)
                scrVfx.instance.enabled = false;

            if (scrVfxPlus.instance != null)
            {
                if (kill)
                {
                    scrVfxPlus.instance.enabled = false;
                    scrVfxPlus.instance.Reset();
                }
                else
                {
                    foreach (var e in scrVfxPlus.instance.effects)
                    {
                        var typeName = e.GetType().ToString();
                        if (typeName == "ffxBloomPlus" || typeName == "ffxSetFilterPlus" ||
                            typeName == "ffxHallOfMirrorsPlus" || typeName == "ffxFlashPlus" ||
                            typeName == "ffxFlash" ||
                            typeName == "ffxShakeScreenPlus" || typeName == "ffxScreenScrollPlus" ||
                            typeName == "ffxScreenTilePlus")
                            e.triggered = true;
                    }
                }

                scrCamera.instance.camobj.clearFlags = CameraClearFlags.Depth;

                foreach (var com in scrCamera.instance.camobj.gameObject.GetComponents<MonoBehaviour>())
                {
                    if (com.ToString() == "Camera (UnityEngine.Transform)" ||
                        com.ToString() == "Camera (UnityEngine.Camera)" ||
                        com.ToString() == "Camera (UnityEngine.AudioListener)" ||
                        com.ToString() == "Camera (scrCamera)")
                        continue;
                    com.enabled = false;
                }
            }

            if (scrCamera.instance != null)
            {
                scrCamera.instance.enabled = true;
                scrCamera.instance.Bgcamstatic.enabled = true;
            }

        }
        
        public static bool CheckObjectIsInCamera(GameObject _target)
        {
            var selectedCamera = scrCamera.instance.camobj;
            var screenPoint = selectedCamera.WorldToViewportPoint(_target.transform.position);
            var onScreen = screenPoint.z > 0 && screenPoint.x > 0 && screenPoint.x < 1 && screenPoint.y > 0 && screenPoint.y < 1;
            return onScreen;
        }

        
        // Play a replay based on the replayInfo
        public static void Play(ReplayInfo rpl, bool deathcam = false)
        {
            if (rpl == null) return;
            IsDeathCam = deathcam;
            
            if(scrController.instance != null)
                scrController.instance.audioPaused = false;
            Time.timeScale = 1;
            BallBorder.CreatedBallBorders.Clear();
            
            if (rpl.IsOfficialLevel)
            {
                GCS.checkpointNum = rpl.StartTile;
                OfficialStartAt = rpl.StartTile;
                
                if (rpl.SongName.IsTaro())
                {
                    GCS.sceneToLoad = rpl.SongName;
                    SceneManager.LoadScene("scnLoading", LoadSceneMode.Single);
                }
                else if (rpl.SongName == "XI-X")
                {
                    GCS.internalLevelName = "XI-X";
                    SceneManager.LoadScene("scnGame", LoadSceneMode.Single);
                }
                else
                {

                    SceneManager.LoadScene(rpl.SongName, LoadSceneMode.Single);
                }

            }
            else
            {
                if (!File.Exists(rpl.Path)) return;
                OfficialStartAt = 0;

                SceneManager.LoadScene("scnGame", LoadSceneMode.Single);
                GCS.customLevelPaths = new string[1];
                GCS.customLevelPaths[0] = rpl.Path;
                GCS.checkpointNum = rpl.StartTile;
            }
            
            

            ReplayBasePatches.Start(rpl);
            KeyboradHook.OnStartInputs();
        }
        
          // Set planet color in player
        public static void SetPlanetColor(scrPlanet planet, ReplayInfo _playingReplayInfo)
        {
            if (planet.isExtra)
                return;
            
            planet.SetRainbow(false);
            if (Persistence.GetSamuraiMode(planet.isRed))
                planet.ToggleSamurai(true);
            planet.SetFaceMode(Persistence.GetFaceMode(planet.isRed), false);
            
            var playerColor =
                ReplayUtils.CustomColor2UnityColor(planet.isRed
                    ? _playingReplayInfo.RedPlanet
                    : _playingReplayInfo.BluePlanet);
            
            if (playerColor == scrPlanet.goldColor || GCS.d_forceGoldPlanets)
            {
                planet.DisableAllSpecialPlanets();
                planet.SwitchToGold();
            }
            else if (playerColor == scrPlanet.overseerColor)
            {
                planet.DisableAllSpecialPlanets();
                planet.SwitchToOverseer();
            }
            else if (playerColor == scrPlanet.rainbowColor)
            {
                planet.EnableCustomColor();
                planet.SetRainbow(true);
            }
            else if (playerColor == Color.red || playerColor == Color.blue)
            {
                var defaultColor = (playerColor == Color.red) ? 0 : 1;
                planet.DisableCustomColor(defaultColor);
            }
            else
            {
                planet.EnableCustomColor();
                var planetColor = playerColor;
                var tailColor = playerColor;
                if (playerColor == scrPlanet.transBlueColor)
                {
                    planetColor = new Color(0.3607843f, 0.7882353f, 0.9294118f);
                    tailColor = Color.white;
                }
                else if (playerColor == scrPlanet.transPinkColor)
                {
                    planetColor = new Color(0.9568627f, 0.6431373f, 0.7098039f);
                    tailColor = Color.white;
                }
                else if (playerColor == scrPlanet.nbYellowColor)
                {
                    planetColor = new Color(0.996f, 0.953f, 0.18f);
                    tailColor = Color.white;
                }
                else if (playerColor == scrPlanet.nbPurpleColor)
                {
                    planetColor = new Color(0.612f, 0.345f, 0.82f);
                    tailColor = Color.black;
                }
                planet.SetPlanetColor(planetColor);
                planet.SetTailColor(tailColor);
            }
            if (scrLogoText.instance != null)
            {
                scrLogoText.instance.UpdateColors();
            }
        }
    }
    
}