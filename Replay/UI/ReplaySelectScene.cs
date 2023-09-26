using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using DG.Tweening;
using Discord;
using HarmonyLib;
using Replay.Functions.Core;
using Replay.Functions.Core.Types;
using Replay.Functions.Watching;
using ReplayLoader;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Replay.UI
{
    public class ReplaySelectScene
    {

        public static Dictionary<ReplayInfo, ReplayUIInfo> ReplayToInfo = new Dictionary<ReplayInfo, ReplayUIInfo>();
        public static int ShareCount = 0;

        public static void SetLanguage()
        {
            GlobalLanguage.ProgressTitle = Replay.CurrentLang.progressTitle;
            GlobalLanguage.SaveError = Replay.CurrentLang.cantSave;
            GlobalLanguage.ToQuit = Replay.CurrentLang.toMainTitle;
            GlobalLanguage.ReplayListTitle = Replay.CurrentLang.replayListTitle;
            GlobalLanguage.LevelLengthTitle = Replay.CurrentLang.levelLengthTitle;
            GlobalLanguage.SaveSuccess = Replay.CurrentLang.saveSuccess;
            GlobalLanguage.PlayButtonTitle = Replay.CurrentLang.playButtonTitle;
            GlobalLanguage.ReplayingTitle = Replay.CurrentLang.replayingText;
            ReplayUI.Instance.ReplayingTitle.text = GlobalLanguage.ReplayingTitle;
        }

        private static IEnumerator WaitAndHide()
        {
            yield return new WaitForSeconds(0.52f);
            ReplayUIUtils.Hide();
            scnReplayIntro.scnReplayIntro.Instance.mainCamera.enabled = true;
        }

        private static void OnReplayPlay(ReplayInfo rpl, ReplayUIInfo rpinfo)
        {
            if (scnReplayIntro.scnReplayIntro.Instance != null)
                scnReplayIntro.scnReplayIntro.Instance.StopAllCoroutines();

            if (!File.Exists(rpl.Path) && !rpl.IsOfficialLevel)
            {
                scnReplayIntro.scnReplayIntro.CanEscape = false;
                ReplayUI.Instance.ShowNotification(Replay.CurrentLang.replayMod, Replay.CurrentLang.cantFindPath, () =>
                {
                    rpinfo.replayViewCard.action = false;
                    scnReplayIntro.scnReplayIntro.CanEscape = true;
                    return true;
                }, null, RDString.language);
                Loader.UpdateLayoutNextFrame();
                return;
            }

            ReplayUIUtils.DoSwipe(() => WatchReplay.Play(rpl));
        }

        private static void OnReplayUpload(ReplayInfo rpl, string f)
        {
            GlobalLanguage.OK = Replay.CurrentLang.okText;
            GlobalLanguage.No = Replay.CurrentLang.noText;
            ServerManager.State = UploadState.None;
            scnReplayIntro.scnReplayIntro.CanEscape = false;
            
            if (rpl.IsOfficialLevel)
            {
                ReplayUI.Instance.ShowNotification(Replay.CurrentLang.replayModText,
                    Replay.CurrentLang.notSupportOfficialLevel,
                    () =>
                    {
                        scrSfx.instance.PlaySfx(SfxSound.MenuSquelch);
                        scnReplayIntro.scnReplayIntro.CanEscape = true;
                        return true;
                    }, null, RDString.language);
                Loader.UpdateLayoutNextFrame();
                return;
            }

            if (ShareCount <= 50)
            {
                ReplayUI.Instance.ShowNotification(Replay.CurrentLang.replayModText,
                    Replay.CurrentLang.reallyShareThisReplay,
                    () =>
                    {
                        scrSfx.instance.PlaySfx(SfxSound.MenuSquelch);
                        ReplayUI.Instance.Message.text = Replay.CurrentLang.uploadingText;
                        ReplayUI.Instance.NoButton.gameObject.SetActive(false);
                        ReplayUI.Instance.YesButton.gameObject.SetActive(false);
                        ServerManager.State = UploadState.None;
                        
                        scnReplayIntro.scnReplayIntro.Instance.StartCoroutine(UploadWait());
                        Task.Run(() => { ServerManager.ShareReplay(f, rpl); });
                        return false;
                    }, () =>
                    {
                        scnReplayIntro.scnReplayIntro.CanEscape = true;
                        scrSfx.instance.PlaySfx(SfxSound.MenuSquelch);
                        return true;
                    }, RDString.language);
                Loader.UpdateLayoutNextFrame();
            }
            else
            {
                ReplayUI.Instance.ShowNotification(Replay.CurrentLang.replayModText,
                    Replay.CurrentLang.cantShareBecauseLimitOver,
                    () =>
                    {
                        scrSfx.instance.PlaySfx(SfxSound.MenuSquelch);
                        scnReplayIntro.scnReplayIntro.CanEscape = true;
                        return true;
                    }, null, RDString.language);
                Loader.UpdateLayoutNextFrame();
            }
        }

        private static void SetPublicRemoveAction(ReplayInfo rpl, ReplayUIInfo rpinfo, string f)
        {
            ShareCount++;
            rpinfo.replayViewCard.Remove.onClick.RemoveAllListeners();
            rpinfo.replayViewCard.Remove.onClick.AddListener(() =>
            {
                scrSfx.instance.PlaySfx(SfxSound.MenuSquelch);
                scnReplayIntro.scnReplayIntro.CanEscape = false;
                
                GlobalLanguage.OK = Replay.CurrentLang.okText;
                GlobalLanguage.No = Replay.CurrentLang.noText;
                
                ReplayUI.Instance.ShowNotification(Replay.CurrentLang.reallyDeleteSharedReplay,
                    Replay.CurrentLang.reallyDeleteSharedReplayMoreMessage, () => RemovingAnimation(rpl, rpinfo, f), () =>
                    {
                        rpinfo.replayViewCard.action = false;
                        scrSfx.instance.PlaySfx(SfxSound.MenuSquelch);
                        scnReplayIntro.scnReplayIntro.CanEscape = true;
                        return true;
                    }, RDString.language);

                Loader.UpdateLayoutNextFrame();
            });
        }

        private static bool RemovingAnimation(ReplayInfo rpl, ReplayUIInfo rpinfo, string f)
        {
            rpinfo.replayViewCard.action = false;
            scrSfx.instance.PlaySfx(SfxSound.MenuSquelch);
            scnReplayIntro.scnReplayIntro.CanEscape = true;
            rpinfo.replayViewCard.transform.DOMoveX(6, 1).SetEase(Ease.OutExpo).OnComplete(() =>
            {
                ServerManager.DeleteReplay(rpl);
                File.Delete(f);
                scnReplayIntro.scnReplayIntro.ReplaysInScroll.Remove(rpinfo.replayViewCard);
                ShareCount--;
                UpdateShareText();
                Object.DestroyImmediate(rpinfo.replayViewCard.gameObject);
            });
            return true;
        }

        public static void Awake()
        {
            GameObject.Find("LoadText").GetComponent<TextMeshProUGUI>().text = Replay.CurrentLang.loadReplay;
            ShareCount = 0;
            ReplayToInfo.Clear();
            GameObject.Find("Main Camera").GetComponent<Camera>().enabled = true;

            GC.Collect();
            scnReplayIntro.scnReplayIntro.Instance.StopAllCoroutines();
            scnReplayIntro.scnReplayIntro.Instance.StartCoroutine(WaitAndHide());

            scrSfx.instance.PlaySfx(SfxSound.ScreenWipeIn);
            var discord = (Discord.Discord)typeof(DiscordController).GetField("discord", AccessTools.all)?
                .GetValue(DiscordController.instance);
            if (discord != null)
            {
                var ac = default(Activity);
                ac.State = "";
                ac.Details = Replay.CurrentLang.replaySceneRPCTitle;
                ac.Assets.LargeImage = "planets_icon_stars";
                ac.Assets.LargeText = "";
                discord.GetActivityManager().UpdateActivity(ac, (result) => { });
            }
            
            ReplayUIUtils.Audio.clip = ADOBase.ownsTaroDLC
                ? scnReplayIntro.scnReplayIntro.Instance.IntroBGMDLC
                : scnReplayIntro.scnReplayIntro.Instance.IntroBGM;

            SetLanguage();

            if (!Directory.Exists(Replay.ReplayOption.savedPath)) return;
            var files = Directory.GetFiles(Replay.ReplayOption.savedPath);

            foreach (var f in files)
            {
                if (!f.EndsWith(".rpl")) continue;

                try
                {
                    var rpl = ReplayUtils.LoadReplay(f);

                    //Table of replay cards displayed on the screen
                    var rpinfo = new ReplayUIInfo
                    {
                        Song = rpl.SongName.Replace("\n", "").Replace("\r", "").Replace("  ", ""),
                        Artist = rpl.ArtistName,
                        StartProgress = (int)(((float)rpl.StartTile / ((float)rpl.AllTile - 1)) * 100),
                        EndProgress = (int)(((float)rpl.EndTile / ((float)rpl.AllTile - 1)) * 100),
                        LevelLength = ReplayUtils.Ms2time((long)(rpl.PlayTime)),
                        Time = rpl.Time
                    };

                    var pre = ReplayUtils.LoadTexture(rpl.PreviewImagePath);
                    if (pre != null)
                        rpinfo.Preview = pre;

                    rpinfo.OnPlay = () => OnReplayPlay(rpl, rpinfo);
                    rpinfo.OnUpload = () => OnReplayUpload(rpl, f);
                    scnReplayIntro.scnReplayIntro.Instance.AddReplayCard(rpinfo);

                    if (rpl.Shared)
                    {
                        rpinfo.replayViewCard.Song.text +=
                            $" <size=15><color=#ffffff99>{rpl.ReplayCode}</color></size>";
                        rpinfo.replayViewCard.Upload.gameObject.SetActive(false);
                    }
                    
                    /*
                     * 또 까먹지 말고 기억할려고 주석
                     * MyReplay = 공유된 리플레이 중 내 리플레이인지 여부
                     * 
                     * 지우는게 내가 공유한 리플레이면은 진짜 지울건지 물어보는 코드
                     */
                    if (rpl.MyReplay)
                        SetPublicRemoveAction(rpl, rpinfo, f);
                    else
                    {
                        rpinfo.OnDelete = () =>
                        {
                            scrSfx.instance.PlaySfx(SfxSound.MenuSquelch);
                            File.Delete(f);
                        };
                    }
                    
                    if (pre != null)
                    {
                        var card = scnReplayIntro.scnReplayIntro.ReplaysInScroll.Last();
                        var v = pre.width / 1036f;
                        card.LevelPreview.rectTransform.sizeDelta = new Vector2(1036, pre.height / v);
                    }
                    
                    ReplayToInfo[rpl] = rpinfo;
                }
                catch (Exception e)
                {
                    Replay.Log(Path.GetFileNameWithoutExtension(f) + "에서 에러");
                    Replay.Log(e);
                }

            }

            UpdateShareText();
        }
        

        public static void OnQuit()
        {
            if (scnReplayIntro.scnReplayIntro.Instance != null)
                scnReplayIntro.scnReplayIntro.Instance.StopAllCoroutines();
            ReplayUIUtils.DoSwipe(() => { SceneManager.LoadScene("scnLevelSelect", LoadSceneMode.Single); });
        }
        
        // When loading a replay file from the server
        public static void OnLoad()
        {
            GlobalLanguage.OK = Replay.CurrentLang.okText;
            GlobalLanguage.No = Replay.CurrentLang.noText;
            
            scnReplayIntro.scnReplayIntro.CanEscape = false;
            scnReplayIntro.scnReplayIntro.Instance.InputField.text = "";
            scnReplayIntro.scnReplayIntro.Instance.InputField.placeholder.gameObject.GetComponent<TextMeshProUGUI>()
                .text = Replay.CurrentLang.enterCodeHintText;
            
            scnReplayIntro.scnReplayIntro.Instance.ShowPopup(Replay.CurrentLang.enterCodeTitle, () =>
            {
                scrSfx.instance.PlaySfx(SfxSound.MenuSquelch);
                var text = scnReplayIntro.scnReplayIntro.Instance.InputField.text;
                if (string.IsNullOrEmpty(text))
                    return false;
                if (text.Length < 8)
                    return false;

                text = text.ToUpper().Trim();

                ReplayUI.Instance.ShowNotification(Replay.CurrentLang.replayModText, Replay.CurrentLang.downloadingText,
                    null, null, RDString.language);
                ServerManager.State = UploadState.None;
                scnReplayIntro.scnReplayIntro.Instance.StartCoroutine(DownloadWait());
                
                Task.Run(() => { ServerManager.DownloadReplay(text); });

                return true;
            }, () =>
            {
                scnReplayIntro.scnReplayIntro.CanEscape = true;
                scrSfx.instance.PlaySfx(SfxSound.MenuSquelch);
                return true;
            });
            
            Loader.UpdateLayoutNextFrame();
        }



        private static IEnumerator UploadWait()
        {
            yield return new WaitUntil(() => ServerManager.State != UploadState.None);

            if (ServerManager.State == UploadState.Success)
            {
                ReplayUI.Instance.Message.text =
                    Replay.CurrentLang.successShareReplay + " <b>" + ServerManager.message + "</b>";
                ShareCount++;
                UpdateShareText();
                ReplayUI.Instance.YesTitle.text = Replay.CurrentLang.copySharedReplayCode;
                ReplayUI.Instance.BbiBbiGameobject.SetActive(true);
                
                Loader.UpdateLayoutNextFrame();

            }
            else
            {
                ReplayUI.Instance.Message.text =
                    Replay.CurrentLang.failShareReplay + " <b>" + ServerManager.message + "</b>";
            }

            HideAndSaveNotification();
        }

        private static void HideAndSaveNotification()
        {
            ReplayUI.Instance.YesButton.gameObject.SetActive(true);
            ReplayUI.Instance.YesButton.onClick.RemoveAllListeners();
            ReplayUI.Instance.YesButton.onClick.AddListener(() =>
            {
                Clipboard.SetText(ServerManager.message);
                scrSfx.instance.PlaySfx(SfxSound.MenuSquelch);
                ReplayUI.Instance.BbiBbiGameobject.SetActive(false);
                scnReplayIntro.scnReplayIntro.CanEscape = true;
            });
        }

        public static void UpdateShareText()
        {
            if (ShareCount < 0)
                ShareCount = 0;
            if (ShareCount > 0)
                scnReplayIntro.scnReplayIntro.Instance.ReplayTitle.text = Replay.CurrentLang.replayListTitle +
                                                                          $"\n<size=15>{Replay.CurrentLang.sharedReplayCount} {ShareCount} / 50 </size>";
            else
                scnReplayIntro.scnReplayIntro.Instance.ReplayTitle.text = Replay.CurrentLang.replayListTitle;
            GlobalLanguage.ReplayListTitle = scnReplayIntro.scnReplayIntro.Instance.ReplayTitle.text;
        }

        private static IEnumerator DownloadWait()
        {
            yield return new WaitUntil(() => ServerManager.State != UploadState.None);
            if (ServerManager.State == UploadState.Success)
            {
                GC.Collect();
                ReplayUI.Instance.BbiBbiGameobject.SetActive(false);
                //invoke sans
                ReplayUIUtils.DoSwipe(() =>
                {
                    WatchReplay.Play(ServerManager.Rpl);
                    scnReplayIntro.scnReplayIntro.CanEscape = true;
                });

            }
            else
            {
                ReplayUI.Instance.Message.text =
                    Replay.CurrentLang.failDownloadReplay + " <b>" + ServerManager.message + "</b>";
                HideAndSaveNotification();
            }
        }

    }
}