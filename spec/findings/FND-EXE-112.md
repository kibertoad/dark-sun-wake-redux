---
id: FND-EXE-112
title: Callback zero branch reads the old index after progress and rereads count after scheduling or a virtual call
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x005A4970..0x005A49FB
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x005A4B1B..0x005A4B22
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x005A4B49..0x005A4B88
tool: Ghidra 12.1.3 PUBLIC bounded zero-branch progress and continuation reading
environment: null
---

## Observation

FND-EXE-109 records zero-word dispatch into `0x005A4970`. The zero
branch reads byte O plus `0x0149`, zero-extends it and calls
`0x005A41F0` with full arguments O, that byte value and zero. On normal
return it freshly reads record pointer R from O plus `0x0150`, then full
count C at R plus sixteen. The call's result is not tested. C zero ORs
mask `0x40` into byte O plus `0x0125` and goes to the ordinary epilogue
recorded in FND-EXE-109; it does not enter the following byte-read or
callback-scheduling path. The consecutive zero/nonzero branches after this
count test select those two paths with no intervening flag change.

For C nonzero the branch retains old full index I from R plus twelve,
decrements C and publishes it modulo thirty-two bits. When decremented C
is nonzero it forms and publishes I plus one modulo thirty-two bits; when
zero it retains I without incrementing. It reads full size L from R plus
eight and compares the selected index unsigned with L. Selected index at
least L subtracts L once and publishes that result; below L makes no further
index store. It then reads the current full base from R at zero and reads
the byte at base plus retained old I, not at the selected or reduced index.
Thus count/index stores precede the byte read, while the address retains the
old index. No local nonzero-size, index-bound or valid-storage check is shown.
Aliasing can affect the freshly read base or size and remains unresolved.

It tests byte O plus `0x0124`. Nonzero freshly reads full fields
`0x0108` and `0x010C` and calls `0x004F2490` with fixed target
`0x005A4BB0`, field `0x0108` as raw float bits, and field `0x010C`
as full callback argument. The just-read byte is not passed to that helper.
FND-EXE-105 describes its empty-free-list exit; this branch does not test its
result or locally retry. Zero `0x0124` instead freshly reads O's first
full table pointer and calls its offset-`0x1C` full target with O, the
zero-extended byte just read, and zero. It does not locally validate this
target or use the callee result as a predicate.

After either call returns normally it freshly rereads O plus `0x0150`
and that record's full count at sixteen. It does not reuse the earlier R
or decremented C for this decision. Fresh count nonzero goes directly to
the ordinary epilogue. Zero loads and zero-extends current byte `0x0118`,
ORs mask two into its low byte and enters the shared publication/priority
path at `0x005A4A09`. That path reads byte `0x011C`, publishes the
modified `0x0118`, selects `0x011E` and performs the gated state transfers
recorded in FND-EXE-110. The entering mask here is two rather than that
finding's value-seven mask sixteen; the shared suffix's tests and writes
are unchanged. Normal and exceptional callee effects, object/record lifetime
and field meaning remain unknown.

## Interpretation

Unlike value one's capture-before-progress path in FND-EXE-111, this zero
branch updates count/index before reading at the saved old index. Its final
count decision follows a fresh object-to-record lookup after either scheduling
or a virtual call. Callee mutations can therefore affect that decision and
cannot be replaced by the earlier count. Q-EXE-009 in FMT-EXE-006 still
requires record producers/bounds, virtual targets, the first callee's effects,
shared transfer effects and storage contracts. No complete consumer reading,
format promotion or actual PATH outcome follows.

## Alternatives

- The byte address uses retained old I even when the stored index advances.
- A single subtraction does not establish general modulo or a valid byte bound.
- Initial zero count marks the object and exits before scheduling or virtual
  dispatch; post-call zero count instead reaches shared flag publication.
- Scheduling and virtual dispatch are alternatives, not two calls made in order.
- Post-call count and record identity are fresh reads, not retained values.

## How to reproduce

Verify FND-EXE-011's executable identity and FND-EXE-099's physical controls.
FND-EXE-109 independently supplies dispatch and frame context. Use the saved
Ghidra program with -noanalysis and ReportInstructionWindow.java at
`0x005A47C0` limit 110 and `0x005A4970` limit 150. Restrict the zero
branch observations to the declared ranges, following the initial zero
exit at `0x005A4B1B`, nonzero progress at `0x005A4B49`, virtual call
at `0x005A4B73`, and shared continuation at `0x005A49E1`. Read the
shared suffix identified in FND-EXE-110 with entry mask two. Track old versus
stored index, full arithmetic, post-store base read, alternative calls and
fresh record/count rereads. Keep reports in GAME_DIR/analysis/exe-batches;
commit no original listings or bytes and execute neither interpreter nor game.