import {readFileSync,writeFileSync} from 'node:fs';import {resolve} from 'node:path';
import {run} from '../../../node_modules/@scientific-method/executable-reader/dist/src/report.js';
const GAME_DIR=process.env.GAME_DIR;if(!GAME_DIR)throw new Error('Set GAME_DIR to the supported Dark Sun build.');
process.env.EVIDENCE_PYTHON=resolve('artifacts/evidence-python/Scripts/python.exe');
const b=GAME_DIR+'/analysis/reporter-audit',d=`${b}/issue5-transfer-release-callers101`;
const c=JSON.parse(readFileSync(`${d}/producer-request-return-order.json`));delete c.target;delete c.searchRegions;c.entry=91236;
const core=JSON.parse(readFileSync(`${b}/issue5-transfer-caller-graph101/actual.json`));
c.regions.push(...core.regions.filter(r=>['graphics-primitive','documented-release-service'].includes(r.name)),{name:'documented-slot-allocator',start:80088,end:80221,segment:0x1bf3,ip:0x27a8,entries:[80088],evidence:'FND-CONFIG-183 complete allocator; native admission remains unknown'});
const f=`${d}/connected-producer-graph.json`;writeFileSync(f,JSON.stringify(c));const r=run(['callees',f]);writeFileSync(`${d}/connected-producer-graph.report.json`,JSON.stringify(r));console.log(JSON.stringify({complete:r.completeWithinDeclaredGraph,nodes:r.nodes.map(n=>({entry:n.entry,complete:n.body.complete,usable:n.boundaryUsable})),unresolved:r.edges.filter(e=>e.classification==='unresolved').map(e=>({caller:e.caller,site:e.site,target:e.target,dependencies:e.dependencies})),unchecked:r.uncheckedEntries}));
