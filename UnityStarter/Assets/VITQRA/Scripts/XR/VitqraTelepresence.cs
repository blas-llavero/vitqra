using System;
using System.Collections.Generic;
using D=Vitqra.VitqraCommunityPolicy;
using M=Vitqra.VitqraAvatarMeeting;

namespace Vitqra
{
    // Closed synthetic pose exercise. No hardware, network, real identities,
    // clinical data, recording or persistence.
    public sealed class VitqraTelepresence
    {
        public const int LifetimeMs=500, MaxTimeMs=60000;
        public enum Sample { Neutral, Turn, LeftGesture, RightGesture, LostHead, LostHands, InvalidCoordinates }
        public enum Result { Saved, Denied, Invalid, NoConsent, Stale, Replay }
        public sealed class Point
        {
            public readonly int x,y,z,yaw;
            internal Point(int x,int y,int z,int yaw){this.x=x;this.y=y;this.z=z;this.yaw=yaw;}
        }
        public sealed class Pose
        {
            public readonly int sequence,capturedMs;
            public readonly Point head,left,right;
            internal Pose(int seq,int ms,Point h,Point l,Point r)
            {sequence=seq;capturedMs=ms;head=h;left=l;right=r;}
        }
        private sealed class Participant
        {
            public bool head,hands;
            public int lastSequence=-1,lastCapture=-1,consentSince;
            public Pose pose;
        }
        private readonly M meeting;
        private readonly Dictionary<D.Actor,Participant> participants=new Dictionary<D.Actor,Participant>();
        private int now;
        public int Now=>now;
        public int Day=>meeting.Day;
        public VitqraTelepresence(D.Resource room){meeting=new M(room);}
        public bool Joined(D.Actor a)=>meeting.Joined(a)&&participants.ContainsKey(a);
        public bool Join(D.Actor a,int profile)
        {
            if(meeting.Join(a,profile)!=M.Result.Ok)return false;
            if(!participants.ContainsKey(a))participants.Add(a,new Participant{consentSince=now});
            return true;
        }
        public void Leave(D.Actor a){meeting.Leave(a);participants.Remove(a);}
        public void Clear(){meeting.Clear();participants.Clear();}
        public bool AdvanceDay(int day)
        {
            if(!meeting.AdvanceDay(day))return false;
            foreach(var a in new List<D.Actor>(participants.Keys))if(!meeting.Joined(a))participants.Remove(a);
            return true;
        }
        public bool AdvanceMs(int ms)
        {
            if(ms<now||ms>MaxTimeMs)return false;
            now=ms;
            foreach(var p in participants.Values)if(p.pose!=null&&now-p.pose.capturedMs>=LifetimeMs)p.pose=null;
            return true;
        }
        public bool HeadConsent(D.Actor a)=>Joined(a)&&participants[a].head;
        public bool HandConsent(D.Actor a)=>Joined(a)&&participants[a].hands;
        public bool SetConsent(D.Actor a,bool head,bool hands)
        {
            if(!Joined(a))return false;
            var p=participants[a];
            if(p.head!=head||p.hands!=hands){p.pose=null;p.consentSince=now;}
            p.head=head;p.hands=hands;return true;
        }
        private static bool Valid(Point p)=>p==null||
            (p.x>=-2000&&p.x<=2000&&p.y>=0&&p.y<=2500&&p.z>=-2000&&p.z<=2000&&p.yaw>=-180&&p.yaw<=180);
        // Fixed fictional room-local millimetres and degrees; no arbitrary poses.
        public Result SubmitFixture(D.Actor a,Sample sample,int sequence,int captureMs)
        {
            if(!Joined(a))return Result.Denied;
            var p=participants[a];
            if(!p.head&&!p.hands)return Result.NoConsent;
            if(!Enum.IsDefined(typeof(Sample),sample)||sequence<0||captureMs<0||captureMs>now)return Result.Invalid;
            if(sequence<=p.lastSequence||captureMs<p.lastCapture)return Result.Replay;
            if(now-captureMs>=LifetimeMs||captureMs<p.consentSince)return Result.Stale;
            p.lastSequence=sequence;p.lastCapture=captureMs;
            Point head=sample==Sample.LostHead?null:new Point(sample==Sample.InvalidCoordinates?3000:0,1600,0,sample==Sample.Turn?45:0);
            Point left=sample==Sample.LostHands?null:new Point(-350,sample==Sample.LeftGesture?1500:1000,250,0);
            Point right=sample==Sample.LostHands?null:new Point(350,sample==Sample.RightGesture?1500:1000,250,0);
            if(!p.head)head=null;if(!p.hands){left=null;right=null;}
            if(!Valid(head)||!Valid(left)||!Valid(right)){p.pose=null;return Result.Invalid;}
            p.pose=new Pose(sequence,captureMs,head,left,right);return Result.Saved;
        }
        public Pose Read(D.Actor viewer,D.Actor subject)
        {
            if(viewer==subject||!Joined(viewer)||!Joined(subject))return null;
            var p=participants[subject];var f=p.pose;
            if(f==null||now-f.capturedMs>=LifetimeMs||(!p.head&&!p.hands))return null;
            return f;
        }
        public bool NetworkEnabled=>false;
        public bool HardwareTrackingEnabled=>false;
        public bool RecordingEnabled=>false;
        public bool ClinicalRelease=>false;
    }
}
