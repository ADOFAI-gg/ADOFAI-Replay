using System;
using System.Collections.Generic;
using Replay.Functions.Menu;
using ReplayLoader;
using UnityEngine;

namespace Replay.UI
{
    public class BallBorder : MonoBehaviour
    {
        public SpriteRenderer sr;
        public static Queue<BallBorder> CreatedBallBorders = new Queue<BallBorder>();
        public static List<BallBorder> CreatedBallBorders2 = new List<BallBorder>();
        private static GameObject ball_border_pref;

        public static Color Perfect = new Color32(134, 227,112, 255);
        public static Color FLPerfect = new Color32(227, 227,112, 255);
        public static Color FastLate = new Color32(235, 154,70, 255);
        public static Color VVFastLate = new Color32(237, 62,62, 255);
        private static int c;
        
        //public static Color Perfect = ColorUtility.ToHtmlStringRGB("#86E370");
        
        
        public static BallBorder Create(Vector3 pos, HitMargin hit, float alpha = 1f)
        {
            if(ball_border_pref == null)
                ball_border_pref = ReplayAssets.Assets.LoadAsset<GameObject>("assets/prefs/ball.prefab");
            
            
            
            if (CreatedBallBorders.Count > 0)
            {
                var b = CreatedBallBorders.Dequeue();
                b.transform.position = pos;
                b.gameObject.SetActive(true);
                b.sr.color = hit switch
                {
                    HitMargin.Perfect => Perfect,
                    HitMargin.EarlyPerfect => FLPerfect,
                    HitMargin.LatePerfect => FLPerfect,
                    HitMargin.VeryEarly => FastLate,
                    HitMargin.VeryLate => FastLate,
                    _ => VVFastLate
                };
                return b;
            }
            else
            {
                
                var go = Instantiate(ball_border_pref, pos, Quaternion.Euler(0, 0, 0));
                go.transform.localScale = Vector3.one;
                go.name = "ball " + c;
                go.transform.position = pos;
                var b = go.AddComponent<BallBorder>();
                b.sr  = go.GetComponent<SpriteRenderer>();
                b.sr.sortingOrder = 99999999;
                b.sr.color = hit switch
                {
                    HitMargin.Perfect => Perfect,
                    HitMargin.EarlyPerfect => FLPerfect,
                    HitMargin.LatePerfect => FLPerfect,
                    HitMargin.VeryEarly => FastLate,
                    HitMargin.VeryLate => FastLate,
                    _ => VVFastLate
                };
                CreatedBallBorders2.Add(b);
                c++;
                return b;


            }
            
        }

        private void OnDisable()
        {
            CreatedBallBorders.Enqueue(this);
        }
        
    }
}