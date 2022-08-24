using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Replay.Functions.Menu;
using Replay.Functions.Watching;
using UnityEngine;
using UnityModManagerNet;
using Debug = UnityEngine.Debug;

namespace Replay.Functions.Core
{
    public class KeyboradHook
    {
        public static void OnKeyPressed(KeyCode keyCode)
        {
            foreach (var k in Replay.KeyViewerOnOff.Values)
            {
                if(k.Enabled)
                    k.OnKeyPressed.Invoke(null, new object[]{keyCode});
            }
        }
        
        public static void OnKeyReleased(KeyCode keyCode)
        {
            foreach (var k in Replay.KeyViewerOnOff.Values)
            {
                if(k.Enabled)
                    k.OnKeyReleased.Invoke(null, new object[]{keyCode});
            }
        }
        

        public static void OnStartInputs()
        {
            foreach (var k in Replay.KeyViewerOnOff.Values)
            {
                if(k.Enabled)
                    k.OnStartInputs.Invoke(null, null);
            }
        }
        
        public static void OnEndInputs()
        {
            foreach (var k in Replay.KeyViewerOnOff.Values)
            {
                if(k.Enabled)
                    k.OnEndInputs.Invoke(null, null);
            }
        }
        

    }
}