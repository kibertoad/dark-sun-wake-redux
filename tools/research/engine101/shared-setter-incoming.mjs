import {readFileSync,writeFileSync,mkdirSync} from 'node:fs';import {resolve} from 'node:path';
import {readMz,incomingCalls} from '../../../node_modules/@scientific-method/executable-reader/dist/src/legacy-image.js';
const bytes=readFileSync('C:/GOG Games/Dark Sun 2/DSUN.EXE'),im=readMz(bytes),d=resolve('UserContent/analysis/reporter-audit/shared-setter101');mkdirSync(d,{recursive:true});
const r=incomingCalls(im,0x16a75,{limit:256,controls:[0x2f04e]});writeFileSync(`${d}/incoming.json`,JSON.stringify(r));console.log(JSON.stringify(r));
