"use strict";
function verify(){
const rows=[[1,5,5,0,20],[2,5,5,1,70],[3,10,10,2,2],[4,15,15,0,24],[5,15,18,1,null],[6,20,20,3,null],[7,20,20,4,null],[8,23,28,0,27],[9,24,24,1,66],[10,30,30,0,99]];
const reference=(a,r,t)=>((a===r&&t<90)||(a===r+2&&t>=10&&t<(r===0?50:70)));
function open(a,r,t,c){
if(r<0||r>1||t<0||t>180||c<0||c>t||c>=30||!reference(a,r,t))return null;
return rows.filter(x=>x[1]<=c&&x[2]<=c).map(x=>x.slice());
}
let checked=0,allowed=0,denied=0;
for(let a=0;a<12;a++)for(let r=0;r<2;r++)for(let t=0;t<=180;t++)for(let c=0;c<=30;c++){
const s=open(a,r,t,c);const expected=c<30&&c<=t&&reference(a,r,t);
if(!!s!==expected)throw Error("access mismatch");
if(s){if(s.some(x=>x[1]>c||x[2]>c||x[0]===10))throw Error("cutoff");allowed++;}else denied++;
checked++;
}
const s=open(2,0,25,25),later=open(2,0,29,29);
if(s.length!==8||later.length!==9||s.some(x=>x[0]===8)||open(2,0,50,25)!==null||open(2,1,25,25)!==null||open(3,1,70,25)!==null)throw Error("fixture");
return {scope:"fictional previsit only",checked,allowed,denied,day25:s.length,day29:later.length,explicit_missing:1,placeholders:2,changeA:4,changeB:-4};
}
console.log(JSON.stringify(verify(),null,2));
