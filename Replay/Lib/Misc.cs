using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Replay.Lib
{
    public class Misc
    {
        public static string ms2time(long ms)
        {
            TimeSpan interval = TimeSpan.FromMilliseconds(ms);
            string timeInterval = interval.ToString().Split('.')[0].Trim().Substring(3);
            return timeInterval;
        }

        public static bool isVaild(int max, int min)
        {
            if ((min <= Main.version || min == -1) && (max >= Main.version || max == -1)) return true;
            return false;
        }

        public static Assembly GetAssemblyByName(string name)
        {
            return AppDomain.CurrentDomain.GetAssemblies().
                SingleOrDefault(assembly => assembly.GetName().Name == name);
        }

        public static bool IsNull(string str)
        {
            if (str == null) return true;
            if (str.Trim().Equals("")) return true;
            if (str.Trim().Length < 1) return true;
            return false;
        }
    }
}
