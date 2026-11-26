"use strict";
function verify(){
const v=[2,2,3,1,2,4,7,8,6,3,2,1,2,3,2,3,4,8,9,7,4,3,2,1,2,2,3,4,7,8];
const scenarios=["Regular","DifficultDaysOnly","Gap","LateArrival","Duplicate","MissingValue","StableOnly","RegularWithExtras","InvalidValue"];
function evaluate(s,cutoff){
const records=[];
for(let d=1;d<=30;d++){
 let x=v[d-1];
 if(s==="DifficultDaysOnly"){if(x>=7)records.push([d,d,false,x]);continue;}
 if(s==="Gap"&&d>=11&&d<=16)continue;
 records.push([d,s==="LateArrival"&&d===24?31:d,true,s==="MissingValue"&&d===15?null:s==="StableOnly"?0:s==="InvalidValue"&&d===12?11:x]);
 if(s==="Duplicate"&&d===12)records.push([d,d,true,x]);
 if(s==="RegularWithExtras"&&x>=7)records.push([d,d,false,x]);
}
const visible=records.filter(e=>e[0]<=cutoff&&e[1]<=cutoff),expected=Math.min(cutoff,30);
let observed=0,gap=0,longestGap=0,low=0,high=0,sum=0,duplicates=0;
const invalid=visible.filter(e=>e[0]<1||e[0]>30||e[1]<e[0]||e[3]!==null&&(e[3]<0||e[3]>10)).length;
for(let d=1;d<=expected;d++){
 let group=visible.filter(e=>e[2]&&e[0]===d);duplicates+=Math.max(0,group.length-1);
 if(group.length===1&&group[0][3]!==null&&group[0][3]>=0&&group[0][3]<=10){let x=group[0][3];observed++;sum+=x;x>=7?high++:low++;gap=0;}
 else{gap++;longestGap=Math.max(gap,longestGap);}
}
const extras=visible.filter(e=>!e[2]).length;
return {gate:invalid||duplicates?"InvalidData":cutoff<30?"AwaitingWindow":observed!==30?"InsufficientCoverage":"ReadyForReview",expected,observed,missing:expected-observed,extras,longestGap,low,high,duplicates,invalid,meanTimes100:observed?Math.floor((sum*100+Math.floor(observed/2))/observed):null,warning:extras>0||observed<expected||low===0||high===0};
}
const cases=scenarios.map(s=>({scenario:s,...evaluate(s,30)}));
const expected=[["ReadyForReview",30,0,0],["InsufficientCoverage",0,7,30],["InsufficientCoverage",24,0,6],["InsufficientCoverage",29,0,1],["InvalidData",29,0,1],["InsufficientCoverage",29,0,1],["ReadyForReview",30,0,0],["ReadyForReview",30,7,0],["InvalidData",29,0,1]];
cases.forEach((r,i)=>{if(JSON.stringify([r.gate,r.observed,r.extras,r.longestGap])!==JSON.stringify(expected[i]))throw Error("Reference case "+i);});
let checks=0;for(const s of scenarios)for(let d=1;d<=31;d++){const r=evaluate(s,d);if(r.observed+r.missing!==r.expected||r.low+r.high!==r.observed||r.gate==="ReadyForReview"&&(d<30||r.observed!==30||r.duplicates||r.invalid))throw Error("Invariant");checks++;}
if(evaluate("LateArrival",31).gate!=="ReadyForReview"||evaluate("StableOnly",30).meanTimes100!==0||evaluate("DifficultDaysOnly",30).meanTimes100!==null||evaluate("Regular",29).gate!=="AwaitingWindow")throw Error("Boundary");
return {scope:"fictional daily protocol only",scenario_checks:cases.length,cutoff_checks:checks,boundary_checks:4,late_day31:evaluate("LateArrival",31),cases};
}
console.log(JSON.stringify(verify(),null,2));
