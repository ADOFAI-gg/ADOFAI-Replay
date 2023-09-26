using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using DG.Tweening;
using Discord;
using HarmonyLib;
using Newgrounds;
using Replay.Functions.Core;
using Replay.Functions.Core.Types;
using Replay.Functions.Saving;
using Replay.Functions.Watching;
using Replay.UI;
using ReplayLoader;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityModManagerNet;
using Debug = System.Diagnostics.Debug;
using Object = UnityEngine.Object;

namespace Replay.Functions.Watching
{
    [HarmonyPatch]
    public class ReplayBasePatches
    {
        private const int max_range = 50;
        
        private static bool _forceMove;
        private static bool _forcePlay;
        private static bool _forceColor;
        private static bool _replayPlanetHit;
        private static bool _beforeNoStopModEnabled;
        private static int _lastSeqID = -999;
        private static bool _dontDie;
        private static int _index;
        private static int _pressIndex;
        private static bool _isOldReplay;
        private static UnityModManager.ModEntry _noStopModModEntry;
        

        private static List<Tween> _requestedHold = new List<Tween>();
        public static Dictionary<int, BallBorder> _ballBorders = new Dictionary<int, BallBorder>();

        internal static bool _paused;
        internal static bool _progressDisplayerCancel;
        internal static ReplayInfo _playingReplayInfo;

        // Disable NoStopMod if installed
        // Not Used

        /*
        private static void DisableNoStopMod()
        {
            if(!Replay.IsUsingNoStopMod) return;
            
            _noStopModModEntry ??= UnityModManager.FindMod("NoStopMod");
            
            if (_noStopModModEntry == null) return;
            _beforeNoStopModEnabled = _noStopModModEntry.Active;
            if (_beforeNoStopModEnabled)
                _noStopModModEntry.OnToggle(_noStopModModEntry, false);
            
            TestGUI.var1 = _index;
        }*/


        // reset all data
        public static void Reset()
        {
            //BallBorder.CreatedBallBorders.Clear();
            _ballBorders.Clear();
            foreach (var n in Object.FindObjectsOfType<BallBorder>())
            {
                n.gameObject.SetActive(false);
            }
            
            if (_forceMove)
            {
                _forceMove = false;
                return;
            }

            foreach (var h in _requestedHold)
                h.Kill();
            _requestedHold.Clear();

            
                _playingReplayInfo = null;
            _index = 0;
            _dontDie = false;
            Cursor.visible = true;

            ReplayFreeCameraPatches._freeCameraMode = true;
            WatchReplay.IsPlaying = false;

            GCS.customLevelPaths = null;
            GCS.checkpointNum = 0;
            ReplayUI.Instance.InGameUI.SetActive(false);
            
            
            if (scnEditor.instance != null)
                scnEditor.instance.floorButtonCanvas.gameObject.SetActive(true);

            if (scrController.instance != null)
            {
                scrController.instance.errorMeter.UpdateLayout(Persistence.hitErrorMeterSize,
                    Persistence.hitErrorMeterShape);
            }

            KeyboradHook.OnEndInputs();
            
            
            GC.Collect();
            

            /*
            if (!Replay.IsUsingNoStopMod) return;
            if (_noStopModModEntry == null) return;
            if (_beforeNoStopModEnabled)
                _noStopModModEntry.OnToggle(_noStopModModEntry, true);
            */
        }

        // Detect if index range is exceeded
        private static bool CheckInvalidIndex()
        {
            return _playingReplayInfo.Tiles.Length - 1 < _index || _index < 0;
        }
        

        /*
        [HarmonyPatch(typeof(scrPlanet))]
        [HarmonyPatch("Update_RefreshAngles")]
        public static class Dialog_FormCaravan_CheckForErrors_Patch
        {
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var started = false;
                CodeInstruction prev = null;
                var list = new List<CodeInstruction>();
                

                foreach (var i in instructions)
                {
                    if (!started)
                        list.Add(i);
                    

                    if (prev != null)
                    {
                        if (prev.opcode == OpCodes.Ldsfld &&
                            prev.operand?.ToString() == "System.Boolean d_stationary" && i.opcode == OpCodes.Brtrue_S)
                            started = true;
                        if (i.opcode == OpCodes.Stfld && i.operand?.ToString() == "System.Double angle")
                            started = false;

                    }
                    prev = i;
                }
                return list.AsEnumerable();
            }
        }*/

        // Planet angle when pressed by player
        private static double GetAngle()
        {
            var planet = scrController.instance.chosenplanet;
            if (CheckInvalidIndex())
                return planet.targetExitAngle;
            
            var tileInfo = _playingReplayInfo.Tiles[_index];
            return tileInfo.HitAngleRatio + planet.targetExitAngle;
        }

        // Hit the planet in replay
        private static void ReplayHit()
        {
            if (CheckInvalidIndex())
                return;

            var r = _playingReplayInfo.Tiles[_index];
            var angle = GetAngle();
            //Replay.Log("111", _index, _playingReplayInfo.Tiles[_index].SeqID, scrController.instance.currentSeqID, angle, scrController.instance.chosenplanet.angle, scrController.instance.isCW, _playingReplayInfo.Tiles[_index].Hitmargin);



            scrController.instance.consecMultipressCounter = 0;
            scrController.instance.multipressPenalty = false;
            scrController.instance.failbar.multipressCounter = 0;
            scrController.instance.failbar.overloadCounter = 0;
            
            

            if (scrController.instance.currentSeqID == r.SeqID)
            {
                if (!scrController.instance.currFloor.midSpin)
                {
                    scrController.instance.chosenplanet.angle = angle;
                    scrController.instance.chosenplanet.cachedAngle = angle;
                }
                
                //if(Replay.ReplayOption.showInputTiming) BallBorder.Create(scrController.instance.chosenplanet.other.transform.position,r.Hitmargin);

                if (Replay.ReplayOption.showInputTiming && _isOldReplay)
                {
                    var b = BallBorder.Create(scrController.instance.chosenplanet.other.transform.position, r.Hitmargin,
                        _index);
                    _ballBorders[_index] = b;
                }



                if (Replay.ReplayOption.showInputTiming &&
                    ReplayUtils.CanGet(_playingReplayInfo.Tiles.Length, _index + max_range) && !_isOldReplay)
                    {
                        if (!_ballBorders.TryGetValue(_index + max_range, out var v))
                        {
                            var t = _playingReplayInfo.Tiles[_index + max_range];
                            var b = BallBorder.Create(
                                ReplayUtils.MiniVector2UnityVector(t.HitTimingPosition),
                                t.Hitmargin, 1f);
                            _ballBorders[_index + max_range] = b;
                        }
  
                    }
                    
                    if (Replay.ReplayOption.showInputTiming &&
                        ReplayUtils.CanGet(_playingReplayInfo.Tiles.Length, _index - max_range))
                    {
                        if (_ballBorders.TryGetValue(_index - max_range, out var v2))
                        {
                            v2.gameObject.SetActive(false);
                        }
  
                    }
                

                _replayPlanetHit = true;
                scrController.instance.noFailInfiniteMargin = r.NoFailHit;
                scrController.instance.Hit();
                scrController.instance.noFailInfiniteMargin = false;
                _replayPlanetHit = false;
            }
            
            var k = _playingReplayInfo.Tiles[_index].Key;
            KeyboradHook.OnKeyPressed(k);
            var tween = DOVirtual.DelayedCall(
                _playingReplayInfo.Tiles[_index].HeldTime / (GCS.currentSpeedTrial / _playingReplayInfo.Speed),
                () => { KeyboradHook.OnKeyReleased(k); });
            tween.OnComplete(() => { _requestedHold.Remove(tween); });
            tween.OnKill(() => { KeyboradHook.OnKeyReleased(k); });
            _index++;
        }

        /*
        private static void KeyboardPress()
        {
            if (_pressIndex < 0 || _pressIndex >= _playingReplayInfo.Presses.Length) return;
            var index = _pressIndex;
            var k = _playingReplayInfo.Presses[index].Key;
            KeyboradHook.OnKeyPressed(k);
            var tween = DOVirtual.DelayedCall(
                _playingReplayInfo.Presses[index].HeldTime / (GCS.currentSpeedTrial / _playingReplayInfo.Speed),
                () => { KeyboradHook.OnKeyReleased(k); });
            tween.OnComplete(() => { _requestedHold.Remove(tween); });
            tween.OnKill(() => { KeyboradHook.OnKeyReleased(k); });
            _pressIndex++;
        }*/


        // replay play
        public static void Start(ReplayInfo replayInfo)
        {
            WatchReplay.IsLoading = true;
            WatchReplay.IsPlaying = true;
            WatchReplay.IsPlanetDied = false;
            _forcePlay = true;
            _playingReplayInfo = replayInfo;
            _index = 0;
            _pressIndex = 0;
            _isOldReplay = ReplayUtils.IsEmptyVector(replayInfo.Tiles[1].HitTimingPosition);
            Cursor.visible = true;

            GCS.speedTrialMode = false;
            GCS.currentSpeedTrial = replayInfo.Speed;
            GCS.nextSpeedRun = replayInfo.Speed;
            WatchReplay.PatchedPitch = GCS.currentSpeedTrial;
            
            //BallBorder.CreatedBallBorders.Clear();

            //DisableNoStopMod();
        }

        // Restart the replay from that point
        public static void ReplayStartAt(int seqID)
        {
            scrController.instance.audioPaused = true;
            var floors = scrLevelMaker.instance.listFloors;
            if (seqID > floors[_playingReplayInfo.EndTile].seqID) seqID = floors[_playingReplayInfo.EndTile].seqID;
            if (seqID < floors[_playingReplayInfo.StartTile].seqID) seqID = floors[_playingReplayInfo.StartTile].seqID;

            _forceMove = true;
            _index = _playingReplayInfo.Tiles.ToList().FindIndex(x => x.SeqID == seqID);

            foreach (var h in _requestedHold)
                h.Kill();
            _requestedHold.Clear();

            WatchReplay.RestartLevelAt(seqID);
            //_ballBorders.Clear();
            
            //BallBorder.CreatedBallBorders.Clear();
        }

        // when is the hit timing
        private static bool IsHitNow()
        {
            if (scrController.instance == null) return false;
            if (scrController.instance.currentState != States.PlayerControl)
                return false;
            if (WatchReplay.IsPaused) return false;

            if (_index >= _playingReplayInfo.Tiles.Length && _playingReplayInfo.EndTile > scrController.instance.currentSeqID)
                return true;

            if (CheckInvalidIndex())
                return false;


            var planet = scrController.instance.chosenplanet;
            //Replay.Log(scrController.instance.currentSeqID, _index, _playingReplayInfo.Tiles[_index].SeqID, GetAngle(), planet.angle, planet.controller.isCW);

            //if (scrController.instance.currentSeqID > _playingReplayInfo.Tiles[_index].SeqID) return true;
            

            var angle = GetAngle();
            var angleOverd = (scrController.instance.isCW && planet.angle >= angle) ||
                             (!scrController.instance.isCW && planet.angle <= angle);
            var validTile = scrController.instance.currentSeqID == _playingReplayInfo.Tiles[_index].SeqID;

           //Replay.Log(_index, _playingReplayInfo.Tiles[_index].SeqID, scrController.instance.currentSeqID, angle, planet.angle, scrController.instance.isCW, _playingReplayInfo.Tiles[_index].Hitmargin);

            if (scrController.instance.currFloor.freeroam)
            {
                var nextTileInfoVailed = ReplayUtils.GetSafeList(_playingReplayInfo.Tiles, _index + 1).SeqID ==
                                         scrController.instance.currFloor.seqID;
                if (angleOverd && validTile && nextTileInfoVailed)
                    return true;
            }
            

            return scrController.instance.gameworld && angleOverd && validTile;
        }
        
        
        [HarmonyPatch(typeof(scrController), "Awake")]
        [HarmonyPostfix]
        public static void SetStartAtPatch()
        {
            _paused = false;
            if (!WatchReplay.IsPlaying) return;
            if (!_playingReplayInfo.IsOfficialLevel) return;
            GCS.checkpointNum = WatchReplay.OfficialStartAt;
            scrController.instance.chosenplanet.hittable = false;
            scrController.instance.chosenplanet.other.hittable = false;
            
            //BallBorder.CreatedBallBorders.Clear();

        }

       


        [HarmonyPatch(typeof(scnGame), "Play")]
        [HarmonyPrefix]
        public static void ForceCustomSeqIDPlayPatch(ref int seqID)
        {
            if (!WatchReplay.IsPlaying) return;
            if (!_forcePlay) return;

            _forcePlay = false;
            seqID = _playingReplayInfo.StartTile;
            GCS.checkpointNum = _playingReplayInfo.StartTile;
            scrController.instance.currentSeqID = _playingReplayInfo.StartTile;
            
            //BallBorder.CreatedBallBorders.Clear();

        }


        [HarmonyPatch(typeof(scrController), "Start")]
        [HarmonyPrefix]
        public static void ForceOfficialSeqIDPlayPatch()
        {
            _paused = scrController.instance.paused;
            
            if (!WatchReplay.IsPlaying) return;
            if (!scrController.instance.gameworld) return;
            if (!_playingReplayInfo.IsOfficialLevel) return;
            if (!_forcePlay) return;
            _forcePlay = false;

            GCS.checkpointNum = _playingReplayInfo.StartTile;
            scrController.instance.currentSeqID = _playingReplayInfo.StartTile;
            
            //BallBorder.CreatedBallBorders.Clear();

        }


        [HarmonyPatch(typeof(scrController), "QuitToMainMenu")]
        [HarmonyPrefix]
        public static bool QuitToMainMenuInReplayingPatch(ref bool ___exitingToMainMenu)
        {
            if (!WatchReplay.IsPlaying) return true;
            Reset();
            
            scrController.instance.paused = true;
            scrController.instance.enabled = false;


            WatchReplay.DisableAllEffects(true);

            ___exitingToMainMenu = true;
            GC.Collect();
            ADOBase.audioManager.StopLoadingMP3File();
            
            GCS.worldEntrance = null;
            GCS.checkpointNum = 0;
            GCS.currentSpeedTrial = 1f;

            scrController.deaths = 0;
            scrConductor.instance.hasSongStarted = false;
            scrConductor.instance.KillAllSounds();
            scrConductor.instance.song.Stop();


            Time.timeScale = 1;
            ReplayUIUtils.DoSwipe(() => { SceneManager.LoadScene("scnReplayIntro"); });
                _progressDisplayerCancel = true;
                scrUIController.instance.WipeToBlack(WipeDirection.StartsFromLeft);
                
                BallBorder.CreatedBallBorders2.Clear();
            BallBorder.CreatedBallBorders.Clear();
            _ballBorders.Clear();


            return false;
        }


    


        [HarmonyPatch(typeof(scrController), "ValidInputWasTriggered")]
        [HarmonyPrefix]
        public static bool DisableSwipePatch()
        {
            if (!WatchReplay.IsPlaying) return true;
            if (!scrController.instance.gameworld) return true;
            if (WatchReplay.IsPaused || WatchReplay.IsPlanetDied)
                return false;
            
            if (scrLevelMaker.instance.listFloors.Count - 1 == scrController.instance.currentSeqID)
                return false;
            
            return true;
        }
        

        [HarmonyPatch(typeof(scrPlanet), "Update_RefreshAngles")]
        [HarmonyPrefix]
        public static bool PauseAngle()
        {
            if(!WatchReplay.IsPlaying) return true;
            if (WatchReplay.IsPlanetDied || WatchReplay.IsPaused)
            {
                scrController.instance.audioPaused = true;
                return false;
            }
            return true;
        }
        

        [HarmonyPatch(typeof(scrPressToStart), "ShowText")]
        [HarmonyPostfix]
        public static void ReplayShowTextPatch(Text ___text)
        {
            if (!WatchReplay.IsPlaying) return;
            var hash = _playingReplayInfo.IsOfficialLevel
                ? 0
                : (scnGame.instance.levelData.isOldLevel
                    ? scnGame.instance.levelData.pathData.GetHashCode()
                    : string.Join("", scnGame.instance.levelData.angleData).GetHashCode());


            if (hash != _playingReplayInfo.PathDataHash)
            {
                Reset();
            
                WatchReplay.DisableAllEffects();
                
                GC.Collect();
                ADOBase.audioManager.StopLoadingMP3File();
                
                GCS.worldEntrance = null;
                GCS.checkpointNum = 0;
                GCS.currentSpeedTrial = 1f;

                if (scrController.instance.pauseMenu.gameObject.activeSelf)
                    scrController.instance.TogglePauseGame();

                scrController.deaths = 0;
                scrController.instance.enabled = false;
                scrController.instance.paused = true;
                scrConductor.instance.hasSongStarted = false;
                scrConductor.instance.KillAllSounds();
                scrConductor.instance.song.Stop();
                
                _progressDisplayerCancel = true;
                scrUIController.instance.WipeToBlack(WipeDirection.StartsFromLeft);
                
                SceneManager.LoadScene("scnReplayIntro");

                GlobalLanguage.OK = Replay.CurrentLang.okText;
                GlobalLanguage.No = Replay.CurrentLang.noText;
                ReplayUI.Instance.ShowNotification(Replay.CurrentLang.replayModText, Replay.CurrentLang.levelDiff,
                    () =>
                    {
                        scrSfx.instance.PlaySfx(SfxSound.MenuSquelch);
                        return true;
                    }, null, RDString.language);
                Loader.UpdateLayoutNextFrame();
                return;
            }
            

            if (CheckInvalidIndex())
            {
                Replay.Log("invalid replay index "+_index);
                ReplayStartAt(_playingReplayInfo.StartTile);
                return;
            }
            
            _pressIndex = 0;

            scrController.instance.noFail = false;
            scrController.instance.chosenplanet.hittable = false;
            scrController.instance.chosenplanet.other.hittable = false;
            scrController.instance.mistakesManager.Reset();
            
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;

            if (scnEditor.instance != null)
                scnEditor.instance.floorButtonCanvas.gameObject.SetActive(false);

            if (WatchReplay.IsDeathCam && Replay.ReplayOption.hideEffectInDeathcam)
                WatchReplay.DisableAllEffects();
            
            FixReplayBugsPatches.SetTileGlow();

            var planet = scrController.instance.chosenplanet;
            WatchReplay.SetPlanetColor(planet, _playingReplayInfo);
            WatchReplay.SetPlanetColor(planet.other, _playingReplayInfo);

            scrUIController.instance.txtCountdown.GetComponent<scrCountdown>().CancelGo();
            ReplayViewingTool.Init(_playingReplayInfo);

            scrConductor.instance.song.volume = 1;
            scrUIController.instance.difficultyContainer.gameObject.SetActive(false);
            GCS.difficulty = _playingReplayInfo.Difficulty;

            ___text.text = Replay.CurrentLang.pressToPlay;
            WatchReplay.IsLoading = false;
            WatchReplay.IsPlanetDied = false;

            WatchReplay.IsPaused = true;
            ReplayViewingTool.TogglePause();

            GCS.hitMarginLimit = HitMarginLimit.None;

            song_loading = false;
            if (!_playingReplayInfo.IsOfficialLevel)
            {
                if (YoutubeStreamAPI.Enabled)
                {
                    if (!string.IsNullOrEmpty((string)scnGame.instance.levelData.songSettings["songURL"]))
                    {
                        var a = typeof(scnGame).GetField("currentSongKey",
                            BindingFlags.Instance | BindingFlags.NonPublic);
                        if (YoutubeStreamAPI.newSongKey != (string)a.GetValue(scnGame.instance))
                        {
                            song_loading = true;
                            scnEditor.instance.StartCoroutine(WaitSongLoading(___text, a));
                        }
                    }
                }
            }




            _ballBorders.Clear();
            if (WatchReplay.Reloaded)
            {
                _ballBorders.Clear();
                BallBorder.CreatedBallBorders2.Clear();
                BallBorder.CreatedBallBorders.Clear();
                WatchReplay.Reloaded = false;
            }

            if (Replay.ReplayOption.showInputTiming)
            {
                for (var n = 0; n < max_range; n++)
                {
                    if (Replay.ReplayOption.showInputTiming &&
                        ReplayUtils.CanGet(_playingReplayInfo.Tiles.Length, _index + n) && !_isOldReplay)
                    {
                        if (!_ballBorders.TryGetValue(_index + n, out var v))
                        {
                            var t = _playingReplayInfo.Tiles[_index + n];
                            var b = BallBorder.Create(
                                ReplayUtils.MiniVector2UnityVector(t.HitTimingPosition),
                                t.Hitmargin, 1f);
                            _ballBorders[_index + n] = b;
                        }

                    }
                }
            }
        }
        
        
        [HarmonyPatch(typeof(scrController), "Start_Rewind")]
        [HarmonyPostfix]
        public static void PlanetColorChangePatch()
        {
            _paused = false;
            if (!WatchReplay.IsPlaying) return;

            var planet = scrController.instance.chosenplanet;
            WatchReplay.SetPlanetColor(planet, _playingReplayInfo);
            WatchReplay.SetPlanetColor(planet.other, _playingReplayInfo);

            scrController.instance.chosenplanet.hittable = false;
            scrController.instance.chosenplanet.other.hittable = false;
        }


        [HarmonyPatch(typeof(scrController), "FailAction")]
        [HarmonyPrefix]
        public static bool NoFailPatch(bool overload)
        {
            if (!WatchReplay.IsPlaying) return true;
            if (_playingReplayInfo.EndTile <= scrController.instance.currentSeqID)
            {

                WatchReplay.IsPlanetDied = true;
                scrController.instance.audioPaused = true;
                Time.timeScale = 0;
                scrController.instance.ChangeState(States.None);
                return false;
            }
            
            return false;
        }


        [HarmonyPatch(typeof(ActivityManager), "UpdateActivity")]
        [HarmonyPrefix]
        public static void SetDiscordActivityPatch(ref Activity activity)
        {
            if (!WatchReplay.IsPlaying) return;
            if (activity.Assets.LargeImage == "planets_icon_stars")
            {
                activity.Details = Replay.CurrentLang.replayModText;
                if (_playingReplayInfo.IsOfficialLevel)
                    activity.State = Replay.CurrentLang.replayingText + ": " + _playingReplayInfo.SongName;
                else
                    activity.State = Replay.CurrentLang.replayingText + ": " + _playingReplayInfo.ArtistName + " - " +
                                     _playingReplayInfo.SongName;
            }
        }


        private static bool song_loading;
        private static IEnumerator WaitSongLoading(Text text, FieldInfo f)
        {
            song_loading = true;
            text.text = Replay.CurrentLang.loading;
            yield return new WaitUntil(() => YoutubeStreamAPI.newSongKey == (string)f.GetValue(scnGame.instance));
            text.text = Replay.CurrentLang.pressToPlay;
            song_loading = false;
        }

        [HarmonyPatch(typeof(scrController), "ValidInputWasTriggered")]
        [HarmonyPrefix]
        public static bool PreventValidInputWasTriggered(ref bool __result)
        {
            if (!WatchReplay.IsPlaying) return true;
            if (song_loading)
            {
                __result = false;
                return false;
            }

            return true;
        }
        
        
        
        [HarmonyPatch(typeof(scnEditor), "ResetScene")]
        [HarmonyPrefix]
        public static void ResetSceneInReplayingPatch()
        {
            if (!WatchReplay.IsPlaying) return;
   
            Reset();
        }
        

        [HarmonyPatch(typeof(scnEditor), "SwitchToEditMode")]
        [HarmonyPrefix]
        public static bool SwitchToEditMode(bool clsToEditor)
        {
            if (_playingReplayInfo == null) return true;
            if (!clsToEditor || _playingReplayInfo.IsOfficialLevel) return true;

            var startTile = WatchReplay.IsDeathCam ? SaveReplayPatches._cachedStartTile : _playingReplayInfo.StartTile;

            foreach (var b in BallBorder.CreatedBallBorders2)
            {
                Object.DestroyImmediate(b.gameObject);
            }
            BallBorder.CreatedBallBorders2.Clear();
            
            BallBorder.CreatedBallBorders.Clear();
            _ballBorders.Clear();
            _lastSeqID = startTile;
            scnEditor.instance.selectedFloorCached = startTile;
            scrController.instance.currentSeqID = startTile;
            

            return true;
        }
        
        
        


        /*

        [HarmonyPatch(typeof(scrPlanet), "MoveToNextFloor")]
        public static class bsdfsdfsdfool
        {
            public static bool Prefix()
            {
                if (!WatchReplay.IsPlaying) return true;
                if (scrMisc.IsValidHit(_playingReplayInfo.Tiles[_index].Hitmargin))
                    return true;
                return false;
            }
            

        }*/
        



        [HarmonyPatch(typeof(scrController), "Hit")]
        public static class IgnoreKeystrokesPatch
        {
            public static bool Prefix()
            {
                if (!WatchReplay.IsPlaying) return true;
                var b = !CheckInvalidIndex() && scrMisc.IsValidHit(_playingReplayInfo.Tiles[_index].Hitmargin);
                if (b || scrController.instance.midspinInfiniteMargin || scrController.instance.noFailInfiniteMargin)
                {
                    scrController.instance.responsive = true;
                    scrController.instance.paused = false;
                    RDInput.SetMapping("Gameplay");
                }

                scrController.instance.keyTimes.Clear();
                if (scrController.instance.currFloor.midSpin) return true;
                if (!_replayPlanetHit) return false;
                return true;
            }
            

            
        }
        
        
        
        [HarmonyPatch(typeof(scrMisc), "GetHitMargin")]
        public static class GetHitMargin
        {
            public static void Postfix(ref HitMargin __result)
            {
                if (!WatchReplay.IsPlaying) return;
                if (_replayPlanetHit)
                {
                    var r = _playingReplayInfo.Tiles[_index];
                    if (r.AutoHit)
                        __result = HitMargin.Perfect;
                    else
                        __result = r.Hitmargin;
                    
                }
            }
        }
        

        [HarmonyPatch(typeof(scrPlanet), "SwitchChosen")]
        [HarmonyPrefix]
        public static void SetForceAnglePatch(scrPlanet __instance)
        {
            if (!WatchReplay.IsPlaying) return;
            var angle = GetAngle();

            
            if (_playingReplayInfo.Tiles[_index].SeqID == scrController.instance.currentSeqID &&
                !scrController.instance.currFloor.midSpin)
            {
                scrController.instance.chosenplanet.angle = angle;
                scrController.instance.chosenplanet.cachedAngle = angle;
            }
        }


        /*
        [HarmonyPatch(typeof(scrController), "PlayerControl_Update")]
        [HarmonyPrefix]
        public static void ReplayKeyboardInput()
        {
            if (!WatchReplay.IsPlaying) return;
            if (!scrController.instance.goShown) return;
            
            if (_playingReplayInfo == null) return;
            if (_playingReplayInfo.Presses == null) return;
            if (_pressIndex < 0 || _pressIndex >= _playingReplayInfo.Presses.Length) return;
            
            var t = (scrConductor.instance.dspTime - scrConductor.instance.dspTimeSongPosZero) * scrConductor.instance.song.pitch;
            
            while (_pressIndex >= 0 && _pressIndex < _playingReplayInfo.Presses.Length && _playingReplayInfo.Presses[_pressIndex].PressTime <= t && !WatchReplay.IsLoading && WatchReplay.IsPlaying)
            {
                if (WatchReplay.IsLoading || _forceMove || _forcePlay || WatchReplay.IsPaused || !WatchReplay.IsPlaying || scrController.instance == null)
                    break;

                KeyboardPress();
            }
        }*/

        [HarmonyPatch(typeof(scrConductor), "Update")]
        [HarmonyPrefix]
        public static void ReplayPlanetUpdatePatch()
        {

            if (!WatchReplay.IsPlaying) return;
            if (_playingReplayInfo == null) return;

            if (scrLevelMaker.instance.listFloors.Count > 0)
                ReplayViewingTool.UpdateTime();

            if (CheckInvalidIndex()) return;
            

            var num0 = 10;
            while (IsHitNow() && !WatchReplay.IsLoading && WatchReplay.IsPlaying && num0 > 0)
            {
                if (WatchReplay.IsLoading || _forceMove || _forcePlay || WatchReplay.IsPaused || !WatchReplay.IsPlaying || scrController.instance == null)
                    break;

                if (CheckInvalidIndex())
                    break;

                ReplayHit();
                num0--;

            }
        }
    }
}