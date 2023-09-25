using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using UnityModManagerNet;
using Application = UnityEngine.Application;
using Object = UnityEngine.Object;

namespace ReplayLoader
{
    [HarmonyPatch]
    public class NoticePatch
    {
        internal static bool _isFirstLoading;
        private static bool _disableAll;

        [HarmonyPatch(typeof(scrController), "Awake")]
        [HarmonyPostfix]
        public static void InitFirstSettingPatch()
        {
            if (_isFirstLoading) return;
            _isFirstLoading = true;

            if (ReplayUI.Instance == null)
            {
                var r = ReplayAssets.Assets.LoadAsset<GameObject>("assets/prefs/replayui.prefab");
                var a = Object.Instantiate(r);
                Object.DontDestroyOnLoad(a);
                ReplayUI.Instance = a.GetComponent<ReplayUI>();
            }

            if (Loader._updateInfo.mustUpdate)
            {
                var body = Loader._updateInfo.data.Split(new[] { "\"body\": \"" }, StringSplitOptions.None)[1].Split('"')[0]
                    .Replace("\\n", "\n").Replace("\\r", "\r");
                var url = Loader._updateInfo.data.Split(new[] { "\"browser_download_url\": \"" }, StringSplitOptions.None)[1]
                    .Split('"')[0];

                GlobalLanguage.OK = Loader.CurrentLang.autoUpdate;
                GlobalLanguage.No = Loader.CurrentLang.nextTimeUpdate;

                _disableAll = true;
                scrController.instance.paused = true;
                Time.timeScale = 0;

                ReplayUI.Instance.ShowNotification(
                    $"{Loader.CurrentLang.newReplayVersion} v" + Loader._updateInfo.version, body,
                    () =>
                    {
                        ReplayUI.Instance.Message.text = Loader.CurrentLang.downloadingText;
                        scrSfx.instance.PlaySfx(SfxSound.MenuSquelch);
                        ReplayUI.Instance.NoButton.gameObject.SetActive(false);
                        ReplayUI.Instance.YesButton.gameObject.SetActive(false);
                        Loader._updateInfo.WebClient.DownloadProgressChanged += (o, e) =>
                        {
                            ReplayUI.Instance.Message.text = Loader.CurrentLang.downloadingText + " ( " +
                                                             e.ProgressPercentage + "% )";
                        };
                        Loader._updateInfo.WebClient.DownloadFileAsync(new Uri(url), Path.Combine(Path.GetTempPath(), "replay-new.zip"));
                        Loader._updateInfo.WebClient.DownloadFileCompleted += (o, e) =>
                        {

                            ReplayUI.Instance.Message.text = Loader.CurrentLang.restartSoon;
                            var thread = new Thread(() =>
                            {
                                foreach (var file in Directory.GetFiles(Loader.unityModEntry.Path))
                                {
                                    try
                                    {
                                        if (Path.GetFileName(file) != "ReplayOption.xml")
                                            File.Delete(file);
                                    }
                                    catch
                                    {
                                        Loader.unityModEntry.Logger.Log("Cant delete " + file);
                                    }
                                }


                                ZipUtils.Unzip(Path.Combine(Path.GetTempPath(), "replay-new.zip"),
                                    Path.Combine(UnityModManager.modsPath, "Replay"));
                                File.Delete(Path.Combine(Path.GetTempPath(), "replay-new.zip"));

                                Thread.Sleep(1000);

                                Application.Quit();
                                Process.Start("steam://rungameid/977950");
                            });
                            thread.IsBackground = true;
                            thread.Start();



                        };
                        return false;
                    }, () =>
                    {
                        _disableAll = false;
                        scrController.instance.paused = false;
                        Time.timeScale = 1;
                        
                        scrSfx.instance.PlaySfx(SfxSound.MenuSquelch);
                        return true;
                    }, RDString.language);
                
                
            } else if (!string.IsNullOrEmpty(Loader.errorMessage))
            {
                _disableAll = true;
                scrController.instance.paused = true;
                Time.timeScale = 0;
                
                GlobalLanguage.OK = Loader.CurrentLang.copyText;
                ReplayUI.Instance.ShowNotification(Loader.CurrentLang.cantLoad, Loader.errorMessage, () =>
                {
                    _disableAll = false;
                    scrController.instance.paused = false;
                    Time.timeScale = 1;
                    scrSfx.instance.PlaySfx(SfxSound.MenuSquelch);
                    Clipboard.SetText(Loader.errorMessage);
                    return true;
                },null,RDString.language);
            }

            ReplayUI.Instance.StartCoroutine(Nextframe());

        }
        
        
        private static IEnumerator Nextframe()
        {
            yield return null;
            UpdateLayout();
            yield return new WaitForEndOfFrame();
            UpdateLayout();
        }
        
        public static void UpdateLayout()
        {
            if (ReplayUI.Instance != null)
            {
                if (ReplayUI.Instance.BbiBbiGameobject != null)
                {
                    var t = ReplayUI.Instance.BbiBbiGameobject.transform?.Find("TextLayout")?.GetComponent<RectTransform>();
                    if(t!=null)
                        LayoutRebuilder.ForceRebuildLayoutImmediate(t);
                    var t2 = ReplayUI.Instance.BbiBbiGameobject.transform?.Find("TextLayout")?.Find("No")?.GetComponent<RectTransform>();
                    if(t2!=null)
                        LayoutRebuilder.ForceRebuildLayoutImmediate(t2);
                    var t3 = ReplayUI.Instance.BbiBbiGameobject.transform?.Find("TextLayout")?.Find("Yes")?.GetComponent<RectTransform>();
                    if(t3!=null)
                        LayoutRebuilder.ForceRebuildLayoutImmediate(t3);
                }
            }
        }
        
        
    }
    
}