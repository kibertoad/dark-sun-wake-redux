// Physical candidates only: no decoding, function ownership or reachability claim.
import { readPe32 } from "./pe-image.mjs";

function range(value) {
  if (!value || !Number.isSafeInteger(value.start) || !Number.isSafeInteger(value.end)
      || value.start < 0 || value.end <= value.start || value.end > 2 ** 32)
    throw new Error("Expected a nonempty half-open 32-bit target range");
  return { start: value.start, end: value.end };
}

export function peTransferCandidates(bytes, { targets, controls, limit = 128 } = {}) {
  if (!Array.isArray(targets) || targets.length === 0 || targets.length > 32
      || !Array.isArray(controls) || controls.length === 0 || controls.length > 32)
    throw new Error("Supply 1..32 targets and independent positive controls");
  if (!Number.isSafeInteger(limit) || limit < 1 || limit > 16384)
    throw new Error("Candidate limit must be 1..16384");
  const requested = targets.map(range), checked = controls.map(range);
  if (checked.some(c => requested.some(t => c.start < t.end && t.start < c.end)))
    throw new Error("Positive controls must be independent of target ranges");
  const image = readPe32(bytes), candidates = [], hits = checked.map(() => 0), regions = [];
  if (bytes.readUInt16LE(bytes.readUInt32LE(0x3c) + 4) !== 0x14c)
    throw new Error("Physical rel32 scan supports i386 PE32 only");
  let total = 0;
  for (const section of image.sections.filter(s => s.code && s.mappedRawSize)) {
    const { rawStart, mappedRawSize, start, view } = section;
    regions.push({ view, fileStart: rawStart, fileEnd: rawStart + mappedRawSize,
      addressStart: start, addressEnd: start + mappedRawSize });
    for (let offset = 0; offset + 5 <= mappedRawSize; offset++) {
      const fileOffset = rawStart + offset, opcode = bytes[fileOffset];
      if (opcode !== 0xe8 && opcode !== 0xe9) continue;
      const site = start + offset, displacement = bytes.readInt32LE(fileOffset + 1);
      const target = (site + 5 + displacement) >>> 0;
      const targetIndices = requested.flatMap((r, i) => target >= r.start && target < r.end ? [i] : []);
      const controlIndices = checked.flatMap((r, i) => target >= r.start && target < r.end ? [i] : []);
      if (!targetIndices.length && !controlIndices.length) continue;
      if (++total > limit) throw new Error(`Physical transfer candidates exceed limit ${limit}`);
      for (const i of controlIndices) hits[i]++;
      candidates.push({ site, fileOffset, target, kind: opcode === 0xe8 ? "call-rel32" : "jump-rel32",
        targetIndices, controlIndices, classification: "raw candidate; boundary and reachability unverified" });
    }
  }
  for (let i = 0; i < hits.length; i++) if (!hits[i]) throw new Error(`Positive control ${i} has no physical candidate`);
  return { format: "PE32", imageBase: image.imageBase, targets: requested,
    controls: checked.map((r, i) => ({ ...r, candidates: hits[i] })), regions, candidates,
    exclusions: ["non-executable sections", "unmapped raw padding", "virtual-only bytes", "cross-region encodings",
      "rel8/rel16 and conditional transfers", "indirect/far/computed transfers", "runtime-written code"],
    scope: "E8/E9 plus four-byte signed displacement at every mapped executable byte; decoding and prefixes unverified, not verified callers" };
}
