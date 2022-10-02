using System;
using DG.Tweening;
using Replay.Functions.Watching;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Replay.UI
{
    public class ReplayBackground : MonoBehaviour
    {
        private void Update()
        {
            if (ReplayUIUtils._createUI)
            {
                if (ReplayUIUtils._swipeImage.anchoredPosition.x > -1920 &&
                    ReplayUIUtils._swipeImage.anchoredPosition.x < 1920)
                { 
                    if (!ReplayUIUtils._swiping)
                    {
                        ReplayUIUtils.Hide();
                    }
                }
            }
            
        }


        private void OnApplicationQuit()
        {
            WatchReplay.IsPlaying = false;
            if(scrController.instance != null)
                scrController.instance.audioPaused = true;
            GCS.checkpointNum = 0;
        }
    }
}