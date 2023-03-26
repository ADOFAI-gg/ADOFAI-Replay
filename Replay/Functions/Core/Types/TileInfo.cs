using System;
using System.Collections.Generic;
using UnityEngine;

namespace Replay.Functions.Core.Types
{
    [Serializable]
    public class TileInfo
    {
        public double HitAngleRatio;
        public int SeqID;
        public bool NoFailHit;
        public bool AutoHit;
        //public HitMargin RealHitMargin;
        public HitMargin Hitmargin;
        public NiceUnityVector HitTimingPosition;
        public KeyCode Key;
        public float HeldTime;

        //collect to server only
        public bool IsFreeroam;
        public int RelativeFloorAngle;
        /*
        public double TargetAngle;
        public double RealHitAngle;
        public double HitTime;
*/
        public double Speed;
    }
}