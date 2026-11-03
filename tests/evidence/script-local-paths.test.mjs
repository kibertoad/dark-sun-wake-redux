import {test} from 'node:test';
import assert from 'node:assert/strict';
import {createHash} from 'node:crypto';
import {mkdtempSync, writeFileSync, rmSync} from 'node:fs';
import {tmpdir} from 'node:os';
import {join, resolve, sep} from 'node:path';
import {fileURLToPath} from 'node:url';
import {spawnSync} from 'node:child_process';
import {enginePython} from '../../tools/tool-dependencies.mjs';

const tool=fileURLToPath(new URL('../../tools/research/exec-census/script_local_paths.py',import.meta.url));
const listing=(...rows)=>rows.map(row=>'GPL/1 '+row).join('\n')+'\n';
function run(t, source, changes={}) {
  const dir=mkdtempSync(join(tmpdir(),'dark-sun-local-paths-'));
  t.after(()=>{
    assert.ok(resolve(dir).startsWith(resolve(tmpdir())+sep));
    rmSync(dir,{recursive:true,force:true});
  });
  const bytes=Buffer.from(source), input=join(dir,'synthetic.txt'), configPath=join(dir,'config.json');
  const config={resource:'GPL/1',roots:[0],target:0x20,local:0,
    listingSha256:createHash('sha256').update(bytes).digest('hex'),...changes};
  writeFileSync(input,bytes); writeFileSync(configPath,JSON.stringify(config));
  const result=spawnSync(enginePython(),['-I',tool,input,configPath],{encoding:'utf8',maxBuffer:1024*1024,timeout:10000});
  if(result.error)throw result.error;
  return {...result,report:result.status===0?JSON.parse(result.stdout):null};
}
function good(t,source,config) {
  const result=run(t,source,config);
  assert.equal(result.status,0,result.stderr);
  assert.match(result.report.toolSha256,/^[a-f0-9]{64}$/);
  assert.match(result.report.configSha256,/^[a-f0-9]{64}$/);
  return result.report;
}

test('local calls carry the callee assignment to the caller only after its matched return',t=>{
  const report=good(t,listing('0000: 13 48','0003: 5E 32766, var2[0], 0, 0, 0','000E: 31',
    '0030: 16 7 -> var2[0]','0035: 15'),{target:3});
  assert.deepEqual(report.roots[0].targetHistories,[{assignment:0x30,otherEffectsSinceAssignment:false}]);
  assert.deepEqual(report.assignments,[{offset:0x30,literal:7,kind:'literal'}]);
  assert.deepEqual(report.roots[0].unresolved,[]);
  const stopped=good(t,listing('0000: 13 48','0003: 5E 32766, var2[0], 0, 0, 0',
    '0030: 16 7 -> var2[0]','0035: 31'),{target:3});
  assert.deepEqual(stopped.roots[0].targetHistories,[]);
  assert.deepEqual(stopped.roots[0].unresolved,[]);
  const nested=good(t,listing('0000: 13 16','0003: 5E 0, 0, 0, 0, 0','0010: 13 48',
    '0013: 16 5 -> var2[0]','0018: 15','0030: 16 7 -> var2[0]','0035: 15'),{target:3});
  assert.deepEqual(nested.roots[0].targetHistories,[{assignment:19,otherEffectsSinceAssignment:false}]);
});

test('conditional assignment bypass remains unknown while an explicit zero stays a literal assignment',t=>{
  const report=good(t,listing('0000: 3E 12','0003: 16 0 -> var2[0]','0008: 12 32',
    '000C: 12 32','0020: 5E 32766, var2[0], 0, 0, 0'));
  assert.deepEqual(report.roots[0].targetHistories,[
    {assignment:null,otherEffectsSinceAssignment:false},{assignment:3,otherEffectsSinceAssignment:false}]);
  assert.deepEqual(report.assignments,[{offset:3,literal:0,kind:'literal'}]);
});

test('other effects remain marked, a later direct assignment resets history, and expression values stay unknown',t=>{
  const prefix=['0000: 16 4 -> var2[0]','0005: 22 1, 2, 3, 4'];
  const before=good(t,listing(...prefix,'0020: 5E 32766, var2[0], 0, 0, 0'));
  assert.deepEqual(before.roots[0].targetHistories,[{assignment:0,otherEffectsSinceAssignment:true}]);
  assert.deepEqual(before.otherEffectSites,[5]);
  const after=good(t,listing(...prefix,'000A: 16 8 -> var2[0]','0020: 5E 32766, var2[0], 0, 0, 0'));
  assert.deepEqual(after.roots[0].targetHistories,[{assignment:10,otherEffectsSinceAssignment:false}]);
  const expression=good(t,listing('0000: 16 var7[4] -> var2[0]','0020: 5E 32766, var2[0], 0, 0, 0'));
  assert.deepEqual(expression.assignments,[{offset:0,literal:null,kind:'expression'}]);
  assert.ok(expression.limits.some(line=>line.includes('not preserved values')));
});

test('cross-script calls, computed and missing targets are explicit boundaries rather than successful continuations',t=>{
  for(const [instruction,reason] of [['14 0, 2','cross-script-call'],['12 var2[4]','computed-or-missing-transfer'],
    ['12 18','missing-instruction-target'],['15','return-to-unmodeled-local-frame'],
    ['19','return-to-unmodeled-script-context'],['FE','unsupported-opcode'],['16 unknown','unreadable-assignment']]) {
    const report=good(t,listing('0000: '+instruction,'0020: 5E 32766, var2[0], 0, 0, 0'));
    assert.deepEqual(report.roots[0].targetHistories,[],instruction);
    assert.equal(report.roots[0].unresolved[0].reason,reason,instruction);
  }
  const branch=good(t,listing('0000: 3E var2[4]','0020: 5E 32766, var2[0], 0, 0, 0'));
  assert.equal(branch.roots[0].targetHistories.length,1);
  assert.equal(branch.roots[0].unresolved[0].reason,'computed-or-missing-transfer');
});

test('a decoder-rejected span cannot silently become a fall-through edge, while a declared later root stays separate',t=>{
  const source=listing('0000: 18 1','0020: 5E 32766, var2[0], 0, 0, 0')+
    'GPL/1 ERROR stopped at 0x0005: synthetic rejection; resumed at the literal target 0x0020\n';
  const report=good(t,source,{roots:[0,32]});
  assert.deepEqual(report.decodeErrors,[5]);
  assert.deepEqual(report.roots[0].targetHistories,[]);
  assert.equal(report.roots[0].unresolved[0].reason,'missing-continuation-or-rejected-span');
  assert.deepEqual(report.roots[1].targetHistories,[{assignment:null,otherEffectsSinceAssignment:false}]);
});

test('menu alternatives carry separate histories and nested control instructions stop rather than being flattened',t=>{
  const report=good(t,listing('0000: 48 var6[1]; row(string5(1 characters), 48, 1);row(string5(1 characters), 64, var7[2])',
    '0010: 5E 32766, var2[0], 0, 0, 0','0030: 16 5 -> var2[0]','0035: 15',
    '0040: 16 6 -> var2[0]','0045: 15'),{target:16});
  assert.deepEqual(report.roots[0].targetHistories,[
    {assignment:48,otherEffectsSinceAssignment:false},{assignment:64,otherEffectsSinceAssignment:false}]);
  assert.equal(report.roots[0].unresolved[0].reason,'menu-availability-and-selection-not-admitted');
  const stopped=good(t,listing('0000: 48 var6[1]; row(string5(1 characters), 48, 1)',
    '0010: 5E 32766, var2[0], 0, 0, 0','0030: 31'),{target:16});
  assert.deepEqual(stopped.roots[0].targetHistories,[]);
  const nested=good(t,listing('0000: 18 ( {0003: 14 0, 2} )','0020: 5E 32766, var2[0], 0, 0, 0'));
  assert.deepEqual(nested.roots[0].targetHistories,[]);
  assert.equal(nested.roots[0].unresolved[0].reason,'nested-control-instruction');
});

test('menu overlap and unresolved targets are retained as boundaries, and depth applies to menu frames too',t=>{
  const rows=Array.from({length:12},()=> 'row(string5(1 characters), 48, 1)').join('; ');
  const overlap=good(t,listing('0000: 48 var6[1]; '+rows,'0010: 5E 0, 0, 0, 0, 0','0030: 15'),{target:16});
  assert.deepEqual(overlap.roots[0].targetHistories,[]);
  assert.equal(overlap.roots[0].unresolved[0].reason,'menu-frame-array-overlap-not-modeled');
  const capped=good(t,listing('0000: 13 48','0010: 5E 0, 0, 0, 0, 0',
    '0030: 48 var6[1]; row(string5(1 characters), 64, 1)','0040: 15'),{target:16,maxDepth:1});
  assert.deepEqual(capped.roots[0].targetHistories,[]);
  assert.equal(capped.roots[0].unresolved[0].reason,'local-call-depth-limit');
  for(const menu of ['var6[1]','var6[1]; row(string5(1 characters), var2[1], 1)']) {
    const report=good(t,listing('0000: 48 '+menu,'0020: 5E 0, 0, 0, 0, 0'));
    assert.deepEqual(report.roots[0].targetHistories,[]);
    assert.ok(report.roots[0].unresolved.some(gap=>/menu-without|menu-target/.test(gap.reason)));
  }
});

test('recursive calls and the shared state budget preserve explicit caps without manufacturing a target hit',t=>{
  const recursive=good(t,listing('0000: 13 0','0020: 5E 32766, var2[0], 0, 0, 0'),{maxDepth:2});
  assert.deepEqual(recursive.roots[0].targetHistories,[]);
  assert.equal(recursive.roots[0].unresolved[0].reason,'local-call-depth-limit');
  const capped=good(t,listing('0000: 18 1','0003: 12 32','0020: 5E 32766, var2[0], 0, 0, 0'),
    {maxStates:1,roots:[0,32]});
  assert.equal(capped.roots.reduce((n,root)=>n+root.visitedStates,0),1);
  assert.ok(capped.roots.every(root=>root.targetHistories.length===0&&root.unresolved.some(gap=>gap.reason==='state-limit')));
});

test('listing identity, structure, roots and limits fail with bounded diagnostics',t=>{
  const source=listing('0000: 18 1','0020: 5E 32766, var2[0], 0, 0, 0');
  for(const config of [{listingSha256:'0'.repeat(64)},{roots:[]},{roots:[0,0]},{roots:[1]},
    {roots:[true]},{resource:'GPL/99999'},{resource:'../GPL/1'},{target:0},{maxStates:0},
    {maxStates:100001},{maxDepth:50},{local:-1},{unrecognized:true}]) {
    const result=run(t,source,config);
    assert.equal(result.status,1,JSON.stringify(config));
    assert.match(result.stderr,/script local paths:/);
    assert.equal(result.stdout,'');
  }
  for(const bad of [listing('0020: 5E 0, 0, 0, 0, 0','0000: 18 1'),
    listing('0000: 18 1','0000: 18 2','0020: 5E 0, 0, 0, 0, 0'),
    source+'GPL/1 malformed\n',source+'GPL/1 ERROR unknown\n',Buffer.from([0xFF]),'']) {
    assert.equal(run(t,bad).status,1);
  }
});

test('byte, line and operand bounds reject oversized or malformed text before analysis',t=>{
  const source=listing('0000: 18 1','0020: 5E 0, 0, 0, 0, 0');
  for(const oversized of [Buffer.alloc(32*1024*1024+1,0x20),source+'X'.repeat(131073)+'\n']) {
    const result=run(t,oversized);
    assert.equal(result.status,1);
    assert.match(result.stderr,/exceeds.*limit/);
  }
  const config=run(t,source,{oversized:'X'.repeat(65536)});
  assert.equal(config.status,1);
  assert.match(config.stderr,/exceeds byte limit/);
  assert.equal(run(t,listing('0000: 18 ( var2[0]','0020: 5E 0, 0, 0, 0, 0')).status,1);
  const badCount=good(t,listing('0000: 12 32, 48','0020: 5E 0, 0, 0, 0, 0'));
  assert.equal(badCount.roots[0].unresolved[0].reason,'wrong-control-operand-count');
  const wrapped=good(t,listing('0000: 12 -1','FFFF: 5E 0, 0, 0, 0, 0'),{target:65535});
  assert.equal(wrapped.roots[0].targetHistories.length,1);
});
