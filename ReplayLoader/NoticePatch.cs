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
        private static bool _isFirstLoading;
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
                var body = Loader._updateInfo.data.Split(new[] { "\"body\": \"" }, StringSplitOptions.None)[1]
                    .Split('"')[0]
                    .Replace("\\n", "\n").Replace("\\r", "\r");
                var url = Loader._updateInfo.data.Split(new[] { "\"browser_download_url\": \"" },
                        StringSplitOptions.None)[1]
                    .Split('"')[0];

                GlobalLanguage.OK = Loader.CurrentLang.autoUpdate;
                GlobalLanguage.No = Loader.CurrentLang.nextTimeUpdate;

                ReplayUI.Instance.ShowNotification(
                    $"{Loader.CurrentLang.newReplayVersion} v" + Loader._updateInfo.version, body,
                    () => DownloadingStart(url), () =>
                    {
                        _disableAll = false;
                        scrController.instance.paused = false;
                        Time.timeScale = 1;

                        scrSfx.instance.PlaySfx(SfxSound.MenuSquelch);
                        return true;
                    }, RDString.language);
                
                Loader.UpdateLayoutNextFrame();
                
                _disableAll = true;
                scrController.instance.paused = true;
                Time.timeScale = 0;
            }
            else if (!string.IsNullOrEmpty(Loader.errorMessage))
                ShowError();
        }


        private static void ShowError()
        {
            GlobalLanguage.OK = Loader.CurrentLang.copyText;
            ReplayUI.Instance.ShowNotification(Loader.CurrentLang.cantLoad, Loader.errorMessage, () =>
            {
                _disableAll = false;
                scrController.instance.paused = false;
                Time.timeScale = 1;
                scrSfx.instance.PlaySfx(SfxSound.MenuSquelch);
                Clipboard.SetText(Loader.errorMessage);
                return true;
            }, null, RDString.language);
            
            Loader.UpdateLayoutNextFrame();
            
            _disableAll = true;
            scrController.instance.paused = true;
            Time.timeScale = 0;
        }


        private static bool DownloadingStart(string url)
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
            Loader._updateInfo.WebClient.DownloadFileAsync(new Uri(url),
                Path.Combine(Path.GetTempPath(), "replay-new.zip"));
            Loader._updateInfo.WebClient.DownloadFileCompleted += (o, e) =>
            {

                ReplayUI.Instance.Message.text = Loader.CurrentLang.restartSoon;
                var thread = new Thread(InstallAndRun)
                {
                    IsBackground = true
                };
                thread.Start();



            };
            return false;
        }

        private static void InstallAndRun()
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
        }


    }

}