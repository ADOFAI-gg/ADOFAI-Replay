using System;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace Replay
{
    public static class ReplayAssets
    {
        public static AssetBundle Assets;
        public static AssetBundle Scenes;

        public static Sprite PauseImage;
        public static Sprite ResumeImage;
        public static AudioClip SwipeIn;
        public static AudioClip SwipeOut;

        public static void Init()
        {
            var asm = Assembly.LoadFile(Path.Combine(Replay.unityModEntry.Path, "ReplayUI.dll"));
            Assets = AssetBundle.LoadFromFile(Path.Combine(Replay.unityModEntry.Path, "replayassets.assets"));
            Scenes = AssetBundle.LoadFromFile(Path.Combine(Replay.unityModEntry.Path, "replayscenes.assets"));

            PauseImage = Assets.LoadAsset<Sprite>("assets/textures/ingameui/pause1.png");
            ResumeImage = Assets.LoadAsset<Sprite>("assets/textures/ingameui/pause2.png");
            SwipeIn = Assets.LoadAsset<AudioClip>("assets/audios/sndscreenwipein.ogg");
            SwipeOut = Assets.LoadAsset<AudioClip>("assets/audios/sndscreenwipeout.ogg");

        }
    }
}