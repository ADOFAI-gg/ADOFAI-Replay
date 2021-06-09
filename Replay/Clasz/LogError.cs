using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Replay.Clasz
{
    public class LogError
    {
        public static void Show(ErrorCodes ecode, string message)
        {
            
            Main.Logger.Log($"<b><color=#ff0000>ErrorCode : {Convert.ToInt32(ecode)}, {ecode.ToString()}\n( reason : {message} )</color></b>");
        }
    }
}
