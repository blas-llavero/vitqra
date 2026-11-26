using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using K = Vitqra.VitqraAssistantStrings.Key;

namespace Vitqra
{
    public sealed class VitqraSpatialWorkspace : MonoBehaviour
    {
        [SerializeField] private Camera headCamera;
        [SerializeField] private Transform trackingSpace;
        private readonly List<VitqraSpatialPanel> panels = new List<VitqraSpatialPanel>();
        private VitqraSettings settings;
        private VitqraSpatialPanel research, advice, notes, configuration, assistant, grabbed;
        private VitqraConversationPanel conversation;
        private VitqraSpeechPlayback playback;
        private bool settingsOpen, conversationOpen = true;
        private VitqraAvatarPreview avatar;
        private bool reviewPending, reviewCompleted;
        public bool ReviewPending => reviewPending;
        public event System.Action<string, string> AdviceChanged;
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
            advice = AddPanel(T(K.Advice), Violet, ReviewAction);
            notes = AddPanel(T(K.Notes), Cyan, Refresh);
            configuration = AddPanel(T(K.Settings), Violet, OpenAvatarSettings);
            configuration.SetSecondaryAction(T(K.ToggleResearch), () => { settings.automaticResearch = !settings.automaticResearch; settings.Save(); Refresh(); });
            assistant = AddPanel(T(K.Assistant), Violet, ChooseNextAvatar);
            assistant.SetSecondaryAction(T(K.Close), CloseAvatarSettings);
            advice.SetSecondaryAction(VitqraChatStrings.Get(settings.language, VitqraChatStrings.Key.Open), OpenConversation);
            var chatObject = new GameObject("Conversation");
            chatObject.transform.SetParent(transform,false);
            conversation = chatObject.AddComponent<VitqraConversationPanel>();
            conversation.InitializeChat(settings.language, MinimizeConversation);
            panels.Add(conversation);
            var speech = GetComponent<VitqraLocalVoice>();
            if(speech==null) speech=gameObject.AddComponent<VitqraLocalVoice>();
            speech.Configure(conversation, VitqraStrings.ResolveLanguage(settings.language));
            playback=GetComponent<VitqraSpeechPlayback>();
            var backend=GetComponent<VitqraSherpaSpeechEngine>();
            if(playback!=null&&backend!=null)playback.Configure(backend);
            assistant.ReserveAvatarSpace();
            avatar = assistant.gameObject.AddComponent<VitqraAvatarPreview>();
            avatar.SetProfile(settings.avatarProfile);
#if VITQRA_XR_HANDS
            gameObject.AddComponent<VitqraOkGesture>().Configure(this);
#endif
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

        private string T(K key) => VitqraAssistantStrings.Get(settings.language, key);
        private string Tip => T((K)((int)K.Tip1 + adviceIndex));

        private void OpenAvatarSettings()
        {
            if (settingsOpen) return;
            settingsOpen = true;
            assistant.transform.SetPositionAndRotation(configuration.transform.position, configuration.transform.rotation);
            configuration.gameObject.SetActive(false);
            assistant.gameObject.SetActive(true);
        }

        private void CloseAvatarSettings()
        {
            settingsOpen = false;
            if (grabbed == assistant) grabbed = null;
            assistant.gameObject.SetActive(false);
            configuration.gameObject.SetActive(true);
        }

        private void OpenConversation()
        {
            conversationOpen = true;
            if (grabbed == advice) grabbed = null;
            advice.gameObject.SetActive(false);
            conversation.gameObject.SetActive(true);
        }

        private void MinimizeConversation()
        {
            conversationOpen = false;
            if (grabbed == conversation) grabbed = null;
            conversation.gameObject.SetActive(false);
            if(playback!=null)playback.Stop();
            advice.gameObject.SetActive(true);
        }

        // Adapter hooks: subscribe directly to the panel, after initialization, on the main thread.
        public VitqraConversationPanel Conversation => conversation;

        public void AddAssistantMessage(string message)
        {
            if (conversation == null || string.IsNullOrWhiteSpace(message)) return;
            conversation.AddAssistantMessage(message);
            if (initialized) OpenConversation();
            if(playback!=null)playback.Speak(message,VitqraStrings.ResolveLanguage(settings.language),settings.avatarProfile);
        }

        private void PresentTip()
        {
            conversation.AddAssistantMessage(Tip, true);
            if (initialized) OpenConversation();
            AdviceChanged?.Invoke(Tip, VitqraStrings.ResolveLanguage(settings.language));
            if(playback!=null)playback.Speak(Tip,VitqraStrings.ResolveLanguage(settings.language),settings.avatarProfile);
        }

        private void ChooseNextAvatar()
        {
            settings.avatarProfile = settings.avatarProfile >= 0 && settings.avatarProfile < 3 ? settings.avatarProfile + 1 : 0;
            settings.Save();
            avatar.SetProfile(settings.avatarProfile);
            Refresh();
        }

        private void ReviewAction()
        {
            if (reviewPending) { ConfirmReview(); return; }
            if (reviewCompleted)
            {
                reviewCompleted = false;
                adviceIndex = (adviceIndex + 1) % 3;
                Refresh();
                PresentTip();
                return;
            }
            reviewPending = true;
            Refresh();
            PresentTip();
        }

        public void ConfirmReview()
        {
            if (!reviewPending) return;
            reviewPending = false;
            reviewCompleted = true;
            Refresh();
        }

        private void Refresh()
        {
            var l = VitqraStrings.Get(settings.language);
            research.SetContent(l[14 + topicIndex] + "\n" + l[9], T(K.NextTopic));
            string status = reviewPending ? "\n" + T(K.PendingReview) : reviewCompleted ? "\n" + T(K.Reviewed) : "";
            advice.SetContent(Tip + status, T(reviewPending ? K.ConfirmReview : reviewCompleted ? K.NextTip : K.ReviewTip));
            string note = PlayerPrefs.GetString("vitqra.preview.note", "");
            if (note.Length > 180) note = note.Substring(0,180) + "...";
            notes.SetContent(string.IsNullOrWhiteSpace(note) ? T(K.NoNote) : note, l[12]);
            configuration.SetContent(settings.provider + "\n" + l[5] + "\n" + l[2] + ": " + T(settings.automaticResearch ? K.On : K.Off), T(K.AvatarSettings));
            int profile = settings.avatarProfile;
            string profileName = profile >= 0 && profile <= 3 ? T((K)((int)K.Man + profile)) : T(K.ChooseAvatar);
            assistant.SetContent(profileName + "\n" + T(K.NoSpeech), T(K.NextAvatar));
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
            PlacePanel(assistant, center, right, columnOffset, lowerRow);
            // The larger conversation occupies the upper right slot instead of Smart Advice.
            // Its lower edge is 0.11 m below eye level, above AI Settings.
            PlacePanel(conversation, center, right, 1.00f, 0.34f);
            settingsOpen = false;
            assistant.gameObject.SetActive(false);
            advice.gameObject.SetActive(!conversationOpen);
            conversation.gameObject.SetActive(conversationOpen);
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
                if(conversation.MessageCount==0) PresentTip();
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
            int onAction = 0;
            foreach (var panel in panels)
            {
                if (panel.Intersect(ray, out float distance, out int region) && distance < nearest)
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
            else if (!grip && trigger && !previousTrigger && closest != null && onAction != 0) closest.InvokeAction(onAction);
            foreach (var panel in panels) panel.Highlight(panel == grabbed || (grabbed == null && panel == closest));
            previousGrip = grip; previousTrigger = trigger; previousPrimary = primary;
        }

        private void OnDestroy()
        {
            foreach (var panel in panels) if (panel != null) Destroy(panel.gameObject);
        }
    }
}
