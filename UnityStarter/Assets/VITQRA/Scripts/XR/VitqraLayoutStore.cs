using System;
using UnityEngine;

namespace Vitqra
{
    // Non-secret, user-relative layout preferences. No room mesh, spatial anchor,
    // camera image, conversation or absolute room coordinates are stored.
    public static class VitqraLayoutStore
    {
        private const string Key = "vitqra.layout.v1";
        public static readonly string[] PanelIds =
            {"research","notes","advice","configuration","assistant","conversation"};

        [Serializable]
        public sealed class Entry
        {
            public string id;
            public Vector3 position;
            public Quaternion rotation;
            public float scale=1f;
        }

        [Serializable]
        public sealed class Layout
        {
            public int version=1;
            public bool conversationOpen=true;
            public bool expandedDiscovery;
            public Entry[] panels;
        }

        private static bool Finite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
        public static bool Valid(Entry e)
        {
            if(e==null || Array.IndexOf(PanelIds,e.id)<0) return false;
            var p=e.position;
            var q=e.rotation;
            if(!Finite(p.x)||!Finite(p.y)||!Finite(p.z)||!Finite(q.x)||!Finite(q.y)||
               !Finite(q.z)||!Finite(q.w)||!Finite(e.scale)) return false;
            float length=q.x*q.x+q.y*q.y+q.z*q.z+q.w*q.w;
            return p.sqrMagnitude>=.25f && p.sqrMagnitude<=16f &&
                length>=.98f && length<=1.02f &&
                e.scale>=VitqraSpatialPanel.MinScale && e.scale<=VitqraSpatialPanel.MaxScale;
        }

        public static Entry Capture(string id, Vector3 origin, Quaternion heading, VitqraSpatialPanel panel)
        {
            var inverse=Quaternion.Inverse(heading);
            return new Entry {id=id,position=inverse*(panel.transform.position-origin),
                rotation=inverse*panel.transform.rotation,scale=panel.SizeScale};
        }

        public static bool Apply(Entry e,Vector3 origin,Quaternion heading,VitqraSpatialPanel panel)
        {
            if(!Valid(e)||panel==null)return false;
            panel.transform.SetPositionAndRotation(origin+heading*e.position,heading*e.rotation);
            panel.SetSizeScale(e.scale);
            panel.ClearResizeHover();
            return true;
        }

        public static Layout Parse(string json)
        {
            if(string.IsNullOrEmpty(json)||json.Length>32768)return null;
            try
            {
                var layout=JsonUtility.FromJson<Layout>(json);
                if(layout==null||layout.version!=1||layout.panels==null||
                   layout.panels.Length!=PanelIds.Length)return null;
                var seen=new System.Collections.Generic.HashSet<string>();
                foreach(var e in layout.panels)
                    if(e==null||Array.IndexOf(PanelIds,e.id)<0||!seen.Add(e.id))return null;
                return layout;
            }
            catch(ArgumentException){return null;}
        }

        public static Layout Load() => Parse(PlayerPrefs.GetString(Key,""));

        public static bool Save(Layout layout)
        {
            try
            {
                PlayerPrefs.SetString(Key,JsonUtility.ToJson(layout));
                PlayerPrefs.Save();
                return true;
            }
            catch(Exception)
            {
                Debug.LogWarning("VITQRA: unable to save spatial layout.");
                return false;
            }
        }
    }
}
