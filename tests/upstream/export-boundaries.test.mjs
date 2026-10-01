import test from 'node:test';
import assert from 'node:assert/strict';
import {readFileSync,writeFileSync,mkdtempSync,rmSync,existsSync} from 'node:fs';
import {tmpdir} from 'node:os';
import {join,resolve} from 'node:path';
import {spawnSync} from 'node:child_process';
const root=resolve(import.meta.dirname,'../..');
const javac=process.env.JAVA_HOME?join(process.env.JAVA_HOME,process.platform==='win32'?'bin/javac.exe':'bin/javac'):'javac';
const java=process.env.JAVA_HOME?join(process.env.JAVA_HOME,process.platform==='win32'?'bin/java.exe':'bin/java'):'java';
test('broad export guards accept local scratch and reject checkout output and traversal',t=>{
 const dir=mkdtempSync(join(tmpdir(),'export-guard-controls-'));t.after(()=>rmSync(dir,{recursive:true,force:true}));
 const names=['ExportEditionAnalysis','ExportFunctionAddressCorrelations','ExportVersionTrackingAddressContexts','ExportVersionTrackingMatches'];
 const methods=names.map((name,i)=>{
  const source=readFileSync(join(root,'tools/ghidra',name+'.java'),'utf8');
  const start=source.indexOf('    private static Path requireLocalOutput(');assert.ok(start>=0);
  let depth=0,end=source.indexOf('{',start);for(;end<source.length;end++){if(source[end]==='{')depth++;if(source[end]==='}'&&--depth===0)break;}
  return source.slice(start,end+1).replace('requireLocalOutput','guard'+i);
 });
 const file=join(dir,'ExportGuardTest.java');
 writeFileSync(file,`import java.nio.file.*; public class ExportGuardTest {\n${methods.join('\n')}\n public static void main(String[] args) throws Exception {for(int i=0;i<4;i++){var m=ExportGuardTest.class.getDeclaredMethod("guard"+i,Path.class); m.setAccessible(true); m.invoke(null,Path.of(args[0],"future-output")); for(String denied:new String[]{args[1],args[1]+"/analysis/original/../../../docs"}) {boolean rejected=false;try {m.invoke(null,Path.of(denied));}catch(java.lang.reflect.InvocationTargetException e){rejected=e.getCause() instanceof IllegalArgumentException;}if(!rejected)throw new AssertionError("accepted "+denied);} } } }`);
 const compile=spawnSync(javac,['-proc:none','-d',dir,file],{encoding:'utf8'});assert.equal(compile.status,0,compile.error?.message||compile.stdout+compile.stderr);
 const run=spawnSync(java,['-cp',dir,'ExportGuardTest',dir,root],{encoding:'utf8'});assert.equal(run.status,0,run.stdout+run.stderr);
 assert.match(readFileSync(join(root,'tools/ghidra/ExportEditionAnalysis.java'),'utf8'),/edition.matches/);
});
