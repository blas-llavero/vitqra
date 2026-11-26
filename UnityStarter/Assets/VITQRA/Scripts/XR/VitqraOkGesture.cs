#if VITQRA_XR_HANDS
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Hands;

namespace Vitqra
{
    // Optional adapter. Enable the symbol only after installing and enabling XR Hands.
    public sealed class VitqraOkGesture : MonoBehaviour
    {
        private readonly List<XRHandSubsystem> subsystems = new List<XRHandSubsystem>();
        private XRHandSubsystem subsystem;
        private VitqraSpatialWorkspace workspace;
        private float held;
        private bool ready;
        private int activeHand;
        private const float HoldSeconds = .7f;

        public void Configure(VitqraSpatialWorkspace target) { workspace = target; }

        private void Update()
        {
            if (workspace == null || !workspace.ReviewPending)
            {
                ResetGesture();
                return;
            }
            if (subsystem == null || !subsystem.running)
            {
                ResetGesture();
                subsystems.Clear();
                SubsystemManager.GetSubsystems(subsystems);
                subsystem = subsystems.Find(s => s.running);
                if (subsystem == null) return;
            }
            bool lv = TryRead(subsystem.leftHand, out bool left);
            bool rv = TryRead(subsystem.rightHand, out bool right);
            // Require a valid tracked neutral hand before a new confirmation.
            if (!lv && !rv) { ResetGesture(); return; }
            if (!ready)
            {
                if (!(lv && left) && !(rv && right)) ready = true;
                return;
            }
            if (activeHand == 0) activeHand = lv && left ? 1 : rv && right ? 2 : 0;
            if (activeHand == 0) return;
            bool samePose = activeHand == 1 ? lv && left : rv && right;
            if (!samePose) { ResetGesture(); return; }
            // Cap per-frame contribution: a tracking or rendering stall cannot confirm alone.
            held += Mathf.Min(Time.unscaledDeltaTime, .05f);
            if (held < HoldSeconds) return;
            ResetGesture();
            workspace.ConfirmReview();
        }

        private void ResetGesture() { held = 0; ready = false; activeHand = 0; }

        private static bool Joint(XRHand hand, XRHandJointID id, out Vector3 position)
        {
            position = Vector3.zero;
            if (!hand.GetJoint(id).TryGetPose(out Pose pose)) return false;
            position = pose.position;
            return true;
        }
        private static bool Extended(XRHand hand, XRHandJointID proximal, XRHandJointID intermediate,
            XRHandJointID distal, XRHandJointID tip, out bool extended)
        {
            extended = false;
            if (!Joint(hand,proximal,out var a) || !Joint(hand,intermediate,out var b) ||
                !Joint(hand,distal,out var c) || !Joint(hand,tip,out var d)) return false;
            float length = Vector3.Distance(a,b) + Vector3.Distance(b,c) + Vector3.Distance(c,d);
            if (length < .001f) return false;
            extended = Vector3.Distance(a,d) / length >= .86f;
            return true;
        }
        private static bool TryRead(XRHand hand, out bool ok)
        {
            ok = false;
            if (!hand.isTracked || !Joint(hand,XRHandJointID.ThumbTip,out var thumb) ||
                !Joint(hand,XRHandJointID.IndexTip,out var index) ||
                !Joint(hand,XRHandJointID.Wrist,out var wrist) ||
                !Joint(hand,XRHandJointID.MiddleProximal,out var middleBase)) return false;
            float handSize = Vector3.Distance(wrist,middleBase);
            if (handSize < .02f) return false;
            if (!Extended(hand,XRHandJointID.MiddleProximal,XRHandJointID.MiddleIntermediate,XRHandJointID.MiddleDistal,XRHandJointID.MiddleTip,out bool m) ||
                !Extended(hand,XRHandJointID.RingProximal,XRHandJointID.RingIntermediate,XRHandJointID.RingDistal,XRHandJointID.RingTip,out bool r) ||
                !Extended(hand,XRHandJointID.LittleProximal,XRHandJointID.LittleIntermediate,XRHandJointID.LittleDistal,XRHandJointID.LittleTip,out bool p)) return false;
            ok = Vector3.Distance(thumb,index) <= Mathf.Clamp(handSize * .22f,.012f,.025f) && m && r && p;
            return true;
        }
    }
}
#endif
