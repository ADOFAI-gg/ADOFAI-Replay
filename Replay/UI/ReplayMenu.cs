using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Replay.Clasz;
using Replay.Patch;
using TinyJson;
using Replay.Lib;
using HarmonyLib;

namespace Replay.UI
{
    internal class ReplayMenu : MonoBehaviour
    {
        private GUIStyle titleStyle, buttonStyle, textStyle, backStyle;
        public Vector2 scrollPosition;
        public List<ReplayData> replays = new List<ReplayData>();
        int n = 0;
        bool close = false, alert = false;
        string message = "", title = "";

        public void closeWindow()
        {
            close = true;
            n = 0;
        }

        private double getPer(double value, double total)
        {
            return Math.Round(value * 100 / total);
        }

        public void ShowAlert(string _title, string _message)
        {
            title = _title;
            message = _message;
            alert = true;
        }


        private Texture2D MakeTex(int width, int height, Color col)
        {
            Color[] pix = new Color[width * height];
            for (int i = 0; i < pix.Length; ++i)
            {
                pix[i] = col;
            }
            Texture2D result = new Texture2D(width, height);
            result.SetPixels(pix);
            result.Apply();
            return result;
        }

        void Start()
        {
            n = 0;
        }

        void OnGUI()
        {
            if (titleStyle == null)
            {

                titleStyle = new GUIStyle(GUI.skin.box);
                titleStyle.font = RDString.GetFontDataForLanguage(RDString.language).font;
                titleStyle.normal.textColor = Color.white;
                titleStyle.fontSize = 50;
                titleStyle.normal.background = MakeTex(2, 2, Color.clear);
                
                buttonStyle = new GUIStyle(GUI.skin.button);
                buttonStyle.font = RDString.GetFontDataForLanguage(RDString.language).font;
                buttonStyle.fontSize = 30;
                //buttonStyle.stretchWidth = false;
                buttonStyle.normal.background = Main.smallcard;
                buttonStyle.hover.background = Main.smallcard;
                buttonStyle.active.background = Main.smallcard;

                textStyle = new GUIStyle();
                textStyle.font = RDString.GetFontDataForLanguage(RDString.language).font;
                textStyle.normal.textColor = Color.white;
                textStyle.fontSize = 40;

                backStyle = new GUIStyle();
                backStyle.font = RDString.GetFontDataForLanguage(RDString.language).font;
                backStyle.normal.textColor = Color.white;
                backStyle.fontSize = 40;
                
                backStyle.normal.background = Main.card;
            }



            if ((n > Screen.width&&!close)||(close&& n < Screen.width))
            {
                    GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Main.background, ScaleMode.ScaleToFit);

                    DirectoryInfo di = new System.IO.DirectoryInfo("./Replay/");

                    GUILayout.BeginArea(new Rect(Screen.width / 2 - 400, Screen.height / 2 - 300, 800, 600), Main.language.name, titleStyle);

                    GUILayout.Space(70);
                    scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Width(800), GUILayout.Height(600));

                    if (replays.Count==0)
                    {
                        GUILayout.BeginHorizontal();
                        GUILayout.Space(250);
                        GUILayout.Label(Main.language.noreplay, textStyle);
                        GUILayout.EndHorizontal();
                        
                    }

                if (replays.Count > 0)
                {
                    foreach (ReplayData data in replays)
                    {
                        if (Misc.IsNull(data.id)) continue;



                        int length = data.path.Length;
                        double startper = getPer(data.start, length);
                        double per = getPer(data.end, length);
                        string text = $"{startper}% ~ {per}%";
                        if (per > 95) text = "<color=#fe566f>sogogi</color>";
                        if (length == data.end) text = "<color=#f8f700>clear</color>";

                        GUILayout.Box("", backStyle, GUILayout.Width(790), GUILayout.Height(120));
                        GUILayout.Space(-125);
                        GUILayout.Label($"<size=60> {data.name}</size> <size=30>{text}</size>", textStyle);
                        string timeInterval = Misc.ms2time(long.Parse(data.time));
                        GUILayout.Space(-20);
                        GUILayout.Label("  " + timeInterval, textStyle);
                        GUILayout.Space(-30);
                        GUILayout.BeginHorizontal();

                        GUILayout.Space(700);
                        if (GUILayout.Button($" {Main.language.play} ", buttonStyle, GUILayout.Width(80), GUILayout.Height(40)))
                        {
                            WorldReplay.reset();
                            WorldReplay.data = data;
                            FileInfo fileInfo = new FileInfo(Main.path + data.id + "/main.adofai");

                            if (!fileInfo.Exists)
                            {
                                LogError.Show(ErrorCodes.FileNotFound, $"{fileInfo.DirectoryName} not found");
                                continue;
                            }

                            GCS.speedTrialMode = !(data.speed.Equals(1));
                            if (GCS.speedTrialMode)
                            {
                                GCS.currentSpeedRun = data.speed;
                                GCS.nextSpeedRun = data.speed;
                            }
                            string path = "";
                            try
                            {
                                path = SteamWorkshop.resultItems[0].path.Split(new string[] { "content\\977950" }, StringSplitOptions.None)[0] + "content/977950/" + data.id + "/main.adofai";
                            }
                            catch
                            {
                                path = Main.path + data.id + "/main.adofai";
                            }
                            PrivateLoad<scrController> privateLoad = new PrivateLoad<scrController>("LoadCustomWorld", scrController.instance);

                            if (Main.version >= 72) privateLoad.Call(new object[] { path, true });
                            else privateLoad.Call(new object[] { path });


                            WorldReplay.Start(data);
                            Main.gui.ShowReplayText();

                            /*
                                if (WorldReplay.Slider != null)
                                {
                                    UnityEngine.Object.DestroyImmediate(WorldReplay.Slider);
                                    WorldReplay.Slider = null;
                                }

                                WorldReplay.Slider = new GameObject().AddComponent<ReplaySlider>();
                                UnityEngine.Object.DontDestroyOnLoad(WorldReplay.Slider);
                                WorldReplay.Slider.replayData = data;
                                WorldReplay.Slider.isStart = true;
                            */

                            Main.gui.ReplayObject.SetActive(WorldReplay.isReplayStart);
                            UnityEngine.Object.DestroyImmediate(PlayHistory.Menu);
                            PlayHistory.Menu = null;
                        }


                        GUILayout.EndHorizontal();

                        GUILayout.Space(-45);
                        GUILayout.BeginHorizontal();
                        GUILayout.Space(610);

                        if (GUILayout.Button($" {Main.language.remove} ", buttonStyle, GUILayout.Width(80), GUILayout.Height(40)))
                        {
                            replays.Remove(data);
                            File.Delete(data.filepath);
                            break;

                        }

                        GUILayout.EndHorizontal();

                        GUILayout.Space(40);

                    }
                }

                    GUILayout.Space(50);
                    GUILayout.EndScrollView();

                    GUILayout.EndArea();
                
                
            }

            if (Event.current.type == EventType.Repaint)
            {
                int max = Screen.width + Screen.width / 2;
                GUI.DrawTexture(new Rect(n - max, 0, max, Screen.height), MakeTex(2, 2, Color.black));

              
                if (n > Screen.width)
                {
                    scrConductor.instance.song.clip = close? Main.mainSong:scrController.instance.halloweenMusic;
                    if(scrController.instance.paused) scrController.instance.TogglePauseGame();
                }
                if (n>max*2)
                {
                    PlayHistory.isMenuOpening = false;
                    if(close)
                    {
                        UnityEngine.Object.DestroyImmediate(PlayHistory.Menu);
                        PlayHistory.Menu = null;
                        
                    }
                }

                n += Screen.width/40;


            }
        }
    }
}
