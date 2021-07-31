using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Replay.Patch;
using Replay.Clasz;
using System.Reflection;
using HarmonyLib;
using Replay.Lib;

namespace Replay.UI
{

    internal class ReplayUI : MonoBehaviour
    {
        public GameObject ReplayObject;
        private Text ReplayText;
        RectTransform rectTransform;
        public string text = "";

        
        private void OnGUI()
        {


            GUI.Label(new Rect(20,20,Screen.width,Screen.height),scrController.instance.chosenplanet.angle+"    "+scrController.instance.chosenplanet.targetExitAngle+text);
        }


        public void ShowSaveText(bool canSave)
        {
            ReplayObject.SetActive(true);
            ReplayText.text = canSave? Main.language.save:Main.language.cantsave;
            
            int x = 200;
            rectTransform.anchoredPosition = new Vector2(x, -30);
            for(int n=1;n<24;n++) {
                Task.Delay(n*10).ContinueWith((task) =>
                {
                    x -= 10;
                    rectTransform.anchoredPosition = new Vector2(x, -30);
                });
            }

            for (int n = 1; n < 24; n++)
            {
                Task.Delay((n * 10)+1000).ContinueWith((task) =>
                {
                    x += 10;
                    rectTransform.anchoredPosition = new Vector2(x, -30);
                });
            }

            Task.Delay((24 * 10) + 1000).ContinueWith((task) =>
            {
                ReplayObject.SetActive(false);
                ReplayText.text = "<color=#ff0000>●</color> Replaying";
                rectTransform.anchoredPosition = new Vector2(-30, -30);
            });

            
        }

        public void ShowReplayText()
        {
            ReplayObject.SetActive(true);
            ReplayText.text = $"<color=#ff0000>●</color> {Main.language.replaying}";

            
            rectTransform = ReplayText.GetComponent<RectTransform>();
            rectTransform.anchoredPosition = new Vector2(-30, -30);
        }
        

        void Awake()
        {
            ReplayObject = new GameObject();
            GameObject.DontDestroyOnLoad(ReplayObject);
            Canvas canvas = ReplayObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scalar = ReplayObject.AddComponent<CanvasScaler>();
            scalar.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scalar.referenceResolution = new Vector2(1920, 1080);
            ReplayObject.AddComponent<GraphicRaycaster>();

            ReplayText = createText("<color=#ff0000>●</color> Replaying");

            
          
            rectTransform.anchoredPosition = new Vector2(-30, -30);

            ReplayObject.SetActive(WorldReplay.isReplayStart);
        }


        private Text createText(string message)
        {
            GameObject textObject = new GameObject();
            textObject.transform.SetParent(ReplayObject.transform);
            Text text = textObject.AddComponent<Text>();
            text.text = message;
            text.font = RDString.GetFontDataForLanguage(RDString.language).font;
            text.fontSize = 60;
            text.color = Color.white;
            text.alignment = TextAnchor.UpperRight;

            rectTransform = textObject.GetComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(1, 1);
            rectTransform.anchorMax = new Vector2(1, 1);
            rectTransform.pivot = new Vector2(1, 1);
            rectTransform.sizeDelta = new Vector2(600, 100);
            
            return text;
        }

        
      
	}
}
