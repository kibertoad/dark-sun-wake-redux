---
id: FND-COMBAT-109
title: The unchanged GPL 37 rejected span contains no relocation opcode byte but native fetch and dispatch preservation remain open
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: GPLDATA.GFF
    offset: 0x00202969..0x0020298A
  - build: BLD-GOG-EN-1.1
    file: GPLDATA.GFF
    offset: 0x0006A244..0x0006A245
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 172C:018F..172C:01C1
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 172C:20B3..172C:210B
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 172C:2805..172C:284D
tool: tools/evidence/report.mjs table and resident_listing.py at 5ee24d55; locked evidence Python; retained script_decode.py index of FND-COMBAT-100
environment: null
---

## Observation

FND-COMBAT-103 reads GPL/37's malformed expression and its native local
continuation. This reading asks whether the rejected source span itself
contains an encoded opcode 0x5E, independently of expression alignment.

### The bounded byte census has an independent positive control

The unchanged span is GPL/37 offsets 0x0D37 through 0x0D58, half-open:
33 bytes at GPLDATA.GFF file 0x202969. Repeating FND-COMBAT-103's
bounded one-byte table read gives zero values equal to 0x5E. Its first
two rows still supply the known 0x77 opcode and rejected 0xB3 expression
byte controls. The repeated report is byte-identical to the retained one.

A separate positive control reads the byte at GPL/119 offset 0x0547,
independently decoded as opcode 0x5E by FND-COMBAT-100/105. GPL/119's
resource begins at file 0x69CFD and has length 4092; the selected byte is
therefore at file 0x6A244, within the resource. The same table reporter
returns 0x5E there. Both reports identify GPLDATA.GFF by size 2191945 and
XXH3 2bd1d2b989eef2dcb0545feb95975fca.

No byte position in the unchanged 33-byte span can itself supply an encoded
0x5E, even if a different alignment reaches it as an instruction start.
This does not repair the malformed expression, determine native recovery,
or exclude effects on the fetched value, script storage or dispatch table.

### Native opcode delivery crosses preserved-state boundaries

The byte-fetch helper at 172C:281B reads through the current script-buffer
pointer using the current signed frame-depth byte, its frame-offset word
and the script's code-start word. Helper 2805 saves the byte in a local,
calls cursor increment 20B3, then reloads the saved byte. Cursor increment
can call the error entry if its current offset fails the capacity test.
No immutable pointer, frame, segment or local-byte storage is inferred.

Opcode fetch 20F5 calls 2805, stores AL in 4C13:032A and reloads that
byte for its return. The dispatcher checks the low opcode byte against
0x80. On the accepted branch it conditionally calls the far callback at
current DS:02F6, then rereads the low opcode byte from its incoming stack
slot before indexing the near-target table at current DS:030A. The
callback receives a separate zero-extended copy of the byte. There is no
second range test after that call. FND-SCRIPT-005 gives the initial table
mapping, including 0x5E to its relocation handler.

These are distinct source-byte, saved-local, opcode-field, stack-argument
and dispatch-table boundaries. A raw-byte absence cannot prove that all
of them retain the same value or target. Nor can it exclude a child
directly entering another script or handler. No callback effects or
admitted target mutation are established by this local body reading.

## Interpretation

Q-COMBAT-059 does not need to discover an encoded 0x5E hidden by a
different instruction alignment inside these unchanged bytes. Its remaining
zero-reader question concerns admitted source/buffer changes or replacement
of the fetch/dispatch context and values, including effects of the error
children and opcode-0x77's indirect byte write in FND-COMBAT-103.

The general incoming-transfer and parameter-admission obligation remains
Q-COMBAT-055. Q-SCRIPT-003 retains the error-child contract. Neither an
immediate-abort assumption nor an observed additional relocation follows.
No native absence, source immutability, complete reading or parity promotion
is claimed. FND-COMBAT-103's returning/error-ordering observations stand.

## Alternatives

- Unknown expression alignment alone cannot reveal an absent byte value in
  the unchanged bounded span. No speculative decoder repair is required for
  this encoded-byte question.
- Source-byte absence is not dispatch absence: saved values, current pointers,
  callback arguments and table targets still need preservation evidence.
- A malformed handler's indirect write might matter through its actual target;
  counting bytes before execution does not establish that target or its aliases.
- Fetches outside this span are not covered by its empty result. Use the
  decoded-site census and Q-COMBAT-055 for their independent admission.

## How to reproduce

Reuse FND-COMBAT-103's rejected-span.json and its fingerprinted archive.
Run report.mjs table with start 0x202969, count 33, stride one and one
unsigned one-byte field named value at row offset zero. Require rows zero
and one to be 0x77 and 0xB3. Count rows whose value equals 0x5E: zero.
Repeat and compare the report files; the accepted repeated report SHA-256
is e0edafbf682fa14044ca05d5f4eedf038e7f406e780e3f87e44a8e03e8f49dd4.
Inspect the named value field, rather than treating a missing property as
an empty result.

Independently obtain GPL/119's resource offset and length with
script_listing.py on the same GPLDATA.GFF, tag `GPL `, number 119. Its
instruction-decoding stop is diagnostic only; this control uses its GFF
resource header. Cross-check the instruction identity with the retained
decoded index, whose SHA-256 is
649122067103640f41d526cb3b1c4ca2bb853b4f71444cf5f6547cc809fc6445.
Run table on one byte at 0x69CFD plus 0x0547, with the same field and
stride. Require 0x5E and the matching archive identity before using the
negative span result. These values and bounds are the full control query.

Use resident_listing.py with the fingerprinted DSUN.EXE and the three
half-open resident ranges in Locations. Follow the actual saved-byte store
and reload, cursor/error call, opcode-field reload and stack-byte reread
after the optional callback. Preserve unresolved child and storage effects.

The original and repeated rejected-span reports, relocation-byte-control
config/report, opcode-byte-gate and opcode-fetch-helper listings remain
under GAME_DIR/analysis/script-gap-20261103/. The GPL/119 header control
remains under GAME_DIR/analysis/local-bank-preservation-20261103/.
No raw span, original listing or decoded script text is committed.
