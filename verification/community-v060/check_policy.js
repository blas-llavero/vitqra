"use strict";
function verify(grants,days){
const ceiling=(a,r,p,o)=>{
const patient=a===0||a===1,clinician=a===2||a===3;
if(r===12)return p===5&&o===0&&(patient||a===7);
if(r<2)return p===0&&((patient&&(o===0||o===1))||(clinician&&o===0));
if(r===10||r===11)return p===0&&o===5&&(patient||clinician);
if(r===2||r===3)return p===1&&a===4&&o===0;
if(r===4||r===5)return o===0&&((a===4&&p===1)||(a===5&&p===2));
if(r===6||r===7)return p===3&&a===6&&(o===0||o===2);
return p===4&&(((patient||a===7)&&[5,0,3].includes(o))||(a===8&&[0,4].includes(o)));
};
const map=new Map(grants.map(g=>[g.slice(0,4).join("|"),g.slice(4)]));if(map.size!==grants.length)throw Error("Duplicate grants");
let checked=0,allowed=0,denied=0;
for(let a=0;a<12;a++)for(let r=0;r<13;r++)for(let p=0;p<6;p++)for(let o=0;o<10;o++)for(const d of days){
const g=map.get([a,r,p,o].join("|")),expected=!!g&&d>=g[0]&&d<g[1]&&d<g[2];
const actual=a<9&&ceiling(a,r,p,o)&&!!g&&d>=g[0]&&d<g[1]&&d<g[2];
if(actual!==expected||actual&&(o>=6||a>=9))throw Error("Unexpected decision");
checked++;actual?allowed++:denied++;
}
return {scope:"fictional policy simulation only",actors:12,resources:13,purposes:6,actions:10,days,explicit_grants:grants.length,checked,allowed,denied};
}
console.log(JSON.stringify(verify([[0,0,0,0,0,90,181],[0,0,0,1,0,90,181],[1,1,0,0,0,90,181],[1,1,0,1,0,90,181],[2,0,0,0,10,70,50],[3,1,0,0,10,70,181],[4,2,1,0,5,65,181],[4,4,1,0,5,65,181],[5,4,2,0,15,60,181],[6,6,3,0,20,80,181],[6,6,3,2,20,80,181],[0,8,4,5,0,90,181],[0,8,4,0,0,90,181],[0,8,4,3,0,90,181],[7,8,4,5,0,90,181],[7,8,4,0,0,90,181],[7,8,4,3,0,90,181],[8,8,4,0,0,90,181],[8,8,4,4,0,90,181],[0,10,0,5,20,30,181],[2,10,0,5,20,30,181],[1,11,0,5,20,30,181],[3,11,0,5,20,30,181],[0,12,5,0,0,181,181],[1,12,5,0,0,181,181],[7,12,5,0,0,181,181]],[0,4,5,9,10,14,15,19,20,29,30,49,50,59,60,64,65,69,70,79,80,89,90,180]),null,2));
