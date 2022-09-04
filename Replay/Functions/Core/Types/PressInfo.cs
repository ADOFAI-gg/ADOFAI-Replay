using System;
using UnityEngine;

namespace Replay.Functions.Core.Types
{
    [Serializable]
    public class PressInfo
    {
        public KeyCode Key;
        public float HeldTime;
        public double PressTime;
    }
}