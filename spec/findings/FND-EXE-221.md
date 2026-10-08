---
id: FND-EXE-221
title: Descriptor 198 has an independently bounded adjacent dispatch table with three undecoded candidate targets
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000874D3..0x000874E7
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00087504..0x0008751A
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x00087552..0x00087576
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x00087622..0x00087646
tool: Ghidra 12.1.3 PUBLIC, engine 13.6.0 and bounded source table reader
environment: null
---

## Observation

In the corrected mapped snapshot, descriptor 198's procedure has a second
dispatch prefix distinct from FND-EXE-173's eleven-entry table. The prefix
at descriptor-relative offset `0x01C3` requires its selector word to equal
two, copies the other word to the index, decrements it at 16-bit width,
rejects an unsigned result above seventeen, doubles the admitted index,
and jumps through a CS-relative word table at displacement `0x0242`.
Thus this local arm admits input words one through eighteen and eighteen
two-byte slots. Its bound is independent of the eleven-entry arm at
displacement `0x022C`, whose own subtraction and unsigned comparison were
checked separately in the same snapshot.

With the descriptor code base at shipped offset `0x00087310`, the second
table occupies `0x00087552..0x00087576`. All eighteen stored offsets are
below the descriptor's 1468-byte code size. They have four distinct values:

| Admitted input word | Stored descriptor-relative offset |
|---|---|
| 1 through 4, and 18 | `0x01D7` |
| 5 through 12, and 15 through 17 | `0x01F4` |
| 13 | `0x01E5` |
| 14 | `0x01F1` |

The competing analyzer-alias base from FND-EXE-173 puts that same
displacement at `0x00087622..0x00087646`. Only four of its eighteen words
are below 1468; their minimum is zero and maximum is 64094. Both physical
reads completed at their eighteen-row limit without truncation.

The saved corrected listing has no instruction at three descriptor-base
candidate targets: offsets `0x01D7`, `0x01E5` and `0x01F1` are
undisassembled in mapped memory. Each query reports the next instruction
at `0x01F4`. The fourth candidate has a decoded three-byte selector-mask
test, the start of the separately bounded eleven-entry arm. This is an
independent positive decoding control, not a proof of native entry.

## Interpretation

The adjacent table cannot inherit the other table's eleven-entry bound.
Its descriptor-base words fit the overlay, but the saved listing omits
three candidate instruction starts. Corrected relocation mapping and
valid-looking target offsets therefore do not by themselves produce a
complete function body or a validated inventory denominator.

These are conditional table interpretations. Actual native CS production,
procedure entry, full input provenance, candidate decoding and reachability,
and each remaining anomalous body fragment stay unresolved in Q-EXE-010.
The prefix reading neither proves the descriptor base is the runtime CS
nor proves the alias interpretation is impossible under every entry.

## Alternatives

One shared bound for both adjacent tables is ruled out by their distinct
index transformations and comparisons. Eighteen in-range words do not prove
eighteen decoded or reachable targets. The listing has only one decoded
start among their four distinct descriptor-relative values.

## How to reproduce

Use the corrected installed mapped snapshot from FND-EXE-174, whose
mapped-source XXH3 is `f48148049b7bd26475845bfcbeb2d11e`, read-only with
automatic analysis disabled. With engine 13.6.0's ReportInstructionWindow,
query `9211:01C3` with count eight and separately `9211:01F4` with count
eight. Query each of `9211:01D7`, `9211:01E5`, `9211:01F1` and
`9211:01F4` with count one. Require each script's explicit output or
undisassembled-start diagnostic; a successful headless exit alone is
insufficient. Address aliases in printed exclusive ends name the same
linear bytes; convert them before interpreting span lengths.

Use `node tools/evidence/report.mjs table` at revision `86062e2` on the
shipped DSUN.EXE with XXH3 `e296af55ba2ecde7e77f555c90f33d0b`. Query
start `0x00087552`, then separately `0x00087622`, with count eighteen,
stride two, limit eighteen, and field targetOffset at offset zero, width
two. CountEvidence is the independently read decrement, unsigned bound
seventeen and doubling. Compare all returned words against code size 1468.
Keep configs, listings and results in GAME_DIR; none is committed.
