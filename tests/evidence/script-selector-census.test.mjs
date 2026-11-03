import {test} from 'node:test';
import assert from 'node:assert/strict';
import {createHash} from 'node:crypto';
import {mkdtempSync,writeFileSync,rmSync} from 'node:fs';
import {tmpdir} from 'node:os';
import {join,resolve,sep} from 'node:path';
import {fileURLToPath} from 'node:url';
import {spawnSync} from 'node:child_process';
import {enginePython} from '../../tools/tool-dependencies.mjs';

const tool=fileURLToPath(new URL('../../tools/research/exec-census/script_selector_census.py',import.meta.url));
const source=[
  'GPL/1 0000: 5E 32766, 65, 0, 0, 0',
  'GPL/1 000B: 5E 32766, var2[0], 0, 0, 0',
  'GPL/1 0016: 5E -5, 0, 0, 0, 0',
  'GPL/1 0021: 5E var9[44], 65536, 0, 0, 0',
  'GPL/1 002C: 5E ( {002F: 22 1, 2, 3, 4} ), -1, 0, 0, 0',
  'GPL/1 0037: 5E var9[38], var9[36], 0, 0, 0',
].join('\n')+'\n';
const baseline={total:6,sentinel:2,otherLiteral:1,nonliteral:3,sentinelLiteralSecond:1,sentinelNonliteralSecond:1,errors:0};
function run(t,input=source,configChanges={}) {
  const dir=mkdtempSync(join(tmpdir(),'dark-sun-selector-census-'));
  t.after(()=>{assert.ok(resolve(dir).startsWith(resolve(tmpdir())+sep));rmSync(dir,{recursive:true,force:true});});
  const bytes=Buffer.from(input), listing=join(dir,'synthetic.txt'), configPath=join(dir,'config.json');
  writeFileSync(listing,bytes);
  writeFileSync(configPath,JSON.stringify({listingSha256:createHash('sha256').update(bytes).digest('hex'),controls:baseline,...configChanges}));
  const result=spawnSync(enginePython(),['-I',tool,listing,configPath],{encoding:'utf8',maxBuffer:1024*1024,timeout:30000});
  if(result.error)throw result.error;
  return {...result,report:result.status===0?JSON.parse(result.stdout):null};
}
function good(t,input=source,config) {
  const result=run(t,input,config);
  assert.equal(result.status,0,result.stderr);
  for(const key of ['listingSha256','toolSha256','parserSha256','configSha256'])assert.match(result.report[key],/^[a-f0-9]{64}$/);
  return result.report;
}

test('selector census retains all first/second categories and handles nested operand commas',t=>{
  const report=good(t);
  assert.deepEqual(report.controls,baseline);
  assert.deepEqual(report.selectorCounts.nonliteral,{literalNonzeroLowWord:1,literalZeroLowWord:1,nonliteral:1});
  assert.equal(report.groups.nonliteral.literalNonzeroLowWord[0].first,'( {002F: 22 1, 2, 3, 4} )');
  const variable=report.groups.nonliteral.nonliteral[0];
  assert.equal(variable.firstLiteral,null);
  assert.equal(variable.secondLiteral,null);
  assert.equal(variable.selectorLowWord,null);
});

test('word narrowing distinguishes explicit zero, high-word-only values and signed literals',t=>{
  const selectors=[65536,-65536,0,65537,-1,32768];
  const text=selectors.map((value,i)=>`GPL/2 ${i.toString(16).toUpperCase().padStart(4,'0')}: 5E var9[44], ${value}, 0, 0, 0`).join('\n')+'\n';
  const report=good(t,text,{controls:{...baseline,sentinel:0,otherLiteral:0,nonliteral:6,sentinelLiteralSecond:0,sentinelNonliteralSecond:0}});
  assert.deepEqual(report.groups.nonliteral.literalZeroLowWord.map(row=>row.secondLiteral),[65536,-65536,0]);
  assert.deepEqual(report.groups.nonliteral.literalNonzeroLowWord.map(row=>row.selectorLowWord),[1,65535,32768]);
  const first=good(t,'GPL/1 0000: 5E 32766, 0, 0, 0, 0\nGPL/1 000B: 5E 98302, 0, 0, 0, 0\nGPL/1 0016: 5E (32766), 0, 0, 0, 0\n',
    {controls:{total:3,sentinel:1,otherLiteral:1,nonliteral:1,sentinelLiteralSecond:1,sentinelNonliteralSecond:0,errors:0}});
  assert.equal(first.groups.otherLiteral.literalZeroLowWord[0].firstLiteral,98302);
  assert.equal(first.groups.nonliteral.literalZeroLowWord[0].firstLiteral,null);
});

test('decoder gaps remain explicit and nested 5E instructions do not become classified top-level starts',t=>{
  const text=source+'GPL/2 0000: 18 ( {0003: 5E 32766, 0, 0, 0, 0} )\n'+
    'GPL/37 stopped at 0x0D39: synthetic rejection; resumed at the literal target 0x0D58\n'+
    'GPL/37 ERROR stopped at 0x0D39: synthetic rejection; resumed at the literal target 0x0D58\n';
  const report=good(t,text,{controls:{...baseline,errors:1}});
  assert.equal(report.indexedInstructions,7);
  assert.deepEqual(report.decoderErrors,[{resource:'GPL/37',stoppedAt:0xD39,resumedAt:0xD58}]);
  assert.equal(report.inlineErrorEchoes,1);
  assert.ok(report.limits.some(line=>line.includes('Nested instructions')));
});

test('a missed positive control, changed fingerprint or incomplete control schema rejects the report',t=>{
  for(const config of [{listingSha256:'0'.repeat(64)}, {controls:{...baseline,total:7}},
    {controls:{...baseline,sentinelLiteralSecond:2}}, {controls:{...baseline,errors:1}},
    {controls:{...baseline,total:true}}, {controls:{total:6}}, {controls:{...baseline,total:0}}, {unknown:true}]) {
    const result=run(t,source,config);
    assert.equal(result.status,1,JSON.stringify(config));
    assert.match(result.stderr,/script selector census:/);
    assert.equal(result.stdout,'');
  }
});

test('malformed, aliased, duplicate or unsorted instructions and unsupported combined errors fail closed',t=>{
  const error='GPL/37 ERROR stopped at 0x0001: synthetic rejection\n';
  for(const text of [source+source,source.split('\n').filter(Boolean).reverse().join('\n')+'\n',
    source.replace('32766, 65, 0, 0, 0','32766, 65, 0, 0'),source.replace('65, 0, 0, 0','(65, 0, 0, 0'),
    source.replace('GPL/1','GPL/01'),source+'GPL/2 0000: FE\n',source+'unknown line\n',
    source+'GPL/37 ERROR unknown\n',source+error+error,
    source+'GPL/37 stopped at 0x0001: unmatched inline rejection\n',
    source+'GPL/37 stopped at 0x0001: inline rejection\nGPL/37 stopped at 0x0001: inline rejection\n'+error,
    source+'GPL/37 ERROR stopped at 0x0001: rejection; stopped at 0x0002: rejection\n',
    source.replace('32766, 65','32766, 2147483648'),Buffer.from([0xFF]),'']) {
    const result=run(t,text);
    assert.equal(result.status,1);
    assert.equal(result.stdout,'');
  }
});

test('census input and output bounds reject oversized data without a partial report',t=>{
  for(const text of [Buffer.alloc(32*1024*1024+1,0x20),source+'X'.repeat(131073)+'\n']) {
    const result=run(t,text);
    assert.equal(result.status,1);
    assert.match(result.stderr,/exceeds.*limit/);
  }
  const oversized=run(t,source,{padding:'X'.repeat(65536)});
  assert.equal(oversized.status,1);
  assert.match(oversized.stderr,/exceeds byte limit/);
  const expression='('+Array(60).fill('var7[1]').join(' + ')+')';
  const rows=Array.from({length:30000},(_,i)=>`GPL/1 ${i.toString(16).toUpperCase().padStart(4,'0')}: 5E var9[44], ${expression}, 0, 0, 0`).join('\n')+'\n';
  const output=run(t,rows,{controls:{total:30000,sentinel:0,otherLiteral:0,nonliteral:30000,sentinelLiteralSecond:0,sentinelNonliteralSecond:0,errors:0}});
  assert.equal(output.status,1);
  assert.match(output.stderr,/report exceeds output limit/);
  assert.equal(output.stdout,'');
});
