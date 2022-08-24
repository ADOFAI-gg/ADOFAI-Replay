using System;

namespace Replay.Functions.Core.Types
{
    [Serializable]
    public struct NiceUnityColor
    {
        public float R;
        public float G;
        public float B;
        public float A;

        public NiceUnityColor(float r, float g, float b, float a)
        {
            R = r;
            G = g;
            B = b;
            A = a;
        }
    }
}