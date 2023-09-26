using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Text;
using DG.Tweening;
using Replay.Functions.Core.Types;
using Replay.UI;
using ReplayLoader;
using UnityEngine;
using Object = UnityEngine.Object;
using ZipFile = System.IO.Compression.ZipFile;

namespace Replay.Functions.Core
{
    public class ServerManager
    {
        public const string TEST_URL = "http://192.168.55.142:7777/";
        public const string SERVER_URL = "http://58.232.145.134:7777/";
        public static UploadState State;
        public static string message;
        public static ReplayInfo Rpl;

        public class FastWebClient : WebClient
        {
            [Obsolete("Obsolete")]
            public FastWebClient()
            {

                ServicePointManager.Expect100Continue = false;
                ServicePointManager.DefaultConnectionLimit = int.MaxValue;
                WebRequest.DefaultWebProxy = null;
                Encoding = Encoding.UTF8;
                Proxy = null;
                
            }
            
            protected override WebRequest GetWebRequest(Uri address)
            {
                var request = base.GetWebRequest(address) as HttpWebRequest;
                if (request == null) return null;
                request.Timeout = 300000;
                request.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
                return request;

            }
        }

        public static void UploadToServer(string data)
        {
            var wc = new WebClient();
            wc.Headers[HttpRequestHeader.ContentType] = "application/json";
            wc.Encoding = Encoding.UTF8;
            wc.Headers["replay"] = "SANSPPAP";
            wc.UploadString((Replay.IsDebug ? TEST_URL : SERVER_URL) + "collect",
                "{\"auth\": \"wasansasinuguna\", \"rpl\":" + data + " }");
        }
        
        
        [Obsolete("Obsolete")]
        public static void DownloadReplay(string code)
        {
            GC.Collect();
            Rpl = null;
            State = UploadState.None;
            try
            {
                if (!Directory.Exists(Path.Combine(Application.dataPath, "DownlodedLevels")))
                    Directory.CreateDirectory(Path.Combine(Application.dataPath, "DownlodedLevels"));

                ReplayUI.Instance.Message.text = "[1] " + Replay.CurrentLang.preparing;
                
                WebRequest.DefaultWebProxy = null;
                ServicePointManager.DefaultConnectionLimit = int.MaxValue;
                var wc = new FastWebClient();
                var downloadPath = Path.Combine(Replay.ReplayOption.savedPath, code + ".rpl");
                if (File.Exists(downloadPath))
                {
                    State = UploadState.Success;
                    return;
                }
                
                wc.DownloadProgressChanged += (s, e) =>
                {
                    ReplayUI.Instance.Message.text = "[1] "+Replay.CurrentLang.downloadingText+" ( " + e.ProgressPercentage + "% )";
                }; 
                wc.DownloadFileAsync(new Uri((Replay.IsDebug ? TEST_URL : SERVER_URL) + "download/" + code + ".rpl"),
                    downloadPath);

                wc.DownloadFileCompleted += (s1, e1) =>
                {
                    
                    if (e1.Cancelled || e1.Error != null)
                    {
                        message = Replay.CurrentLang.invalidReplayCode;
                        State = UploadState.Fail;
                        return;
                    }
                    ReplayUI.Instance.Message.text = "[2] " + Replay.CurrentLang.preparing;
                    Rpl = ReplayUtils.LoadReplay(downloadPath);
                    
                    if (!Directory.Exists(
                            Path.Combine(Application.dataPath, "DownlodedLevels", Rpl.PathDataHash.ToString())))
                    {
                        Directory.CreateDirectory(Path.Combine(Application.dataPath, "DownlodedLevels",
                            Rpl.PathDataHash.ToString()));

                        wc = new FastWebClient();

                        wc.DownloadProgressChanged += (s, e) =>
                        {
                            ReplayUI.Instance.Message.text = "[2] "+Replay.CurrentLang.preparing+ " ( " + e.ProgressPercentage + "% )";
                        }; 
                        wc.DownloadFileAsync(new Uri((Replay.IsDebug ? TEST_URL : SERVER_URL) + "download/" + Rpl.PathDataHash + ".zip"),
                            Path.Combine(Application.dataPath, "DownlodedLevels", Rpl.PathDataHash + ".zip"));
              
                        wc.DownloadFileCompleted += (sender, e) =>
                        {
                            GC.Collect();
                            if (e.Cancelled || e.Error != null)
                            {
                                message = Replay.CurrentLang.failDownloadReplayShort;
                                State = UploadState.Fail;
                                return;
                            }
                            ReplayUI.Instance.Message.text = Replay.CurrentLang.preparing;
                            
                            
                            var ibm437 = Encoding.GetEncoding("IBM437");
                            var euckr = Encoding.GetEncoding("euc-kr");
                            
                            

                            ZipUtils.Unzip(Path.Combine(Application.dataPath, "DownlodedLevels", Rpl.PathDataHash + ".zip"),
                                Path.Combine(Application.dataPath, "DownlodedLevels", Rpl.PathDataHash.ToString()));
                            File.Delete(Path.Combine(Application.dataPath, "DownlodedLevels", Rpl.PathDataHash + ".zip"));
                        
                            var name = new FileInfo(Rpl.Path).Name;
                            Rpl.Path = Path.Combine(Application.dataPath, "DownlodedLevels", Rpl.PathDataHash.ToString(),name);
                            Rpl.Shared = true;
                            Rpl.MyReplay = false;
                            Rpl.ReplayCode = code;
                            if(File.Exists(Path.Combine(Application.dataPath, "DownlodedLevels", Rpl.PathDataHash.ToString(),"ReplayScreenShot")))
                                Rpl.PreviewImagePath = Path.Combine(Application.dataPath, "DownlodedLevels", Rpl.PathDataHash.ToString(),"ReplayScreenShot");
                            ReplayUtils.SaveReplayWithPath(downloadPath,Rpl);
                        
                            State = UploadState.Success;
                        
                        };
                    }
                    else
                    {
                        ReplayUI.Instance.Message.text = Replay.CurrentLang.preparing;
                        var name = new FileInfo(Rpl.Path).Name;
                        Rpl.Path = Path.Combine(Application.dataPath, "DownlodedLevels", Rpl.PathDataHash.ToString(),name);
                        Rpl.Shared = true;
                        Rpl.MyReplay = false;
                        Rpl.ReplayCode = code;
                        if(File.Exists(Path.Combine(Application.dataPath, "DownlodedLevels", Rpl.PathDataHash.ToString(),"ReplayScreenShot")))
                            Rpl.PreviewImagePath = Path.Combine(Application.dataPath, "DownlodedLevels", Rpl.PathDataHash.ToString(),"ReplayScreenShot");
                        ReplayUtils.SaveReplayWithPath(downloadPath,Rpl);

                        State = UploadState.Success;
                    }
                    
                };
                
            }
            catch (Exception e)
            {
                message = e.Message;
                State = UploadState.Fail;
            }

        }


        public static void DeleteReplay(ReplayInfo rpl)
        {
            State = UploadState.None;
            message = null;
            try
            {
                if (!rpl.MyReplay)
                    throw new Exception(Replay.CurrentLang.removeOnlyMyReplay);
                var wc = new WebClient();
                wc.Encoding = Encoding.UTF8;
                wc.Headers["replay"] = "SANSPPAP";
                wc.Headers["replay-id"] = rpl.ReplayCode;
                wc.Headers["level-id"] = rpl.PathDataHash.ToString();
                wc.Headers["client-id"] = Replay.ClientID;
                var data = wc.UploadString((Replay.IsDebug ? TEST_URL : SERVER_URL) + "delete","");
                if (data.StartsWith("Error:"))
                {
                    State = UploadState.Fail;
                    Replay.Log(data);
                    return;
                }

                State = UploadState.Success;
            }
            catch (Exception e)
            {
                State = UploadState.Fail;
                Replay.Log(e);
                //message = e.Message;
            }
        }
        

        [Obsolete("Obsolete")]
        public static void ShareReplay(string rplpath, ReplayInfo rpl)
        {
            GC.Collect();
            State = UploadState.None;
            message = null;
            var p = Directory.GetParent(rpl.Path).FullName;
            var p2 = Directory.GetParent(rplpath).FullName;
            try
            {
                ReplayUI.Instance.Message.text = Replay.CurrentLang.preparing;
                var levelId = rpl.PathDataHash.ToString();

                message = ReplayUtils.RandomString(8);
                
                ZipUtils.Zip(Path.Combine(Path.GetTempPath(),levelId+".zip"), Directory.GetFiles(p));
                

                var wc = new FastWebClient();
                wc.Encoding = Encoding.UTF8;
                wc.Headers["replay"] = "SANSPPAP";
                wc.Headers["level-id"] = levelId;
                wc.Headers["client-id"] = Replay.ClientID;
                File.Move(rplpath,Path.Combine(p2,message+".rpl"));
                

                var datasize = File.ReadAllBytes(Path.Combine(Path.GetTempPath(),levelId+".zip")).Length;
                wc.UploadProgressChanged += (s, e) =>
                {
                    var i = ((float)((float)e.BytesSent / (float)datasize));
                    if (i > 1)
                    {
                        ReplayUI.Instance.Message.text =
                            "[1] " + Replay.CurrentLang.preparing;
                        
                    }
                    else
                    {
                        ReplayUI.Instance.Message.text =
                            "[1] " + Replay.CurrentLang.uploadingText + " ( " + (i * 100) + "% ) ";
                    }
                };
                wc.UploadFileAsync( new Uri((Replay.IsDebug ? TEST_URL : SERVER_URL) + "upload"), Path.Combine(Path.GetTempPath(),levelId+".zip"));
                
                wc.UploadFileCompleted += (s1, e1) =>
                {
                     GC.Collect();
                     ReplayUI.Instance.Message.text = Replay.CurrentLang.preparing;
                    
                    if (e1.Error != null)
                    {
                        if(!string.IsNullOrEmpty(message))
                            File.Delete(Path.Combine(Path.GetTempPath(),message+".zip"));
                
                        if(File.Exists(Path.Combine(p2,message+".rpl")))
                            File.Move(Path.Combine(p2,message+".rpl"),rplpath);
                        Replay.Log(e1.Error);
                        message = e1.Error.Message;
                        State = UploadState.Fail;
                        return;
                    }

                    if (e1.Result != null && e1.Result.Length != 0)
                    {
                        var data = Encoding.UTF8.GetString(e1.Result);
                        if (data.StartsWith("Error:"))
                        {
                            if(!string.IsNullOrEmpty(message))
                                File.Delete(Path.Combine(Path.GetTempPath(),message+".zip"));
                
                            if(File.Exists(Path.Combine(p2,message+".rpl")))
                                File.Move(Path.Combine(p2,message+".rpl"),rplpath);
                            Replay.Log(data);
                            message = data;
                            State = UploadState.Fail;
                            return;
                        }
                    }
                    

                    wc = new FastWebClient();
                    wc.Encoding = Encoding.UTF8;
                    wc.Headers["replay"] = "SANSPPAP";
                    wc.Headers["level-id"] = levelId;
                    wc.Headers["client-id"] = Replay.ClientID;
                    var datasize2 = File.ReadAllBytes(Path.Combine(p2,message+".rpl")).Length;
                    
                    wc.UploadProgressChanged += (s, e) =>
                    {
                        var i = ((float)((float)e.BytesSent / (float)datasize2));
                        if (i > 1)
                        {
                            ReplayUI.Instance.Message.text =
                                "[2] " + Replay.CurrentLang.preparing;
                        }
                        else
                        {
                            ReplayUI.Instance.Message.text =
                                "[2]" + Replay.CurrentLang.uploadingText + " ( " + (i * 100) + "% )";
                        }
                    };
                    wc.UploadFileAsync(new Uri((Replay.IsDebug ? TEST_URL : SERVER_URL) + "upload"), Path.Combine(p2,message+".rpl"));
                    wc.UploadFileCompleted += (s, e) =>
                    {
                        GC.Collect();
                        ReplayUI.Instance.Message.text = Replay.CurrentLang.preparing;

                        if (e.Error != null)
                        {
                            if (!string.IsNullOrEmpty(message))
                                File.Delete(Path.Combine(Path.GetTempPath(), message + ".zip"));

                            if (File.Exists(Path.Combine(p2, message + ".rpl")))
                                File.Move(Path.Combine(p2, message + ".rpl"), rplpath);
                            Replay.Log(e.Error);
                            message = e.Error.Message;
                            State = UploadState.Fail;
                            return;
                        }
                        
                        if (e.Result != null && e.Result.Length != 0)
                        {
                            var data = Encoding.UTF8.GetString(e.Result);
                            if (data.StartsWith("Error:"))
                            {
                                if(!string.IsNullOrEmpty(message))
                                    File.Delete(Path.Combine(Path.GetTempPath(),message+".zip"));
                
                                if(File.Exists(Path.Combine(p2,message+".rpl")))
                                    File.Move(Path.Combine(p2,message+".rpl"),rplpath);
                                Replay.Log(data);
                                message = data;
                                State = UploadState.Fail;
                                return;
                            }
                        }

                        File.Move(Path.Combine(p2, message + ".rpl"), rplpath);

                        var rpinfo = ReplaySelectScene.ReplayToInfo[rpl];
                        rpl.Shared = true;
                        rpl.MyReplay = true;
                        rpl.ReplayCode = message;
                        

                        ReplayUtils.SaveReplayWithPath(rplpath, rpl);

                        ReplaySelectScene.ReplayToInfo[rpl].replayViewCard.Song.text +=
                            $" <size=15><color=#ffffff99>{rpl.ReplayCode}</color></size>";
                        File.Delete(Path.Combine(Path.GetTempPath(), levelId + ".zip"));
                        
                        rpinfo.replayViewCard.Upload.gameObject.SetActive(false);
                        rpinfo.replayViewCard.Remove.onClick.RemoveAllListeners();
                        rpinfo.replayViewCard.Remove.onClick.AddListener(() =>
                        {
                            scrSfx.instance.PlaySfx(SfxSound.MenuSquelch);
                            scnReplayIntro.scnReplayIntro.CanEscape = false;
                            GlobalLanguage.OK = Replay.CurrentLang.okText;
                            GlobalLanguage.No = Replay.CurrentLang.noText;
                            ReplayUI.Instance.ShowNotification(Replay.CurrentLang.reallyDeleteSharedReplay, Replay.CurrentLang.reallyDeleteSharedReplayMoreMessage,
                                () =>
                                {
                                    rpinfo.replayViewCard.action = false;
                                    scrSfx.instance.PlaySfx(SfxSound.MenuSquelch);
                                    scnReplayIntro.scnReplayIntro.CanEscape = true;
                                    rpinfo.replayViewCard.transform.DOMoveX(6, 1).SetEase(Ease.OutExpo).OnComplete(() =>
                                    {
                                        ServerManager.DeleteReplay(rpl);
                                        scnReplayIntro.scnReplayIntro.ReplaysInScroll.Remove(rpinfo.replayViewCard);
                                        ReplaySelectScene.ShareCount--;
                                        ReplaySelectScene.UpdateShareText();
                                        File.Delete(rplpath);
                                        Object.DestroyImmediate(rpinfo.replayViewCard.gameObject);
                                    });
                                    return true;
                                }, () =>
                                {
                                    rpinfo.replayViewCard.action = false;
                                    scnReplayIntro.scnReplayIntro.CanEscape = true;
                                    scrSfx.instance.PlaySfx(SfxSound.MenuSquelch);
                                    return true;
                                }, RDString.language);
                            Loader.UpdateLayoutNextFrame();
                            
                        });
                        State = UploadState.Success;
                    };

                };
            }
            catch(Exception e)
            {
                if(!string.IsNullOrEmpty(message))
                    File.Delete(Path.Combine(Path.GetTempPath(),message+".zip"));
                
                if(File.Exists(Path.Combine(p2,message+".rpl")))
                    File.Move(Path.Combine(p2,message+".rpl"),rplpath);
                Replay.Log(e);
                message = e.Message;
                State = UploadState.Fail;
            }
        }
    }
}