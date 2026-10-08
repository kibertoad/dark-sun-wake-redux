---
id: FND-EXE-173
title: Overlay body anomalies include non-code fragments and a segment-alias dispatch discrepancy
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x0006D081..0x0006D150
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x00067D99..0x00067DCF
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x0008171E..0x0008173E
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x00094E46..0x00094E49
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x00094EB8..0x00094F5E
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x0008BD04..0x0008BDB3
  - build: BLD-GOG-EN-1.1
    file: CD:DSUN.EXE
    kind: file-data
    offset: 0x00055519..0x0005553D
  - build: BLD-GOG-EN-1.1
    file: CD:DSUN.EXE
    kind: file-data
    offset: 0x0007485E..0x00074890
  - build: BLD-GOG-EN-1.1
    file: CD:DSUN.EXE
    kind: file-data
    offset: 0x0007497D..0x000749AB
  - build: BLD-GOG-EN-1.1
    file: CD:DSUN.EXE
    kind: file-data
    offset: 0x00081642..0x000816AE
  - build: BLD-GOG-EN-1.1
    file: CD:DSUN.EXE
    kind: file-data
    offset: 0x000870C4..0x0008710E
  - build: BLD-GOG-EN-1.1
    file: CD:DSUN.EXE
    kind: file-data
    offset: 0x00087278..0x00087285
  - build: BLD-GOG-EN-1.1
    file: CD:DSUN.EXE
    kind: file-data
    offset: 0x0008913A..0x00089168
  - build: BLD-GOG-EN-1.1
    file: CD:DSUN.EXE
    kind: file-data
    offset: 0x00095EE2..0x000961E5
  - build: BLD-GOG-EN-1.1
    file: CD:DSUN.EXE
    kind: file-data
    offset: 0x0009933E..0x000995C3
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00087504..0x0008751A
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x0008753C..0x00087552
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x0008760C..0x00087622
tool: Source-layout classifier at b69bbdd, bounded table reader, and Ghidra 12.1.3 PUBLIC
environment: null
---

## Observation

The measured installed and disc overlay snapshots contain fifteen body ranges
that do not lie wholly within a declared overlay Code ranges row. These are
body fragments, not fifteen misplaced function entries. Source-layout reading
with FND-EXE-003's overlay counts and the complete fixup-count controls places
all fifteen associated entries inside declared overlay code. Thirteen spans
touch fixup tables, six touch verified zero padding, and one disc span is in
the resident load image. Spans crossing classes contribute to several counts.

| File | Function entry (descriptor) | Complete anomalous span partition |
|---|---|---|
| `DSUN.EXE` | `0x0006B581` (183) | `0x0006D081..0x0006D090` zero-padding; `0x0006D090..0x0006D150` overlay-code |
| `DSUN.EXE` | `0x0006C01D` (183) | `0x00067D99..0x00067DCF` overlay-fixups |
| `DSUN.EXE` | `0x0007A5A7` (190) | `0x0008171E..0x0008173E` overlay-fixups |
| `DSUN.EXE` | `0x00087459` (198) | `0x00094E46..0x00094E49` overlay-fixups |
| `DSUN.EXE` | `0x0008772D` (198) | `0x00094EB8..0x00094F3B` overlay-fixups; `0x00094F3B..0x00094F5E` zero-padding |
| `DSUN.EXE` | `0x0008AA6A` (202) | `0x0008BD04..0x0008BD90` overlay-fixups; `0x0008BD90..0x0008BDB3` zero-padding |
| `CD:DSUN.EXE` | `0x0005E2ED` (173) | `0x00055519..0x0005553D` resident-load-image |
| `CD:DSUN.EXE` | `0x000677FC` (180) | `0x0007485E..0x00074890` overlay-fixups |
| `CD:DSUN.EXE` | `0x0006EA00` (184) | `0x0007497D..0x00074988` overlay-fixups; `0x00074988..0x000749AB` zero-padding |
| `CD:DSUN.EXE` | `0x0007A577` (190) | `0x00081642..0x000816AE` overlay-fixups |
| `CD:DSUN.EXE` | `0x000877FD` (198) | `0x000870C4..0x0008710E` overlay-fixups |
| `CD:DSUN.EXE` | `0x00088A01` (199) | `0x00087278..0x00087285` overlay-fixups |
| `CD:DSUN.EXE` | `0x00092C03` (208) | `0x0008913A..0x00089168` overlay-fixups |
| `CD:DSUN.EXE` | `0x0009674B` (211) | `0x00095EE2..0x00095F1A` overlay-fixups; `0x00095F1A..0x00095F30` zero-padding; `0x00095F30..0x000961E5` overlay-code |
| `CD:DSUN.EXE` | `0x00097BED` (211) | `0x0009933E..0x00099371` overlay-fixups; `0x00099371..0x00099380` zero-padding; `0x00099380..0x000995C3` overlay-code |

These are physical classifications of analyzer-owned ranges, not a proof that
original execution enters the fragments. The resident fragment can be real code
without establishing its association with an overlay function. No region is
silently removed and no Code ranges row is widened.

### One computed-jump discrepancy

The installed snapshot associates the three-byte fixup fragment at file offset
`0x00094E46` with the function whose entry is `0x00087459`, descriptor 198.
Its decoded incoming reference is a computed jump at analysis alias `921E:0135`.
The artifact's decoded fragment starts with a saved code-segment value and a
jump; interpreting fixup bytes as instructions is not native reachability.

The bounded source-mapped dispatch prefix first tests a word with mask three.
On its nonzero arm it copies another word to the index, subtracts ten at
16-bit width, rejects an unsigned result above ten, doubles the admitted index,
and jumps through a CS-relative word table with displacement `0x022C`.
That permits eleven two-byte entries, independently of neighboring data.
Caller inputs, the complete procedure and runtime segment admission are unread.

FND-EXE-003 places descriptor 198's code at `0x00087310..0x000878CC`.
In the mapped artifact its base is `9211:0000`; the function is presented at
alias `921E:0079`. Those linear addresses describe its entry but the segment
bases differ by 208 bytes. Using the descriptor base for the table gives
shipped offsets `0x0008753C..0x00087552`; using the alias base gives
`0x0008760C..0x00087622`. Both bounded reads use the verified shipped file.
All eleven descriptor-base words are within the overlay's 1,468-byte code
extent, with offsets 522 through 535. Only three alias-base words are within
that extent; their values range from 276 through 65,331. A linear alias alone
does not prove the CS-relative accesses denote the same storage.

## Interpretation

The denominator anomalies include analyzer-owned data and padding fragments.
The bounded example supplies a concrete segment/table interpretation discrepancy,
not a finding that the shipped game executes fixup data or that every anomalous
body has the same cause. Q-EXE-010 retains native CS admission, jump-table target
validation and function-boundary reconciliation before inventory replacement.
The full loader contract remains Q-EXE-001. No status promotion, native run or
complete_reading declaration follows from these measurements.

## Alternatives

Fifteen entries outside declared overlay code is ruled out by the separate
entry classification. Treating every anomalous span as entirely non-code is
also wrong: three cross into code and one is in the resident load image.
A descriptor-base table and an analyzer-alias-base table remain competing
storage interpretations until the actual CS producer is established. Their
bounded word results favor the descriptor-base interpretation, but valid-looking
targets alone do not establish the original segment or all reachable paths.

## How to reproduce

At b69bbdd use `node tools/evidence/report.mjs overlay-bodies <local-config.json>`.
Set sourceKind to `mz`, formatControls.overlays to 49, and ranges to the complete
spans and separate entries in the table above. For the installed source use
XXH3 `e296af55ba2ecde7e77f555c90f33d0b` and formatControls.fixups 8262; for
the verified disc source use `318cd5ec0559901add3780097162a919` and fixups 8280.
The classifier uses the bounded published MZ/FBOV reader; do not substitute a
mapped derivative as the licensed source for physical classification.

Use the existing installed overlay project read-only with automatic analysis
disabled, whose mapped-image XXH3 is `4dbb333b307f78f77a32c8a7362a0799`.
Read two instructions at `9F9A:02A6`, eight at `921E:0079`, and thirty at
`921E:00F3`; query references to `9F9A:02A6`. Restrict the dispatch claim to
the cited source-mapped prefix, not subsequent instructions the window prints.

Use the bounded `table` report over the installed original with start `0x8753C`
and separately `0x8760C`, count 11, stride 2, limit 11, and one field
`targetOffset` with offset 0 and width 2. Set countEvidence to the unsigned
zero-through-ten bound after subtraction and doubling. Compare each word
against the descriptor's code size 1468, retaining both storage interpretations.
The finding states every query value; configurations and rich results stay in
GAME_DIR and no original code or broad analysis export is committed.