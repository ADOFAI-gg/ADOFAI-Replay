using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Replay.Clasz
{
    public class ReplayData
    {
        public string id = "";
        public string name = "SANS";
        public int start = 0;
        public string path = "";
        public int end = 0;
        public string time = ""; // TinyJSON이 long 인식 못해서 string
        public List<TileInfo> angles = new List<TileInfo>();
        public int difficulty = 1;
        public float speed = 1f;
        public string filepath = "";
    }
}
