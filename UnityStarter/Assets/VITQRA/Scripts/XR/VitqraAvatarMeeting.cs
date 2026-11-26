using System;
using System.Collections.Generic;
using D=Vitqra.VitqraCommunityPolicy;

namespace Vitqra
{
    // Closed local role-play: no microphone, user text, remote peers, transport,
    // recording, AI inference, real identity or access to health records.
    public sealed class VitqraAvatarMeeting
    {
        public enum Phrase { Greeting, Ready, ThankYou }
        public enum Result { Ok, Denied, Invalid, NotJoined, Muted, Busy }
        public sealed class Caption
        {
            public readonly D.Actor speaker;
            public readonly Phrase phrase;
            public readonly int untilSecond;
            internal Caption(D.Actor a,Phrase p,int end){speaker=a;phrase=p;untilSecond=end;}
        }
        private sealed class Member
        {
            public int profile;
            public bool muted=true;
        }
        private readonly Dictionary<D.Actor,Member> members=new Dictionary<D.Actor,Member>();
        private Caption caption;
        private int day=20,second;
        public readonly D.Resource room;
        public int Day=>day;
        public int Second=>second;
        public VitqraAvatarMeeting(D.Resource r)
        {
            if(r!=D.Resource.PrivateRoomA&&r!=D.Resource.PrivateRoomB)throw new ArgumentException("Fictional room only");
            room=r;
        }
        private bool Allowed(D.Actor actor)=>D.Evaluate(actor,room,D.Purpose.Care,D.Action.Join,day).outcome==D.Outcome.AllowSimulation;
        private void ClearSpeech(D.Actor actor){if(caption!=null&&caption.speaker==actor)caption=null;}
        public bool AdvanceDay(int next)
        {
            if(next<day||next>180)return false;
            day=next;
            foreach(var a in new List<D.Actor>(members.Keys))if(!Allowed(a)){members.Remove(a);ClearSpeech(a);}
            return true;
        }
        public bool AdvanceSecond(int next)
        {
            if(next<second||next>86400)return false;
            second=next;
            if(caption!=null&&second>=caption.untilSecond)caption=null;
            return true;
        }
        public Result Join(D.Actor actor,int profile)
        {
            if(!Allowed(actor))return Result.Denied;
            if(profile<0||profile>3)return Result.Invalid;
            if(!members.ContainsKey(actor))members.Add(actor,new Member{profile=profile});
            return Result.Ok;
        }
        public Result SetProfile(D.Actor actor,int profile)
        {
            if(!Allowed(actor))return Result.Denied;
            if(profile<0||profile>3)return Result.Invalid;
            Member m;if(!members.TryGetValue(actor,out m))return Result.NotJoined;
            m.profile=profile;return Result.Ok;
        }
        public Result SetMuted(D.Actor actor,bool muted)
        {
            if(!Allowed(actor))return Result.Denied;
            Member m;if(!members.TryGetValue(actor,out m))return Result.NotJoined;
            m.muted=muted;if(muted)ClearSpeech(actor);return Result.Ok;
        }
        public Result SpeakFixture(D.Actor actor,Phrase phrase)
        {
            if(!Allowed(actor))return Result.Denied;
            if(!Enum.IsDefined(typeof(Phrase),phrase)||second>86395)return Result.Invalid;
            Member m;if(!members.TryGetValue(actor,out m))return Result.NotJoined;
            if(m.muted)return Result.Muted;
            if(caption!=null)return Result.Busy;
            caption=new Caption(actor,phrase,second+5);return Result.Ok;
        }
        public void Leave(D.Actor actor){members.Remove(actor);ClearSpeech(actor);}
        public void Clear(){members.Clear();caption=null;}
        public bool Joined(D.Actor actor)=>Allowed(actor)&&members.ContainsKey(actor);
        public int Profile(D.Actor actor)=>Joined(actor)?members[actor].profile:-1;
        public bool Muted(D.Actor actor)=>!Joined(actor)||members[actor].muted;
        public Caption ReadCaption(D.Actor viewer)
        {return Joined(viewer)&&caption!=null&&Joined(caption.speaker)&&!Muted(caption.speaker)&&second<caption.untilSecond?caption:null;}
        public bool MicrophoneEnabled=>false;
        public bool NetworkEnabled=>false;
        public bool RecordingEnabled=>false;
        public bool ClinicalRelease=>false;
    }
}
