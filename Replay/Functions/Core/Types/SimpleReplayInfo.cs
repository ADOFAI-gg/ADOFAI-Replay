namespace Replay.Functions.Core.Types
{
    public class SimpleReplayInfo
    {
        public string SongName;
        public string AuthorName;
        public string ArtistName;
        public Difficulty Difficulty;
        public float Pitch;
        public int StartSeqID;
        public int EndSeqID;
        public bool IsOfficialLevel;
        public TileInfo[] Tiles;
    }
}