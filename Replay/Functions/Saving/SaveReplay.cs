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
        public const string TEST_URL = ServerManager.TEST_URL;
        public const string SERVER_URL = ServerManager.SERVER_URL;

        public static void UploadToServer(string data)
        {
            var wc = new WebClient();
            wc.Headers[HttpRequestHeader.ContentType] = "application/json";
            wc.Encoding = Encoding.UTF8;
            wc.UploadString(Replay.IsDebug ? TEST_URL : SERVER_URL,
                "{\"auth\": \"여기 서버에다가 테러 비스무리한거 하면은 IP밴 때리고 IP추적해서 빠따날립니다.\", \"rpl\":" + data + " }");
        }
        
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
