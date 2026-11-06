---
id: FND-PARTY-270
title: Tag-prefix operand census finds only its copy-source argument and no direct overlapping scalar writer
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000704C9..0x0007054D
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00071C08..0x00071C41
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1BAD:0003..1BAD:0017
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 487F:0000..487F:0012
tool: tools/research/exec-census/window_operands.py, resident_listing.py and overlay_listing.py at 7b5eb22b; Python 3.12.10, Capstone wheel 5.0.9 and xxhash 4.0.1; FND-PARTY-066/215/219/269 and FND-RNG-056 reuse
environment: null
---

## Observation

Q-PARTY-124 retains the source and frame conditions of the two tag-builder
calls classified in FND-PARTY-269. The source is installed DSUN.EXE,
634416 bytes, XXH3-128 `e296af55ba2ecde7e77f555c90f33d0b`.
The census uses coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv at 7b5eb22b;
its LF-normalized SHA-256 remains
`69555e580c3964e90ec43822629c29e3b909ecc264353b4dec0ccc82bdbf76ea`.

The existing window_operands reporter searches encoded memory
displacements and non-relocated, non-branch immediate values, first in
the inventory's linear decodes and then at every other byte of the
resident image and overlay code. The prefix window 1672..167A has one
inventory operand: overlay 187 +06A9 pushes the copy-source offset 1672.
Expanding the window to 166F..167A has the same result. The three added
lower offsets include every possible start of an ordinary byte, word or
doubleword store that could overlap the first prefix byte. There is no
fixed-displacement scalar writer to classify in that window.

Both windows have the same three alternate-decoding candidates:

| Candidate | Relationship to surrounding decoded instructions |
|---|---|
| 1BAD:000D | Starts in the final operand byte of the instruction at 000B; its apparent SS-indexed write is not an instruction of that decode. |
| 487F:0004 | Starts in the operand byte of the comparison at 0003; its apparent ENTER is not a prefix pointer or writer in that decode. |
| overlay 187 +1DF4 | Starts in the second byte of the OR at +1DF3; its apparent RET immediate is not a prefix pointer in that decode. |

These classifications do not prove that alternate entries are impossible.
No separate reachable entry at any of those three bytes is established.
The overlay context was read from inventory entry file 71C08, rather than
starting a new linear decode at the candidate itself.

The independent generation-state control window 42C0..42C4 returns 46
inventory operands, including the direct word stores at overlay 183
+0292/+02A4 and +053D recorded in FND-PARTY-066. FND-PARTY-269's
independently read copy-source push is also recovered. Thus the empty
prefix-writer result is accompanied by recovered memory-write and literal
address controls. The control window's nine alternate-decoding candidates
remain separate from its inventory instructions.

The reporter labels four operations in the builder's inventory row as
string stores. All four are actually MOVSX at +06DC, +06E5, +06FA and
+070E: its mnemonic prefix test also matches MOVSX as MOVS. This local
reporter diagnostic is not evidence of four string destinations. The
operand matches above are independently selected by operand type and
encoded displacement; they do not use that string counter.
The connected helper contracts in FND-PARTY-269 already bound
the eight-byte copy and at-most-five-byte numeric output. Under its
intact-frame and DS/SS conditions, the conversion and uppercase helpers
cannot reach the separate copied prefix. No additional callback or
interrupt instruction occurs in those local helper bodies.

Existing segment evidence must be reused at its actual scope.
FND-PARTY-215 records startup's explicit SS replacement and initialization
dispatch, and FND-PARTY-219 observes DS equal to SS at game-main entry.
The latter does not observe either later tag-builder call. FND-RNG-056's
abstract DS walk excludes indirect paths and assumes overlay trampolines
preserve DS; it cannot turn that main-entry observation into a universal
preservation result for these callers.

## Interpretation

Remove an unknown direct fixed-displacement prefix writer, overlapping
byte/word/doubleword writer, and literal in-window pointer producer from
the remaining inventory-decoded source alternatives. Retain the one known
copy-source use. Under FND-PARTY-269's local conditions, the copied prefix
also needs no additional unspecified child-preservation investigation.

The actual remaining source mechanisms are computed or copied destination
addresses, segment aliases, block operations or instructions outside the
tested decode. A pointer formed from a base outside this window can reach
the prefix without encoding its offset. Larger or wrapping accesses are
not covered by the three-byte overlap margin. Actual segment preservation
from the observed main entry to either caller remains distinct; do not
repeat startup equality as though it had never been observed.

This narrows Q-PARTY-124 without declaring the prefix immutable or either
reader unreachable. FMT-PARTY-007 remains supported; no complete reading
or answered queue deletion follows.

## Alternatives

- A zero fixed-displacement writer count is not a whole-program no-write
  result. Register-only accesses, string destinations, aliases and addresses
  assembled from outside the window remain outside that count.
- Inventory decodes and the raw second pass do not establish all possible
  native instruction streams. The three inside-instruction candidates
  cannot be promoted to writers merely because their bytes decode.
- Startup/main equality and an abstract DS-preservation walk do not prove
  the later tag-builder's actual DS/SS state or frame validity.

## How to reproduce

With the locked evidence Python, run window_operands.py with installed
DSUN.EXE, the stated inventory and windows `1672..167A 42C0..42C4`, then
`166F..167A 42C0..42C4`. Retain the controls, candidate classifications
and string-operation counts, not only the prefix summary.

Use resident_listing.py for `1BAD:0003..1BAD:0017` and
`487F:0000..487F:0012`, and overlay_listing.py for descriptor 187,
file `71C08..71C41`. Reuse FND-PARTY-269's builder and connected helper
reports and FND-PARTY-066's independent state-write reading. The new
local reports under GAME_DIR/analysis/class-table-closure-20261106/ have
prefix-source-operands, prefix-overlap-operands,
prefix-alternate-resident-context and prefix-alternate-overlay-anchored
stems. Reuse FND-PARTY-215/219 and FND-RNG-056 for the segment limits.
No original game ran.
