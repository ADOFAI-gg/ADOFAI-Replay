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
        public static AudioClip Click;

        public static void Init()
        {
            var asm = Assembly.Load(File.ReadAllBytes(Path.Combine(Replay.unityModEntry.Path, "ReplayUI.dll")));
            Assets = AssetBundle.LoadFromFile(Path.Combine(Replay.unityModEntry.Path, "replayassets.assets"));
            Scenes = AssetBundle.LoadFromFile(Path.Combine(Replay.unityModEntry.Path, "replayscenes.assets"));

            PauseImage = Assets.LoadAsset<Sprite>("assets/textures/ingameui/pause1.png");
            ResumeImage = Assets.LoadAsset<Sprite>("assets/textures/ingameui/pause2.png");
            Click = Assets.LoadAsset<AudioClip>("assets/audios/sndMenuSquelch.ogg");

        }
    }
}