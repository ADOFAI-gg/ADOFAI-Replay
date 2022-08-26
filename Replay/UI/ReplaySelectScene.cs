using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Discord;
using HarmonyLib;
using Replay.Functions.Core;
using Replay.Functions.Core.Types;
using Replay.Functions.Watching;
using UnityEngine;

namespace Replay.UI
{
    public class ReplaySelectScene
    {
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

        
        public static void Awake()
        {
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
            


            if (ADOBase.ownsTaroDLC)
                ReplayUIUtils.Audio.clip = scnReplayIntro.scnReplayIntro.Instance.IntroBGMDLC;
            else
                ReplayUIUtils.Audio.clip = scnReplayIntro.scnReplayIntro.Instance.IntroBGM;
            

            SetLanguage();

            if (!Directory.Exists(Replay.ReplayOption.savedPath)) return;
            var files = Directory.GetFiles(Replay.ReplayOption.savedPath);
            foreach (var f in files)
            {
                if (!f.EndsWith(".rpl")) continue;

                try
                {
                    var rpl = ReplayUtils.LoadReplay(f);
                    var rpinfo = new ReplayUIInfo
                    {
                        Song = rpl.SongName.Replace("\n", "").Replace("\r", ""),
                        Artist = rpl.ArtistName,
                        StartProgress = (int)(((float)rpl.StartTile / ((float)rpl.AllTile - 1)) * 100),
                        EndProgress = (int)(((float)rpl.EndTile / ((float)rpl.AllTile - 1)) * 100),
                        LevelLength = ReplayUtils.Ms2time((long)(rpl.PlayTime)),
                        Time = rpl.Time
                    };
                    
                    var pre = ReplayUtils.LoadTexture(rpl.PreviewImagePath);
                    if (pre != null)
                        rpinfo.Preview = pre;

                    rpinfo.OnDelete = () =>
                    {
                        scrSfx.instance.PlaySfx(SfxSound.MenuSquelch);
                        File.Delete(f);
                    };
                    rpinfo.OnPlay = () => { ReplayUIUtils.DoSwipe(() => { WatchReplay.Play(rpl); }); };
                    scnReplayIntro.scnReplayIntro.Instance.AddReplayCard(rpinfo);

                    if (pre != null)
                    {
                        var card = scnReplayIntro.scnReplayIntro.ReplaysInScroll.Last();
                        var v = pre.width / 1036f;
                        card.LevelPreview.rectTransform.sizeDelta = new Vector2(1036, pre.height / v);
                    }
                }
                catch (Exception e)
                {
                    Replay.Log(e);
                }

            }
        }
        
    }
}