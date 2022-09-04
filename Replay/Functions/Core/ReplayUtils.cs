using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.RegularExpressions;
using ADOFAI;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using GDMiniJSON;
using HarmonyLib;
using Newgrounds;
using Replay.Functions.Core.Types;
using Replay.Functions.Watching;
using TinyJson;
using UnityEngine;
using UnityModManagerNet;

namespace Replay.Functions.Core
{
    public static class ReplayUtils
    {
        [DllImport("user32.dll")]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        public static extern bool PostMessage(IntPtr hWnd, UInt32 Msg, int wParam, int lParam);

        [DllImport("user32.dll")]
        public static extern void keybd_event(uint vk, uint scan, uint flags, uint extraInfo);

        [DllImport("user32.dll")]
        public static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        public static extern int GetKeyboardState(byte[] pbKeyState);

        public static TileInfo GetSafeList(TileInfo[] lists, int index)
        {
            if (lists.Length - 1 < index) return lists[lists.Length - 1];
            if (index < 0) return lists[0];
            return lists[index];
        }
        
        public static scrFloor GetSafeList(List<scrFloor> lists, int index)
        {
            if (lists.Count - 1 < index) return lists[lists.Count - 1];
            if (index < 0) return lists[0];
            return lists[index];
        }
        
        public static HitMargin GetHitMargin(float hitangle, float refangle, bool isCW, float bpmTimesSpeed, float conductorPitch, double marginScale = 1.0)
        {
            float num = (hitangle - refangle) * (float)(isCW ? 1 : -1);
            HitMargin result = HitMargin.TooEarly;
            float num2 = num;
            num2 = 57.29578f * num2;
            double adjustedAngleBoundaryInDeg = scrMisc.GetAdjustedAngleBoundaryInDeg(HitMarginGeneral.Counted, (double)bpmTimesSpeed, (double)conductorPitch, marginScale);
            double adjustedAngleBoundaryInDeg2 = scrMisc.GetAdjustedAngleBoundaryInDeg(HitMarginGeneral.Perfect, (double)bpmTimesSpeed, (double)conductorPitch, marginScale);
            double adjustedAngleBoundaryInDeg3 = scrMisc.GetAdjustedAngleBoundaryInDeg(HitMarginGeneral.Pure, (double)bpmTimesSpeed, (double)conductorPitch, marginScale);
            if ((double)num2 > -adjustedAngleBoundaryInDeg)
            {
                result = HitMargin.VeryEarly;
            }
            if ((double)num2 > -adjustedAngleBoundaryInDeg2)
            {
                result = HitMargin.EarlyPerfect;
            }
            if ((double)num2 > -adjustedAngleBoundaryInDeg3)
            {
                result = HitMargin.Perfect;
            }
            if ((double)num2 > adjustedAngleBoundaryInDeg3)
            {
                result = HitMargin.LatePerfect;
            }
            if ((double)num2 > adjustedAngleBoundaryInDeg2)
            {
                result = HitMargin.VeryLate;
            }
            if ((double)num2 > adjustedAngleBoundaryInDeg)
            {
                result = HitMargin.TooLate;
            }
            return result;
        }


        public static List<UnityModManager.ModEntry> GetKeyviewers()
        {
            var keyviewers = new List<UnityModManager.ModEntry>();
            foreach (var m in UnityModManager.modEntries)
            {
                var asm = m.Assembly;
                if (asm == null) continue;
                foreach (var t in asm.GetTypes())
                {
                    if (t.Name != "ReplayInput") continue;
                    keyviewers.Add(m);
                    Replay.KeyViewerOnOff[m.Info.DisplayName] = new KeyviewerInput(t);
                    break;
                }

            }

            return keyviewers;
        }

        public static Color CustomColor2UnityColor(NiceUnityColor c)
        {
            return new Color(c.R, c.G, c.B, c.A);
        }

        public static NiceUnityColor UnityColor2CustomColor(Color c)
        {
            return new NiceUnityColor(c.r, c.g, c.b, c.a);
        }

        public static string ObjectToJSON(object rpl, bool isArray = false)
        {
            var str = isArray ? "" : "{";
            var fields = rpl.GetType().GetFields(AccessTools.all);
            var n = 1;

            foreach (var f in fields)
            {
                var isLast = n == fields.Length;
                var v = f.GetValue(rpl);
                if (v == null) continue;
                if (v is bool b)
                {
                    if(b)
                        str += $"\"{f.Name}\": true";
                    else
                        str += $"\"{f.Name}\": false";
                    
                    if (!isLast)
                        str += ", ";
                }
                else if (v.GetType().IsPrimitive)
                {
                    str += $"\"{f.Name}\": {v}";
                    if (!isLast)
                        str += ", ";
                }
                else if (v is string || v.GetType().IsEnum)
                {
                    str += $"\"{f.Name}\": \"{v.ToString().Replace("\\","/")}\"";
                    if (!isLast)
                        str += ", ";
                }
                else if (v is DateTime time)
                {
                    str += $"\"{f.Name}\": {time.Ticks}";
                    if (!isLast)
                        str += ", ";
                }
                else
                {
                    if (IsList(v))
                    {
                        str += $"\"{f.Name}\": [";
                        var count2 = 1;
                        foreach (var l in (IList)v)
                        {
                            if (l.GetType().IsPrimitive)
                            {
                                str += $"{l}";
                            }
                            else
                            {
                                str += $"{ObjectToJSON(l, false).Trim()}"; 
                            }
                            if (count2 != ((IList)v).Count)
                                str += ", ";
                            count2++;
                        }
                        
                        str += "]";
                        if (!isLast)
                            str += ", ";
                    }
                    else
                    {
                        str += $"\"{f.Name}\": {ObjectToJSON(v, false)}";
                        if (!isLast)
                            str += ", ";
                    }
                }

                n++;
            }

            if (!isArray)
                str += "}";
            return str;
        }

        public static bool IsList(object value)
        {
            var type = value.GetType();
            var targetType = typeof(IList<>);
            return type.GetInterfaces().Any(i => i.IsGenericType
                                                 && i.GetGenericTypeDefinition() == targetType);
        }
        

        public static ReplayInfo LoadReplay(string replayPath)
        {
            if (!File.Exists(replayPath)) throw new Exception(Replay.CurrentLang.cantFindPath);
            try
            {
                var stream = new FileStream(replayPath, FileMode.Open);
                var deserializer = new BinaryFormatter();
                var result = (ReplayInfo)deserializer.Deserialize(stream);
                stream.Close();
                return result;
            }
            catch
            {
                throw new Exception(Replay.CurrentLang.cantLoad);
            }
        }

        public static string Ms2time(long ms)
        {
            var interval = TimeSpan.FromMilliseconds(ms);
            var timeInterval = interval.ToString().Split('.')[0].Trim().Substring(3);
            return timeInterval;
        }

        public static Texture2D LoadTexture(string path)
        {
            try
            {
                var tex = new Texture2D(2, 2);
                var fileData =
                    File.ReadAllBytes(path);
                tex.LoadImage(fileData);
                return tex;
            }
            catch
            {
                return null;
            }
        }
        
        public static Texture2D DuplicateTexture(Texture2D source)
        {
            var renderTex = RenderTexture.GetTemporary(
                source.width,
                source.height,
                0,
                RenderTextureFormat.Default,
                RenderTextureReadWrite.Linear);

            Graphics.Blit(source, renderTex);
            var previous = RenderTexture.active;
            RenderTexture.active = renderTex;
            var readableText = new Texture2D(source.width, source.height);
            readableText.ReadPixels(new Rect(0, 0, renderTex.width, renderTex.height), 0, 0);
            readableText.Apply();
            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(renderTex);
            return readableText;
        }

        
        
        public static TweenerCore<Vector2, Vector2, VectorOptions> DOAnchorPos(RectTransform target, Vector2 endValue, float duration, bool snapping = false)
        {
            TweenerCore<Vector2, Vector2, VectorOptions> tweenerCore = DOTween.To(() => target.anchoredPosition, delegate(Vector2 x)
            {
                target.anchoredPosition = x;
            }, endValue, duration);
            tweenerCore.SetOptions(snapping).SetTarget(target);
            return tweenerCore;
        }
        
        public static string RemoveHTML(string html)
        {
            return Regex.Replace(html, "<.*?>", String.Empty).Trim();
        }

        
        public static string RemoveInvalidChars(string filename)
        {
            return string.Concat(filename.Split(Path.GetInvalidFileNameChars()));
        }

        public static void RegisterRPL()
        {
            var appidPath = $"{Path.GetFullPath(".")}\\steam_appid.txt";
            if (!File.Exists(appidPath)) {
                using(StreamWriter sw = File.CreateText(appidPath)) sw.WriteLine("977950");
            }

            var runnerPath = $"{Path.GetFullPath(".")}\\runner.bat";
            if (!File.Exists(runnerPath)) {
                using (StreamWriter sw = File.CreateText(runnerPath)) {
                    sw.WriteLine($"cd {Path.GetFullPath(".")}");
                    sw.WriteLine($"start /d \"{Path.GetFullPath(".")}\" /b \"\" \"{Path.GetFullPath(".")}\\{Application.productName}.exe\" %1");
                }
            }

            var result = FileAssociations.SetAssociation(".rpl", "replay", "ADOFAI Replay File",
                $"{Path.GetFullPath(".")}\\runner.bat", $"{Path.GetFullPath(".")}\\{Application.productName}.exe");
        }


        public static void SaveReplay(string replayName, ReplayInfo replayInfo)
        {
            try
            {
                
                var stream = new FileStream(Path.Combine(Replay.ReplayOption.savedPath, RemoveInvalidChars(replayName)), FileMode.Create);
                var deserializer = new BinaryFormatter();
                deserializer.Serialize(stream, replayInfo); 
                stream.Close();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                throw new Exception(Replay.CurrentLang.cantSave);

            }
        }

    }
}