---
id: FND-COMBAT-114
title: Restore file copying resolves DOS drive and directory state and normalizes each component to eight plus three characters
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 455B:0001..455B:0256
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 455B:025C..455B:0298
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 455B:029A..455B:02D3
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 455B:02D7..455B:049E
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 455B:04A4..455B:05DF
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 44DE:041F..44DE:0426
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 44DE:0541..44DE:0569
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:09B0..1000:09DC
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:40D7..1000:40F6
tool: tools/research/exec-census/resident_listing.py at 990db12e; locked evidence Python and Capstone
environment: null
---

## Observation

FND-COMBAT-113's copy helper calls resident 455B:0001 for each
pathname before opening either file. It supplies explicit far output
addresses in its SS frame. This reading settles the local pathname
transformation, separately from the actual input strings, runtime tables,
directory results, buffer admission and file identity in Q-COMBAT-018.
All offsets below are hexadecimal.

### Drive and directory preparation

0001 initializes a result word to FFFF and output index to zero.
It obtains the input length through 1000:40D7; zero returns FFFF
after writing an output terminator. That length helper scans at most
FFFF bytes, returns zero for a null far pointer and supplies no caller
capacity or independent proof of a terminated string.

For a nonempty string, a first byte admitted by the current DS:3575
classification table with mask 0C, followed by a colon, supplies the
drive. The byte is passed through 1000:09B0 and reduced by ASCII a
to a zero-based drive index; the input offset advances by two. Otherwise
44DE:041F requests INT 21h/AH 19 and zero-extends its returned AL as
the drive index. The helper writes index plus ASCII a and a colon.

An immediately rooted remainder beginning with slash or backslash
adds one backslash and consumes that separator. A relative remainder
instead writes a backslash at output offset two, calls 44DE:0541 with
drive index plus one and output offset three, and rejects AX FFFF.
0541 requests AH 47 with that drive and far DS:SI output; it maps
carry-clear to AX zero and carry-set to FFFF, retaining the error in
DS:33BE. Its success does not independently establish supplied bytes,
their termination or output extent.

The relative branch passes the output starting at offset two through
02D7, rejects its nonzero result, converts the drive byte through
09B0 and obtains the resulting output length. It appends a backslash
when the output does not already end with one. Thus a relative path
depends on current per-drive directory state, not merely its input bytes.

### Remainder and normalization

0001 appends the unconsumed input through explicit far source and
destination pointers. Its index checks use the marker 50, decimal 80.
On a reached limit it writes a terminator at the current index and
returns FFFF; it does not silently report success for that limit exit.
Those checks alone are not a proof that every write, including the
directory request and failure terminators, fits a particular caller span.

After appending a terminator it calls 04A4 on output offset two.
That helper compacts the path in place, normalizes separators to
backslashes, removes a component consisting of a single dot, and
backs up one emitted component for a double dot. It rejects a backup
past its initial rooted boundary, or a dot-prefixed component whose
remainder is neither a separator nor the terminator. Ordinary components
are copied without character-class or eight-plus-three checks in this
pass. A trailing separator after a nonroot component is removed.

Only a zero 04A4 result reaches the second 02D7 pass. That helper
processes each component's basename until dot, separator, colon or
terminator. It tests every consumed ordinary byte through 025C, but
emits at most eight basename bytes. After a dot it emits at most three
extension bytes while consuming the rest; an empty extension removes
the emitted dot. It normalizes separators and rejects a trailing
separator on its own path. A colon is accepted only when immediately
followed by the string terminator; otherwise that local route fails.
Successful completion returns zero.

Character acceptance and conversion depend on current tables. 025C
accepts classification masks 0C or 02, otherwise uses 029A to look for
the byte in the near string at DS:33DA. 029A delegates the far byte
search to 1000:4000; that runtime search and the actual string contents
remain prerequisites rather than an inferred universal character set.
09B0 leaves a byte unchanged unless current classification mask 04
is set, in which case it adds 20. The local transformation therefore
cannot be promoted to a character-domain claim without table admission.

0001 reports zero only after both final normalization passes return
zero. It does not compare the two independently normalized copy paths
or determine whether DOS resolves them to distinct files.

## Interpretation

The unresolved local path helper in FND-COMBAT-113 is now accounted
for through its drive choice, relative-directory request, component
compaction, character gates, width limits and return tests. It is not
a byte-preserving pathname copy. Distinct long components can produce
the same emitted eight-plus-three spelling under admitted character
tables and successful compaction. That conditional transformation is
not evidence that any native save path collides or is truncated.

Q-COMBAT-018 retains the actual source and directory strings, table
and buffer lifetime/aliases, external drive/directory and file results,
and subsequent archive selection and contents. Absolute inputs skip
the directory request; relative inputs require its outcome. No native
copy, file modification or original execution was performed, and no
rule or parity status changes.

## Alternatives

- Returning zero is not proof that the emitted name preserves every input byte.
- Consuming an overlong component does not mean every consumed byte is emitted.
- A drive prefix without a following root separator still takes the directory path.
- Local component normalization does not establish DOS file identity.
- The output index marker does not admit an external directory write's capacity.

## How to reproduce

Resolve DSUN.EXE through tools/game-dir.mjs and locked evidence Python.
The reader verifies size 634416 and XXH3-128
e296af55ba2ecde7e77f555c90f33d0b. Use resident_listing.py with the
locations above. The 0001 inventory has disjoint ranges ending at
0090, 00B7, 00EC, 0191, 01B6 and 0256, resuming at 0092, 00B9,
00EE, 0193 and 01B8; skip those alignment gaps. Read 02D7 and
04A4 from their established entries and distinguish input advancement
from output advancement. Keep character tables and 1000:4000 as
explicit dependencies. Retain the original-derived report only in
GAME_DIR/analysis/hit-incoming-20261104/restore-path-normalization.txt.
No original listing, strings or file payload is committed.
