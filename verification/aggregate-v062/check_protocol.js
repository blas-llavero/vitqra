"use strict";
function verify(){
const groups=[["A",60,36,18,30],["B",60,34,10,33],["C",60,39,2,36],["D",12,7,5,6]];
const publish=([group,n,...counts])=>n>=20&&counts.every(c=>c>=5&&c<=n-5)?{group,state:"Visible",denominator:n,cells:counts}:{group,state:"Withheld",denominator:null,cells:[]};
const open=(a,r,p,d)=>r===4&&((a===4&&p===1&&d>=5&&d<65)||(a===5&&p===2&&d>=15&&d<60))?groups.map(publish):null;
let checked=0,allowed=0,denied=0;
for(let a=0;a<12;a++)for(let r=0;r<13;r++)for(let p=0;p<6;p++)for(let d=0;d<=180;d++){
 const out=open(a,r,p,d);
 const expected=r===4&&((a===4&&p===1&&d>=5&&d<65)||(a===5&&p===2&&d>=15&&d<60));
 if(!!out!==expected)throw Error("Authorization mismatch");
 if(out){if(out.filter(x=>x.state==="Withheld").some(x=>x.denominator!==null||x.cells.length!==0))throw Error("Suppression");allowed++;}else denied++;
 checked++;
}
const boundary=[[19,9,9,9],[20,4,10,10],[20,16,10,10],[20,5,15,10],[20,-1,10,10],[20,21,10,10],[20,0,10,10],[20,20,10,10]];
for(let i=0;i<boundary.length;i++){let r=publish(["X",...boundary[i]]);if((r.state==="Visible")!==(i===3)||i!==3&&(r.denominator!==null||r.cells.length))throw Error("Boundary");}
const rows=open(5,4,2,25);
if(JSON.stringify(rows)!==JSON.stringify(open(4,4,1,25)))throw Error("Inconsistent release");
return {scope:"fictional aggregate only",checked,allowed,denied,boundary_checks:boundary.length,visible_groups:rows.filter(r=>r.state==="Visible").length,withheld_groups:rows.filter(r=>r.state==="Withheld").length,rows};
}
console.log(JSON.stringify(verify(),null,2));
