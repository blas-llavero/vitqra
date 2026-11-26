using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

namespace Vitqra
{
    public sealed class VitqraSpatialWorkspace : MonoBehaviour
    {
        [SerializeField] private Camera headCamera;
        [SerializeField] private Transform trackingSpace;
        private readonly List<VitqraSpatialPanel> panels = new List<VitqraSpatialPanel>();
        private VitqraSettings settings;
        private VitqraSpatialPanel research, advice, notes, configuration, grabbed;
        private Vector3 grabOffset;
        private bool previousGrip, previousTrigger, previousPrimary;
        private ulong activeDevice;
        private bool initialized, hasController;
        private int topicIndex, adviceIndex;
        private static readonly Color Cyan = new Color(.24f,.95f,.92f);
        private static readonly Color Violet = new Color(.75f,.64f,1f);

        public void Configure(Camera camera, Transform space)
        {
            headCamera = camera;
            trackingSpace = space;
        }

        private void Start()
        {
            if (headCamera == null || trackingSpace == null)
            {
                Debug.LogError("VITQRA: assign the XR camera and its tracking-space parent.", this);
                enabled = false;
                return;
            }
            settings = VitqraSettings.Load();
            var l = VitqraStrings.Get(settings.language);
            research = AddPanel(l[7], Cyan, () => { topicIndex = (topicIndex + 1) % 5; Refresh(); });
            advice = AddPanel("Smart Advice", Violet, () => { adviceIndex = (adviceIndex + 1) % 3; Refresh(); });
            notes = AddPanel(l[10], Cyan, Refresh);
            configuration = AddPanel(l[4], Violet, () => { settings.automaticResearch = !settings.automaticResearch; settings.Save(); Refresh(); });
            foreach (var panel in panels) panel.gameObject.SetActive(false);
            Refresh();
        }

        private VitqraSpatialPanel AddPanel(string title, Color accent, System.Action callback)
        {
            var obj = new GameObject(title);
            obj.transform.SetParent(transform, false);
            var panel = obj.AddComponent<VitqraSpatialPanel>();
            panel.Initialize(title, "", "", callback, accent);
            panels.Add(panel);
            return panel;
        }

        private void Refresh()
        {
            var l = VitqraStrings.Get(settings.language);
            research.SetContent(l[14 + topicIndex] + "\n" + l[9], "Next topic");
            string[] messages = { "Configure an AI provider before research.", "Use only verified scientific sources.", "AI hypotheses require scientific review." };
            advice.SetContent(messages[adviceIndex], "Next tip");
            string note = PlayerPrefs.GetString("vitqra.preview.note", "");
            if (note.Length > 180) note = note.Substring(0,180) + "...";
            notes.SetContent(string.IsNullOrWhiteSpace(note) ? "No sample note saved." : note, l[12]);
            configuration.SetContent(settings.provider + "\n" + l[5] + "\n" + l[2] + ": " + (settings.automaticResearch ? "ON" : "OFF"), "Toggle automatic research");
        }

        private void PlacePanels()
        {
            Vector3 forward = Vector3.ProjectOnPlane(headCamera.transform.forward, Vector3.up);
            if (forward.sqrMagnitude < 0.001f) forward = Vector3.forward;
            forward.Normalize();
            Vector3 right = Vector3.Cross(Vector3.up, forward);
            Vector3 center = headCamera.transform.position + forward * 1.65f;
            // Leave the central view open at startup and after a layout reset.
            const float columnOffset = 0.85f;
            const float upperRow = 0.12f;
            const float lowerRow = -0.42f;
            PlacePanel(research, center, right, -columnOffset, upperRow);
            PlacePanel(notes, center, right, -columnOffset, lowerRow);
            PlacePanel(advice, center, right, columnOffset, upperRow);
            PlacePanel(configuration, center, right, columnOffset, lowerRow);
            initialized = true;
        }

        private void PlacePanel(VitqraSpatialPanel panel, Vector3 center, Vector3 right, float x, float y)
        {
            panel.transform.position = center + right * x + Vector3.up * y;
            FaceUser(panel.transform);
            panel.gameObject.SetActive(true);
        }

        private void FaceUser(Transform panel)
        {
            Vector3 away = Vector3.ProjectOnPlane(panel.position - headCamera.transform.position, Vector3.up);
            if (away.sqrMagnitude > 0.001f) panel.rotation = Quaternion.LookRotation(away, Vector3.up);
        }

        private static bool ReadPose(InputDevice device, out Vector3 position, out Quaternion rotation)
        {
            position = Vector3.zero;
            rotation = Quaternion.identity;
            return device.isValid &&
                device.TryGetFeatureValue(CommonUsages.isTracked, out bool tracked) && tracked &&
                device.TryGetFeatureValue(CommonUsages.devicePosition, out position) &&
                device.TryGetFeatureValue(CommonUsages.deviceRotation, out rotation);
        }

        private void Update()
        {
            if (!initialized)
            {
                var head = InputDevices.GetDeviceAtXRNode(XRNode.Head);
                if (!head.isValid || !head.TryGetFeatureValue(CommonUsages.isTracked, out bool tracked) || !tracked) return;
                PlacePanels();
            }

            var device = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
            if (!ReadPose(device, out Vector3 localPosition, out Quaternion localRotation))
            {
                device = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
                if (!ReadPose(device, out localPosition, out localRotation))
                {
                    grabbed = null;
                    hasController = false;
                    foreach (var panel in panels) panel.Highlight(false);
                    return;
                }
            }

            device.TryGetFeatureValue(CommonUsages.gripButton, out bool grip);
            device.TryGetFeatureValue(CommonUsages.triggerButton, out bool trigger);
            device.TryGetFeatureValue(CommonUsages.primaryButton, out bool primary);
            if (!hasController || device.deviceId != activeDevice)
            {
                activeDevice = device.deviceId;
                grabbed = null;
                previousGrip = grip;
                previousTrigger = trigger;
                previousPrimary = primary;
                hasController = true;
            }
            Vector3 origin = trackingSpace.TransformPoint(localPosition);
            Quaternion rotation = trackingSpace.rotation * localRotation;
            Ray ray = new Ray(origin, rotation * Vector3.forward);

            if (primary && !previousPrimary) { grabbed = null; PlacePanels(); }
            VitqraSpatialPanel closest = null;
            float nearest = float.PositiveInfinity;
            bool onAction = false;
            foreach (var panel in panels)
            {
                if (panel.Intersect(ray, out float distance, out bool region) && distance < nearest)
                {
                    closest = panel; nearest = distance; onAction = region;
                }
            }
            if (grip && !previousGrip && closest != null)
            {
                grabbed = closest;
                grabOffset = Quaternion.Inverse(rotation) * (grabbed.transform.position - origin);
            }
            if (!grip) grabbed = null;
            if (grabbed != null)
            {
                Vector3 target = origin + rotation * grabOffset;
                Vector3 delta = target - headCamera.transform.position;
                if (delta.sqrMagnitude > 0.0001f)
                    grabbed.transform.position = headCamera.transform.position + delta.normalized * Mathf.Clamp(delta.magnitude, .75f, 3f);
                FaceUser(grabbed.transform);
            }
            else if (!grip && trigger && !previousTrigger && closest != null && onAction) closest.InvokeAction();
            foreach (var panel in panels) panel.Highlight(panel == grabbed || (grabbed == null && panel == closest));
            previousGrip = grip; previousTrigger = trigger; previousPrimary = primary;
        }

        private void OnDestroy()
        {
            foreach (var panel in panels) if (panel != null) Destroy(panel.gameObject);
        }
    }
}
