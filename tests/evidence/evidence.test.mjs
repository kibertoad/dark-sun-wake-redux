import { test } from "node:test";
import assert from "node:assert/strict";
import { mkdtempSync, writeFileSync, readFileSync, rmSync, existsSync } from "node:fs";
import { tmpdir } from "node:os";
import { join } from "node:path";
import { sourceXxh3 } from "@scientific-method/executable-reader";
import { readMz, incomingCalls } from "../../tools/evidence/legacy-image.mjs";
import { reviewFlow, boundedTable } from "../../tools/evidence/review.mjs";
import { inventoryPath, parseInventory, joinInventories, verifyInventory } from "../../tools/evidence/inventory.mjs";
import { run } from "../../tools/evidence/report.mjs";
import { readPe32 } from "../../tools/evidence/pe-image.mjs";
import { peTransferCandidates } from "../../tools/evidence/pe-transfers.mjs";
import { unchangedInventory } from "../../tools/evidence/migrate-inventory.mjs";
import { joinOverlayViews } from "../../tools/evidence/join-overlay-views.mjs";

test('inventory migration preserves complete disjoint bodies and rejects unresolved changes',()=>{
 const old='start\tsize\n1000:0000\t4\n';
 const fresh='start\tsize\tranges\n1000:0000\t4\t1000:0000..1000:0002 1000:0004..1000:0006\n';
 const provenance='key\tvalue\nfunctions\t1\nsummed_body_bytes\t4\nunique_body_bytes\t4\nbody_bytes_outside_regions\t0\nentries_without_instruction\t0\n';
 const regions='start\tsize\tbody_bytes\tinstructions\tinstructions_outside\tdata\tdata_outside\tundefined\tundefined_outside\n1000:0000\t8\t4\t4\t0\t0\t0\t4\t4\n';
 assert.equal(unchangedInventory(old,fresh,provenance,regions),fresh);
 assert.throws(()=>unchangedInventory(old.replace('\t4','\t5'),fresh,provenance,regions),/changed/);
 assert.throws(()=>unchangedInventory(old,fresh,provenance.replace('entries_without_instruction\t0','entries_without_instruction\t1'),regions),/anomalies/);
 assert.throws(()=>unchangedInventory(old,fresh.replace('1000:0004..1000:0006','1000:0001..1000:0003'),provenance,regions),/body/);
 assert.throws(()=>unchangedInventory(old,fresh.replace('1000:0004..1000:0006','1000:0008..1000:000A'),provenance,regions),/body/);
 assert.throws(()=>unchangedInventory(old.replace('start\tsize','start\tsize\tname').replace('\t4\n','\t4\tname\n'),fresh,provenance,regions),/Annotations/);
});

test('overlay view join converts overlay rows to file offsets and keeps bodies in their own code range',()=>{
 const columns='start\tsize\tbody_bytes\tinstructions\tinstructions_outside\tdata\tdata_outside\tundefined\tundefined_outside\n';
 const provenance=(n,without=0)=>`key\tvalue\nfunctions\t1\nsummed_body_bytes\t${n}\nunique_body_bytes\t${n}\nbody_bytes_outside_regions\t0\nentries_without_instruction\t${without}\n`;
 const resident={tsv:'start\tsize\tranges\n1000:0000\t4\t1000:0000..1000:0004\n',provenance:provenance(4),regions:columns+'1000:0000\t256\t4\t4\t0\t0\t0\t252\t252\n'};
 const overlay={tsv:'start\tsize\tranges\n1011:0002\t3\t1011:0000..1011:0003\n',provenance:provenance(3),regions:columns+'1011:0000\t16\t3\t3\t0\t0\t0\t13\t13\n'};
 const overlays=[{start:0x310,end:0x320,mappedStart:'1011:0000'},{start:0x320,end:0x330,mappedStart:'1012:0000'}];
 const base={resident,overlay,headerBytes:0x200,imageEnd:0x300,overlays};
 const joined=joinOverlayViews(base);
 assert.equal(joined.tsv,'start\tsize\tranges\n1000:0000\t4\t1000:0000..1000:0004\n0x00000312\t3\t0x00000310..0x00000313\n');
 assert.equal(joined.regions,columns+'1000:0000\t256\t4\t4\t0\t0\t0\t252\t252\n0x00000310\t16\t3\t3\t0\t0\t0\t13\t13\n');
 // A body part in the next overlay's code range is outside the entry's own range.
 const across={...overlay,tsv:'start\tsize\tranges\n1011:0002\t4\t1011:0000..1011:0003 1012:0000..1012:0001\n',provenance:provenance(4)};
 assert.throws(()=>joinOverlayViews({...base,overlay:across}),/outside its entry's code range/);
 assert.throws(()=>joinOverlayViews({...base,resident:{...resident,tsv:'start\tsize\tranges\n1000:0000\t4\t1000:00FE..1000:0102\n'}}),/outside the load image|Invalid body/);
 assert.throws(()=>joinOverlayViews({...base,overlay:{...overlay,provenance:provenance(3,1)}}),/without an instruction/);
 const flagged=joinOverlayViews({...base,overlay:{...overlay,provenance:provenance(3,1)},expectedWithoutInstruction:{resident:[],overlay:['1011:0002']}});
 assert.deepEqual(flagged.report.withoutInstruction,['0x00000312']);
 assert.throws(()=>joinOverlayViews({...base,overlays:[{...overlays[0],mappedStart:'1011:0001'},overlays[1]]}),/in place/);
 // A normalized end in another segment is written in the start's segment.
 const normalized={...resident,tsv:'start\tsize\tranges\n1000:0000\t4\t1000:0000..1000:0002 1000:0010..1001:0002\n'};
 assert.match(joinOverlayViews({...base,resident:normalized}).tsv,/1000:0000\.\.1000:0002 1000:0010\.\.1000:0012\n/);
});

function synthetic() {
  const b = Buffer.alloc(592), w = (p, n) => b.writeUInt16LE(n, p), d = (p, n) => b.writeUInt32LE(n, p);
  b.write("MZ"); w(4, 1); w(8, 4); w(6, 2); w(24, 28);
  w(28, 19); w(32, 35);
  b[80] = 0x9A; w(81, 32); w(83, 12);
  b[96] = 0x9A; w(97, 16); w(99, 13); // Different segment pair, same trampoline.
  b.write("FBOV", 512); d(516, 64); d(520, 128); d(524, 2);
  w(130, 48); // Descriptor 0: resident span over load-image bytes 0 to 48 (file 64 to 112).
  w(136, 12); w(140, 2);
  w(256, 0x3FCD); d(260, 0); w(264, 32); w(266, 2); w(268, 1);
  w(288, 0x3FCD); w(290, 0);
  b[532] = 0x9A; w(533, 16); w(535, 0); w(560, 7);
  return b;
}

test("MZ and FBOV operands retain raw, loaded, trampoline and canonical identities", () => {
  const image = readMz(synthetic());
  const r = image.resolveOperand(83, 32);
  assert.equal(r.raw, 12); assert.equal(r.loadedAddress, "100C:0020");
  assert.equal(r.fileOffset, "0x00000120"); assert.equal(r.canonicalTarget, "0x00000210");
  assert.equal(image.resolveOperand(99, 16).canonicalTarget, r.canonicalTarget);
  const overlay = image.resolveOperand(535, 16);
  assert.equal(overlay.kind, "FBOV fixup"); assert.equal(overlay.descriptor, 0);
  assert.equal(overlay.canonicalTarget, "0x00000050");
  assert.equal(image.resolveOperand(85, 0).relocated, false);
});

test("incoming candidates preserve aliases, controls and truncation", () => {
  const image = readMz(synthetic()), report = incomingCalls(image, 528, { controls: [80, 96], limit: 1 });
  assert.equal(report.total, 2); assert.equal(report.matches.length, 1); assert.equal(report.truncated, true);
  assert.match(report.matches[0].classification, /candidate/);
  assert.throws(() => incomingCalls(image, 528, { controls: [81] }), /Positive control/);
  assert.match(incomingCalls(image, 81).negative, /no positive control/);
  const negative = incomingCalls(image, 81, { controls: [80] });
  assert.match(negative.negative, /this domain/);
  // A coverage control need not call the target; the report shows what it resolved to.
  assert.deepEqual(negative.controls, [{ callSite: "0x00000050", canonicalTarget: "0x00000210" }]);
  // A call byte before an unresolvable relocated word is reported, not fatal to the search.
  const bogus = synthetic(); bogus.writeUInt16LE(0x0F00, 99);
  const partial = incomingCalls(readMz(bogus), 528, { controls: [80] });
  assert.equal(partial.total, 1); assert.equal(partial.unresolved.length, 1); assert.equal(partial.unresolved[0].callSite, "0x00000060");
  // An unresolved site cannot serve as a control.
  assert.throws(() => incomingCalls(readMz(bogus), 528, { controls: [96] }), /Positive control/);
});

for (const [name, edit, error] of [
  ["truncated MZ", (b) => b.subarray(0, 20), /header/],
  ["page tail", (b) => { b.writeUInt16LE(512, 2); return b; }, /page/],
  ["relocation table", (b) => { b.writeUInt16LE(63, 24); return b; }, /relocation table/],
  ["duplicate relocation", (b) => { b.writeUInt16LE(19, 32); return b; }, /Duplicate/],
  ["payload outside envelope", (b) => { b.writeUInt32LE(1, 516); return b; }, /code and fixups/],
  ["wrong descriptor type", (b) => { b.writeUInt16LE(2, 132); return b; }, /trap prefix/],
  ["missing trap", (b) => { b[256] = 0; return b; }, /trap prefix/],
  ["odd fixups", (b) => { b.writeUInt16LE(3, 266); return b; }, /fixup dimensions/],
  ["bad fixup operand", (b) => { b.writeUInt16LE(31, 560); return b; }, /fixup operand/],
  ["bad descriptor token", (b) => { b.writeUInt16LE(16, 535); return b; }, /invalid FBOV fixup/],
  ["trampoline outside code", (b) => { b.writeUInt16LE(32, 290); return b; }, /trampoline/],
  ["extended executable", (b) => { b.writeUInt32LE(64, 60); b.write("PE", 64); return b; }, /unsupported/],
  ["extended header past load image", (b) => { b.writeUInt32LE(576, 60); b.write("NE", 576); return b; }, /unsupported/],
]) test(`reject ${name}`, () => assert.throws(() => readMz(edit(synthetic())), error));

test("flow traverses every branch beyond an early return and counts discontiguous bytes", () => {
  const report = reviewFlow({ entries: [10], instructions: [
    { start: 10, size: 2, kind: "branch", next: [20, 40], calls: [] },
    { start: 20, size: 1, kind: "return", next: [], calls: [] },
    { start: 40, size: 3, kind: "call", next: [50], calls: [200] },
    { start: 50, size: 1, kind: "return", next: [], calls: [] },
  ] }, 10);
  assert.equal(report.bodyBytes, 7); assert.equal(report.spans.length, 4);
  assert.equal(report.exits.length, 2); assert.equal(report.calls.length, 1);
  assert.equal(report.localPathsResolved, true); assert.match(report.status, /never promotes/);
});

test("explicit overlapping targets survive while undecoded edges remain gaps", () => {
  const graph = { entries: [0], instructions: [
    { start: 0, size: 4, kind: "branch", next: [2, 30], calls: [] },
    { start: 2, size: 1, kind: "return", next: [], calls: [] },
  ] };
  const result = reviewFlow(graph, 0);
  assert.equal(result.overlaps.length, 1); assert.equal(result.bodyBytes, 4);
  assert.equal(result.localPathsResolved, false); assert.match(result.gaps[0].reason, /No verified/);
  assert.match(reviewFlow(graph, 0, 1).gaps[0].reason, /limit/);
});

test("other entries and analyzer ownership cannot silently join a caller", () => {
  const graph = { entries: [0, 10], instructions: [
    { start: 0, size: 1, kind: "ordinary", next: [10], calls: [], owner: 20 },
    { start: 10, size: 1, kind: "return", next: [], calls: [] },
  ] };
  const report = reviewFlow(graph, 0);
  assert.equal(report.ownership.length, 1); assert.match(report.gaps[0].reason, /another exported/);
  const twice = reviewFlow({ ...graph, instructions: [{ ...graph.instructions[0], next: [10, 10] }, graph.instructions[1]] }, 0);
  assert.equal(twice.gaps.filter((g) => /another exported/.test(g.reason)).length, 1);
  assert.equal(report.localPathsResolved, false);
});

test("table count and widths prohibit decoding beyond the declared layout", () => {
  const bytes = Buffer.from([1, 0, 2, 0, 255, 255]);
  const layout = { start: 0, count: 2, stride: 2, fields: [{ name: "tag", offset: 0, width: 2 }], countEvidence: "synthetic loop bound" };
  assert.deepEqual(boundedTable(bytes, layout).rows, [{ tag: 1 }, { tag: 2 }]);
  assert.throws(() => boundedTable(bytes, { ...layout, count: 4 }), /outside/);
  assert.throws(() => boundedTable(bytes, { ...layout, countEvidence: "" }), /evidence/);
  assert.throws(() => boundedTable(bytes, { ...layout, fields: [{ name: "tag", offset: 1, width: 2 }] }), /field/);
  assert.throws(() => boundedTable(bytes, { ...layout, fields: [{ offset: 0, width: 2 }] }), /field/);
});

test("portable paths retain manifest identity and reject traversal/collisions", () => {
  assert.equal(inventoryPath("BLD-EXAMPLE", "CD:GAME.EXE"), "coverage/BLD-EXAMPLE/@CD/GAME.EXE.tsv");
  assert.equal(inventoryPath("BLD-EXAMPLE", "CD2:DIR/GAME.EXE"), "coverage/BLD-EXAMPLE/@CD2/DIR/GAME.EXE.tsv");
  for (const path of ["../GAME.EXE", "CD:/GAME.EXE", "C:/GAME.EXE", "@CD/GAME.EXE", "CON.txt", "a\\b", "a./b", "a//b"]) assert.throws(() => inventoryPath("BLD-EXAMPLE", path));
});

test("inventory views report exclusions and reject aliased ownership conflicts", () => {
  const image = readMz(synthetic()), view = { name: "resident", ranges: [{ start: 64, end: 512 }], text: "start\tsize\n1001:0000\t8\n" };
  const mapped = { name: "mapped", ranges: [{ start: 528, end: 560 }], text: "start\tsize\n1001:0000\t8\n0x0210\t2\n" };
  const joined = joinInventories(image, "CD:GAME.EXE", [view, mapped], { codeRanges: [{ start: 528, end: 540 }] });
  // Starts are in the standard's notation: segmented in the load image, a file offset in overlay code.
  assert.equal(joined.tsv, "start\tsize\n1001:0000\t8\n0x00000210\t2\n"); assert.equal(joined.views[1].excluded, 1);
  // An overlay offset must lie in a row of the build's Code ranges, and each row inside an overlay payload.
  assert.throws(() => joinInventories(image, "GAME.EXE", [view, mapped]), /Code ranges/);
  // An overlay start that the view does not own is excluded, and needs no Code ranges row.
  assert.equal(joinInventories(image, "GAME.EXE", [{ ...view, text: mapped.text }]).views[0].excluded, 1);
  assert.throws(() => joinInventories(image, "GAME.EXE", [view, mapped], { codeRanges: [{ start: 530, end: 540 }] }), /Code ranges/);
  assert.throws(() => joinInventories(image, "GAME.EXE", [view, mapped], { codeRanges: [{ start: 100, end: 120 }] }), /overlay payload/);
  assert.throws(() => joinInventories(image, "GAME.EXE", [view, mapped], { codeRanges: [{ start: 528, end: 600 }] }), /overlay payload/);
  // A file offset in the load image is written as the segment:offset that names the same byte.
  assert.equal(joinInventories(image, "GAME.EXE", [{ ...view, text: "start\tsize\n0x0104\t4\n1001:0001\t2\n" }]).tsv, "start\tsize\n1001:0001\t2\n100C:0004\t4\n");
  assert.throws(() => joinInventories(image, "GAME.EXE", [{ ...view, text: "start\tsize\n0x0050\t4\n1001:0000\t2\n" }]), /aliased/);
  // The standard places the load image at segment 0x1000; another load segment would misspell every start.
  assert.throws(() => joinInventories(readMz(synthetic(), 0x2000), "GAME.EXE", [view]), /0x1000/);
  assert.throws(() => joinInventories(image, "GAME.EXE", [view, { ...view, name: "second" }]), /Conflicting/);
  assert.throws(() => parseInventory("start\tsize\tname\n0x01\t1\tFromOriginal\n"), /only start and size/);
  assert.throws(() => parseInventory("start\tsize\n0x01\t1\n0x01\t1\n"), /Duplicate/);
  assert.throws(() => joinInventories(image, "GAME.EXE", [{ ...view, ranges: [{ start: 600, end: 610 }] }]), /outside/);
  // Overlapping ownership across views is rejected even when no start collides.
  assert.throws(() => joinInventories(image, "GAME.EXE", [view, { name: "other", ranges: [{ start: 256, end: 300 }], text: "start\tsize\n0x0104\t4\n" }]), /Conflicting ownership/);
  // Segment arithmetic never reaches overlay payload; overlay starts must be canonical offsets.
  assert.throws(() => image.address(0x101D, 0), /resident load image/);
  // A discontiguous function's body count must not be mistaken for start + size.
  assert.doesNotThrow(() => joinInventories(image, "GAME.EXE", [{ ...view, text: "start\tsize\n0x01FF\t5\n" }]));
});

test("command interface checks identity and writes only the canonical inventory path", (t) => {
  const dir = mkdtempSync(join(tmpdir(), "evidence-")); t.after(() => rmSync(dir, { recursive: true, force: true }));
  const bytes = synthetic(); writeFileSync(join(dir, "synthetic.exe"), bytes);
  writeFileSync(join(dir, "view.tsv"), "start\tsize\n1001:0000\t8\n");
  const cfg = { source: "synthetic.exe", xxh3: sourceXxh3(bytes), site: 83, targetOffset: 32,
    build: "BLD-EXAMPLE", manifest: "CD:GAME.EXE", writeRoot: "out", views: [{ name: "resident", path: "view.tsv", ranges: [{ start: 64, end: 512 }] }] };
  const path = join(dir, "report.json"); writeFileSync(path, JSON.stringify(cfg));
  assert.equal(run(["operand", path]).relocated, true);
  const result = run(["inventory", path]); assert.equal(result.tsv, undefined);
  assert.ok(existsSync(join(dir, "out", result.destination)));
  assert.equal(readFileSync(join(dir, "out", result.destination), "utf8"), "start\tsize\n1001:0000\t8\n");
  assert.throws(() => run(["inventory", path]), /exist/);
  cfg.inventory = { path: `out/${result.destination}`, repositoryPath: result.destination };
  writeFileSync(path, JSON.stringify(cfg));
  assert.equal(run(["inventory-check", path]).rows, 1);
  cfg.inventory.repositoryPath = "wrong.tsv"; writeFileSync(path, JSON.stringify(cfg));
  assert.throws(() => run(["inventory-check", path]), /repositoryPath/);
  cfg.inventory = { path: `out/${result.destination}`, repositoryPath: "coverage/BLD-EXAMPLE/@CD/OTHER.EXE.tsv" }; writeFileSync(path, JSON.stringify(cfg));
  assert.throws(() => run(["inventory-check", path]), /repositoryPath/);
  cfg.xxh3 = "0".repeat(32); writeFileSync(path, JSON.stringify(cfg));
  assert.throws(() => run(["operand", path]), /baseline/);
  // A config from before the move to xxh3 is refused rather than checked against nothing.
  writeFileSync(path, JSON.stringify({ ...cfg, xxh3: undefined, sha256: "0".repeat(64) }));
  assert.throws(() => run(["operand", path]), /sha256 is no longer read/);
  assert.throws(() => run(["unknown", path]), /Unknown/);
});

test("x86 commands reach scientific-method-engine through the executable reader", (t) => {
  // A synthetic MZ whose resident code makes a far call through a relocated segment to a routine that
  // loads AX and returns far. The engine comes from EVIDENCE_PYTHON, or python.
  const dir = mkdtempSync(join(tmpdir(), "evidence-x86-")); t.after(() => rmSync(dir, { recursive: true, force: true }));
  const bytes = Buffer.alloc(512);
  bytes.write("MZ"); bytes.writeUInt16LE(1, 4); bytes.writeUInt16LE(4, 8);
  bytes.writeUInt16LE(1, 6); bytes.writeUInt16LE(28, 24); bytes.writeUInt16LE(3, 28);
  bytes.set([0x9a, 0x10, 0, 0, 0, 0xc3], 64);
  bytes.set([0xb8, 0xff, 0xff, 0xcb], 80);
  writeFileSync(join(dir, "source.bin"), bytes);
  const path = join(dir, "config.json");
  writeFileSync(path, JSON.stringify({ source: "source.bin", sourceKind: "mz", xxh3: sourceXxh3(bytes), entry: 64,
    regions: [{ name: "resident", start: 64, end: 84, ip: 0, segment: 4096, entries: [64], evidence: "synthetic mapped MZ" }] }));
  const report = run(["x86-returns", path]);
  assert.equal(report.completeWithinModel, true);
  assert.equal(report.paths[0].registers.ax.value, 65535);
  assert.ok(report.paths[0].events.some((e) => e.kind === "call-return"));
  assert.throws(() => run(["x86-unknown", path]), /Unknown x86 report command/);
});


test("hardware and indirect boundaries prevent an unqualified local-path result", () => {
  const report = reviewFlow({ entries: [0], instructions: [
    { start: 0, size: 1, kind: "ordinary", next: [1], calls: [], externalEffects: ["port I/O"] },
    { start: 1, size: 1, kind: "indirect", next: [], calls: [] },
  ] }, 0);
  assert.equal(report.localPathsResolved, false);
  assert.equal(report.gaps.length, 2);
  assert.match(report.gaps[0].reason, /Hardware/);
});

test('committed inventory validates identity, columns, numeric aliases and explicit legacy paths',()=>{
 const image=readMz(synthetic()),build='BLD-EXAMPLE',manifest='CD:GAME.EXE',path=inventoryPath(build,manifest);
 const text='start\tsize\tname\tout_of_scope\n1001:0000\t8\tneutralHelper\toutside selected slice\n';
 const check=(t=text,p=path,options={})=>verifyInventory(image,build,manifest,t,p,options);
 assert.equal(check().rows,1);
 assert.equal(check(text.replace('outside selected slice','')).rows,1);
 assert.throws(()=>check(text,'CON.tsv'),/Unsafe/);
 assert.equal(check(text,'coverage/BLD-EXAMPLE/CD/GAME.EXE.tsv',{legacyPath:'coverage/BLD-EXAMPLE/CD/GAME.EXE.tsv',legacyEvidence:'documented historical path'}).legacyPathEvidence,'documented historical path');
 assert.throws(()=>check(text,'coverage/BLD-EXAMPLE/CD/GAME.EXE.tsv'),/destination/);
 assert.throws(()=>check(text,path,{legacyPath:'../bad.tsv',legacyEvidence:'bad'}),/Legacy/);
 assert.throws(()=>check(text,path,{legacyPath:'safe.tsv'}),/Legacy/);
 // Any spelling of a byte in the load image passes; the manifest-prefixed file offset does not.
 assert.equal(check(text.replace('1001:0000','1000:0010')).rows,1);
 assert.throws(()=>check(text.replace('1001:0000','CD:GAME.EXE+0x00000050')),/segmented or a canonical file offset/);
 assert.throws(()=>check(text.replace('1001:0000','0x00000050')),/noncanonical/);
 assert.throws(()=>check(text.replace('1001:0000','1001:000a')),/noncanonical/);
 assert.throws(()=>check(text.replace('1001:0000','101D:0000')),/resident load image/);
 assert.throws(()=>check(text.replace('\t8\t','\t0\t')),/body/);
 assert.throws(()=>check('start\tsize\n1001:0000\t1\n1000:0010\t1\n'),/Duplicate/);
 // An overlay start is an eight-digit file offset inside a row of the build's Code ranges.
 const overlay='start\tsize\n0x00000210\t2\n', codeRanges=[{start:528,end:540}];
 assert.equal(check(overlay,path,{codeRanges}).rows,1);
 assert.throws(()=>check(overlay),/Code ranges/);
 assert.throws(()=>check(overlay.replace('0x00000210','0x0210'),path,{codeRanges}),/noncanonical/);
 assert.throws(()=>check(text.replace('neutralHelper','FUN_00000040')),/analyzer/);
 assert.throws(()=>check(text.replace('out_of_scope','bytes')),/columns/);
 assert.throws(()=>check(text.replace('name\tout_of_scope','name\tname')),/columns/);
 assert.throws(()=>check('start\tsize\n'),/row count/);
 assert.throws(()=>check(text,'/bad.tsv'),/Unsafe/);
});

// A synthetic PE32 based where no executable of the period loads, with an executable .text at
// 0x7F001000..0x7F001100 and a data section at 0x7F002000..0x7F002200.
function syntheticPe({ magic = 0x10B, signature = "PE\0\0", sizeOfHeaders = 0x200, textAddress = 0x1000, dataVirtualSize = 0x200 } = {}) {
  const b = Buffer.alloc(0x400), section = (at, virtualSize, address, characteristics) => {
    b.writeUInt32LE(virtualSize, at + 8); b.writeUInt32LE(address, at + 12); b.writeUInt32LE(characteristics, at + 36);
  };
  b.write("MZ"); b.writeUInt32LE(0x80, 0x3C); b.write(signature, 0x80, "latin1");
  b.writeUInt16LE(0x14C, 0x84); b.writeUInt16LE(2, 0x86); b.writeUInt16LE(0xE0, 0x94);
  b.writeUInt16LE(magic, 0x98); b.writeUInt32LE(0x7F000000, 0x98 + 28); b.writeUInt32LE(0x3000, 0x98 + 56); b.writeUInt32LE(sizeOfHeaders, 0x98 + 60);
  section(0x178, 0x100, textAddress, 0x60000020); section(0x1A0, dataVirtualSize, 0x2000, 0xC0000040);
  return b;
}

test("PE inventories write flat virtual addresses inside executable sections", (t) => {
  const image = readPe32(syntheticPe());
  assert.deepEqual(image.ranges, [{ view: "section-0", start: 0x7F001000, end: 0x7F001100 }]);
  const view = { name: "text", ranges: [{ start: 0x7F001000, end: 0x7F001100 }], text: "start\tsize\n0x7f001040\t16\n0x7F001000\t8\n" };
  assert.equal(joinInventories(image, "GAME.EXE", [view]).tsv, "start\tsize\n0x7F001000\t8\n0x7F001040\t16\n");
  assert.throws(() => joinInventories(image, "GAME.EXE", [{ ...view, text: "start\tsize\n0x7F002000\t4\n" }]), /executable sections/);
  assert.throws(() => joinInventories(image, "GAME.EXE", [{ ...view, text: "start\tsize\n1000:0000\t4\n" }]), /32-bit virtual address/);
  assert.throws(() => joinInventories(image, "GAME.EXE", [view], { codeRanges: [{ start: 0x7F001000, end: 0x7F001010 }] }), /MZ overlay code/);
  const path = inventoryPath("BLD-EXAMPLE", "GAME.EXE"), check = (text) => verifyInventory(image, "BLD-EXAMPLE", "GAME.EXE", text, path);
  assert.equal(check("start\tsize\tname\n0x7F001000\t8\tneutralHelper\n").rows, 1);
  assert.throws(() => check("start\tsize\n0x7f001000\t8\n"), /noncanonical/);
  assert.throws(() => check("start\tsize\nGAME.EXE+0x7F001000\t8\n"), /32-bit virtual address/);
  assert.throws(() => readPe32(syntheticPe({ magic: 0x20B })), /PE32\+/);
  assert.throws(() => readPe32(syntheticPe({ signature: "LE\0\0" })), /LE has no PE section table/);
  // The executable reader's loader refuses these files, so the inventory refuses them too.
  assert.throws(() => readPe32(syntheticPe({ sizeOfHeaders: 0x100 })), /header extent/);
  assert.throws(() => readPe32(syntheticPe({ textAddress: 0x100 })), /overlaps headers/);
  assert.throws(() => readPe32(syntheticPe({ dataVirtualSize: 0 })), /escapes image or overlaps headers/);
  // The command interface reads a pe32 source for inventories only.
  const dir = mkdtempSync(join(tmpdir(), "evidence-pe-")); t.after(() => rmSync(dir, { recursive: true, force: true }));
  const bytes = syntheticPe(); writeFileSync(join(dir, "game.exe"), bytes); writeFileSync(join(dir, "view.tsv"), view.text);
  const config = join(dir, "report.json"), cfg = { source: "game.exe", sourceKind: "pe32", xxh3: sourceXxh3(bytes), build: "BLD-EXAMPLE",
    manifest: "GAME.EXE", writeRoot: "out", views: [{ name: "text", path: "view.tsv", ranges: view.ranges }] };
  writeFileSync(config, JSON.stringify(cfg));
  const result = run(["inventory", config]);
  assert.equal(readFileSync(join(dir, "out", result.destination), "utf8"), "start\tsize\n0x7F001000\t8\n0x7F001040\t16\n");
  writeFileSync(config, JSON.stringify({ ...cfg, target: 0x7F001000 }));
  assert.throws(() => run(["incoming", config]), /MZ sources only/);
  writeFileSync(config, JSON.stringify({ ...cfg, sourceKind: "le" }));
  assert.throws(() => run(["inventory", config]), /sourceKind/);
});

test('physical PE transfers preserve signed/wrapping and interior targets, exclude padding/data, and fail closed', t => {
  const b=syntheticPe(), base=0x7f001000;
  b.writeUInt32LE(0x80,0x178+8); b.writeUInt32LE(0x100,0x178+16); b.writeUInt32LE(0x200,0x178+20);
  b.writeUInt32LE(0x100,0x1a0+16); b.writeUInt32LE(0x300,0x1a0+20);
  const write=(file,site,target,op=0xe8)=>{b[file]=op;b.writeInt32LE((target-site-5)|0,file+1);};
  write(0x200,base,base+0x60); write(0x220,base+0x20,base+0x43); write(0x260,base+0x60,base+0x40,0xe9);
  write(0x290,base+0x90,base+0x44); write(0x300,0x7f002000,base+0x44);
  write(0x27c,base+0x7c,base+0x44);
  const cfg={sourceKind:'pe32',targets:[{start:base+0x40,end:base+0x50}],controls:[{start:base+0x60,end:base+0x61}]};
  let r=peTransferCandidates(b,cfg);
  assert.deepEqual(r.candidates.map(c=>[c.site-base,c.target-base,c.kind]),[[0,0x60,'call-rel32'],[0x20,0x43,'call-rel32'],[0x60,0x40,'jump-rel32']]);
  assert.equal(r.regions[0].fileEnd,0x280);
  assert.throws(()=>peTransferCandidates(b,{...cfg,limit:2}),/exceed limit/);
  assert.throws(()=>peTransferCandidates(b,{...cfg,controls:[{start:base+0x70,end:base+0x71}]}),/no physical candidate/);
  assert.throws(()=>peTransferCandidates(b,{...cfg,controls:cfg.targets}),/independent/);
  assert.throws(()=>peTransferCandidates(b,{...cfg,limit:0}),/limit/);
  assert.throws(()=>peTransferCandidates(b,{...cfg,targets:[{start:2,end:1}]}),/half-open/);
  const dir=mkdtempSync(join(tmpdir(),'physical-pe-')); t.after(()=>rmSync(dir,{recursive:true,force:true}));
  writeFileSync(join(dir,'source.bin'),b); const path=join(dir,'query.json');
  const query={...cfg,source:'source.bin',xxh3:sourceXxh3(b)}; writeFileSync(path,JSON.stringify(query));
  assert.equal(run(['pe-transfers',path]).candidates.length,3);
  writeFileSync(path,JSON.stringify({...query,xxh3:'0'.repeat(32)}));
  assert.throws(()=>run(['pe-transfers',path]),/xxh3 does not match/);
  b.writeUInt16LE(0x1c0,0x84); assert.throws(()=>peTransferCandidates(b,cfg),/i386/); b.writeUInt16LE(0x14c,0x84);
  b.writeUInt32LE(0xffffd000,0x98+28);
  write(0x200,0xffffe000,0x10); write(0x220,0xffffe020,0x20);
  r=peTransferCandidates(b,{targets:[{start:0x20,end:0x21}],controls:[{start:0x10,end:0x11}]});
  assert.deepEqual(r.candidates.map(c=>c.target),[0x10,0x20]);
});

test('physical relative forms require kind controls and preserve signed short and conditional destinations',()=>{
  const b=syntheticPe(), base=0x7f001000;
  b.writeUInt32LE(0x80,0x178+8); b.writeUInt32LE(0x100,0x178+16); b.writeUInt32LE(0x200,0x178+20);
  b.fill(0x90,0x200,0x300);
  const put=(offset,kind,target)=>{
    const site=base+offset, at=0x200+offset;
    if(kind==='jump-rel8'||kind==='conditional-rel8') {
      b[at]=kind==='jump-rel8'?0xeb:0x7f; b.writeInt8(target-site-2,at+1);
    } else {
      const conditional=kind==='conditional-rel32', length=conditional?6:5;
      b[at]=conditional?0x0f:kind==='call-rel32'?0xe8:0xe9;
      if(conditional)b[at+1]=0x80;
      b.writeInt32LE((target-site-length)|0,at+(conditional?2:1));
    }
  };
  const forms=['call-rel32','jump-rel32','jump-rel8','conditional-rel8','conditional-rel32'];
  const controls=forms.map((kind,i)=>({start:base+0x60+i,end:base+0x61+i,kind}));
  forms.forEach((kind,i)=>put(i*8,kind,controls[i].start));
  put(0x30,'jump-rel8',base+0x41); put(0x34,'conditional-rel8',base+0x42);
  put(0x38,'conditional-rel32',base+0x43);
  const cfg={forms,targets:[{start:base+0x40,end:base+0x50}],controls};
  let r=peTransferCandidates(b,cfg);
  assert.deepEqual(r.controls.map(c=>c.candidates),[1,1,1,1,1]);
  assert.deepEqual(r.candidates.filter(c=>c.targetIndices.length).map(c=>[c.site-base,c.target-base,c.encodingBytes]),
    [[0x30,0x41,2],[0x34,0x42,2],[0x38,0x43,6]]);
  put(0x40,'jump-rel8',base+0x10); put(0x44,'conditional-rel8',base+0x11);
  put(0x48,'conditional-rel32',base+0x12);
  r=peTransferCandidates(b,{...cfg,targets:[{start:base+0x10,end:base+0x13}]});
  for(const target of [base+0x10,base+0x11,base+0x12])
    assert(r.candidates.some(c=>c.target===target&&c.targetIndices.length));
  put(0x7b,'conditional-rel32',base+0x43); // Complete in raw padding, not in mapped bytes.
  b[0x27f]=0xeb; b[0x280]=0xc0; // Short encoding crosses the mapped boundary.
  r=peTransferCandidates(b,cfg);
  assert(!r.candidates.some(c=>c.site===base+0x7b||c.site===base+0x7f));
  assert.throws(()=>peTransferCandidates(b,{...cfg,forms:['conditional-rel8','conditional-rel8']}),/unique/);
  assert.throws(()=>peTransferCandidates(b,{...cfg,forms:['loop-rel8']}),/supported/);
  assert.throws(()=>peTransferCandidates(b,{...cfg,controls:controls.slice(1)}),/every selected form/);
  assert.throws(()=>peTransferCandidates(b,{...cfg,controls:controls.map(({kind,...range})=>range)}),/selected kind/);
  assert.throws(()=>peTransferCandidates(b,{...cfg,controls:[null]}),/control range/);
  const wrong=controls.map(c=>({...c})); wrong[2].start++; wrong[2].end++;
  assert.throws(()=>peTransferCandidates(b,{...cfg,controls:wrong}),/no physical candidate/);
  assert.throws(()=>peTransferCandidates(b,{...cfg,limit:1}),/exceed limit/);
  assert(r.exclusions.includes('rel16 and LOOP/JCXZ-family transfers'));
});

test('the reader bodies report partitions body fragments and keeps entry placement separate',t=>{
 const b=synthetic();
 const dir=mkdtempSync(join(tmpdir(),'overlay-body-'));t.after(()=>rmSync(dir,{recursive:true,force:true}));
 writeFileSync(join(dir,'source.bin'),b);const path=join(dir,'query.json');
 const functions=[{entry:532,body:[{start:552,end:570}]},{entry:532,body:[{start:80,end:85}]},{entry:80,body:[{start:510,end:534}]}];
 const cfg={source:'source.bin',sourceKind:'mz',xxh3:sourceXxh3(b),formatControls:{overlays:1,fixups:1},functions};
 writeFileSync(path,JSON.stringify(cfg));
 const parts=(r,i)=>r.functions[i].fragments.flatMap(f=>f.parts);
 let r=run(['x86-bodies',path]);
 assert.deepEqual(parts(r,0).map(p=>[p.start,p.end,p.kind]),[[552,560,'overlay-code'],[560,562,'fixup-table'],[562,570,'zero-padding']]);
 assert.equal(r.functions[1].entry.kind,'overlay-code'); assert.equal(parts(r,1)[0].kind,'resident');
 for(let i=0;i<3;i++) assert.equal(parts(r,i).reduce((n,p)=>n+p.end-p.start,0),r.functions[i].bytes);
 b[565]=1; writeFileSync(join(dir,'source.bin'),b);
 writeFileSync(path,JSON.stringify({...cfg,xxh3:sourceXxh3(b)}));
 assert.notEqual(parts(run(['x86-bodies',path]),0)[2].kind,'zero-padding'); b[565]=0;
 writeFileSync(join(dir,'source.bin'),b);
 writeFileSync(path,JSON.stringify({...cfg,formatControls:{overlays:2}}));assert.throws(()=>run(['x86-bodies',path]));
});
