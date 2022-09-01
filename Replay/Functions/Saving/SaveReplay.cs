using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Text;
using Replay.Functions.Core;
using UnityEngine;

namespace Replay.Functions.Saving
{
    public class SaveReplay
    {
        public static IEnumerator CaptureScreen(string path)
        {
            yield return null;
            var p = scrController.instance.chosenplanet.transform.parent.gameObject;
            var gui = GameObject.Find("GUI");
            p.SetActive(false);
            gui.SetActive(false);
            
            yield return new WaitForEndOfFrame();
 
            var screenTex = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
            var area = new Rect(0f, 0f, Screen.width, Screen.height);
            screenTex.ReadPixels(area, 0, 0);
            File.WriteAllBytes(path, screenTex.EncodeToPNG());
            
            p.SetActive(true);
            gui.SetActive(true);
        }
        
        
        
    }
}
