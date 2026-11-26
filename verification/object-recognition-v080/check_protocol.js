"use strict";
function verify(){
 function raster(k,r,dx,dy){return Array.from({length:576},(_,i)=>{let x=i%24-12-dx,y=Math.floor(i/24)-12-dy;return k===0?x*x+y*y<=r*r:k===1?Math.abs(x)<=r&&Math.abs(y)<=r:k===2?y>=-r&&y<=r&&Math.abs(x)<=Math.floor((y+r)/2):k===3?false:k===4?Math.abs(y)<=1&&Math.abs(x)<=9:k===5?x*x+y*y<=64&&x*x+y*y>=36:((x-6)**2+y*y<=9||(x+6)**2+y*y<=9);});}
 const shifts=[-3,-2,-1,0,1,2,3],templates=Array.from({length:3},(_,k)=>[5,6,7,8].flatMap(r=>shifts.flatMap(dx=>shifts.map(dy=>raster(k,r,dx,dy)))));
 function classify(a){const scores=templates.map(ts=>{let best=0;for(let t of ts){let i=0,u=0;for(let j=0;j<576;j++){i+=a[j]&&t[j]?1:0;u+=a[j]||t[j]?1:0;}best=Math.max(best,u?Math.floor(1000*i/u):0);}return best;});let order=[0,1,2].sort((a,b)=>scores[b]-scores[a]);return {scores,k:scores[order[0]]>=750&&scores[order[0]]-scores[order[1]]>=80?order[0]:3};}
 let out=[];for(let noise of [0,6,18,45]){let counts=[];for(let k=0;k<3;k++){let a=[0,0,0,0];for(let r of [5,6,7,8])for(let dx of [-2,1,2])for(let dy of [-2,1,2]){let v=raster(k,r,dx,dy),seed=(k+1)*987+r*33+dx*11+dy;for(let j=0;j<noise;j++){seed=(Math.imul(seed,1664525)+1013904223)>>>0;let p=seed%576;v[p]=!v[p];}a[classify(v).k]++;}counts.push(a);}let correct=counts.reduce((n,a,k)=>n+a[k],0),abstained=counts.reduce((n,a)=>n+a[3],0);out.push({flip_attempts:noise,counts,correct,abstained,wrong:108-correct-abstained});}
 let unknown=[3,4,5,6].map(k=>classify(raster(k,6,0,0)).k);
 if(out.some(o=>o.wrong!==0)||unknown.some(k=>k!==3)||out[3].abstained!==64)throw Error("Reference results");
 return {scope:"synthetic raster reference only",grid:[24,24],templates:588,minimum_iou_permille:750,minimum_margin_permille:80,known_cases:432,unknown_cases:4,unknown_rejected:unknown.filter(k=>k===3).length,noise_results:out,camera:false,network:false,clinical_release:false};
}
console.log(JSON.stringify(verify(),null,2));
