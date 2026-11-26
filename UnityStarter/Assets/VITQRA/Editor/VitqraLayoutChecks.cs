using System;
using UnityEditor;
using UnityEngine;

namespace Vitqra
{
    public static class VitqraLayoutChecks
    {
        [MenuItem("VITQRA/Run Spatial Layout Checks")]
        public static void Run()
        {
            var obj=new GameObject("Layout checks");
            try
            {
                var panel=obj.AddComponent<VitqraSpatialPanel>();
                var origin=new Vector3(4,1.6f,7);
                var heading=Quaternion.Euler(0,40,0);
                var relative=new Vector3(.9f,.2f,1.65f);
                var rotation=Quaternion.Euler(0,25,0);
                obj.transform.SetPositionAndRotation(origin+heading*relative,heading*rotation);
                panel.SetSizeScale(1.3f);
                var e=VitqraLayoutStore.Capture("research",origin,heading,panel);
                Check(VitqraLayoutStore.Valid(e),"Valid entry");
                var nextOrigin=new Vector3(-2,1.8f,3);
                var nextHeading=Quaternion.Euler(0,130,0);
                Check(VitqraLayoutStore.Apply(e,nextOrigin,nextHeading,panel),"Restore");
                Check(Vector3.Distance(obj.transform.position,nextOrigin+nextHeading*relative)<.001f,"Relative position");
                Check(Quaternion.Angle(obj.transform.rotation,nextHeading*rotation)<.1f,"Relative rotation");
                Check(Mathf.Approximately(panel.SizeScale,1.3f),"Scale");
                var entries=new VitqraLayoutStore.Entry[VitqraLayoutStore.PanelIds.Length];
                for(int i=0;i<entries.Length;i++)
                    entries[i]=new VitqraLayoutStore.Entry {id=VitqraLayoutStore.PanelIds[i],position=e.position,rotation=e.rotation,scale=e.scale};
                var layout=new VitqraLayoutStore.Layout {conversationOpen=false,panels=entries};
                var restored=VitqraLayoutStore.Parse(JsonUtility.ToJson(layout));
                Check(restored!=null&&!restored.conversationOpen,"Visibility round trip");
                Check(restored.panels.Length==entries.Length,"All panel identifiers");
                entries[1].id=entries[0].id;
                Check(VitqraLayoutStore.Parse(JsonUtility.ToJson(layout))==null,"Duplicate identifier rejected");
                Check(VitqraLayoutStore.Parse("not JSON")==null,"Malformed JSON rejected");
                e.scale=2f;
                Check(!VitqraLayoutStore.Valid(e),"Invalid scale");
                e.scale=1f;e.position=new Vector3(100,0,0);
                Check(!VitqraLayoutStore.Valid(e),"Invalid position");
                e.position=relative;e.rotation=new Quaternion(0,0,0,0);
                Check(!VitqraLayoutStore.Valid(e),"Invalid rotation");
                Debug.Log("VITQRA spatial layout checks passed.");
            }
            finally {UnityEngine.Object.DestroyImmediate(obj);}
        }
        private static void Check(bool ok,string name)
        {
            if(!ok)throw new InvalidOperationException("VITQRA layout check failed: "+name);
        }
    }
}
