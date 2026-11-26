using System;
using UnityEngine;
using UnityEngine.UI;

namespace Vitqra
{
    public sealed class VitqraSpatialPanel : MonoBehaviour
    {
        public const float Width = 0.64f;
        public const float Height = 0.34f;
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

        public void Highlight(bool value) { if (background != null) background.color = value ? Hover : Normal; }
        public void InvokeAction(int region) { if (region == 1) activate?.Invoke(); else if (region == 2) secondaryActivate?.Invoke(); }

        // Intersect the finite panel rectangle; no physics layer or collider required.
        public bool Intersect(Ray ray, out float distance, out int actionRegion)
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

        private static RectTransform Child(string name, Transform parent, Vector2 position, Vector2 size)
        {
            var obj = new GameObject(name, typeof(RectTransform));
            var rect = (RectTransform)obj.transform;
            rect.SetParent(parent, false);
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(.5f,.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            return rect;
        }
        private static Image Box(string name, Transform parent, Vector2 position, Vector2 size, Color color)
        {
            var image = Child(name, parent, position, size).gameObject.AddComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            return image;
        }
        private static Text Label(string name, Transform parent, Vector2 position, Vector2 size, string content, int fontSize, Color color)
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
