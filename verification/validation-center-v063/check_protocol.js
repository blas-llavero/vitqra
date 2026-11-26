"use strict";
function verify(){
let checks=0;const ck=x=>{if(!x)throw Error("case "+checks);checks++;};
class Center{
constructor(){this.rev=1;this.day=20;this.h=[];}
advance(d){if(d<this.day||d>180)return false;this.day=d;return true;}
access(a,r){return a==="reviewer"&&["domain","methods"].includes(r)&&this.day>=20&&this.day<80;}
status(){let h=this.h.filter(x=>x.rev===this.rev);return h.some(x=>x.v==="reject")?"rejected":h.some(x=>x.v==="revise")?"revision":this.rev===1?"quality":h.filter(x=>x.v==="yes").length===2?"favorable":"awaiting";}
submit(a,r,rev,v,why){if(!this.access(a,r))return "denied";if(!["yes","revise","reject"].includes(v)||!["check","data","method","evidence"].includes(why))return "invalid";if(rev!==this.rev)return "stale";if(this.h.some(x=>x.rev===rev&&x.r===r))return "duplicate";if(["rejected","revision","favorable"].includes(this.status()))return "closed";if(v==="yes"&&this.rev===1)return "quality";if((v==="yes")!==(why==="check"))return "invalid";this.h.push({rev,r,v,why});return "saved";}
correct(a,r){if(!this.access(a,r)||this.status()!=="revision"||this.rev>=3)return false;this.rev++;return true;}
}
let c=new Center(),a="reviewer",d="domain",m="methods";
ck(c.status()==="quality");ck(c.submit(a,d,1,"yes","check")==="quality");ck(c.submit("ai",d,1,"revise","data")==="denied");ck(c.submit(a,"conflicted",1,"revise","data")==="denied");ck(c.submit(a,"unverified",1,"revise","data")==="denied");ck(!c.correct(a,d));ck(c.submit(a,d,1,"revise","data")==="saved");ck(c.status()==="revision");ck(c.submit(a,m,1,"yes","check")==="closed");ck(c.correct(a,d));ck(c.status()==="awaiting");ck(c.submit(a,d,1,"yes","check")==="stale");ck(c.submit(a,d,2,"yes","method")==="invalid");ck(c.submit(a,d,2,"yes","check")==="saved");ck(c.submit(a,d,2,"yes","check")==="duplicate");ck(c.status()==="awaiting");ck(c.submit(a,m,2,"yes","check")==="saved");ck(c.status()==="favorable");ck(c.h.length===3);ck(!c.correct(a,d));ck(c.advance(80));ck(!c.access(a,d));ck(c.submit(a,m,2,"reject","evidence")==="denied");ck(!c.advance(25));
let e=new Center();ck(e.submit(a,m,1,"reject","evidence")==="saved");ck(e.status()==="rejected");ck(!e.correct(a,d));
let f=new Center();ck(f.submit(a,d,1,"revise","data")==="saved"&&f.correct(a,d));ck(f.submit(a,d,2,"yes","check")==="saved");ck(f.submit(a,m,2,"revise","method")==="saved"&&f.correct(a,m));ck(f.status()==="awaiting");ck(f.submit(a,d,2,"yes","check")==="stale");ck(f.submit(a,d,3,"revise","method")==="saved");ck(!f.correct(a,d));
return {scope:"fictional reference simulation only",checks,example_history:c.h,final_example:c.status(),clinical_release:false,scientific_approval:false,certificate:false};
}
console.log(JSON.stringify(verify(),null,2));
