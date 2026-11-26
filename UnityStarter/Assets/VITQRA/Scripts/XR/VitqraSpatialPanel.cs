using System;
using UnityEngine;
using UnityEngine.UI;

namespace Vitqra
{
    public class VitqraSpatialPanel : MonoBehaviour
    {
        public const float Width = 0.64f;
        public const float Height = 0.34f;
        public const float MinScale = .7f;
        public const float MaxScale = 1.5f;
        public const int ShrinkAction = 1001, ResetSizeAction = 1002, GrowAction = 1003;
        private const float ResizeGap = .008f;
        private const float ResizeHeight = .052f;
        private const float ResizeButtonWidth = .11f;
        private const float ResizePitch = .12f;
        private Text scaleLabel;
        private string sizeTitle;
        private float resizeY;
        private bool resizeReady;
        private GameObject resizeToolbar;
        public const float ResizeRevealSeconds = .5f;
        public const float ResizeHideSeconds = 1.5f;
        private const float ResizeEdgeWidth = .025f;
        private float edgeDwell, awayTime;
        public bool ResizeControlsVisible => resizeToolbar != null && resizeToolbar.activeSelf;
        public float SizeScale { get; private set; } = 1f;
        public virtual float BaseWidth => Width;
        public virtual float BaseHeight => Height;
        private Image background;
        private Text body;
        private Text action;
        private Action activate, secondaryActivate;
        private Text secondaryAction;
        private RectTransform actionBackground;
        private RectTransform canvasRect;
        private static readonly Color Normal = new Color(0.025f, 0.065f, 0.10f, 0.96f);
        private static readonly Color Hover = new Color(0.07f, 0.20f, 0.24f, 0.98f);

        public void Initialize(string heading, string message, string actionLabel, Action callback, Color accent)
        {
            activate = callback;
            var canvasObject = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas));
            canvasObject.transform.SetParent(transform, false);
            var rect = (RectTransform)canvasObject.transform;
            canvasRect = rect;
            rect.sizeDelta = new Vector2(640, 340);
            rect.localScale = Vector3.one * 0.001f;
            canvasObject.GetComponent<Canvas>().renderMode = RenderMode.WorldSpace;
            background = Box("Background", rect, Vector2.zero, new Vector2(640, 340), Normal);
            Box("Accent", rect, new Vector2(0, 166), new Vector2(640, 8), accent);
            Label("Heading", rect, new Vector2(0, 128), new Vector2(590, 44), heading, 31, accent);
            body = Label("Body", rect, new Vector2(0, 16), new Vector2(590, 170), message, 24, Color.white);
            actionBackground = Box("Action", rect, new Vector2(0, -131), new Vector2(590, 50), new Color(0.10f, 0.27f, 0.31f, 1)).rectTransform;
            action = Label("Action label", rect, new Vector2(0, -131), new Vector2(570, 48), actionLabel, 24, Color.white);
        }

        // A small separate toolbar avoids changing the existing content or button layout.
        // All hit regions use panel-local coordinates, including after uniform scaling.
        public void InitializeResizeControls(string localizedSizeTitle)
        {
            if (resizeReady) return;
            sizeTitle = localizedSizeTitle;
            resizeY = BaseHeight * .5f + ResizeGap + ResizeHeight * .5f;
            var obj = new GameObject("Resize Toolbar", typeof(RectTransform), typeof(Canvas));
            resizeToolbar = obj;
            obj.SetActive(false);
            var rect = (RectTransform)obj.transform;
            rect.SetParent(transform, false);
            rect.localPosition = new Vector3(0, resizeY, 0);
            rect.sizeDelta = new Vector2(350, 52);
            rect.localScale = Vector3.one * .001f;
            obj.GetComponent<Canvas>().renderMode = RenderMode.WorldSpace;
            var color = new Color(.10f,.27f,.31f,.98f);
            Box("Shrink",rect,new Vector2(-120,0),new Vector2(110,52),color);
            Label("Shrink label",rect,new Vector2(-120,0),new Vector2(100,48),"-",32,Color.white);
            Box("Reset size",rect,Vector2.zero,new Vector2(110,52),color);
            scaleLabel=Label("Scale label",rect,Vector2.zero,new Vector2(106,48),"",17,Color.white);
            Box("Grow",rect,new Vector2(120,0),new Vector2(110,52),color);
            Label("Grow label",rect,new Vector2(120,0),new Vector2(100,48),"+",32,Color.white);
            resizeReady=true;
            SetSizeScale(SizeScale);
        }

        public void SetSizeScale(float scale)
        {
            if (float.IsNaN(scale) || float.IsInfinity(scale)) return;
            SizeScale=Mathf.Clamp(scale,MinScale,MaxScale);
            transform.localScale=Vector3.one*SizeScale;
            if(scaleLabel!=null)
                scaleLabel.text=sizeTitle+"\n"+Mathf.RoundToInt(SizeScale*100f)+"%";
        }

        // Called once per tracked-controller frame; only the nearest panel may accumulate dwell.
        public void UpdateResizeHover(bool onEdge, bool onToolbar, float elapsed)
        {
            if (!resizeReady || !gameObject.activeInHierarchy)
            {
                ClearResizeHover();
                return;
            }
            // A stalled frame alone cannot reveal the toolbar.
            float dt = Mathf.Clamp(elapsed,0f,.1f);
            if (!ResizeControlsVisible)
            {
                awayTime=0f;
                edgeDwell=onEdge?edgeDwell+dt:0f;
                if(edgeDwell+0.00001f>=ResizeRevealSeconds)
                {
                    resizeToolbar.SetActive(true);
                    edgeDwell=0f;
                }
                return;
            }
            if(onEdge || onToolbar) awayTime=0f;
            else
            {
                awayTime+=dt;
                if(awayTime+0.00001f>=ResizeHideSeconds) ClearResizeHover();
            }
        }

        public void ClearResizeHover()
        {
            edgeDwell=0f;
            awayTime=0f;
            if(resizeToolbar!=null) resizeToolbar.SetActive(false);
        }

        protected virtual void OnDisable() { ClearResizeHover(); }

        // The activation strip is inside all four edges and scales with the panel.
        public bool IsResizeEdge(Ray ray)
        {
            if(!resizeReady || !gameObject.activeInHierarchy ||
               Vector3.Dot(ray.direction,transform.forward)<=.0001f) return false;
            if(!new Plane(transform.forward,transform.position).Raycast(ray,out float distance)||distance>4f) return false;
            Vector3 local=transform.InverseTransformPoint(ray.GetPoint(distance));
            float dx=BaseWidth*.5f-Mathf.Abs(local.x);
            float dy=BaseHeight*.5f-Mathf.Abs(local.y);
            return dx>=0f && dy>=0f && Mathf.Min(dx,dy)<=ResizeEdgeWidth;
        }

        public bool TryResizeAction(int region)
        {
            if(!ResizeControlsVisible || !gameObject.activeInHierarchy) return false;
            switch(region)
            {
                case ShrinkAction: SetSizeScale(Mathf.Round((SizeScale-.1f)*10f)/10f); return true;
                case ResetSizeAction: SetSizeScale(1f); return true;
                case GrowAction: SetSizeScale(Mathf.Round((SizeScale+.1f)*10f)/10f); return true;
                default: return false;
            }
        }

        public bool TryIntersectResize(Ray ray, out float distance, out int region)
        {
            distance=0; region=0;
            if(!resizeReady || !ResizeControlsVisible || !gameObject.activeInHierarchy ||
                Vector3.Dot(ray.direction,transform.forward)<=.0001f) return false;
            if(!new Plane(transform.forward,transform.position).Raycast(ray,out distance)||distance>4f) return false;
            Vector3 local=transform.InverseTransformPoint(ray.GetPoint(distance));
            if(Mathf.Abs(local.y-resizeY)>ResizeHeight*.5f) return false;
            if(Mathf.Abs(local.x+ResizePitch)<=ResizeButtonWidth*.5f) region=ShrinkAction;
            else if(Mathf.Abs(local.x)<=ResizeButtonWidth*.5f) region=ResetSizeAction;
            else if(Mathf.Abs(local.x-ResizePitch)<=ResizeButtonWidth*.5f) region=GrowAction;
            return region!=0;
        }

        public void SetContent(string message, string actionLabel)
        {
            body.text = message;
            action.text = actionLabel;
        }

        public void ReserveAvatarSpace()
        {
            body.rectTransform.anchoredPosition = new Vector2(94, 16);
            body.rectTransform.sizeDelta = new Vector2(350, 170);
        }

        public void SetSecondaryAction(string label, Action callback)
        {
            secondaryActivate = callback;
            actionBackground.anchoredPosition = new Vector2(-150, -131);
            actionBackground.sizeDelta = new Vector2(290, 50);
            action.rectTransform.anchoredPosition = new Vector2(-150, -131);
            action.rectTransform.sizeDelta = new Vector2(280, 48);
            Box("Secondary action", canvasRect, new Vector2(150, -131), new Vector2(290, 50), new Color(0.10f, 0.27f, 0.31f, 1));
            secondaryAction = Label("Secondary label", canvasRect, new Vector2(150, -131), new Vector2(280, 48), label, 21, Color.white);
            action.fontSize = 21;
        }

        public virtual void Highlight(bool value) { if (background != null) background.color = value ? Hover : Normal; }
        public virtual void InvokeAction(int region) { if (region == 1) activate?.Invoke(); else if (region == 2) secondaryActivate?.Invoke(); }

        // Intersect the finite panel rectangle; no physics layer or collider required.
        public virtual bool Intersect(Ray ray, out float distance, out int actionRegion)
        {
            distance = 0;
            actionRegion = 0;
            if (!gameObject.activeInHierarchy || Vector3.Dot(ray.direction, transform.forward) <= 0.0001f) return false;
            if (!new Plane(transform.forward, transform.position).Raycast(ray, out distance) || distance > 4f) return false;
            Vector3 local = transform.InverseTransformPoint(ray.GetPoint(distance));
            if (Mathf.Abs(local.x) > Width * 0.5f || Mathf.Abs(local.y) > Height * 0.5f) return false;
            if (Mathf.Abs(local.y + 0.131f) <= 0.025f && Mathf.Abs(local.x) <= 0.295f)
            {
                if (secondaryActivate == null) actionRegion = 1;
                else if (local.x <= -0.005f) actionRegion = 1;
                else if (local.x >= 0.005f) actionRegion = 2;
            }
            return true;
        }

        protected static RectTransform Child(string name, Transform parent, Vector2 position, Vector2 size)
        {
            var obj = new GameObject(name, typeof(RectTransform));
            var rect = (RectTransform)obj.transform;
            rect.SetParent(parent, false);
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(.5f,.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            return rect;
        }
        protected static Image Box(string name, Transform parent, Vector2 position, Vector2 size, Color color)
        {
            var image = Child(name, parent, position, size).gameObject.AddComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            return image;
        }
        protected static Text Label(string name, Transform parent, Vector2 position, Vector2 size, string content, int fontSize, Color color)
        {
            var text = Child(name,parent,position,size).gameObject.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = fontSize;
            text.color = color;
            text.alignment = TextAnchor.MiddleCenter;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            text.supportRichText = false;
            text.raycastTarget = false;
            text.text = content;
            return text;
        }
    }
}
