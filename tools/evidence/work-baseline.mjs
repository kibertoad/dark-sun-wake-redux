#!/usr/bin/env node
// Compares measured analysis snapshots with existing inventories; addresses/counts only.
import {readFileSync,writeFileSync,mkdirSync,cpSync,existsSync,readdirSync,rmSync} from 'node:fs';
import {resolve,join,dirname} from 'node:path';
import {spawnSync} from 'node:child_process';
import {fileURLToPath} from 'node:url';
import {parseOptions} from '../../node_modules/@scientific-method/standard-checker/dist/options.js';
import {loadSpec} from '../../node_modules/@scientific-method/standard-checker/dist/load/spec.js';
import {codeLocations} from '../../node_modules/@scientific-method/standard-checker/dist/inventory.js';
const root=resolve(import.meta.dirname,'../..');
const text=p=>readFileSync(p,'utf8').replace(/^\uFEFF/,'');
function tsv(p){const lines=text(p).trimEnd().split(/\r?\n/),cols=lines.shift().split('\t');return lines.map(l=>Object.fromEntries(l.split('\t').map((v,i)=>[cols[i],v])));}
export function union(ranges){const sorted=ranges.map(r=>[...r]).sort((a,b)=>a[0]-b[0]),out=[];for(const [s,e]of sorted){if(!Number.isSafeInteger(s)||!Number.isSafeInteger(e)||e<=s)throw Error('Invalid body range');if(out.length&&s<=out.at(-1)[1])out.at(-1)[1]=Math.max(e,out.at(-1)[1]);else out.push([s,e]);}return out;}
export const bytes=r=>union(r).reduce((n,[s,e])=>n+e-s,0);
export function validateRegion(r){
 for(const k of Object.keys(r).filter(k=>k!=='start'))if(!/^\d+$/.test(String(r[k]))||!Number.isSafeInteger(Number(r[k])))throw Error('Invalid audit count');
 if(Number(r.instructions)+Number(r.data)+Number(r.undefined)!==Number(r.size))throw Error('Invalid region partition');
 for(const k of ['instructions','data','undefined'])if(Number(r[k+'_outside'])>Number(r[k]))throw Error('Outside count exceeds total');
 if(Number(r.body_bytes)>Number(r.size))throw Error('Body count exceeds region');
}
export function inventoryDelta(prior,current){return {added:[...current.keys()].filter(k=>!prior.has(k)).length,removedOrMerged:[...prior.keys()].filter(k=>!current.has(k)).length,changedBodySizes:[...current].filter(([k,f])=>prior.has(k)&&prior.get(k)!==f.size).length};}
export function missingExecutables(buildFiles,measured,excluded){
 const excludedPaths=new Set(excluded.map(x=>x.file)),missing=[];
 for(const[build,files]of buildFiles)for(const f of files){
  if(excludedPaths.has(f.path)||measured.has(f.path)||!['MZ','COM','NE','PE','LE','LX','ELF'].includes(f.unpacked?.format??f.format))continue;
  const alias=files.find(a=>!excludedPaths.has(a.path)&&measured.has(a.path)&&a.xxh3===f.xxh3&&a.size===f.size);
  missing.push({build,file:f.path,byteIdenticalMeasuredAlias:alias?.path??null});
 }
 return missing;
}
function linear(s){if(s.includes(':')){const[a,b]=s.split(':').map(x=>parseInt(x,16));return a*16+b;}return parseInt(s,16);}
const pair=n=>`${Math.floor(n/16).toString(16).toUpperCase().padStart(4,'0')}:${(n%16).toString(16).toUpperCase().padStart(4,'0')}`;
const hex=n=>'0x'+n.toString(16).toUpperCase().padStart(8,'0');
function main(directory){
 const base=resolve(directory),allViews=JSON.parse(text(join(base,'views.json'))),reportRoot=join(base,'measurement-root');
 const scope=JSON.parse(text(join(root,'tools/evidence/research-scope.json')));
 const views=allViews.filter(v=>!scope.excludedExecutables.some(x=>x.file===v.manifest));
 mkdirSync(reportRoot,{recursive:true});cpSync(join(root,'spec'),join(reportRoot,'spec'),{recursive:true});
 for(const excluded of scope.excludedExecutables){
  if(!/^[A-Za-z0-9_.-]+(?:\/[A-Za-z0-9_.-]+)*$/.test(excluded.file)||excluded.file.split('/').includes('..'))throw Error('Unsafe excluded manifest');
  rmSync(join(reportRoot,'coverage/BLD-GOG-EN-1.1',excluded.file+'.tsv'),{force:true});
 }
 const problems=[];const spec=loadSpec({config:parseOptions(['--root',root]),problem:(...args)=>problems.push(args)});
 if(problems.length)throw Error(`Spec load has ${problems.length} problems`);
 const byFile=new Map(),audits=[];
 for(const view of views){
  for(const suffix of ['.tsv','.provenance.tsv','.regions.tsv'])if(text(view.stem+suffix)!==text(view.stem.replace(/-a$/,'-b')+suffix))throw Error(`Repeat differs: ${view.key}`);
  const rows=tsv(view.stem+'.tsv'),prov=Object.fromEntries(tsv(view.stem+'.provenance.tsv').map(r=>[r.key,r.value]));
  const regions=tsv(view.stem+'.regions.tsv');
  for(const r of regions)validateRegion(r);
  let source=view.source;if(view.key.endsWith('-overlay'))source=views.find(v=>v.key===view.key.replace('-overlay','-resident')).source;
  const raw=readFileSync(source),header=raw[0]===77&&raw[1]===90?raw.readUInt16LE(8)*16:0;
  const overlay=view.key.endsWith('-overlay');
  const canonical=a=>overlay?hex(linear(a)-65536+header):(a.includes(':')?a.toUpperCase():hex(linear(a)));
  const numeric=a=>overlay?linear(a)-65536+header:linear(a);
  const funcs=rows.map(r=>{const ranges=r.ranges.split(' ').map(x=>x.split('..'));const body=ranges.map(([a,b])=>[numeric(a),numeric(b)]);if(bytes(body)!==Number(r.size))throw Error(`Body size mismatch ${view.key} ${r.start}`);return{start:canonical(r.start),size:Number(r.size),space:overlay?'offset':'address',physicalBody:body.map(([a,b])=>header&&!overlay?[a-65536+header,b-65536+header]:[a,b]),ranges:ranges.map(([a,b])=>canonical(a)+'..'+canonical(b)).join(' '),body};});
  if(!byFile.has(view.manifest))byFile.set(view.manifest,[]);byFile.get(view.manifest).push(...funcs);
  const totals={};for(const k of Object.keys(regions[0]).filter(k=>k!=='start'))totals[k]=regions.reduce((n,r)=>n+Number(r[k]),0);
  if(bytes(funcs.flatMap(f=>f.body))!==Number(prov.unique_body_bytes))throw Error(`Union differs ${view.key}`);
  audits.push({file:view.manifest,view:view.key,functions:funcs.length,sourceXxh3:prov.xxh3,snapshot:prov.snapshot,regions:regions.length,...totals,bodyBytesOutsideRegions:Number(prov.body_bytes_outside_regions),entriesWithoutInstruction:Number(prov.entries_without_instruction)});
 }
 const changes=[];
 for(const[file,funcs]of byFile){
  const relative=file.replace(/^CD:/,'@CD/');const dst=join(reportRoot,'coverage/BLD-GOG-EN-1.1',relative+'.tsv');mkdirSync(dirname(dst),{recursive:true});
  const starts=new Set();for(const f of funcs){if(starts.has(f.start))throw Error(`Duplicate function ${file} ${f.start}`);starts.add(f.start);}
  writeFileSync(dst,'start\tsize\tranges\n'+funcs.map(f=>`${f.start}\t${f.size}\t${f.ranges}\n`).join(''));
  const oldPath=join(root,'coverage/BLD-GOG-EN-1.1',file.replace(/^CD:/,'CD/')+'.tsv'),old=tsv(oldPath);
  const resident=views.find(v=>v.manifest===file&&!v.key.endsWith('-overlay'));const raw=readFileSync(resident.source),header=raw[0]===77&&raw[1]===90?raw.readUInt16LE(8)*16:0;
  const oldKey=s=>{if(s.includes('+')){const off=parseInt(s.split('+')[1],16);const mapped=views.find(v=>v.manifest===file&&v.key.endsWith('-overlay'));const mzSize=(raw.readUInt16LE(4)-1)*512+(raw.readUInt16LE(2)||512);return off<mzSize?pair(65536+off-header):hex(off);}return s.includes(':')?pair(linear(s)):hex(linear(s));};
  const newKey=f=>f.start.includes(':')?pair(linear(f.start)):f.start;
  const prior=new Map(old.map(f=>[oldKey(f.start),Number(f.size)])),current=new Map(funcs.map(f=>[newKey(f),f]));
  changes.push({file,previousFunctions:old.length,currentFunctions:funcs.length,...inventoryDelta(prior,current),previousSummedBodyBytes:old.reduce((n,f)=>n+Number(f.size),0),currentUniqueBodyBytes:bytes(funcs.flatMap(f=>f.physicalBody))});
 }
 const run=spawnSync(process.execPath,[join(root,'node_modules/@scientific-method/standard-checker/dist/coverage.js'),'--root',reportRoot,'--json','--list'],{encoding:'utf8',maxBuffer:32*1024*1024});
 const standardCoverage=JSON.parse(run.stdout);
 const citations=new Map();let unparsedLocations=0;
 for(const e of spec.entries.values())if(e.meta.status!=='superseded')for(const{loc,range}of codeLocations(e.meta,spec.buildFiles)){
  if(!range){unparsedLocations++;continue;}const key=loc.file+'\\0'+range.space;if(!citations.has(key))citations.set(key,[]);citations.get(key).push({start:Number(range.start),end:Number(range.end),id:e.meta.id});
 }
 const inventories=[];
 for(const[file,funcs]of byFile){
  const list=funcs.map(f=>({...f,citedBy:[...new Set((citations.get(file+'\\0'+f.space)??[]).filter(r=>f.body.some(([a,b])=>r.start<b&&a<r.end)).map(r=>r.id))]}));
  const cited=list.filter(f=>f.citedBy.length);inventories.push({file,functions:list.length,cited:cited.length,bytes:bytes(list.flatMap(f=>f.physicalBody)),citedBytes:bytes(cited.flatMap(f=>f.physicalBody)),uncited:list.filter(f=>!f.citedBy.length).map(f=>({start:f.start,size:f.size})),list});
 }
 const coverage={inventories,standardReporterProblems:standardCoverage.problems,standardReporterUnread:standardCoverage.unread,unparsedLocations};
 if(unparsedLocations)throw Error('Unparsed spec code locations: '+unparsedLocations);
 writeFileSync(join(base,'citation-coverage.json'),JSON.stringify(coverage,null,2));
 const entriesByStatus={};for(const e of spec.entries.values())if(e.meta.status)entriesByStatus[e.meta.status]=(entriesByStatus[e.meta.status]??0)+1;
 const dataFiles=[];for(const[build,files]of spec.buildFiles)for(const f of files.filter(f=>f.format==='data')){
  const ids=[...spec.entries.values()].filter(e=>e.meta.id.startsWith('FMT-')&&['supported','established'].includes(e.meta.status)&&(e.meta.builds??[]).includes(build)&&(e.meta.files??[]).some(pattern=>{const re='^'+String(pattern).replace(/[.+?^${}()|[\]\\]/g,'\\$&').replace(/\*/g,'.*')+'$';return new RegExp(re).test(f.path);})).map(e=>e.meta.id);
  dataFiles.push({build,file:f.path,supportedFormats:ids});
 }
 const queue={};for(const file of readdirSync(join(root,'queue')).filter(x=>x.endsWith('.md')&&x!=='README.md')){let kind='';for(const line of text(join(root,'queue',file)).split(/\r?\n/)){if(line.startsWith('## '))kind=line.slice(3);if(/^- Q-[A-Z]+-\d+\./.test(line))queue[kind]=(queue[kind]??0)+1;}}
 const missingCodeFiles=missingExecutables(spec.buildFiles,byFile,scope.excludedExecutables);
 const citationAreas={};for(const f of coverage.inventories)for(const fn of f.list){const areas=new Set(fn.citedBy.map(id=>id.split('-')[1]));if(!areas.size)areas.add('unassigned');for(const area of areas){citationAreas[area]??={functions:0,files:{}};citationAreas[area].functions++;citationAreas[area].files[f.file]??=[];citationAreas[area].files[f.file].push(...fn.physicalBody);}}
 for(const r of Object.values(citationAreas)){r.uniqueBytes=Object.values(r.files).reduce((n,ranges)=>n+bytes(ranges),0);delete r.files;}
 const parityText=text(join(root,'PARITY.md'));const parity={status:{},code:{}};let section='status';for(const line of parityText.split(/\r?\n/)){if(line==='| Code | Rows |')section='code';if(line==='## Areas')break;const m=/^\| ([a-z]+) \| (\d+) \|$/.exec(line);if(m)parity[section][m[1]]=Number(m[2]);}
 const report={citationAreas,areaAssignmentNote:'Areas group spec citations, not proved function roles; a function may appear in several areas. Do not sum these counts.',missingCodeFiles,externalLibraryScope:'Imported library implementations and generated runtime code are not part of these shipped-image inventories; their behavioural dependencies remain conditional.',parity,standardReporterAnomalies:coverage.standardReporterProblems,generatedAt:new Date().toISOString(),revision:spawnSync('git',['rev-parse','HEAD'],{cwd:root,encoding:'utf8'}).stdout.trim(),audit:audits,inventoryChanges:changes,citationCoverage:coverage.inventories.map(({list,uncited,...r})=>({...r,uncitedFunctions:uncited.length})),entriesByStatus,dataFormatCoverage:{files:dataFiles.length,supported:dataFiles.filter(f=>f.supportedFormats.length).length},queue,openReports:readdirSync(join(root,'docs/reports')).filter(x=>/^R-\d+\.md$/.test(x)).length,completeReadingCount:[...spec.entries.values()].some(e=>Array.isArray(e.meta.complete_reading)&&e.meta.complete_reading.length)?null:0,limitations:['Function discovery remains provisional; unassigned decoded and undefined bytes are reported separately.','These are research measurements, not a gameplay completion percentage.','No entry supplies complete_reading evidence; the formally qualifying complete-reading count is zero, not a substitute count of recorded findings.','Fresh snapshot discovery and boundary changes are separate from new research evidence.','All discovered definitions remain in the full range-aware comparison; bodies outside declared overlay code ranges are explicit anomalies, not silently omitted.']};
 writeFileSync(join(base,'work-comparison.json'),JSON.stringify(report,null,2));console.log(JSON.stringify(report,null,2));
}
if(process.argv[1] && resolve(process.argv[1])===fileURLToPath(import.meta.url))main(process.argv[2]);
