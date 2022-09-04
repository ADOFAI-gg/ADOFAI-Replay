using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using UnityEngine;
using UnityModManagerNet;

namespace Replay.Functions.Core
{
    public class AdofaiTweaksAPI
    {
        private static Assembly _adofaiTweaksAssembly;
        private static PropertyInfo _keyLimitSetting;
        private static List<KeyCode> _emptyKeyCode = new List<KeyCode>();

        public static List<KeyCode> ActiveKeys
        {
            get
            {
                if (_keyLimitSetting == null)
                    return _emptyKeyCode;
                var instance = _keyLimitSetting.GetValue(null);
                var result =
                    instance.GetType().GetProperty("ActiveKeys", AccessTools.all).GetValue(instance) as List<KeyCode>;
                return result ?? _emptyKeyCode;
            }
        }

        public static bool IsEnabled
        {
            get
            {
                if (_keyLimitSetting == null)
                    return false;
                var instance = _keyLimitSetting.GetValue(null);
                return (bool)instance.GetType().GetProperty("IsEnabled", AccessTools.all).GetValue(instance);
            }
        }

        public static void Init()
        {
            var adofaiTweaks = UnityModManager.FindMod("AdofaiTweaks");
            if (adofaiTweaks != null)
            {
                _adofaiTweaksAssembly = adofaiTweaks.Assembly;
                var t = adofaiTweaks.Assembly.GetType("AdofaiTweaks.Tweaks.KeyLimiter.KeyLimiterPatches");
                _keyLimitSetting = t.GetProperty("Settings", AccessTools.all);
            }
        }
    }
}