using System;
using UnityEditor;
using UnityEngine;

namespace Vitqra
{
    // Manual editor smoke tests; no external test framework or XR device required.
    public static class VitqraResizeChecks
    {
        [MenuItem("VITQRA/Run Window Resize Checks")]
        public static void Run()
        {
            GameObject root = new GameObject("Resize checks");
            try
            {
                var regular = new GameObject("Regular panel").AddComponent<VitqraSpatialPanel>();
                regular.transform.SetParent(root.transform,false);
                regular.transform.position = new Vector3(-1,0,1.65f);
                var chat = new GameObject("Conversation panel").AddComponent<VitqraConversationPanel>();
                chat.transform.SetParent(root.transform,false);
                chat.transform.position = new Vector3(1,0,1.65f);
                regular.InitializeResizeControls("Size");
                chat.InitializeResizeControls("Size");
                regular.TryResizeAction(VitqraSpatialPanel.GrowAction);
                Check(Mathf.Approximately(regular.SizeScale,1.1f),"Grow");
                Check(Mathf.Approximately(chat.SizeScale,1f),"Independent scale");
                for(int i=0;i<20;i++)regular.TryResizeAction(VitqraSpatialPanel.GrowAction);
                Check(Mathf.Approximately(regular.SizeScale,1.5f),"Upper bound");
                for(int i=0;i<30;i++)regular.TryResizeAction(VitqraSpatialPanel.ShrinkAction);
                Check(Mathf.Approximately(regular.SizeScale,.7f),"Lower bound");
                foreach(var panel in new VitqraSpatialPanel[]{regular,chat})
                {
                    foreach(float scale in new[]{.7f,1f,1.5f})
                    {
                        panel.SetSizeScale(scale);
                        panel.transform.rotation=Quaternion.Euler(0,25,0);
                        float y=panel.BaseHeight*.5f+.034f;
                        CheckHit(panel,-.12f,y,VitqraSpatialPanel.ShrinkAction);
                        CheckHit(panel,0,y,VitqraSpatialPanel.ResetSizeAction);
                        CheckHit(panel,.12f,y,VitqraSpatialPanel.GrowAction);
                        Check(!panel.TryIntersectResize(RayAt(panel,.4f,y),out _,out _),"Outside toolbar");
                        Check(panel.Intersect(RayAt(panel,0,0),out _,out _),"Scaled content bounds");
                        Check(!panel.Intersect(RayAt(panel,panel.BaseWidth*.5f+.01f,0),out _,out _),"Outside content");
                    }
                    panel.TryResizeAction(VitqraSpatialPanel.ResetSizeAction);
                    Check(Mathf.Approximately(panel.SizeScale,1f),"Reset");
                    panel.SetSizeScale(float.NaN);
                    Check(Mathf.Approximately(panel.SizeScale,1f),"Reject invalid scale");
                    panel.gameObject.SetActive(false);
                    Check(!panel.TryIntersectResize(RayAt(panel,0,panel.BaseHeight*.5f+.034f),out _,out _),"Hidden toolbar");
                }
                Debug.Log("VITQRA window resize checks passed.");
            }
            finally { UnityEngine.Object.DestroyImmediate(root); }
        }
        private static Ray RayAt(VitqraSpatialPanel panel,float x,float y)
        {
            Vector3 point=panel.transform.TransformPoint(new Vector3(x,y,0));
            return new Ray(point-panel.transform.forward, panel.transform.forward);
        }
        private static void CheckHit(VitqraSpatialPanel panel,float x,float y,int expected)
        {
            Check(panel.TryIntersectResize(RayAt(panel,x,y),out _,out int region)&&region==expected,"Toolbar hit region");
        }
        private static void Check(bool result,string name)
        {
            if(!result)throw new InvalidOperationException("VITQRA resize check failed: "+name);
        }
    }
}
