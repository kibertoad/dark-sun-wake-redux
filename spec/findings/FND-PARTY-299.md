---
id: FND-PARTY-299
title: The reflection helper distinguishes intact DATA 1001 from a second null result under the pinned default vector bytes
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00092115..0x000921C3
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:0452..1000:046E
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00084C1C..0x00084C59
  - build: BLD-GOG-EN-1.1
    file: RESOURCE.GFF
    offset: 0x000186CB..0x0001890B
tool: Ghidra 12.1.3 read-only INST-handler-20261106; packaged ReportMemoryBlockForFileOffset.java, ReportDecompileMatches.java and ReportDecompileWindow.java; overlay_listing.py/resident_listing.py at 1be5a507, Python 3.12.10, Capstone 5.0.9, xxhash 4.0.1; SRC-RUSTDOS-181 and FND-PARTY-068/298
environment: null
---

## Observation

FND-PARTY-298 leaves the helper after the failed-getter flag unresolved.
The reflection caller supplies one as the second argument to overlay 208
+0885 and takes its queued-number branch only for signed AX at most
eight. This reads the helper's complete owned span, 174 bytes, from
file 92115 through the far return at 921C2. Offsets below are hex;
returned numbers and counts identified as decimal are decimal.

### Copy and return calculation

Exact first argument FFFF returns AX zero without a getter call.
Other values call 2C5F:0003, then copy 49 bytes (73 decimal) from
its returned far pointer to a local buffer at SS:BP-4E. There is no
null-result test before the copy. The helper rebinds its working pointer
to that local buffer. The copy ends at BP-6, before the separate local
pointer at BP-4; admitted frame/stack and source backing remain conditions.

Resident 1000:0452 clears direction, halves CX, copies words, then
uses the retained odd-count carry to copy the final byte. Thus this
call copies 36 words and one byte. It restores DS/SI/DI and removes
the two far-pointer arguments. This confirms the count independently
of an inferred decompiler signature; it is not a resource-length read.

For copied type byte +0C equal to 3, 4, 5, 7 or 8, the helper sets
its result word to eight. Other types form the word sum of copied
word +09 and unsigned byte +0B times the unsigned low byte of the
second argument. Only the low product and word-sized sum are retained.
Copied byte +17 equal to FF skips another test. Otherwise that byte
is sign-extended to a word, multiplied by 0012h (18 decimal), and used to read field
+05 in the fixed table at segment 51F1. Bits 2..3 equal to one
double the result with word wrap; other values leave it unchanged.
The common ending returns one for a zero result and otherwise returns
the result word. Instructions at +0903/+0904 use word sign extension
and multiplication despite Capstone's generic CWDE spelling.

There is no own DI operand. The getter and copy's saved-memory/return
contracts still matter to the caller's retained number; the AX result
alone does not settle that separate preservation obligation.

### Intact DATA 1001

FND-PARTY-068 reads its first 72 payload bytes as zero. All fields
used above lie there. Type zero takes the arithmetic path; the base
and multiplier are zero, so the result is zero even if the later
table test doubles it. Under an intact copy and normal continuation,
the return is therefore one, which passes the caller's signed test.
The helper does not independently reject resource 1001 on this input.

### Second null result with default vector bytes

For a zero getter result the copy instead reads real-mode addresses
0000:0000 through 0000:0048 inclusive. SRC-RUSTDOS-181's unchanged default source table
puts vectors two, three and five at F000:FF53. Its relevant fields
then give these values:

| Copied field | Vector-table identity | Value under that default |
|---|---|---|
| Word +09 | Vector two offset high byte and segment low byte | 00FF |
| Byte +0B | Vector two segment high byte | F0 |
| Byte +0C | Vector three offset low byte | 53 |
| Byte +17 | Vector five segment high byte | F0 |

Type 53 takes the arithmetic branch. With second argument one, the
result before the optional doubling is 255 + 240 = 495 decimal.
The table read can leave it 495 or double it to 990; neither wraps
or has its sign bit set, and both exceed eight. Its table contents
need not be assumed to choose between those two rejecting results.
Normal table access/return is still a condition, not proof of mapped
storage for every signed index. These source defaults do not establish
the live release's vectors or an actual original-game failure.

## Interpretation

The two getter calls have distinct roles. Under the pinned source's
unchanged default vector bytes, a first null result passes the initial
flag (FND-PARTY-298), but a second null result fails this helper's
threshold. An intact first payload fails the initial flag, while an
intact second payload makes this helper return one. A first-failure,
second-intact sequence therefore passes these two necessary conditions;
this is a conditional path, not evidence it occurs or queues 1001.

Q-PARTY-134 retains actual getter outcome/content transitions, active
vector bytes, caller DI/frame preservation, caster and queue admission.
The unknown helper-result calculation is removed for these named cases.
Stale/partial buffers and changed vectors remain explicit alternatives.
Q-PARTY-124 retains both getter accesses. FMT-PARTY-007 stays supported;
no complete reading, whole-site exclusion or queue closure is declared.

## Alternatives

- Two failed requests do not imply the same branch as a failed request
  followed by an intact copy; the helper uses a second getter result.
- Treating a nonzero helper return as rejection ignores the actual signed
  threshold: intact DATA 1001 produces one and passes it.
- Changed vector fields or partial/stale copied bytes can change the
  calculation; the source-default result does not cover those states.

## How to reproduce

Use the installed DSUN.EXE identity from FND-PARTY-295 and its current
inventory row 00092115..000921C3. In the read-only Ghidra project,
ReportMemoryBlockForFileOffset.java 92115 maps it to fbov208::09cf15.
Run ReportDecompileMatches.java there with literals -1 and 73; the
former matches the signed sentinel and signed-byte test. The initial
hex-literal query misses its controls and supplies no negative proof.
A separate twelve-line opening window gives navigation/types only.

Verify every owned instruction with overlay_listing.py descriptor 208,
file 92115..921C3, and resident_listing.py 1000:0452..046E. Check the
copy count, no null guard, byte/word arithmetic and signed caller test.
Reuse FND-PARTY-068 and SRC-RUSTDOS-181's immutable default-vector
construction for the two input cases. Reports reflection-helper-map,
decompile, window, controls, context and reflection-copy-context remain
under GAME_DIR/analysis/handler-census-20261106/. No original game,
emulator session or emulated call ran.
