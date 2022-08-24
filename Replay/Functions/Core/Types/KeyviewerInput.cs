using System;
using System.Reflection;
using HarmonyLib;
using UnityModManagerNet;

namespace Replay.Functions.Core.Types
{
    public class KeyviewerInput
    {
        public MethodInfo OnKeyPressed;
        public MethodInfo OnKeyReleased;
        public MethodInfo OnStartInputs;
        public MethodInfo OnEndInputs;
        public bool Enabled;

        public KeyviewerInput(Type replayInput)
        {
            OnKeyPressed = replayInput.GetMethod("OnKeyPressed", AccessTools.all);
            OnKeyReleased = replayInput.GetMethod("OnKeyReleased", AccessTools.all);
            OnStartInputs = replayInput.GetMethod("OnStartInputs", AccessTools.all);
            OnEndInputs = replayInput.GetMethod("OnEndInputs", AccessTools.all);
            Enabled = true;
        }
    }
}