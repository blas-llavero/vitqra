"use strict";
function run(){
let s=271828183;const next=b=>{s=(Math.imul(s,1664525)+1013904223)>>>0;return (s>>>8)%b;};
let canonical="synthetic-multimodal-v1\nseed=271828183\ncutoff=60\nhorizon=120\n";
const groups=[{assigned:0,unknown:0,missing:0,available:[0,0,0,0],rows:[]},{assigned:0,unknown:0,missing:0,available:[0,0,0,0],rows:[]}];
const scores=rows=>Array.from({length:4},(_,m)=>{const r=rows.filter(x=>x[0]===m&&x[1]<=60&&x[2]<=60&&x[3]!==null).sort((a,b)=>a[1]-b[1]);return m===0?r.length>=2?r.at(-1)[3]-r[0][3]:null:r.length?r.at(-1)[3]:null;});
let perturbations=0;
for(let id=0;id<600;id++){
const a=next(11)-5,b=next(11)-5,c=next(11)-5,g=next(11)-5,start=40+next(11)-5,end=start+a+next(7)-3,image=b+next(7)-3,symptom=c+next(7)-3,genetic=g+next(7)-3;
const ms=next(20)===0,me=next(15)===0,mi=next(10)===0,mc=next(12)===0,mg=next(15)===0;
const td=next(12)===0?5:0,im=next(12)===0?10:0,sy=next(15)===0?7:0,ge=next(15)===0?65:0;
const ev=next(100)<Math.max(5,Math.min(95,45+3*a+3*b+3*c+3*g)),when=75+next(46),followup=next(12)===0?90:120,event=ev&&when<=followup?when:null;
const rows=[[0,0,0,ms?null:start],[0,60,60+td,me?null:end],[1,55,55+im,mi?null:image],[2,60,60+sy,mc?null:symptom],[3,0,ge,mg?null:genetic],...Array.from({length:4},(_,m)=>[m,100,100,ev?999:-999])];
const hold=id%3===0,group=groups[hold?1:0];group.assigned++;
canonical+=`${id}|${hold?"holdout":"development"}|${followup}|${event??"none"}\n`;for(const r of rows)canonical+=r.map(x=>x??"missing").join("|")+"\n";
const v=scores(rows);const changed=rows.map(r=>[r[0],r[1],r[2],r[1]>60||r[2]>60?99999:r[3]]);
if(JSON.stringify(v)!==JSON.stringify(scores(changed)))throw Error("leakage");perturbations++;
const y=event!==null?true:followup<120?null:false;if(y===null){group.unknown++;continue;}
v.forEach((x,i)=>{if(x!==null)group.available[i]++;});
if(v.some(x=>x===null)){group.missing++;continue;}group.rows.push({id,y,v:[...v,v.reduce((a,b)=>a+b,0)]});
}
const metrics=(rows,m,t)=>{let TP=0,FP=0,TN=0,FN=0;for(const r of rows){let pos=t!==null&&r.v[m]>=t;if(r.y){if(pos)TP++;else FN++;}else{if(pos)FP++;else TN++;}}return {TP,FP,TN,FN};};
const [tr,te]=groups.map(g=>g.rows);
if(tr.some(x=>te.some(y=>x.id===y.id)))throw Error("overlap");
for(const g of groups)if(g.rows.length+g.unknown+g.missing!==g.assigned)throw Error("denominator");
const p=tr.filter(r=>r.y).length,n=tr.length-p;if(!p||!n)throw Error("classes");
const wilson=(k,n)=>{if(!n)return null;let z=1.959963984540054,p=k/n,den=1+z*z/n,c=(p+z*z/(2*n))/den,h=z*Math.sqrt(p*(1-p)/n+z*z/(4*n*n))/den;return [Math.max(0,c-h),Math.min(1,c+h)];};
const rates=m=>{let {TP,FP,TN,FN}=m;let sens=TP/(TP+FN),spec=TN/(TN+FP);return {...m,n:TP+FP+TN+FN,sensitivity:sens,specificity:spec,balanced_accuracy:(sens+spec)/2,sensitivity_wilson95:wilson(TP,TP+FN),specificity_wilson95:wilson(TN,TN+FP)};};
const models=["Temporal","ImageProxy","SymptomProxy","GeneticProxy","Fusion"].map((modality,m)=>{let best=-1,t=-33;for(let x=-33;x<=33;x++){let q=metrics(tr,m,x),u=q.TP*n+q.TN*p;if(u>=best){best=u;t=x;}}return {modality,threshold:t,development:rates(metrics(tr,m,t)),holdout:rates(metrics(te,m,t))};});
return {scope:"synthetic only; no clinical validation",protocol:"synthetic-multimodal-v1",perturbations,development:{...groups[0],rows:undefined,eligible:tr.length},holdout:{...groups[1],rows:undefined,eligible:te.length},models,baseline:rates(metrics(te,0,null)),canonical_length:canonical.length};
}
const r=run();if(JSON.stringify(r)!==JSON.stringify(run())||r.perturbations!==600||r.development.assigned!==400||r.holdout.assigned!==200)throw Error('Fixture failure');console.log(JSON.stringify(r,null,2));
