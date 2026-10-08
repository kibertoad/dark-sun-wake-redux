import {readFileSync,writeFileSync,mkdirSync} from 'node:fs';
import {resolve} from 'node:path';
import assert from 'node:assert/strict';
import {run,sourceXxh3} from '../../../node_modules/@scientific-method/executable-reader/dist/src/report.js';
import {readMz} from '../../../node_modules/@scientific-method/executable-reader/dist/src/legacy-image.js';
const GAME_DIR=process.env.GAME_DIR;if(!GAME_DIR)throw new Error('Set GAME_DIR to the supported Dark Sun build.');
process.env.EVIDENCE_PYTHON=resolve('artifacts/evidence-python/Scripts/python.exe');
const source=GAME_DIR+'/DSUN.EXE',bytes=readFileSync(source),image=readMz(bytes),dir=GAME_DIR+'/analysis/reporter-audit/issue5-writers91';mkdirSync(dir,{recursive:true});
const rows=readFileSync('coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv','utf8').trim().split(/\r?\n/).slice(1).map(line=>{const [a,s]=line.split('\t');return {start:Number(a.split('+')[1]),size:Number(s)}});
const regions=[],excluded=[];
for(const {start,size} of rows){const end=start+size,container=image.ranges.find(r=>start>=r.start&&end<=r.end);if(!container){excluded.push({start,size,reason:'outside one source container'});continue;}let segment,ip;if(container.view==='resident'){segment=image.loadSegment+Math.floor((start-image.header)/16);ip=(start-image.header)%16;}else{const o=image.overlays.find(o=>o.start===container.start);segment=image.loadSegment+image.descriptors[o.descriptor].segment;ip=start-o.start;}if(ip+size>65536){excluded.push({start,size,reason:'analysis instruction boundary'});continue;}regions.push({name:`inventory-${start}`,start,end,segment,ip,entries:[start],evidence:'committed function inventory entry/bounds; MZ resident or source descriptor analysis view only'});}
const merged=[];for(const r of regions){const last=merged.at(-1);if(last&&r.start<last.end){assert(image.ranges.some(v=>last.start>=v.start&&Math.max(last.end,r.end)<=v.end));last.end=Math.max(last.end,r.end);last.entries.push(r.start);last.evidence+='; overlapping inventory entries preserved';}else merged.push({...r});}regions.splice(0,regions.length,...merged);
const summaries=[];
for(const field of [0xa119,0xa11b]) for(let i=0;i<regions.length;i+=128){const batch=regions.slice(i,i+128),cfg={source,sourceKind:'mz',xxh3:sourceXxh3(bytes),regions:batch,query:{offset:field}};const name=`field-${field}-batch-${i/128}`,file=`${dir}/${name}.json`;writeFileSync(file,JSON.stringify(cfg));const r=run(['operand-candidates',file]);writeFileSync(`${dir}/${name}.report.json`,JSON.stringify(r));summaries.push({field,batch:i/128,partial:r.partialSearch,truncated:r.truncated,counts:r.counts,gaps:r.gaps,candidates:r.candidates.filter(c=>c.countedAsUse).map(c=>({site:c.site,region:c.region,access:c.access,width:c.width,segmentRegister:c.effectiveSegmentRegister}))});console.log(JSON.stringify(summaries.at(-1)));}
writeFileSync(`${dir}/summary.json`,JSON.stringify({excluded,summaries}));assert(summaries.some(s=>s.candidates.some(c=>c.access.includes('read'))));
console.log(JSON.stringify({declaredFunctions:regions.length,excluded:excluded.length,writerLeads:summaries.flatMap(s=>s.candidates.filter(c=>c.access.includes('write')).map(c=>({field:s.field,...c})))}));
