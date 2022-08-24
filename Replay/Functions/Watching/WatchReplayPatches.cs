using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DG.Tweening;
using Discord;
using HarmonyLib;
using Newgrounds;
using Replay.Functions.Core;
using Replay.Functions.Core.Types;
using Replay.Functions.Watching;
using Replay.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityModManagerNet;

namespace Replay.Functions.Menu
{
    [HarmonyPatch]
    public class WatchReplayPatches
    {
        private static bool _forceMove;
        private static bool _forcePlay;
        private static bool _forceColor;
        private static bool _replayPlanetHit;
        private static bool _beforeNoStopModEnabled;
        private static bool _dontDie;
        private static int _index;
        private static UnityModManager.ModEntry _noStopModModEntry;
        private static ReplayInfo _playingReplayInfo;

        private static float _cameraScale;
        private static bool _freeCameraMode = true;
        private static Vector3 _lastRotate;
        private static float _lastZoom;

        private static List<Tween> _requestedHold = new List<Tween>();
        
        internal static bool _progressDisplayerCancel;

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
            _freeCameraMode = true;
            _dontDie = false;

            WatchReplay.IsPlaying = false;

            GCS.customLevelPaths = null;
            GCS.standaloneLevelMode = false;
            GCS.checkpointNum = 0;
            ReplayUI.Instance.InGameUI.SetActive(false);

            if (scrController.instance != null)
            {
                scrController.instance.errorMeter.UpdateLayout(Persistence.GetHitErrorMeterSize(),
                    Persistence.GetHitErrorMeterShape());
            }

            KeyboradHook.OnEndInputs();

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

        // Planet angle when pressed by player
        private static double GetAngle()
        {
            if (CheckInvalidIndex())
                return Math.PI;

            var planet = scrController.instance.chosenplanet;
            var tileInfo = _playingReplayInfo.Tiles[_index];
            return tileInfo.HitAngleRatio + planet.targetExitAngle;
        }

        // Hit the planet in replay
        private static void ReplayHit()
        {
            if (CheckInvalidIndex())
                return;
            _replayPlanetHit = true;

            var r = _playingReplayInfo.Tiles[_index];
            var angle = GetAngle();

            scrController.instance.consecMultipressCounter = 0;
            if (_playingReplayInfo.Tiles[_index].SeqID == scrController.instance.currentSeqID &&
                !scrController.instance.currFloor.midSpin)
            {
                scrController.instance.chosenplanet.angle = angle;
                scrController.instance.chosenplanet.cachedAngle = angle;
            }

            scrController.instance.noFailInfiniteMargin = r.NoFailHit;
            scrController.instance.Hit();
            scrController.instance.noFailInfiniteMargin = false;

            var k = _playingReplayInfo.Tiles[_index].Key;
            KeyboradHook.OnKeyPressed(k);
            var tween = DOVirtual.DelayedCall(
                _playingReplayInfo.Tiles[_index].HeldTime / (GCS.currentSpeedTrial / _playingReplayInfo.Speed),
                () => { KeyboradHook.OnKeyReleased(k); });
            tween.OnComplete(() => { _requestedHold.Remove(tween); });
            tween.OnKill(() => { KeyboradHook.OnKeyReleased(k); });
            _index++;
        }


        // replay play
        public static void Start(ReplayInfo replayInfo)
        {
            WatchReplay.IsLoading = true;
            WatchReplay.IsPlaying = true;
            WatchReplay.IsPlanetDied = false;
            _forcePlay = true;
            _playingReplayInfo = replayInfo;
            _index = 0;

            GCS.currentSpeedTrial = replayInfo.Speed;
            GCS.nextSpeedRun = replayInfo.Speed;
            WatchReplay.PatchedPitch = GCS.currentSpeedTrial;

            //DisableNoStopMod();
        }

        // Restart the replay from that point
        public static void ReplayStartAt(int seqID)
        {
            var floors = scrLevelMaker.instance.listFloors;
            if (seqID > floors[_playingReplayInfo.EndTile].seqID) seqID = floors[_playingReplayInfo.EndTile].seqID;
            if (seqID < floors[_playingReplayInfo.StartTile].seqID) seqID = floors[_playingReplayInfo.StartTile].seqID;

            _forceMove = true;
            _index = _playingReplayInfo.Tiles.ToList().FindIndex(x => x.SeqID == seqID);

            foreach (var h in _requestedHold)
                h.Kill();
            _requestedHold.Clear();

            WatchReplay.RestartLevelAt(seqID);
        }

        // when is the hit timing
        private static bool IsHitNow()
        {
            if (scrController.instance == null) return false;
            if (scrController.instance.currentState != States.PlayerControl)
                return false;

            if (CheckInvalidIndex())
                return false;

            var planet = scrController.instance.chosenplanet;

            if (scrController.instance.currentSeqID > _playingReplayInfo.Tiles[_index].SeqID) return true;

            var angle = GetAngle();
            var angleOverd = (scrController.instance.isCW && planet.angle >= angle) ||
                             (!scrController.instance.isCW && planet.angle <= angle);
            var validTile = scrController.instance.currentSeqID == _playingReplayInfo.Tiles[_index].SeqID;

            if (scrController.instance.currFloor.freeroam)
            {
                var nextTileInfoVailed = ReplayUtils.GetSafeList(_playingReplayInfo.Tiles, _index + 1).SeqID ==
                                         scrController.instance.currFloor.seqID;
                if (angleOverd && validTile && nextTileInfoVailed)
                    return true;
            }

            return scrController.isGameWorld && angleOverd && validTile;
        }




        // Fix Tile Glow Bug
        private static void SetTileGlow()
        {
            for (var n = GCS.checkpointNum; n <= _playingReplayInfo.EndTile; n++)
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


        [HarmonyPatch(typeof(PauseMenu), "RefreshLayout")]
        [HarmonyPostfix]
        public static void DisableOtherButtonsPatch(ref GeneralPauseButton[] ___pauseButtons)
        {
            if (!WatchReplay.IsPlaying) return;

            ___pauseButtons = ___pauseButtons.Where(b =>
            {
                if (b is PauseButton button)
                {
                    return !(button.rdString == "pauseMenu.restart" || button.rdString == "pauseMenu.practice" ||
                             button.rdString == "pauseMenu.settings" || button.rdString == "pauseMenu.next" ||
                             button.rdString == "pauseMenu.previous");
                }

                return false;
            }).ToArray();

        }


        [HarmonyPatch(typeof(scrController), "Awake")]
        [HarmonyPostfix]
        public static void SetStartAtPatch()
        {
            if (!WatchReplay.IsPlaying) return;
            if (!_playingReplayInfo.IsOfficialLevel) return;

            GCS.checkpointNum = WatchReplay.OfficialStartAt;
            scrController.instance.chosenplanet.hittable = false;
            scrController.instance.chosenplanet.other.hittable = false;
        }


        [HarmonyPatch(typeof(CustomLevel), "Play")]
        [HarmonyPrefix]
        public static void ForceCustomSeqIDPlayPatch(ref int seqID)
        {
            if (!WatchReplay.IsPlaying) return;
            if (!_forcePlay) return;

            _forcePlay = false;
            seqID = _playingReplayInfo.StartTile;
            GCS.checkpointNum = _playingReplayInfo.StartTile;
            scrController.instance.currentSeqID = _playingReplayInfo.StartTile;
        }


        [HarmonyPatch(typeof(scrController), "Start")]
        [HarmonyPrefix]
        public static void ForceOfficialSeqIDPlayPatch()
        {
            if (!WatchReplay.IsPlaying) return;
            if (!scrController.isGameWorld) return;
            if (!_playingReplayInfo.IsOfficialLevel) return;
            if (!_forcePlay) return;
            _forcePlay = false;

            GCS.checkpointNum = _playingReplayInfo.StartTile;
            scrController.instance.currentSeqID = _playingReplayInfo.StartTile;
        }


        [HarmonyPatch(typeof(scrController), "QuitToMainMenu")]
        [HarmonyPrefix]
        public static bool QuitToMainMenuInReplayingPatch(ref bool ___exitingToMainMenu)
        {
            if (!WatchReplay.IsPlaying) return true;
            Reset();

            ___exitingToMainMenu = true;
            RDUtils.SetGarbageCollectionEnabled(true);
            ADOBase.audioManager.StopLoadingMP3File();

            GCS.standaloneLevelMode = false;
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

            ReplayUIUtils.DoSwipe(() => { SceneManager.LoadScene("scnReplayIntro"); });
            _progressDisplayerCancel = true;
            scrUIController.instance.WipeToBlack(WipeDirection.StartsFromLeft);

            return false;
        }


        [HarmonyPatch(typeof(scrUIController), "WipeToBlack")]
        [HarmonyPrefix]
        public static bool CancelProgressDisplayerPatch()
        {
            if (_progressDisplayerCancel)
            {
                _progressDisplayerCancel = false;
                return false;
            }

            return true;
        }


        [HarmonyPatch(typeof(scrController), "ValidInputWasTriggered")]
        [HarmonyPrefix]
        public static bool DisableSwipePatch()
        {
            if (!WatchReplay.IsPlaying) return true;
            if (!scrController.isGameWorld) return true;
            if (scrLevelMaker.instance.listFloors.Count - 1 == scrController.instance.currentSeqID)
                return false;
            return true;
        }


        [HarmonyPatch(typeof(scrController), "TogglePauseGame")]
        [HarmonyPostfix]
        public static void TogglePauseGamePatch()
        {
            if (!WatchReplay.IsPlaying) return;
            if (!scrController.instance.pauseMenu.gameObject.activeSelf)
            {
                if (WatchReplay.IsPlanetDied)
                {
                    scrController.instance.enabled = false;
                    Time.timeScale = 0;
                    scrController.instance.audioPaused = true;
                }
            }
        }


        [HarmonyPatch(typeof(scrHitErrorMeter), "UpdateLayout")]
        [HarmonyPostfix]
        public static void SetHitErrorMeterYPatch(scrHitErrorMeter __instance)
        {
            if (!WatchReplay.IsPlaying) return;
            var p = __instance.wrapperRectTransform.anchoredPosition;
            __instance.wrapperRectTransform.anchoredPosition = new Vector2(0, p.y + 100);
        }


        [HarmonyPatch(typeof(scrPressToStart), "ShowText")]
        [HarmonyPostfix]
        public static void ReplayShowTextPatch(Text ___text)
        {
            if (!WatchReplay.IsPlaying) return;
            var hash = _playingReplayInfo.IsOfficialLevel
                ? 0
                : (CustomLevel.instance.levelData.isOldLevel
                    ? CustomLevel.instance.levelData.pathData.GetHashCode()
                    : string.Join("", CustomLevel.instance.levelData.angleData).GetHashCode());


            if (hash != _playingReplayInfo.PathDataHash)
            {
                scrController.instance.QuitToMainMenu();
                GlobalLanguage.OK = Replay.CurrentLang.okText;
                GlobalLanguage.No = Replay.CurrentLang.noText;
                ReplayUI.Instance.ShowNotification(Replay.CurrentLang.replayModText, Replay.CurrentLang.levelDiff,
                    () => { }, null, RDString.language);
                ReplayViewingTool.UpdateLayout();
                return;
            }

            if (CheckInvalidIndex())
            {
                Replay.Log("invalid replay index "+_index);
                ReplayStartAt(_playingReplayInfo.StartTile);
                return;
            }

            scrController.instance.chosenplanet.hittable = false;
            scrController.instance.chosenplanet.other.hittable = false;

            SetTileGlow();

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

            if (WatchReplay.IsPaused)
            {
                scrController.instance.enabled = false;
                Time.timeScale = 0;
                scrController.instance.audioPaused = true;
            }
            else
            {
                Time.timeScale = 1;
                scrController.instance.audioPaused = false;
            }
        }


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


        [HarmonyPatch(typeof(scrController), "Start_Rewind")]
        [HarmonyPostfix]
        public static void PlanetColorChangePatch()
        {
            if (!WatchReplay.IsPlaying) return;

            var planet = scrController.instance.chosenplanet;
            WatchReplay.SetPlanetColor(planet, _playingReplayInfo);
            WatchReplay.SetPlanetColor(planet.other, _playingReplayInfo);

            scrController.instance.chosenplanet.hittable = false;
            scrController.instance.chosenplanet.other.hittable = false;

        }


        [HarmonyPatch(typeof(scrController), "FailAction")]
        [HarmonyPrefix]
        public static bool NoFailPatch()
        {
            if (!WatchReplay.IsPlaying) return true;
            if (_playingReplayInfo.EndTile - 1 > scrController.instance.currentSeqID) return false;

            WatchReplay.IsPlanetDied = true;
            scrController.instance.audioPaused = true;
            Time.timeScale = 0;
            scrController.instance.ChangeState(States.None);
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


        [HarmonyPatch(typeof(scrUIController), "WipeToBlack")]
        [HarmonyPrefix]
        public static void WipeToBlackInReplayingPatch()
        {
            if (!WatchReplay.IsPlaying) return;
            Reset();
        }


        [HarmonyPatch(typeof(scnEditor), "ResetScene")]
        [HarmonyPrefix]
        public static void ResetSceneInReplayingPatch()
        {
            if (!WatchReplay.IsPlaying) return;
            Reset();
        }


        [HarmonyPatch(typeof(scrController), "Hit")]
        [HarmonyPrefix]
        public static bool IgnoreKeystrokesPatch()
        {
            if (!WatchReplay.IsPlaying ||
                scrController.instance.currentState != States.PlayerControl) return true;

            scrController.instance.keyTimes.Clear();
            if (scrController.instance.currFloor.midSpin) return true;
            if (!_replayPlanetHit)
                return false;
            _replayPlanetHit = false;
            return true;
        }


        [HarmonyPatch(typeof(ffxCameraPlus), "StartEffect")]
        [HarmonyPrefix]
        public static bool StopCameraMovingPatch(ffxCameraPlus __instance)
        {
            if (_freeCameraMode) return true;
            _lastRotate = new Vector3(0, 0, __instance.targetRot + __instance.vfx.camAngle);
            _lastZoom = __instance.targetZoom;
            return false;
        }


        [HarmonyPatch(typeof(scrPlanet), "SwitchChosen")]
        [HarmonyPrefix]
        public static void SetForceAnglePatch(scrPlanet __instance)
        {
            if (!WatchReplay.IsPlaying) return;
            if (scrController.instance.midspinInfiniteMargin || scrController.instance.currFloor.midSpin) return;
            var angle = GetAngle();

            __instance.angle = angle;
            __instance.cachedAngle = angle;
        }


        [HarmonyPatch(typeof(scnEditor), "Update")]
        [HarmonyPrefix]
        public static void CameraMovePatch()
        {
            if (WatchReplay.IsPlaying)
            {
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


        [HarmonyPatch(typeof(scrController), "PlayerControl_Enter")]
        [HarmonyPrefix]
        public static void FixSongNotPlayingBugPatch()
        {
            if (scrConductor.instance.song != null)
                scrConductor.instance.song.volume = 1;

            if (WatchReplay.IsPlaying)
                FixFreeroamBug();
        }


        [HarmonyPatch(typeof(scrController), "OnApplicationQuit")]
        [HarmonyPrefix]
        public static void OnApplicationQuitPatch()
        {
            WatchReplay.IsPlaying = false;
            GCS.checkpointNum = 0;
        }


        [HarmonyPatch(typeof(scrConductor), "Update")]
        [HarmonyPrefix]
        public static void ReplayPlanetUpdatePatch()
        {
            if (!WatchReplay.IsPlaying) return;
            if (_playingReplayInfo == null) return;

            if (scrLevelMaker.instance.listFloors.Count > 0)
                ReplayViewingTool.UpdateTime();

            if (CheckInvalidIndex()) return;

            while (IsHitNow() && !WatchReplay.IsLoading)
            {
                if (WatchReplay.IsLoading || _forceMove || _forcePlay || WatchReplay.IsPaused || !WatchReplay.IsPlaying)
                    break;

                if (CheckInvalidIndex())
                    break;

                ReplayHit();

            }
        }
    }
}