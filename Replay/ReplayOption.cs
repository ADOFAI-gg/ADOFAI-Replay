using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using UnityEngine;
using UnityModManagerNet;

namespace Replay
{
    public class ReplayOption : UnityModManager.ModSettings
    {
        public bool saveEverytimeDied;
        public bool saveEveryLevelComplete;
        public bool saveBySpecifiedKey = true;
        public bool saveWhen90P;
        public bool replayCount20delte;
        public bool hideEffectInDeathcam;
        public bool disableOttoSave;
        public int specifiedKeyCode = (int)KeyCode.F11;
        public int specifiedDeathCamKeyCode = (int)KeyCode.F9;
        public List<string> noUsingKeyviewers = new List<string>();
        public int CanICollectReplayFile = 0;
        public string savedPath;

        public override void Save(UnityModManager.ModEntry modEntry) {
            var filepath = GetPath(modEntry);
            try {
                using (var writer = new StreamWriter(filepath)) {
                    var serializer = new XmlSerializer(GetType());
                    serializer.Serialize(writer, this);
                }
            } catch {
            }
        }
       
        public override string GetPath(UnityModManager.ModEntry modEntry) {
            return Path.Combine(modEntry.Path, GetType().Name + ".xml");
        }
  
    }
}