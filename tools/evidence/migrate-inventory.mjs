#!/usr/bin/env node
// Publish only unchanged, anomaly-free repeated baseline exports. Overlay repairs are separate.
import {readFileSync, writeFileSync, mkdirSync} from 'node:fs';
import {resolve, join, dirname} from 'node:path';
import {fileURLToPath} from 'node:url';
import {createHash} from 'node:crypto';
import {sourceXxh3} from '@scientific-method/executable-reader';
import {union, bytes, validateRegion} from './work-baseline.mjs';
const text=p=>readFileSync(p,'utf8').replace(/^\uFEFF/,'');
function rows(s){const lines=s.trimEnd().split(/\r?\n/),columns=lines.shift().split('\t');
 return {columns,rows:lines.map(l=>{const values=l.split('\t');if(values.length!==columns.length)throw Error('Invalid TSV width');return Object.fromEntries(columns.map((c,i)=>[c,values[i]]));})};}
const canonical=s=>{if(/^[0-9a-f]{4}:[0-9a-f]{4}$/i.test(s))return s.toUpperCase();if(/^(?:0x)?[0-9a-f]{8}$/i.test(s))return '0x'+s.replace(/^0x/,'').toUpperCase();throw Error('Unsupported address notation');};
const numeric=s=>s.includes(':')?parseInt(s.slice(0,4),16)*16+parseInt(s.slice(5),16):Number(s);
export function unchangedInventory(previous, current, provenance, regions){
 const old=rows(previous),fresh=rows(current),p=Object.fromEntries(rows(provenance).rows.map(r=>[r.key,r.value])),audit=rows(regions).rows;
 if(old.columns.join('\t')!=='start\tsize')throw Error('Annotations require explicit reconciliation');
 if(fresh.columns.join('\t')!=='start\tsize\tranges')throw Error('Expected complete body ranges');
 if(Number(p.body_bytes_outside_regions)!==0||Number(p.entries_without_instruction)!==0)throw Error('Snapshot has unresolved body or entry anomalies');
 audit.forEach(validateRegion);
 const allowed=union(audit.map(r=>[numeric(canonical(r.start)),numeric(canonical(r.start))+Number(r.size)]));
 const prior=new Map(old.rows.map(r=>[numeric(canonical(r.start)),Number(r.size)]));
 if(prior.size!==old.rows.length||fresh.rows.length!==prior.size||fresh.rows.length!==Number(p.functions))throw Error('Function census differs');
 const seen=new Set(),all=[];let last=-1,summed=0;
 const output=fresh.rows.map(r=>{const start=canonical(r.start),at=numeric(start),size=Number(r.size);
  if(!/^[1-9][0-9]*$/.test(r.size)||!Number.isSafeInteger(size)||at<=last||seen.has(at)||prior.get(at)!==size)throw Error('Function starts or sizes changed');
  seen.add(at);last=at;
  const ranges=r.ranges.split(' ').map(part=>{const pair=part.split('..');if(pair.length!==2)throw Error('Invalid body range');return pair.map(canonical);});
  const body=ranges.map(([a,b])=>[numeric(a),numeric(b)]);
  if(bytes(body)!==size||body.reduce((n,[a,b])=>n+b-a,0)!==size||!body.some(([a,b])=>at>=a&&at<b)||body.some(([a,b])=>!allowed.some(([c,d])=>a>=c&&b<=d)))throw Error('Invalid or out-of-region body');
  all.push(...body);summed+=size;return `${start}\t${size}\t${ranges.map(p=>p.join('..')).join(' ')}\n`;
 });
 if(summed!==Number(p.summed_body_bytes)||bytes(all)!==Number(p.unique_body_bytes))throw Error('Body totals disagree');
 return 'start\tsize\tranges\n'+output.join('');
}
function main(){
 const [config,output,...manifests]=process.argv.slice(2);if(!config||!output||!manifests.length)throw Error('Supply views.json, staging directory and manifest paths');
 const root=resolve(import.meta.dirname,'../..'),views=JSON.parse(text(config));
 const scope=JSON.parse(text(join(root,'tools/evidence/research-scope.json')));
 const revision=createHash('sha256').update(readFileSync(join(root,'tools/ghidra/ExportResearchBaseline.java'))).digest('hex');
 const staged=[];
 for(const manifest of manifests){
  if(scope.excludedExecutables.some(x=>x.file===manifest))throw Error('Executable excluded from game research scope');
  if(!/^[A-Za-z0-9_.-]+(?:\/[A-Za-z0-9_.-]+)*$/.test(manifest)||manifest.split('/').includes('..'))throw Error('Unsafe manifest path');
  const candidates=views.filter(v=>v.manifest===manifest);if(candidates.length!==1)throw Error('Multi-view inventories require mapping reconciliation');
  const v=candidates[0],raw=readFileSync(v.source),p=Object.fromEntries(rows(text(v.stem+'.provenance.tsv')).rows.map(r=>[r.key,r.value]));
  if(sourceXxh3(raw)!==v.xxh3||createHash('md5').update(raw).digest('hex')!==v.md5||p.xxh3!==v.xxh3||p.source_md5!==v.md5||p.exporter_revision!==revision)throw Error('Source or exporter identity differs');
  for(const suffix of ['.tsv','.provenance.tsv','.regions.tsv'])if(text(v.stem+suffix)!==text(v.stem.replace(/-a$/,'-b')+suffix))throw Error('Repeated exports differ');
  const path='coverage/BLD-GOG-EN-1.1/'+manifest+'.tsv';
  const tsv=unchangedInventory(text(join(root,path)),text(v.stem+'.tsv'),text(v.stem+'.provenance.tsv'),text(v.stem+'.regions.tsv'));
  staged.push({path,tsv,stem:v.stem});
 }
 // Validate every selected file before writing any; exclusive writes never replace an inventory.
 for(const item of staged){const dst=join(resolve(output),item.path);mkdirSync(dirname(dst),{recursive:true});writeFileSync(dst,item.tsv,{flag:'wx'});
  for(const suffix of ['.provenance.tsv','.regions.tsv'])writeFileSync(dst.slice(0,-4)+suffix,text(item.stem+suffix),{flag:'wx'});
 }
 console.log(JSON.stringify({staged:staged.map(x=>x.path),limitation:'Unchanged anomaly-free single views only; original inventory remains untouched.'}));
}
if(process.argv[1]&&resolve(process.argv[1])===fileURLToPath(import.meta.url))main();
