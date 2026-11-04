// Read-only disc-audio Survey. Packet bodies remain opaque; no sample decoder.
// Framing: https://www.xiph.org/ogg/doc/framing.html
// Identification: https://www.xiph.org/vorbis/doc/Vorbis_I_spec.html section 4.2.
import {readFileSync, statSync} from 'node:fs';
import {join, resolve} from 'node:path';
import {pathToFileURL} from 'node:url';
import {sourceXxh3} from '@scientific-method/executable-reader';
import {parseYaml} from '../../node_modules/@scientific-method/standard-checker/dist/yaml.js';
import {gameDir} from '../game-dir.mjs';

const MAX_FILE = 16 * 1024 * 1024, MAX_PACKET = 1024 * 1024;
const crcTable = Uint32Array.from({length:256}, (_, byte) => {
  let value = byte << 24;
  for (let bit=0; bit<8; bit++) value = (value << 1) ^ (value < 0 ? 0x04C11DB7 : 0);
  return value >>> 0;
});
export function oggCrc(page) {
  let crc = 0;
  for (let i=0; i<page.length; i++) {
    const byte = i>=22 && i<26 ? 0 : page[i];
    crc = ((crc << 8) ^ crcTable[((crc >>> 24) ^ byte) & 255]) >>> 0;
  }
  return crc;
}
const fail = message => { throw new Error(`Ogg Survey: ${message}`); };

export function inspectOgg(bytes) {
  if (!Buffer.isBuffer(bytes) || bytes.length<27 || bytes.length>MAX_FILE) fail('unsupported file size');
  let at=0, pages=0, packets=0, serial, ended=false, pending=0, parts=[], firstByte;
  let identification, finalGranule;
  while (at<bytes.length) {
    if (ended || at+27>bytes.length) fail('trailing data or truncated page header');
    if (bytes.toString('latin1',at,at+4)!=='OggS' || bytes[at+4]!==0) fail('capture/version');
    const flags=bytes[at+5], count=bytes[at+26], headerEnd=at+27+count;
    if (flags & ~7 || headerEnd>bytes.length) fail('flags or segment table');
    if (Boolean(flags & 2)!==(pages===0) || Boolean(flags & 1)!==(pending>0)) fail('BOS/continuation');
    const currentSerial=bytes.readUInt32LE(at+14), sequence=bytes.readUInt32LE(at+18);
    if (pages===0) serial=currentSerial;
    if (serial!==currentSerial || sequence!==pages) fail('serial/sequence; chained streams unsupported');
    let bodySize=0;
    for (let i=at+27;i<headerEnd;i++) bodySize+=bytes[i];
    const end=headerEnd+bodySize;
    if (end>bytes.length) fail('truncated page body');
    if (oggCrc(bytes.subarray(at,end))!==bytes.readUInt32LE(at+22)) fail('page CRC');
    let body=headerEnd;
    for (let i=at+27;i<headerEnd;i++) {
      const size=bytes[i];
      if (pending+size>MAX_PACKET) fail('packet limit');
      if (pending===0 && size) firstByte=bytes[body];
      if (packets<3) parts.push(bytes.subarray(body,body+size));
      pending+=size; body+=size;
      if (size===255) continue;
      if (!pending) fail('empty Vorbis packet');
      if (packets<3) {
        const packet=Buffer.concat(parts);
        if (packet.length<7 || packet[0]!==[1,3,5][packets] || packet.toString('latin1',1,7)!=='vorbis') fail('Vorbis header order/signature');
        if (packets===0) {
          if (packet.length!==30) fail('identification extent');
          const version=packet.readUInt32LE(7), channels=packet[11], rate=packet.readUInt32LE(12);
          const small=packet[28]&15, large=packet[28]>>>4;
          if (version!==0 || channels===0 || rate===0 || small<6 || large>13 || small>large || !(packet[29]&1)) fail('identification fields');
          identification={version,channels,rate,blockSizes:[2**small,2**large]};
        }
      } else if (firstByte & 1) fail('non-audio packet after headers');
      packets++; pending=0; parts=[]; firstByte=undefined;
    }
    if (pages===0 && (packets!==1 || pending)) fail('identification must occupy the first page alone');
    ended=Boolean(flags & 4);
    finalGranule=bytes.readBigUInt64LE(at+6);
    if (ended && (pending || end!==bytes.length)) fail('unfinished packet or data after EOS');
    pages++; at=end;
  }
  if (!ended || pending || packets<4 || finalGranule===0xFFFFFFFFFFFFFFFFn) fail('missing complete EOS/audio');
  return {bytes:bytes.length,pages,packets,serial,identification,finalGranule:finalGranule.toString(),
    limits:'page CRC/framing and identification validated; comment/setup bodies and audio samples not decoded'};
}

export function surveyTracks() {
  const dir=gameDir();
  if (!dir) return {skipped:'licensed GAME_DIR absent'};
  const manifestPath=new URL('../../spec/builds/BLD-GOG-EN-1.1.files.yaml',import.meta.url);
  const manifest=parseYaml(readFileSync(manifestPath,'utf8').replace(/^\uFEFF/,''),String(manifestPath),message=>fail(String(message)));
  const rows=[];
  for (let track=2;track<=41;track++) {
    const path=`MUSIC/Track${String(track).padStart(2,'0')}.ogg`;
    const record=manifest.files.find(row=>row.path===path);
    if (!record || record.size>MAX_FILE) fail(`missing/oversized manifest record: ${path}`);
    const input=join(dir,path), stat=statSync(input);
    if (!stat.isFile() || stat.size!==record.size) fail(`source size differs: ${path}`);
    const bytes=readFileSync(input), xxh3=sourceXxh3(bytes);
    if (xxh3!==record.xxh3) fail(`source hash differs: ${path}`);
    rows.push({path,track,xxh3,...inspectOgg(bytes)});
  }
  return {build:'BLD-GOG-EN-1.1',tracks:rows};
}
if (process.argv[1] && import.meta.url===pathToFileURL(resolve(process.argv[1])).href) {
  if (process.argv.length!==2) fail('usage: node tools/research/ogg-tracks.mjs');
  console.log(JSON.stringify(surveyTracks(),null,2));
}
