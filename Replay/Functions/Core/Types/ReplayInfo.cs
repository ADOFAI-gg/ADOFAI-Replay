using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

namespace Replay.Functions.Core.Types
{
    [Serializable]
    public class ReplayInfo
    {
        public string PreviewImagePath;
        public string Path;
        public string SongName;
        public string AuthorName;
        public string ArtistName;
        public Difficulty Difficulty;
        public float Speed;
        public int StartTile;
        public int EndTile;
        public int AllTile;
        public long PlayTime;
        public bool IsOfficialLevel;
        public DateTime Time;
        public TileInfo[] Tiles;
        //public PressInfo[] Presses;
        public NiceUnityColor RedPlanet = new NiceUnityColor(1,0,0,1), BluePlanet = new NiceUnityColor(0,0,1,1);
        public int PathDataHash;

    }
}