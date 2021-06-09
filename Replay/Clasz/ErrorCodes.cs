using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Replay.Clasz
{
    public enum ErrorCodes
    {
        FileNotFound = 0,
        FailedParsing = 1,
        CantSaved = 2,
        OverlapGUI = 3,
        CantForcePlay = 4
    }
}
