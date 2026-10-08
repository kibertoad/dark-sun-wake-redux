import {readFileSync,writeFileSync} from 'node:fs';
import {resolve} from 'node:path';
import assert from 'node:assert/strict';
import {run} from '../../../node_modules/@scientific-method/executable-reader/dist/src/report.js';
process.env.EVIDENCE_PYTHON=resolve('artifacts/evidence-python/Scripts/python.exe');
const dir='C:/GOG Games/Dark Sun 2/analysis/reporter-audit/issue5-writers91';
const summary=JSON.parse(readFileSync(`${dir}/summary.json`));const owner=summary.summaries.find(s=>s.field===0xa119&&s.candidates.some(c=>c.site===209427));assert(owner);
const base=JSON.parse(readFileSync(`${dir}/field-${owner.field}-batch-${owner.batch}.json`));
const query=(name,cfg)=>{const file=`${dir}/${name}.json`;writeFileSync(file,JSON.stringify(cfg));const r=run(['operand-candidates',file]);writeFileSync(`${dir}/${name}.report.json`,JSON.stringify(r));console.log(JSON.stringify({name,partial:r.partialSearch,truncated:r.truncated,counts:r.counts,controlSites:r.controls}));return r;};
const positive=query('actual-writer-control',{...base,controls:[209427]});assert(positive.candidates.some(c=>c.site===209427&&c.countedAsUse&&c.access.includes('write')&&c.width===4));
try{query('wrong-interior-site',{...base,controls:[209428]});throw Error('wrong candidate accepted')}catch(e){assert.match(e.message,/positive control/);console.log('Wrong interior candidate rejected');}
const cap=query('one-byte-search',{...base,scanLimit:1});assert(cap.partialSearch);assert(!cap.candidates.some(c=>c.site===209427));
const consumer=summary.summaries.find(s=>s.field===0xa119&&s.candidates.some(c=>c.site===210610));assert(consumer);const cc=JSON.parse(readFileSync(`${dir}/field-${consumer.field}-batch-${consumer.batch}.json`));query('known-callback-read-controls',{...cc,controls:[210610,210625,210657,210672]});
console.log('Callback writer lead and known consumer controls passed; semantic producer and reachability remain unclaimed');
