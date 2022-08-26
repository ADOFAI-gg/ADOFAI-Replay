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
                if (ReplayUIUtils._swipeImage.anchoredPosition.x >= -1920 &&
                    ReplayUIUtils._swipeImage.anchoredPosition.x <= 1920)
                {

                    if (ReplayUIUtils._movingTween1 != null && ReplayUIUtils._movingTween2 != null)
                    {
                        if (!ReplayUIUtils._movingTween1.active && !ReplayUIUtils._movingTween2.active)
                        {
                            ReplayUIUtils.Hide();
                        }
                    } else if (ReplayUIUtils._movingTween1 != null)
                    {
                        if (!ReplayUIUtils._movingTween1.active && ReplayUIUtils._movingTween2 == null)
                        {
                            ReplayUIUtils.Hide();
                        }
                    }
                }
                
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (SceneManager.GetActiveScene().name == "scnReplayIntro")
                {
                    ReplayUIUtils.DoSwipe(()=>SceneManager.LoadScene("scnNewIntro"));
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