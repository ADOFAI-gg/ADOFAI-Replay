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
        public KeyCode Key;
        public float HeldTime;
        public HitMargin RealHitMargin;
        public HitMargin Hitmargin;

        //collect to server only
        public bool IsFreeroam;
        public int RelativeFloorAngle;
        public double TargetAngle;
        public double RealHitAngle;
        public double HitTime;
    }
}