"use strict";
function verify(){
let checks=0;const ck=v=>{if(!v)throw Error("case "+checks);checks++;};
class Room{
constructor(room){this.room=room;this.day=20;this.sec=0;this.m=new Map();this.c=null;}
allowed(a){return (a===this.room||a===this.room+2)&&this.day>=20&&this.day<30;}
joined(a){return this.allowed(a)&&this.m.has(a);}
join(a,p){if(!this.allowed(a)||p<0||p>3)return false;if(!this.m.has(a))this.m.set(a,{p,muted:true});return true;}
mute(a,m){if(!this.joined(a))return false;this.m.get(a).muted=m;if(m&&this.c?.a===a)this.c=null;return true;}
speak(a,p){if(!this.joined(a)||this.m.get(a).muted||this.c||p<0||p>2||this.sec>86395)return false;this.c={a,p,end:this.sec+5};return true;}
read(a){return this.joined(a)&&this.c&&this.joined(this.c.a)&&!this.m.get(this.c.a).muted&&this.sec<this.c.end?this.c:null;}
leave(a){this.m.delete(a);if(this.c?.a===a)this.c=null;}
time(s){if(s<this.sec||s>86400)return false;this.sec=s;if(this.c&&s>=this.c.end)this.c=null;return true;}
advance(d){if(d<this.day||d>180)return false;this.day=d;for(const a of this.m.keys())if(!this.allowed(a))this.leave(a);return true;}
}
let r=new Room(0);
ck(!r.join(1,0));ck(!r.join(3,0));ck(!r.join(10,0));ck(!r.join(0,4));
ck(r.join(0,0)&&r.join(2,1));ck(r.m.get(0).muted&&r.m.get(2).muted);
ck(!r.speak(0,0));ck(r.mute(0,false)&&r.speak(0,0));ck(r.read(2)?.p===0&&!r.read(1));
ck(r.mute(2,false)&&!r.speak(2,1));ck(r.time(4)&&!!r.read(2));ck(r.time(5)&&!r.read(2));
ck(r.speak(2,1)&&r.mute(2,true)&&!r.read(0));ck(r.speak(0,2));r.leave(0);ck(!r.read(2));
ck(r.join(0,3)&&r.m.get(0).muted);ck(r.mute(0,false)&&r.speak(0,0));ck(r.advance(30)&&!r.joined(0)&&!r.joined(2)&&!r.read(0));ck(!r.advance(25));ck(!r.time(0));
let b=new Room(1);ck(b.join(1,1)&&b.join(3,2)&&!b.join(0,0)&&!b.join(2,0));ck(b.mute(1,false)&&b.time(86396)&&!b.speak(1,1));
return {scope:"fictional local room reference only",checks,caption_seconds:5,rooms:2,profiles:4,network:false,microphone:false,recording:false};
}
console.log(JSON.stringify(verify(),null,2));
