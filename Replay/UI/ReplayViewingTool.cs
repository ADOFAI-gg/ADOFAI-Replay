using System;
using DG.Tweening;
using Replay.Functions.Core;
using Replay.Functions.Core.Types;
using Replay.Functions.Menu;
using Replay.Functions.Watching;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Replay.UI
{
    public class ReplayViewingTool
    {
        private static Tween _pitchUpDown;
        private static Tween _sliderUpdate;
        private static bool _isEditingTime;
        private static bool _valueChanging;
        private static ReplayInfo _playingReplayInfo;
        private static int _goBackStack;
        private static bool _listenerAdded;
        
        public static Image PauseImage;

        public static void Init(ReplayInfo rpl)
        {
            if (rpl == _playingReplayInfo) return;
            _playingReplayInfo = rpl;
            
            ReplayUI.Instance.PitchText.text = $"{GCS.currentSpeedTrial: 0.0}x";
            ReplayUI.Instance.InGameUI.gameObject.SetActive(true);
            ReplayUI.Instance.EndTime.text = ReplayUtils.Ms2time((_playingReplayInfo.PlayTime));

            if (_listenerAdded) return;
            ReplayUI.Instance.PitchUp.onClick.AddListener(PitchUp);
            ReplayUI.Instance.PitchDown.onClick.AddListener(PitchDown);
            ReplayUI.Instance.Go10s.onClick.AddListener(Go10Second);
            ReplayUI.Instance.Back10s.onClick.AddListener(Back10Second);
            ReplayUI.Instance.Pause.onClick.AddListener(TogglePause);
            ReplayUI.Instance.PositionSlider.onValueChanged.AddListener(OnValueChange);
            ReplaySlider.OnMouseDown = OnPointerDown;
            
            _listenerAdded = true;

        }

        // UpdateLayout
        public static void UpdateLayout()
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(ReplayUI.Instance.BbiBbiGameobject.transform.Find("TextLayout").GetComponent<RectTransform>());
            LayoutRebuilder.ForceRebuildLayoutImmediate(ReplayUI.Instance.BbiBbiGameobject.transform.Find("TextLayout").Find("No").GetComponent<RectTransform>());
            LayoutRebuilder.ForceRebuildLayoutImmediate(ReplayUI.Instance.BbiBbiGameobject.transform.Find("TextLayout").Find("Yes").GetComponent<RectTransform>());
        }

        
        // Find tiles for that time
        public static int FindFloorBySecond(double currentTime, int startTile, int endTile)
        {
            var startTime = scrLevelMaker.instance.listFloors[startTile].entryTime;
            var floors = scrLevelMaker.instance.listFloors;
            var distnace = 999.0;
            var beforeDistance = 999.0;
            var chooseSeqID = _playingReplayInfo.StartTile;
            
            for (var n = startTile; n < endTile + 1; n++)
            {
                if(floors[n].midSpin || floors[n].freeroam) continue;
                var time = floors[n].entryTime - startTime;
                if (Math.Abs(time - currentTime) < distnace)
                {
                    beforeDistance = distnace;
                    distnace = Math.Abs(time - currentTime);
                    chooseSeqID = n;
                }
                if (beforeDistance < Math.Abs(time - currentTime)) break;
            }

            if (_playingReplayInfo.StartTile > chooseSeqID)
                chooseSeqID = _playingReplayInfo.StartTile;
            if (_playingReplayInfo.EndTile < chooseSeqID)
                chooseSeqID = _playingReplayInfo.EndTile;

            return chooseSeqID;
        }

        
        // toggle pause
        public static void TogglePause()
        {
            if (scrController.instance.pauseMenu.gameObject.activeSelf) return;
            WatchReplay.IsPaused = !WatchReplay.IsPaused;

            if (WatchReplay.IsPaused)
            {
                PauseImage.sprite = ReplayAssets.ResumeImage;
                if (WatchReplay.IsPlanetDied) return;
                scrController.instance.enabled = false;
                scrController.instance.audioPaused = true;
                Time.timeScale = 0;
            }
            else
            {
                PauseImage.sprite = ReplayAssets.PauseImage;
                if (WatchReplay.IsPlanetDied) return;
                scrController.instance.audioPaused = false;
                scrController.instance.enabled = true;
                Time.timeScale = 1;
            }
        }
        
        
        // set slider value
        public static void UpdateTime()
        {
            if (scrController.instance == null) return;
            if (_playingReplayInfo == null) return;
            if (ReplayUI.Instance == null) return;
            if (ReplayUI.Instance.PositionSlider == null) return;

            var startTime = scrLevelMaker.instance.listFloors[_playingReplayInfo.StartTile].entryTime;
            var cd = GCS.checkpointNum == 0
                ? scrConductor.instance.crotchetAtStart * scrConductor.instance.adjustedCountdownTicks
                : 0;
            var time = WatchReplay.GetPlayTime() - startTime;
            var s = scrController.instance.currentState;
            
            
            
            if (s == States.Start || s == States.Countdown || s == States.Checkpoint || s == States.None || s == States.Won || WatchReplay.IsLoading)
            {
                time = scrController.instance.currFloor.entryTime - startTime;
                if (!_isEditingTime && !_valueChanging)
                    ReplayUI.Instance.PositionSlider.value = (float)(time/((_playingReplayInfo.PlayTime*0.001f)-cd));
                if ((_playingReplayInfo.PlayTime * 0.001f) - cd > time)
                    ReplayUI.Instance.CurrentTime.text = ReplayUtils.Ms2time((long)(time+cd)*1000);
                return;
            }
            
            if (!_isEditingTime && !_valueChanging)
                ReplayUI.Instance.PositionSlider.value =(float)(time/((_playingReplayInfo.PlayTime*0.001f)-cd));
            if ((_playingReplayInfo.PlayTime * 0.001f) - cd > time)
                ReplayUI.Instance.CurrentTime.text = ReplayUtils.Ms2time((long)(time+cd)*1000);
        }

        
        // Slider Go, Back 10 seconds
        public static void Go10Second()
        {
            if (scrController.instance.pauseMenu.gameObject.activeSelf) return;
            if (WatchReplay.IsResetLevel) return;
            _goBackStack++;
            
            if (_sliderUpdate != null)
            {
                _sliderUpdate.Kill();
                _sliderUpdate = null;
            }
            
            _sliderUpdate = DOVirtual.DelayedCall(0.3f, () =>
            {
                if (WatchReplay.IsResetLevel)
                {
                    _goBackStack = 0;
                    return;
                }
                var startTime = scrLevelMaker.instance.listFloors[_playingReplayInfo.StartTile].entryTime;
                var currentTime = (scrController.instance.currFloor.entryTime + (_goBackStack * 10)) - startTime;
                var seqID = FindFloorBySecond(currentTime, _playingReplayInfo.StartTile, _playingReplayInfo.EndTile);

                WatchReplayPatches.ReplayStartAt(seqID);
                _goBackStack = 0;
            });
        }
        
        public static void Back10Second()
        {
            if (scrController.instance.pauseMenu.gameObject.activeSelf) return;
            if (WatchReplay.IsResetLevel) return;
            _goBackStack--;
            
            if (_sliderUpdate != null)
            {
                _sliderUpdate.Kill();
                _sliderUpdate = null;
            }
            
            _sliderUpdate = DOVirtual.DelayedCall(0.3f, () =>
            {
                if (WatchReplay.IsResetLevel)
                {
                    _goBackStack = 0;
                    return;
                }
                var startTime = scrLevelMaker.instance.listFloors[_playingReplayInfo.StartTile].entryTime;
                var currentTime = (scrController.instance.currFloor.entryTime + (_goBackStack * 10)) - startTime;
                var seqID = FindFloorBySecond(currentTime, _playingReplayInfo.StartTile, _playingReplayInfo.EndTile);

                WatchReplayPatches.ReplayStartAt(seqID);
                _goBackStack = 0;
            });
        }

        
        // Adjust directly through the slider
        public static void OnValueChange(float value)
        {
            if (scrController.instance.pauseMenu.gameObject.activeSelf) return;
            if (!_isEditingTime) return;
            if (_sliderUpdate != null)
            {
                _sliderUpdate.Kill();
                _sliderUpdate = null;
            }

            _valueChanging = true;
            _sliderUpdate = DOVirtual.DelayedCall(0.3f, () =>
            {
                if (!_valueChanging) return;
                if (WatchReplay.IsResetLevel)
                {
                    _goBackStack = 0;
                    _valueChanging = false;
                    _isEditingTime = false;
                    return;
                }
                
                var startTime = scrLevelMaker.instance.listFloors[_playingReplayInfo.StartTile].entryTime;
                var endTime = scrLevelMaker.instance.listFloors[_playingReplayInfo.EndTile].entryTime;
                var currentTime = (endTime - startTime) * value;
                var seqID = FindFloorBySecond(currentTime, _playingReplayInfo.StartTile, _playingReplayInfo.EndTile);
                
                WatchReplayPatches.ReplayStartAt(seqID);
                _goBackStack = 0;
                _valueChanging = false;
                _isEditingTime = false;
            });

        }

        
        // Detect if the slider is pressed
        public static void OnPointerDown()
        {
            if (scrController.instance.pauseMenu.gameObject.activeSelf) return;
            if (WatchReplay.IsResetLevel) return;
            _isEditingTime = true;

            OnValueChange(ReplayUI.Instance.PositionSlider.value);
        }
        
        
        // Pitch Up Down
        public static void PitchUp()
        {
            GCS.currentSpeedTrial += 0.1f;
            GCS.nextSpeedRun = GCS.currentSpeedTrial;
            _valueChanging = true;

            if (GCS.currentSpeedTrial > 10)
            {
                GCS.currentSpeedTrial = 10;
                GCS.nextSpeedRun = GCS.currentSpeedTrial;
            }

            ReplayUI.Instance.PitchText.text = $"{GCS.currentSpeedTrial: 0.0}x";

            if (_pitchUpDown != null)
            {
                _pitchUpDown.Kill();
                _pitchUpDown = null;
            }
            _pitchUpDown = DOVirtual.DelayedCall(0.3f, () =>
            {
                WatchReplay.PatchedPitch = GCS.currentSpeedTrial;
                WatchReplayPatches.ReplayStartAt(scrController.instance.currentSeqID-1);
                ReplayUI.Instance.PitchText.text = $"{GCS.currentSpeedTrial: 0.0}x";
                _valueChanging = false;
            });
        }
        
        public static void PitchDown()
        {
            GCS.currentSpeedTrial -= 0.1f;
            GCS.nextSpeedRun = GCS.currentSpeedTrial;
            _valueChanging = true;

            if (GCS.currentSpeedTrial < 0.1)
            {
                GCS.currentSpeedTrial = 0.1f;
                GCS.nextSpeedRun = GCS.currentSpeedTrial;
            }

            ReplayUI.Instance.PitchText.text = $"{GCS.currentSpeedTrial: 0.0}x";

            if (_pitchUpDown != null)
            {
                _pitchUpDown.Kill();
                _pitchUpDown = null;
            }
            _pitchUpDown = DOVirtual.DelayedCall(0.3f, () =>
            {
                WatchReplay.PatchedPitch = GCS.currentSpeedTrial;
                WatchReplayPatches.ReplayStartAt(scrController.instance.currentSeqID-1);
                ReplayUI.Instance.PitchText.text = $"{GCS.currentSpeedTrial: 0.0}x";
                _valueChanging = false;
            });
        }
    }
}