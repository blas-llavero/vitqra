"use strict";
function verify(){
let checks=0;const ck=x=>{if(!x)throw Error("case "+checks);checks++;};
class Session {
constructor(room){this.room=room;this.day=20;this.now=0;this.p=new Map();}
allowed(a){return [this.room,this.room+2].includes(a)&&this.day>=20&&this.day<30;}
joined(a){return this.allowed(a)&&this.p.has(a);}
join(a,profile=0){if(!this.allowed(a)||profile<0||profile>3)return false;if(!this.p.has(a))this.p.set(a,{h:false,k:false,last:-1,capture:-1,since:this.now,f:null});return true;}
leave(a){this.p.delete(a);}
consent(a,h,k){if(!this.joined(a))return false;const p=this.p.get(a);if(p.h!==h||p.k!==k){p.f=null;p.since=this.now;}p.h=h;p.k=k;return true;}
advance(ms){if(ms<this.now||ms>60000)return false;this.now=ms;for(const p of this.p.values())if(p.f&&ms-p.f.ms>=500)p.f=null;return true;}
advanceDay(d){if(d<this.day||d>180)return false;this.day=d;for(const a of this.p.keys())if(!this.allowed(a))this.leave(a);return true;}
submit(a,s,seq,ms){if(!this.joined(a))return "Denied";const p=this.p.get(a);if(!p.h&&!p.k)return "NoConsent";if(s<0||s>6||seq<0||ms<0||ms>this.now)return "Invalid";if(seq<=p.last||ms<p.capture)return "Replay";if(this.now-ms>=500||ms<p.since)return "Stale";p.last=seq;p.capture=ms;if(s===6&&p.h){p.f=null;return "Invalid";}p.f={ms,head:p.h&&s!==4,left:p.k&&s!==5,right:p.k&&s!==5,yaw:s===1?45:0};return "Saved";}
read(a,b){return a!==b&&this.joined(a)&&this.joined(b)?this.p.get(b).f:null;}
}
let s=new Session(0);
ck(!s.join(1)&&!s.join(3)&&!s.join(10));ck(!s.join(0,4));ck(s.join(0)&&s.join(2));ck(s.submit(0,0,0,0)==="NoConsent");ck(s.read(2,0)===null);
ck(s.consent(0,true,false));ck(s.submit(0,1,1,0)==="Saved");ck(s.read(2,0).head&&!s.read(2,0).left&&s.read(2,0).yaw===45);ck(s.read(0,0)===null&&s.read(1,0)===null);ck(s.submit(0,0,1,0)==="Replay");
ck(s.advance(499)&&s.read(2,0)!==null);ck(s.advance(500)&&s.read(2,0)===null);ck(s.submit(0,0,2,0)==="Stale");ck(s.submit(0,0,2,501)==="Invalid");
ck(s.consent(0,true,true)&&s.submit(0,0,2,499)==="Stale");ck(s.submit(0,0,2,500)==="Saved");ck(s.read(2,0).head&&s.read(2,0).left&&s.read(2,0).right);ck(s.submit(0,0,3,499)==="Replay");
ck(s.submit(0,4,3,500)==="Saved"&&!s.read(2,0).head&&s.read(2,0).left);ck(s.submit(0,5,4,500)==="Saved"&&s.read(2,0).head&&!s.read(2,0).left);
ck(s.submit(0,6,5,500)==="Invalid"&&s.read(2,0)===null);ck(s.submit(0,0,6,500)==="Saved");ck(s.consent(0,false,true)&&s.read(2,0)===null);ck(s.submit(0,2,7,500)==="Saved"&&!s.read(2,0).head&&s.read(2,0).left);ck(s.consent(0,false,false)&&s.read(2,0)===null);ck(s.consent(0,true,true)&&s.read(2,0)===null);
ck(s.submit(0,0,8,500)==="Saved");s.leave(0);ck(s.read(2,0)===null);ck(s.join(0)&&s.submit(0,0,0,500)==="NoConsent");ck(s.consent(0,true,true)&&s.submit(0,0,1,500)==="Saved");ck(s.advanceDay(30)&&s.read(2,0)===null&&!s.joined(0));ck(!s.advanceDay(20)&&!s.advance(499));
let b=new Session(1);ck(b.join(1)&&b.join(3)&&!b.join(0));
return {scope:"fictional local reference only",checks,stale_after_ms:500,network:false,hardware:false,recording:false};
}
console.log(JSON.stringify(verify(),null,2));
