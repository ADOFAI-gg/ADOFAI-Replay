﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using DG.Tweening;
using DG.Tweening.Core;
using HarmonyLib;
using Newgrounds;
using OggVorbisEncoder.Setup;
using Replay.Functions.Core;
using Replay.Functions.Core.Types;
using Replay.Functions.Watching;
using Replay.UI;
using TinyJson;
using UnityEngine;
using UnityEngine.Networking;

namespace Replay.Functions.Saving
{
    [HarmonyPatch]
    public static class SaveReplayPatches
    {
        private static bool _isSave;
        private static bool _tryDeathCamMode;
        private static float _startTime;
        private static ReplayInfo _replayInfo;
        private static List<TileInfo> _pressInfos = new List<TileInfo>();
        private static Queue<KeyCode> _pressedKeys = new Queue<KeyCode>();
        private static Dictionary<KeyCode, TileInfo> _heldPressInfo = new Dictionary<KeyCode, TileInfo>();
        private static CustomControllerStates _states = CustomControllerStates.PlayerControl;
        private static float _lastFrame;


        // Save replay data
        private static void Save()
        {
            if (_isSave) return;
            _isSave = true;

            var floor = scrLevelMaker.instance.listFloors;
            _replayInfo.EndTile = scrController.instance.currentSeqID;
            _replayInfo.AllTile = floor.Count;

            var official = scrController.instance != null && scrController.instance.gameworld &&
                           scnEditor.instance == null;

            if (official)
            {
                var s = scrController.instance.lofiBackground?.GetComponent<SpriteRenderer>();
                if (s != null)
                {
                    var t = s?.sprite?.texture;
                    if (t != null)
                    {
                        if (!Directory.Exists(Path.Combine(Application.dataPath, "Screenshot",
                                scrController.instance.levelName)))
                        {
                            var bytes = ReplayUtils.DuplicateTexture(t).EncodeToPNG();
                            File.WriteAllBytes(
                                Path.Combine(Application.dataPath, "Screenshot", scrController.instance.levelName),
                                bytes);
                        }

                        _replayInfo.PreviewImagePath = Path.Combine(Application.dataPath, "Screenshot",
                            scrController.instance.levelName);
                    }
                }

                if (string.IsNullOrEmpty(_replayInfo.PreviewImagePath))
                {
                    var path = Path.Combine(Application.dataPath, "Screenshot",
                        scrController.instance.levelName);
                    if (!File.Exists(path))
                        scrConductor.instance.StartCoroutine(SaveReplay.CaptureScreen(path));
                    _replayInfo.PreviewImagePath = path;
                }
            }
            else if (!string.IsNullOrEmpty(CustomLevel.instance.levelPath))
            {
                var path = Directory.GetParent(CustomLevel.instance.levelPath).FullName;

                if (File.Exists(Path.Combine(path, CustomLevel.instance.levelData.previewImage)))
                    _replayInfo.PreviewImagePath = Path.Combine(path, CustomLevel.instance.levelData.previewImage);
                else if (File.Exists(Path.Combine(path, CustomLevel.instance.levelData.bgImage)))
                    _replayInfo.PreviewImagePath = Path.Combine(path, CustomLevel.instance.levelData.bgImage);
                
                if (string.IsNullOrEmpty(_replayInfo.PreviewImagePath))
                {
                    if (!File.Exists(Path.Combine(path, "ReplayScreenShot")))
                        scnEditor.instance.StartCoroutine(
                            SaveReplay.CaptureScreen(Path.Combine(path, "ReplayScreenShot")));
                    _replayInfo.PreviewImagePath = Path.Combine(path, "ReplayScreenShot");
                }

            }


            _replayInfo.IsOfficialLevel = official;
            _replayInfo.Path = official ? GCS.sceneToLoad : CustomLevel.instance.levelPath;
            _replayInfo.Speed = scrConductor.instance.song.pitch;
            _replayInfo.AuthorName = official ? "7 beat games" : CustomLevel.instance.levelData.author;
            _replayInfo.SongName = official ? scrController.instance.levelName : CustomLevel.instance.levelData.song;
            _replayInfo.ArtistName = official ? "ADOFAI" : CustomLevel.instance.levelData.artist;
            _replayInfo.Tiles = _pressInfos.ToArray();
            _replayInfo.RedPlanet = ReplayUtils.UnityColor2CustomColor(Persistence.GetPlayerColor(true));
            _replayInfo.BluePlanet = ReplayUtils.UnityColor2CustomColor(Persistence.GetPlayerColor(false));
            _replayInfo.Difficulty = GCS.difficulty;
            _replayInfo.PlayTime =
                (long)(((floor[scrController.instance.currentSeqID].entryTime -
                        floor[_replayInfo.StartTile].entryTime) * 1000));
            _replayInfo.PathDataHash = official
                ? 0
                : (CustomLevel.instance.levelData.isOldLevel
                    ? CustomLevel.instance.levelData.pathData.GetHashCode()
                    : string.Join("", CustomLevel.instance.levelData.angleData).GetHashCode());
            _replayInfo.Time = DateTime.Now;

            _replayInfo.SongName = ReplayUIUtils.RemoveHTML(_replayInfo.SongName);

            if (!official)
            {
                if (!string.IsNullOrEmpty(CustomLevel.instance.levelData.songFilename))
                {
                    var song2 = Path.GetFileNameWithoutExtension(CustomLevel.instance.levelData.songFilename);
                    if (string.IsNullOrEmpty(ReplayUIUtils.RemoveHTML(_replayInfo.SongName)))
                        _replayInfo.SongName = song2;
                    
                    if (string.IsNullOrEmpty(_replayInfo.SongName) && !string.IsNullOrEmpty(_replayInfo.Path))
                        _replayInfo.SongName = Path.GetFileNameWithoutExtension(_replayInfo.Path);
                }
            }

            GlobalLanguage.SaveSuccess = Replay.CurrentLang.saveSuccess;
            GlobalLanguage.SaveError = Replay.CurrentLang.cantSave;
            ReplayUI.Instance.ShowSaveLabel(() =>
            {
                if (string.IsNullOrEmpty(_replayInfo.Path)) throw new Exception("level path is null");
                if (_replayInfo.StartTile == scrController.instance.currentSeqID)
                    throw new Exception("the start tile and the end tile are the same");
                ReplayUtils.SaveReplay(_replayInfo.SongName + (_replayInfo.GetHashCode()) + ".rpl", _replayInfo);

                if (Directory.Exists(Replay.ReplayOption.savedPath))
                {
                    var files = Directory.GetFiles(Replay.ReplayOption.savedPath);
                    if (files.Length > 20)
                        File.Delete(files[0]);
                        
                }

                var sri = new SimpleReplayInfo
                {
                    Difficulty = _replayInfo.Difficulty,
                    Pitch = _replayInfo.Speed,
                    Tiles = _replayInfo.Tiles,
                    ArtistName = _replayInfo.ArtistName,
                    AuthorName = _replayInfo.AuthorName,
                    SongName = _replayInfo.SongName,
                    IsOfficialLevel = _replayInfo.IsOfficialLevel,
                    EndSeqID = _replayInfo.EndTile,
                    StartSeqID = _replayInfo.StartTile
                };

                if (Replay.ReplayOption.CanICollectReplayFile == 1)
                    SaveReplay.UploadToServer(ReplayUtils.ObjectToJSON(sri));
            });
        }


        //Simple replay playback
        private static void ShowDeathCam()
        {
            if (_tryDeathCamMode) return;
            _tryDeathCamMode = true;
            _isSave = true;
            
            if (_replayInfo.StartTile == scrController.instance.currentSeqID)
            {
                Replay.Log("the start tile and the end tile are the same");
                return;
            }

            var floor = scrLevelMaker.instance.listFloors;
            _replayInfo.EndTile = scrController.instance.currentSeqID;
            _replayInfo.AllTile = floor.Count;

            var official = scrController.instance != null && scrController.instance.gameworld &&
                           scnEditor.instance == null;
            _replayInfo.IsOfficialLevel = official;
            _replayInfo.Path = official ? GCS.sceneToLoad : CustomLevel.instance.levelPath;
            
            if (string.IsNullOrEmpty(_replayInfo.Path))
            {
                Replay.Log("level path is null");
                return;
            }
            
            if (!File.Exists(_replayInfo.Path))
            {
                Replay.Log("level path is null");
                return;
            }
            
            _replayInfo.Speed = scrConductor.instance.song.pitch;
            _replayInfo.AuthorName = official ? "7 beat games" : CustomLevel.instance.levelData.author;
            _replayInfo.SongName = official ? scrController.instance.levelName : CustomLevel.instance.levelData.song;
            _replayInfo.ArtistName = official ? "ADOFAI" : CustomLevel.instance.levelData.artist;
            _replayInfo.RedPlanet = ReplayUtils.UnityColor2CustomColor(Persistence.GetPlayerColor(true));
            _replayInfo.BluePlanet = ReplayUtils.UnityColor2CustomColor(Persistence.GetPlayerColor(false));
            _replayInfo.Difficulty = GCS.difficulty;
            _replayInfo.PathDataHash = official
                ? 0
                : (CustomLevel.instance.levelData.isOldLevel
                    ? CustomLevel.instance.levelData.pathData.GetHashCode()
                    : string.Join("", CustomLevel.instance.levelData.angleData).GetHashCode());
            _replayInfo.Time = DateTime.Now;

            var startTime = scrLevelMaker.instance.listFloors[_replayInfo.StartTile].entryTime;
            var currentTime = scrController.instance.currFloor.entryTime - 20 - startTime;
            var chooseSeqID =
                ReplayViewingTool.FindFloorBySecond(currentTime, _replayInfo.StartTile, _replayInfo.EndTile);
            _replayInfo.StartTile = chooseSeqID;
            _replayInfo.Tiles = _pressInfos.Where(x => x.SeqID >= chooseSeqID).ToArray();
            _replayInfo.PlayTime =
                (long)((floor[scrController.instance.currentSeqID].entryTime -
                        floor[chooseSeqID].entryTime) * 1000);

            WatchReplay.Play(_replayInfo);
        }



        // All key inputs
        private static KeyCode GetInput()
        {
            var keyCode = KeyCode.None;
            if (_lastFrame == Time.unscaledTime)
            {
                if (_pressedKeys.Count > 0)
                    keyCode = _pressedKeys.Dequeue();
            }
            else
            {
                _pressedKeys.Clear();
                foreach (var k in Replay.AllKeyCodes)
                {
                    if (Input.GetKeyDown((KeyCode)k))
                        _pressedKeys.Enqueue((KeyCode)k);
                }

                if (_pressedKeys.Count > 0)
                    keyCode = _pressedKeys.Dequeue();
            }

            return keyCode;
        }
        
        
        private static void ResetReplayInfo()
        {
            _pressInfos.Clear();
            _replayInfo = new ReplayInfo
            {
                StartTile = GCS.checkpointNum
            };
            _states = CustomControllerStates.PlayerControl;
            _isSave = false;
            _tryDeathCamMode = false;

        }


        [HarmonyPatch(typeof(scrController), "ValidInputWasTriggered")]
        [HarmonyPrefix]
        public static bool DisableMoveScenePatch()
        {
            if (WatchReplay.IsPlaying) return true;

            if ((Replay.ReplayOption.saveBySpecifiedKey &&
                 Input.GetKeyDown((KeyCode)Replay.ReplayOption
                     .specifiedKeyCode)) &&
                (_states == CustomControllerStates.Fail || _states == CustomControllerStates.Won))
                return false;
            return true;
        }

        
        [HarmonyPatch(typeof(scrController), "Start_Rewind")]
        [HarmonyPostfix]
        public static void LevelSettingPatch()
        {
            if (WatchReplay.IsPlaying) return;
            if (!scrController.isGameWorld) return;

            ResetReplayInfo();
        }
        


        [HarmonyPatch(typeof(scrConductor), "StartMusicCo")]
        [HarmonyPostfix]
        public static void SetStartTimePatch()
        {
            if (WatchReplay.IsPlaying) return;
            _startTime = Time.time;
        }


        [HarmonyPatch(typeof(scrController), "OnLandOnPortal")]
        [HarmonyPrefix]
        public static void OnLandOnPortalPatch()
        {
            if (!scrController.isGameWorld) return;
            if (WatchReplay.IsPlaying) return;
            if (_replayInfo == null) return;
            
            _states = CustomControllerStates.Won;
            if (Replay.ReplayOption.saveEveryLevelComplete || (Replay.ReplayOption.saveBySpecifiedKey &&
                                                               Input.GetKeyDown((KeyCode)Replay.ReplayOption
                                                                   .specifiedKeyCode)))
                Save();
            
        }


        [HarmonyPatch(typeof(scrController), "Hit")]
        [HarmonyPrefix]
        public static void HitPatch()
        {
            var controller = scrController.instance;
            var planet = controller.chosenplanet;
            var isFreeroam = controller.currFloor.freeroam && !scrController.isGameWorld;

            if (WatchReplay.IsPlaying) return;
            if (!scrController.isGameWorld && !isFreeroam) return;
            if (scrController.instance.currFloor.midSpin) return;
            var keyCode = GetInput();

            var t = new TileInfo
            {
                HitAngleRatio = planet.angle - planet.targetExitAngle,
                SeqID = controller.currentSeqID,
                Key = keyCode,
                NoFailHit = scrController.instance.noFailInfiniteMargin,
                HeldTime = Time.unscaledDeltaTime,
            };
            _heldPressInfo[keyCode] = t;
            if (Replay.ReplayOption.CanICollectReplayFile == 1)
            {
                t.HitTime = Time.timeAsDouble - _startTime;
                t.Hitmargin = scrMisc.GetHitMargin((float)planet.angle, (float)planet.targetExitAngle,
                    planet.controller.isCW, (float)(planet.conductor.bpm * planet.controller.speed),
                    planet.conductor.song.pitch);
                t.RealHitAngle = planet.angle;
                t.TargetAngle = Math.Abs(planet.targetExitAngle) > 0.001? planet.targetExitAngle:0;
                t.IsFreeroam = controller.currFloor.freeroam;
                t.RelativeFloorAngle = Mathf.RoundToInt((float)(scrController.instance.currentSeqID == 0
                    ? (controller.currFloor.exitangle * (180 / Math.PI) + 90)
                    : ((controller.currFloor.angleLength * (180 / Math.PI)) % 360)));
            }
            
            _pressInfos.Add(t);
            _lastFrame = Time.unscaledTime;

        }


        [HarmonyPatch(typeof(scrController), "PlayerControl_Update")]
        [HarmonyPrefix]
        public static void KeyInputDetectPatch()
        {
            try
            {
                if (WatchReplay.IsPlaying) return;
                if (!scrController.instance.goShown) return;

                foreach (var keyCode in Replay.AllKeyCodes)
                {
                    if (Input.GetKey(keyCode))
                    {
                        if (_heldPressInfo.TryGetValue(keyCode, out var v))
                            _heldPressInfo[keyCode].HeldTime += Time.unscaledDeltaTime;
                    }
                }
            }
            catch
            {

            }
        }

        [HarmonyPatch(typeof(scrController), "FailAction")]
        [HarmonyPrefix]
        public static void FailActionPatch()
        {
            if (WatchReplay.IsPlaying) return;
            if (scrController.instance.noFail || scrController.instance.currFloor.isSafe) return;
            if (_replayInfo == null) return;
            _states = CustomControllerStates.Fail;
            if (Replay.ReplayOption.saveEverytimeDied ||
                (Replay.ReplayOption.saveWhen90P && (scrController.instance.percentComplete >= 0.9)))
                Save();
        }


        [HarmonyPatch(typeof(scrUIController), "WipeToBlack")]
        [HarmonyPrefix]
        public static void WipeToBlackPatch()
        {
            if (WatchReplay.IsPlaying) return;
            _states = CustomControllerStates.PlayerControl;
            _isSave = false;
            _tryDeathCamMode = false;
        }


        [HarmonyPatch(typeof(scnEditor), "ResetScene")]
        [HarmonyPrefix]
        public static void ResetScenePatch()
        {
            if (WatchReplay.IsPlaying) return;
            _states = CustomControllerStates.PlayerControl;
            _isSave = false;
            _tryDeathCamMode = false;
        }


        [HarmonyPatch(typeof(scrConductor), "Update")]
        [HarmonyPrefix]
        public static void SaveCheckPatch()
        {
            if (scrController.instance == null) return;
            if (!WatchReplay.IsPlaying)
            {
                if (_states == CustomControllerStates.Won && (Replay.ReplayOption.saveBySpecifiedKey &&
                                                              Input.GetKeyDown(
                                                                  (KeyCode)Replay.ReplayOption.specifiedKeyCode)))
                    Save();

                if (_states == CustomControllerStates.Fail && (Replay.ReplayOption.saveBySpecifiedKey &&
                                                               Input.GetKeyDown(
                                                                   (KeyCode)Replay.ReplayOption.specifiedKeyCode)))
                    Save();

                if (_states == CustomControllerStates.Fail && Input.GetKeyDown(
                        (KeyCode)Replay.ReplayOption.specifiedDeathCamKeyCode))
                    ReplayUIUtils.DoSwipe(ShowDeathCam);
                
            }
        }

    }
}