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

        //collect to server only
        public HitMargin Hitmargin;
        public bool IsFreeroam;
        public int RelativeFloorAngle;
        public double TargetAngle;
        public double RealHitAngle;
        public double HitTime;
    }
}