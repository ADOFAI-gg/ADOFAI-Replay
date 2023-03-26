using System;
using Rewired.Utils.Classes.Data;

namespace Replay.Functions.Core.Types
{
    [Serializable]
    public struct NiceUnityVector
    {
        public float x;
        public float y ;
        
        public NiceUnityVector(float _x, float _y)
        {
            x = _x;
            y = _y;
        }
    }
    
    
}