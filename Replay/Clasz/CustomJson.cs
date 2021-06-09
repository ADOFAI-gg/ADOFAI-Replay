using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TinyJson;

namespace Replay.Clasz
{
    public class CustomJson
    {
        public static string stringify(ReplayData obj)
        {
            try
            {
                string result = obj.ToJson();
                List<string> strs = new List<string>();
                foreach (TileInfo tile in obj.angles)
                {
                    strs.Add(tile.ToJson());
                }
                return result.Replace("\"angles\":[]", $":[{string.Join(",", strs)}]");
                
            } catch (Exception e)
            {
                LogError.Show(ErrorCodes.CantSaved, e.Message);
                return new ReplayData().ToJson();
            }
           
        }


        public static ReplayData parse(string json)
        {
            try {
                ReplayData result = JSONParser.FromJson<ReplayData>(json);
                return result;
            } catch (Exception e) 
            {
                LogError.Show(ErrorCodes.FailedParsing, e.Message);
                return new ReplayData();
            }
       }
              

    }
}
