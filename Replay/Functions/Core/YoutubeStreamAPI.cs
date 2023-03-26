using System.Reflection;
using HarmonyLib;
using UnityModManagerNet;

namespace Replay.Functions.Core
{
    public class YoutubeStreamAPI
    {
        private static Assembly _youtubeStreamAssembly;
        private static FieldInfo _checkSongURL;
        public static bool Enabled;

        public static string newSongKey
        {
            get
            {
                if(Enabled)
                    return (string)_checkSongURL.GetValue(null);
                return "";
            }
            set
            {
                _checkSongURL?.SetValue(null, value);   
            }
        }
        
        public static void Init()
        {
            var youtubeStream = UnityModManager.FindMod("YouTubeStream");
            Enabled = youtubeStream != null;
            if (youtubeStream != null)
            {
                _youtubeStreamAssembly = youtubeStream.Assembly;
                if (_youtubeStreamAssembly == null) return;
                var t = youtubeStream.Assembly.GetType("YouTubeStream.MainPatch.CheckSongURL");
                _checkSongURL = t.GetField("newSongKey", AccessTools.all);
            }
        }
    }
}