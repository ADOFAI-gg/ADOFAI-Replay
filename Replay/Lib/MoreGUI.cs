using System;
using UnityEngine;

namespace Replay.Lib
{
    public class MoreGUI
    {
        public static bool isOpen = false;
        public static int n = 1;

        public static string Dropdown(Rect rect, string[] texts, string selectText, GUIStyle style, Action<string> action = null)
        {
            n = 1;
            if (GUI.Button(rect, selectText,style)) isOpen = !isOpen;
            
            if (isOpen)
            {
                foreach (string s in texts)
                {
                    if (s == selectText)
                    {
                        continue;
                    }

                    if (GUI.Button(new Rect(rect.x, rect.y - rect.height * n, rect.width, rect.height), s,style))
                    {
                        action?.Invoke(s);
                        isOpen = false;
                        return s;
                    }
                    n++;
                }
                
            }

            return selectText;

        }
    }
}