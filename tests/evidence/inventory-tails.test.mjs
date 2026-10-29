import {test} from 'node:test';
import assert from 'node:assert/strict';
import {spawnSync} from 'node:child_process';
import {fileURLToPath} from 'node:url';
import {enginePython} from '../../tools/tool-dependencies.mjs';

const script=fileURLToPath(new URL('../../tools/research/exec-census/inventory_tails.py',import.meta.url));
const code='import json,runpy,sys; m=runpy.run_path(sys.argv[1]); p=json.load(sys.stdin); print(json.dumps(m["analyze"](bytes.fromhex(p["body"]),p["inventory"],p.get("view","file"))))';
function fixture(hex){
 const payload=Buffer.from(hex,'hex'), body=Buffer.alloc(144+payload.length);
 body.write('MZ');body.writeUInt16LE(128,2);body.writeUInt16LE(1,4);body.writeUInt16LE(2,8);
 body.writeUInt16LE(3,64);body.writeUInt16LE(32,66);body.writeUInt16LE(3,68);
 body[80]=0xcd;body[81]=0x3f;body.writeUInt16LE(payload.length,88);
 body.write('FBOV',128);body.writeUInt32LE(payload.length,132);body.writeUInt32LE(64,136);body.writeUInt32LE(1,140);
 payload.copy(body,144);return body;
}
const offset=n=>'0x'+n.toString(16).toUpperCase().padStart(8,'0');
function inventory(rows,mapped=false){
 const addr=n=>mapped?'1000:'+(n-32).toString(16).toUpperCase().padStart(4,'0'):offset(n);
 return 'start\tsize\tranges\n'+rows.map(([start,end])=>`${addr(start)}\t${end-start}\t${addr(start)}..${addr(end)}`).join('\n')+'\n';
}
function analyze(body,rows,view='file'){
 const input=JSON.stringify({body:body.toString('hex'),inventory:typeof rows==='string'?rows:inventory(rows,view==='mapped'),view});
 const result=spawnSync(enginePython(),['-I','-c',code,script],{input,encoding:'utf8',maxBuffer:65536});
 if(result.error)throw result.error;
 if(result.status!==0)throw new Error(result.stderr);
 return JSON.parse(result.stdout);
}
test('tail census decodes prefixed returns and does not mistake an immediate byte for a return',()=>{
 const report=analyze(fixture('b800cb66cb'),[[144,147],[147,149]]);
 assert.equal(report.rowsChecked,2);
 assert.deepEqual(report.rows.map(r=>r.status),['fallthrough_adjacent_entry','terminal_return']);
 assert.equal(report.rows[0].nextOwner,offset(147));
 assert.equal(report.rows[1].lastInstruction,offset(147));
 const mapped=analyze(fixture('b800cb66cb'),[[144,147],[147,149]],'mapped');
 assert.deepEqual(mapped.rows,report.rows);
});
test('conditional jumps, code ends and undecoded bytes remain separate from terminal transfers',()=>{
 assert.equal(analyze(fixture('7500cb'),[[144,146],[146,147]]).rows[0].status,'fallthrough_adjacent_entry');
 assert.equal(analyze(fixture('eb00'),[[144,146]]).rows[0].status,'terminal_jump');
 assert.equal(analyze(fixture('90'),[[144,145]]).rows[0].status,'fallthrough_code_end');
 assert.equal(analyze(fixture('900f'),[[144,146]]).rows[0].status,'incomplete_decode');
 assert.equal(analyze(fixture('9090cb'),[[144,145],[146,147]]).rows[0].status,'fallthrough_unowned');
 const held='start\tsize\tranges\n0x00000091\t1\t0x00000091..0x00000092\n0x00000090\t3\t0x00000090..0x00000091 0x00000092..0x00000094\n';
 const other=analyze(fixture('cb9090cb'),held).rows[0];
 assert.equal(other.status,'fallthrough_other_body');
 assert.equal(other.nextOwner,offset(144));
});
test('tail census rejects malformed metadata, overlapping ownership and vacuous selections',()=>{
 const body=fixture('9090cb');
 assert.throws(()=>analyze(body,[[144,146],[145,147]]),/Overlapping inventory bodies/);
 assert.throws(()=>analyze(body,[[144,145],[144,145]]),/Duplicate overlay entry/);
 assert.throws(()=>analyze(body,inventory([[144,145]],true)),/require --view mapped/);
 assert.throws(()=>analyze(body,[[144,148]]),/outside its entry's overlay/);
 assert.throws(()=>analyze(body,'start\tsize\tranges\n'),/No overlay rows selected/);
 assert.throws(()=>analyze(body,'start\tsize\tranges\n0x00000090\t2\t0x00000090..0x00000091\n'),/Invalid body ranges/);
 assert.throws(()=>analyze(body,'start\tsize\tranges\n0x00000090\t1\tbroken\n'),/Malformed inventory body range/);
});
test('source framing errors are bounded before descriptor or payload access',()=>{
 assert.throws(()=>analyze(Buffer.from('4d5a','hex'),[[144,145]]),/outside its boundary/);
 const body=fixture('cb');body.writeUInt32LE(65537,140);
 assert.throws(()=>analyze(body,[[144,145]]),/Invalid FBOV descriptor count/);
 const outside=fixture('cb');outside.writeUInt32LE(8,84);
 assert.throws(()=>analyze(outside,[[144,145]]),/outside its boundary/);
});
test('input reads stay bounded when stream contents exceed the reported file size',()=>{
 const control=`import io,runpy,sys,types
m=runpy.run_path(sys.argv[1])
class ChangedInput:
 def stat(self): return types.SimpleNamespace(st_size=1)
 def open(self,mode): return io.BytesIO(b'five!')
for limit in (2,-2):
 try: m['read_limited'](ChangedInput(),limit)
 except ValueError: pass
 else: raise AssertionError('oversized or unbounded read accepted')
print('bounded')`;
 const result=spawnSync(enginePython(),['-I','-c',control,script],{encoding:'utf8',maxBuffer:65536});
 assert.equal(result.status,0,result.stderr);
 assert.equal(result.stdout.trim(),'bounded');
});
