"use strict";
function verify(){
let count=0;
const check=(v)=>{if(!v)throw Error("fixture "+count);count++;};
class Diary {
constructor(alternate=false){this.alt=alternate;this.day=1;this.rows=[];this.opt=false;this.device=false;this.last=-1;this.minute=1080;}
due(d){return d>=1&&d<=30&&(!this.alt||d%2===1);}
advance(d){if(d<this.day||d>30)return false;this.day=d;return true;}
record(s,extra=false,owner=true){if(!owner||s<0||s>5||!extra&&!this.due(this.day))return false;let k=extra?"E":"S";if(this.rows.some(r=>r[0]===this.day&&r[1]===k))return false;this.rows.push([this.day,k,s]);return true;}
import(){if(!this.device||this.rows.some(r=>r[0]===this.day&&r[1]==="D"))return false;this.rows.push([this.day,"D",40+this.day%9]);return true;}
poll(t){if(!this.opt||!this.due(this.day)||t<this.minute||t>=1440||this.last===this.day||this.rows.some(r=>r[0]===this.day&&r[1]==="S"))return false;this.last=this.day;return true;}
inspect(c){if(c<1||c>this.day)return null;let rows=this.rows.filter(r=>r[0]<=c),dist=[0,0,0,0,0,0],expected=0;for(let d=1;d<=c;d++)if(this.due(d))expected++;for(let r of rows)if(r[1]==="S")dist[r[2]]++;let responded=dist.reduce((a,b)=>a+b,0);return {expected,responded,known:dist.slice(0,4).reduce((a,b)=>a+b,0),unknown:dist[4],unable:dist[5],unanswered:expected-responded,extras:rows.filter(r=>r[1]==="E").length,device:rows.filter(r=>r[1]==="D").length,distribution:dist};}
}
let d=new Diary();check(!d.opt&&!d.device);check(!d.poll(1080));d.opt=true;check(!d.poll(1079)&&d.poll(1080)&&!d.poll(1081));check(!d.record(3,false,false));check(d.record(0)&&!d.record(3));check(d.record(3,true));check(!d.import());d.device=true;check(d.import()&&!d.import());d.device=false;check(d.advance(2)&&!d.import());check(d.record(4)&&d.advance(3)&&d.record(5));check(d.advance(5)&&d.record(1));check(d.advance(6)&&d.record(2)&&d.advance(7)&&d.record(3));let report=d.inspect(7);check(JSON.stringify(report)===JSON.stringify({expected:7,responded:6,known:4,unknown:1,unable:1,unanswered:1,extras:1,device:1,distribution:[1,1,1,1,1,1]}));check(d.inspect(1).responded===1&&d.inspect(8)===null);check(!d.advance(1)&&!d.advance(31));
let alt=new Diary(true);alt.opt=true;check(alt.advance(2)&&!alt.poll(1080)&&!alt.record(0));check(alt.record(0,true)&&alt.advance(30));check(alt.inspect(30).expected===15&&alt.inspect(30).responded===0);
let stable=new Diary();for(let i=1;i<=30;i++){check(stable.advance(i));check(stable.record(0));}check(stable.inspect(30).distribution.join(",")==="30,0,0,0,0,0");
return {scope:"fictional reference simulation only",checks:count,report,stable:stable.inspect(30),alternate:alt.inspect(30)};
}
console.log(JSON.stringify(verify(),null,2));
