import { test } from "node:test";
import assert from "node:assert/strict";
import { mkdtempSync, mkdirSync, writeFileSync, rmSync } from "node:fs";
import { tmpdir } from "node:os";
import { join } from "node:path";
import { listDisc, listInstall, readList, reconcile } from "../../tools/evidence/build-listing.mjs";
import {inspectOgg, oggCrc} from '../../tools/research/ogg-tracks.mjs';

// Independent bit-at-a-time checksum; synthetic pages never use the production CRC writer.
function bitCrc(bytes) {
  let value=0;
  for (let i=0;i<bytes.length;i++) {
    value ^= (i>=22 && i<26 ? 0 : bytes[i]) << 24;
    for (let j=0;j<8;j++) value=((value<<1)^((value>>>31)?0x04C11DB7:0))>>>0;
  }
  return value;
}
function oggPage(sequence,flags,lacing,body) {
  const page=Buffer.alloc(27+lacing.length+body.length);
  page.write('OggS'); page[5]=flags; page.writeBigUInt64LE(123n,6);
  page.writeUInt32LE(17,14); page.writeUInt32LE(sequence,18); page[26]=lacing.length;
  Buffer.from(lacing).copy(page,27); body.copy(page,27+lacing.length);
  page.writeUInt32LE(bitCrc(page),22); return page;
}
test('Ogg survey validates page-spanning packets and rejects corrupt framing and identities',()=>{
  const id=Buffer.alloc(30); id.write('\x01vorbis','ascii'); id[11]=2;
  id.writeUInt32LE(44100,12); id[28]=0xB8; id[29]=1;
  const comment=Buffer.concat([Buffer.from('\x03vorbis'),Buffer.alloc(248)]);
  const setup=Buffer.from('\x05vorbisopaque'), audio=Buffer.from([0,4]);
  const first=oggPage(0,2,[30],id);
  const middle=oggPage(1,0,[255],comment);
  const last=oggPage(2,5,[0,setup.length,audio.length],Buffer.concat([setup,audio]));
  const valid=Buffer.concat([first,middle,last]);
  assert.equal(oggCrc(first),bitCrc(first));
  assert.deepEqual(inspectOgg(valid).identification,{version:0,channels:2,rate:44100,blockSizes:[256,2048]});
  assert.equal(inspectOgg(valid).packets,4);
  const bad=(source,mutate,reseal=true)=>{
    const copy=Buffer.from(source); mutate(copy);
    if(reseal) copy.writeUInt32LE(bitCrc(copy),22);
    return copy;
  };
  const reject=parts=>assert.throws(()=>inspectOgg(Buffer.concat(parts)),/Ogg Survey:/);
  reject([bad(first,p=>p[28]^=1,false),middle,last]); // independently corrupt CRC
  reject([first,middle,last.subarray(0,last.length-1)]);
  reject([first,middle.subarray(0,27)]);
  reject([first,bad(middle,p=>p.writeUInt32LE(18,14)),last]);
  reject([first,bad(middle,p=>p.writeUInt32LE(4,18)),last]);
  reject([bad(first,p=>p[0]|=128),middle,last]);
  reject([bad(first,p=>p[29]|=128),middle,last]);
  reject([first,middle,bad(last,p=>p[5]=4)]); // missing continued packet
  reject([first,middle,bad(last,p=>p[5]=1)]); // missing EOS
  reject([bad(first,p=>p[5]=6),middle,last]); // premature EOS
  reject([first,middle,last,Buffer.from([0])]);
  reject([first,bad(middle,p=>p[28]=5),last]); // wrong second header kind
  for (const [offset,value] of [[7,1],[11,0],[12,0],[28,0x68],[29,0]]) {
    const invalid=Buffer.from(id); invalid[offset]=value;
    if(offset===12) invalid.writeUInt32LE(0,12);
    reject([oggPage(0,2,[30],invalid),middle,last]);
  }
  reject([first,middle,bad(last,p=>p[p.length-2]=1)]);
  reject([first,middle,oggPage(2,5,[255],Buffer.alloc(255))]);
  assert.throws(()=>inspectOgg(Buffer.alloc(16*1024*1024+1)),/file size/);
});

// One ISO 9660 directory record.
function record(extent, size, flags, id) {
  const name = Buffer.from(id, "latin1"), len = 33 + name.length + (name.length % 2 === 0 ? 1 : 0);
  const r = Buffer.alloc(len);
  r[0] = len; r.writeUInt32LE(extent, 2); r.writeUInt32BE(extent, 6); r.writeUInt32LE(size, 10); r.writeUInt32BE(size, 14);
  r[25] = flags; r.writeUInt16LE(1, 28); r.writeUInt16BE(1, 30); r[32] = name.length; name.copy(r, 33);
  return r;
}
const dir = (self, parent, ...entries) => Buffer.concat([record(self, 2048, 2, "\0"), record(parent, 2048, 2, "\x01"), ...entries]);

// A volume with GAME.DAT (3 bytes) at the root and SUB/LONG.BIN (2,500 bytes, two sectors) below it.
function image(sectorSize, dataOffset) {
  const user = new Map();
  const pvd = Buffer.alloc(2048); pvd[0] = 1; pvd.write("CD001", 1, "latin1"); pvd.write("SYNTH".padEnd(32), 40, "latin1");
  record(18, 2048, 2, "\0").copy(pvd, 156);
  const end = Buffer.alloc(2048); end[0] = 255; end.write("CD001", 1, "latin1");
  user.set(16, pvd); user.set(17, end);
  user.set(18, dir(18, 18, record(20, 3, 0, "GAME.DAT;1"), record(19, 2048, 2, "SUB")));
  user.set(19, dir(19, 18, record(21, 2500, 0, "LONG.BIN;1")));
  user.set(20, Buffer.from("abc"));
  const long = Buffer.alloc(2500, 7); user.set(21, long.subarray(0, 2048)); user.set(22, long.subarray(2048));
  const out = Buffer.alloc(23 * sectorSize, 0xEE);
  for (let i = 0; i < 23; i++) {
    const data = Buffer.alloc(2048); (user.get(i) ?? Buffer.alloc(0)).copy(data);
    data.copy(out, i * sectorSize + dataOffset);
  }
  return out;
}

test("disc listing walks the ISO 9660 tree of a raw image and hashes each file over its own bytes", (t) => {
  const root = mkdtempSync(join(tmpdir(), "listing-")); t.after(() => rmSync(root, { recursive: true, force: true }));
  for (const [layout, size, offset] of [["MODE2/2352", 2352, 24], ["MODE1/2352", 2352, 16], ["ISO", 2048, 0]]) {
    const file = join(root, `${layout.replace("/", "-")}.img`); writeFileSync(file, image(size, offset));
    const disc = listDisc(file, layout);
    assert.equal(disc.volume, "SYNTH");
    assert.deepEqual(disc.files.map((f) => [f.path, f.size]), [["CD:GAME.DAT", 3], ["CD:SUB/LONG.BIN", 2500]]);
    assert.equal(disc.files[0].xxh3, "06b05ab6733a618578af5f94892f3950");
  }
  const wrong = join(root, "wrong.img"); writeFileSync(wrong, image(2352, 24));
  assert.throws(() => listDisc(wrong, "MODE1/2352"), /No primary volume descriptor/);
  assert.throws(() => listDisc(wrong, "MODE3"), /Unknown sector layout/);
});

test("reconcile reports unlisted, absent, mismatched and stale paths, and directory exclusions cover their files", (t) => {
  const root = mkdtempSync(join(tmpdir(), "listing-")); t.after(() => rmSync(root, { recursive: true, force: true }));
  mkdirSync(join(root, "game", "saves"), { recursive: true });
  writeFileSync(join(root, "game", "A.EXE"), "abc"); writeFileSync(join(root, "game", "saves", "S.SAV"), "x");
  writeFileSync(join(root, "game", "WRAP.CFG"), "");
  assert.deepEqual(listInstall(join(root, "game")).map((f) => f.path), ["A.EXE", "WRAP.CFG", "saves/S.SAV"]);
  writeFileSync(join(root, "m.yaml"), "files:\n  - path: A.EXE\n    format: MZ\n    size: 3\n    xxh3: 06b05ab6733a618578af5f94892f3950\n  - path: \"CD:B.DAT\"\n    format: data\n    size: 1\n    xxh3: 00\n");
  writeFileSync(join(root, "o.yaml"), "other_files:\n  - path: WRAP.CFG\n    reason: wrapper\n  - path: saves/\n    reason: written at run time\n  - path: GONE.TXT\n    reason: stale\n");
  const manifest = readList(join(root, "m.yaml"), "files"), other = readList(join(root, "o.yaml"), "other_files");
  assert.deepEqual(manifest.map((f) => f.path), ["A.EXE", "CD:B.DAT"]);
  const listing = [
    { path: "A.EXE", size: 3, xxh3: "06b05ab6733a618578af5f94892f3950" },
    { path: "WRAP.CFG", size: 0, xxh3: "-" },
    { path: "saves/S.SAV", size: 1, xxh3: "-" },
    { path: "CD:EXTRA.EXE", size: 9, xxh3: "ff" },
  ];
  const { rows, unused } = reconcile(listing, manifest, other);
  assert.deepEqual(rows.map((r) => [r.kind, r.path, !!r.mismatch]), [
    ["manifest", "A.EXE", false], ["other", "WRAP.CFG", false], ["other", "saves/S.SAV", false],
    ["unlisted", "CD:EXTRA.EXE", false], ["absent", "CD:B.DAT", false],
  ]);
  assert.deepEqual(unused, ["GONE.TXT"]);
  const changed = reconcile([{ ...listing[0], xxh3: "00" }], manifest.slice(0, 1), []);
  assert.equal(changed.rows[0].mismatch, true);
  assert.throws(() => readList(join(root, "o.yaml"), "files"), /does not start with files:/);
});
