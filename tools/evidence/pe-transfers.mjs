// Physical candidates only: no decoding, function ownership or reachability claim.
import { readPe32 } from "./pe-image.mjs";

function range(value) {
  if (!value || !Number.isSafeInteger(value.start) || !Number.isSafeInteger(value.end)
      || value.start < 0 || value.end <= value.start || value.end > 2 ** 32)
    throw new Error("Expected a nonempty half-open 32-bit target range");
  return { start: value.start, end: value.end };
}

const FORMS = ['call-rel32', 'jump-rel32', 'jump-rel8', 'conditional-rel8', 'conditional-rel32'];

export function peTransferCandidates(bytes, { targets, controls, limit = 128, forms } = {}) {
  if (!Array.isArray(targets) || targets.length === 0 || targets.length > 32
      || !Array.isArray(controls) || controls.length === 0 || controls.length > 32)
    throw new Error("Supply 1..32 targets and independent positive controls");
  if (!Number.isSafeInteger(limit) || limit < 1 || limit > 16384)
    throw new Error("Candidate limit must be 1..16384");
  const explicitForms = forms !== undefined;
  if (explicitForms && (!Array.isArray(forms) || !forms.length || forms.length > FORMS.length
      || new Set(forms).size !== forms.length || forms.some(f => !FORMS.includes(f))))
    throw new Error('Supply unique supported transfer forms');
  const selected = forms ?? ['call-rel32', 'jump-rel32'];
  const requested = targets.map(range), checked = controls.map(c => {
    if (!c || typeof c !== 'object') throw new Error('Expected a control range');
    if ((explicitForms || c.kind !== undefined) && !selected.includes(c.kind))
      throw new Error('Each explicit-form control must name a selected kind');
    return { ...range(c), ...(c.kind === undefined ? {} : { kind: c.kind }) };
  });
  if (explicitForms && selected.some(f => !checked.some(c => c.kind === f)))
    throw new Error('Supply an independent positive control for every selected form');
  if (checked.some(c => requested.some(t => c.start < t.end && t.start < c.end)))
    throw new Error("Positive controls must be independent of target ranges");
  const image = readPe32(bytes), candidates = [], hits = checked.map(() => 0), regions = [];
  if (bytes.readUInt16LE(bytes.readUInt32LE(0x3c) + 4) !== 0x14c)
    throw new Error("Physical relative-transfer scan supports i386 PE32 only");
  let total = 0;
  for (const section of image.sections.filter(s => s.code && s.mappedRawSize)) {
    const { rawStart, mappedRawSize, start, view } = section;
    regions.push({ view, fileStart: rawStart, fileEnd: rawStart + mappedRawSize,
      addressStart: start, addressEnd: start + mappedRawSize });
    for (let offset = 0; offset < mappedRawSize; offset++) {
      const fileOffset = rawStart + offset, opcode = bytes[fileOffset];
      let kind, length, displacementOffset;
      if (opcode === 0xe8 || opcode === 0xe9) {
        kind = opcode === 0xe8 ? 'call-rel32' : 'jump-rel32'; length = 5; displacementOffset = 1;
      } else if (opcode === 0xeb) {
        kind = 'jump-rel8'; length = 2; displacementOffset = 1;
      } else if (opcode >= 0x70 && opcode <= 0x7f) {
        kind = 'conditional-rel8'; length = 2; displacementOffset = 1;
      } else if (opcode === 0x0f && offset + 1 < mappedRawSize
          && bytes[fileOffset + 1] >= 0x80 && bytes[fileOffset + 1] <= 0x8f) {
        kind = 'conditional-rel32'; length = 6; displacementOffset = 2;
      } else continue;
      if (!selected.includes(kind) || offset + length > mappedRawSize) continue;
      const site = start + offset, displacement = length === 2
        ? bytes.readInt8(fileOffset + displacementOffset) : bytes.readInt32LE(fileOffset + displacementOffset);
      const target = (site + length + displacement) >>> 0;
      const targetIndices = requested.flatMap((r, i) => target >= r.start && target < r.end ? [i] : []);
      const controlIndices = checked.flatMap((r, i) => target >= r.start && target < r.end
        && (r.kind === undefined || r.kind === kind) ? [i] : []);
      if (!targetIndices.length && !controlIndices.length) continue;
      if (++total > limit) throw new Error(`Physical transfer candidates exceed limit ${limit}`);
      for (const i of controlIndices) hits[i]++;
      candidates.push({ site, fileOffset, target, kind, encodingBytes: length,
        targetIndices, controlIndices, classification: "raw candidate; boundary and reachability unverified" });
    }
  }
  for (let i = 0; i < hits.length; i++) if (!hits[i]) throw new Error(`Positive control ${i} has no physical candidate`);
  return { format: "PE32", imageBase: image.imageBase, forms: selected, targets: requested,
    controls: checked.map((r, i) => ({ ...r, candidates: hits[i] })), regions, candidates,
    exclusions: ["non-executable sections", "unmapped raw padding", "virtual-only bytes", "cross-region encodings",
      ...FORMS.filter(f => !selected.includes(f)), "rel16 and LOOP/JCXZ-family transfers",
      "indirect/far/computed transfers", "runtime-written code"],
    scope: "Selected opcode forms at every mapped executable byte, with signed displacement and 32-bit wrapping; decoding and prefixes unverified, not verified callers" };
}
