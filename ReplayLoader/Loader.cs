using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using HarmonyLib;
using ReplayLoader.Languages;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityModManagerNet;
using Application = System.Windows.Forms.Application;

namespace ReplayLoader
{
    public static class Loader
    {
        private static Dictionary<SystemLanguage, LocalizedText> _languages = new Dictionary<SystemLanguage, LocalizedText>
        {
            {SystemLanguage.Korean, new Korean()},
            {SystemLanguage.English, new English()},
            {SystemLanguage.Japanese, new Japanese()},
        };
        
        public static LocalizedText CurrentLang => _languages.TryGetValue(RDString.language, out var v)
            ? v
            : _languages[SystemLanguage.English];
        
        private static Harmony _replayHarmony;
        
        internal static UnityModManager.ModEntry unityModEntry;
        internal static UpdateInfo _updateInfo = new UpdateInfo();
        internal static string errorMessage;
        
        private static void UpdateLayout()
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
        
        public static void UpdateLayoutNextFrame()
        {
            UpdateLayout();
            ReplayUI.Instance?.StartCoroutine(NextframeUpdate());
        }
        
        
        private static IEnumerator NextframeUpdate()
        {
            yield return null;
            UpdateLayout();
            yield return new WaitForEndOfFrame();
            UpdateLayout();
        }
        
        public static void Setup(UnityModManager.ModEntry modEntry)
        {
            _replayHarmony = new Harmony("replay.loader.patcher");
            unityModEntry = modEntry;
            ReplayAssets.Init();
            _replayHarmony.PatchAll();

            try
            {
                var nowVersion = 0;
                int.TryParse(unityModEntry.Info.Version.Replace(".", ""), out nowVersion);

                var wc = new WebClient();
                wc.Encoding = Encoding.UTF8;
                wc.Headers.Add("user-agent",
                    "Mozilla/5.0 (Linux; Android 6.0; Nexus 5 Build/MRA58N) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/105.0.0.0 Mobile Safari/537.36");
                
                var data = wc.DownloadString(
                    "https://api.github.com/repos/ADOFAI-gg/ADOFAI-Replay/releases/latest");
                var latestVersionString =
                    data.Split(new[] { "\"tag_name\": \"" }, StringSplitOptions.None)[1].Split('"')[0]
                        .Replace("v", "");

                unityModEntry.Logger.Log(
                    $"Current Version: {unityModEntry.Info.Version}, Latest Version: {latestVersionString}");
                var latestVersion = 0;
                int.TryParse(latestVersionString.Replace(".", ""), out latestVersion);

                if (latestVersion > nowVersion)
                {
                    _updateInfo.data = data;
                    _updateInfo.mustUpdate = true;
                    _updateInfo.WebClient = wc;
                    _updateInfo.version = latestVersionString;
                }
                    
                
            }
            catch 
            {
                unityModEntry.Logger.Log("cant updated");
            }

            if (!_updateInfo.mustUpdate)
            {
                try
                {
                    var a = Assembly.LoadFrom(Path.Combine(modEntry.Path, "Replay.dll"));
                    a.GetType("Replay.Replay").GetMethod("Setup", AccessTools.all).Invoke(null, new object[] {unityModEntry });
                    if(SceneManager.GetActiveScene().name != "scnSplash")
                        NoticePatch.InitFirstSettingPatch();
                    //Replay.Replay.Setup(unityModEntry);
                }
                catch (Exception e)
                {
                    errorMessage = e.ToString();
                }
            }
            
            
        }
    }
}