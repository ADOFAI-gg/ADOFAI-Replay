using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Replay.Clasz
{
    public class TileInfo
    {
        public int num;
        public double angle;
        public string time;

        public TileInfo(int _num, double _angle, string _time)
        {
            this.num = _num;
            this.angle = _angle;
            this.time = _time;
        }
    }
}
