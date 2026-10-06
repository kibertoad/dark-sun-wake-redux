#!/usr/bin/env node
// Lists every file of an installed build and of the ISO 9660 volume on its raw disc image, and
// reconciles the listing with the build's manifest and its list of other files. Each installed file
// is listed by its path relative to the install directory; each disc file as `CD:` and its path on
// the disc, without the `;1` version suffix. A path in the listing must be in the manifest with the
// same size and xxh3, or in the other-files list. An other-files path ending in `/` covers the whole
// directory. A manifest path the listing does not hold is reported too.
//
//   node tools/evidence/build-listing.mjs <install dir> <disc image> <manifest> <other-files>
//
// The disc image is read as the track whose sectors the cue sheet gives as MODE2/2352: 2,352-byte
// sectors with 2,048 bytes of user data after a 24-byte sync, header and subheader. MODE1/2352 puts
// the data after 16 bytes, and an ISO file (2,048-byte sectors) after none; --sector gives either.
// Output is one line per path: `manifest`, `other` or `unlisted`, then the path, size and xxh3, with
// `MISMATCH` where size or hash differ from the manifest, and `absent` lines for manifest paths
// the listing lacks. The exit code is 1 when any path is unlisted, mismatched or absent.
import { readFileSync, readdirSync, statSync, openSync, readSync, closeSync } from "node:fs";
import { join, relative, resolve } from "node:path";
import { pathToFileURL } from "node:url";
import { sourceXxh3 } from "@scientific-method/executable-reader";

const layouts = { "MODE2/2352": [2352, 24], "MODE1/2352": [2352, 16], ISO: [2048, 0] };

export function listInstall(root) {
  const out = [];
  const walk = (dir) => {
    for (const name of readdirSync(dir).sort()) {
      const full = join(dir, name), st = statSync(full);
      if (st.isDirectory()) walk(full);
      else out.push({ path: relative(root, full).split("\\").join("/"), size: st.size, full });
    }
  };
  walk(root);
  return out;
}

// Reads the primary volume descriptor at sector 16 and walks the directory tree from its root record.
export function listDisc(image, layout = "MODE2/2352") {
  const [sectorSize, dataOffset] = layouts[layout] ?? (() => { throw new Error(`Unknown sector layout ${layout}`); })();
  const fd = openSync(image, "r");
  const sector = (lba, count = 1) => {
    const out = Buffer.alloc(2048 * count);
    const raw = Buffer.alloc(sectorSize);
    for (let i = 0; i < count; i++) {
      if (readSync(fd, raw, 0, sectorSize, (lba + i) * sectorSize) !== sectorSize) throw new Error(`Short read at sector ${lba + i}`);
      raw.copy(out, i * 2048, dataOffset, dataOffset + 2048);
    }
    return out;
  };
  try {
    const pvd = sector(16);
    if (pvd[0] !== 1 || pvd.toString("latin1", 1, 6) !== "CD001") throw new Error("No primary volume descriptor at sector 16");
    const volume = pvd.toString("latin1", 40, 72).trimEnd();
    const files = [], seen = new Set();
    const walk = (lba, size, prefix) => {
      if (seen.has(lba)) throw new Error(`Directory loop at sector ${lba}`);
      seen.add(lba);
      const data = sector(lba, Math.ceil(size / 2048));
      for (let pos = 0; pos < size;) {
        const len = data[pos];
        if (len === 0) { pos = (Math.floor(pos / 2048) + 1) * 2048; continue; }
        const extent = data.readUInt32LE(pos + 2), length = data.readUInt32LE(pos + 10), flags = data[pos + 25];
        const idLen = data[pos + 32], id = data.subarray(pos + 33, pos + 33 + idLen);
        pos += len;
        if (idLen === 1 && (id[0] === 0 || id[0] === 1)) continue;
        if (flags & 0x80) throw new Error(`Multi-extent file ${prefix}${id.toString("latin1")} is not supported`);
        const name = id.toString("latin1").replace(/;\d+$/, "").replace(/\.$/, "");
        if (flags & 0x02) walk(extent, length, `${prefix}${name}/`);
        else files.push({ path: `CD:${prefix}${name}`, size: length, bytes: () => sector(extent, Math.ceil(length / 2048)).subarray(0, length) });
      }
    };
    walk(pvd.readUInt32LE(156 + 2), pvd.readUInt32LE(156 + 10), "");
    return { volume, files: files.map((f) => ({ path: f.path, size: f.size, xxh3: sourceXxh3(f.bytes()) })) };
  } finally {
    closeSync(fd);
  }
}

// Reads the `path`, `size` and `xxh3` of each item of a manifest's `files` list, or the `path`
// of each item of an `other_files` list. Only the item keys this tool needs are read.
export function readList(file, key) {
  const items = [];
  const lines = readFileSync(file, "utf8").split(/\r?\n/);
  if (lines[0].trim() !== `${key}:`) throw new Error(`${file} does not start with ${key}:`);
  for (const line of lines.slice(1)) {
    const m = /^ {2}(?:- | {2})(path|size|xxh3): *(.*)$/.exec(line);
    if (!m) continue;
    if (line.startsWith("  - ")) items.push({});
    let value = m[2].trim();
    if (/^".*"$/.test(value)) value = JSON.parse(value);
    else if (/^'.*'$/.test(value)) value = value.slice(1, -1).replace(/''/g, "'");
    items.at(-1)[m[1]] = m[1] === "size" ? Number(value) : value;
  }
  return items;
}

export function reconcile(listing, manifest, other) {
  const byPath = new Map(manifest.map((f) => [f.path, f]));
  const dirs = other.filter((o) => o.path.endsWith("/")).map((o) => o.path);
  const exact = new Set(other.filter((o) => !o.path.endsWith("/")).map((o) => o.path));
  const rows = [], listed = new Set();
  for (const f of listing) {
    listed.add(f.path);
    const m = byPath.get(f.path);
    if (m) rows.push({ kind: "manifest", ...f, mismatch: m.size !== f.size || m.xxh3 !== f.xxh3 });
    else if (exact.has(f.path) || dirs.some((d) => f.path.startsWith(d))) rows.push({ kind: "other", ...f });
    else rows.push({ kind: "unlisted", ...f });
  }
  for (const m of manifest) if (!listed.has(m.path)) rows.push({ kind: "absent", path: m.path, size: m.size, xxh3: m.xxh3 });
  const unused = [...exact].filter((p) => !listed.has(p)).concat(dirs.filter((d) => !listing.some((f) => f.path.startsWith(d))));
  return { rows, unused };
}

function main([install, image, manifestFile, otherFile, ...rest]) {
  if (!otherFile) throw new Error("Usage: node tools/evidence/build-listing.mjs <install dir> <disc image> <manifest> <other-files> [--sector MODE2/2352|MODE1/2352|ISO]");
  const layout = rest[0] === "--sector" ? rest[1] : "MODE2/2352";
  const manifest = readList(manifestFile, "files"), other = readList(otherFile, "other_files");
  const otherPaths = new Set(other.map((o) => o.path));
  // Files under a directory the other-files list leaves out are counted, never hashed.
  const installed = listInstall(resolve(install)).map((f) => ({
    path: f.path, size: f.size,
    xxh3: [...otherPaths].some((d) => d.endsWith("/") && f.path.startsWith(d)) ? "-" : sourceXxh3(readFileSync(f.full)),
  }));
  const disc = listDisc(image, layout);
  const { rows, unused } = reconcile([...installed, ...disc.files], manifest, other);
  console.log(`volume ${disc.volume}; installed ${installed.length}, disc ${disc.files.length}, manifest ${manifest.length}, other ${other.length}`);
  let bad = 0;
  for (const r of rows) {
    const flag = r.mismatch ? "\tMISMATCH" : "";
    if (r.mismatch || r.kind === "unlisted" || r.kind === "absent") bad++;
    console.log(`${r.kind}\t${r.path}\t${r.size}\t${r.xxh3}${flag}`);
  }
  for (const p of unused) { bad++; console.log(`stale-other\t${p}`); }
  const count = (k) => rows.filter((r) => r.kind === k).length;
  console.log(`manifest ${count("manifest")}, other ${count("other")}, unlisted ${count("unlisted")}, absent ${count("absent")}, mismatched ${rows.filter((r) => r.mismatch).length}, stale other ${unused.length}`);
  return bad ? 1 : 0;
}

if (process.argv[1] && import.meta.url === pathToFileURL(resolve(process.argv[1])).href) {
  try { process.exitCode = main(process.argv.slice(2)); }
  catch (error) { console.error(`build-listing: ${error.message}`); process.exitCode = 2; }
}
