---
id: FND-COMBAT-147
title: Map reconstruction requests a region archive and applies two bitmap planes through bounded-coordinate byte flag setters
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00071B1E..0x00071C08
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00068AE7..0x00068BB5
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 25AF:017F..25AF:023B
tool: tools/research/exec-census/overlay_listing.py and resident_listing.py at cc8dd9f7; project-local Python 3.12.10 and Capstone 5.0.7; inventory entry and overlay fixup checks
environment: null
---

## Observation

Q-COMBAT-066 retains the reached reconstruction children of the numbered
reader. FND-COMBAT-111 identifies overlay 187 +1CEE after the transfer loop.
FND-SAVE-022 and FND-EXE-639 give earlier summaries. This reads its complete
own body, the region wrapper 182 +0297 and all four leaf flag setters to
bound their resource and memory effects. Offsets and masks below are
hexadecimal unless a quantity is explicitly decimal.

### Full-pointer gate, resource result and cleanup

The helper initializes its bit index, byte index and result byte to zero.
Its entry gate compares the complete double word DS:0538 with zero. Null
jumps directly to the final result return, skipping both the region-open
request and the later region-handle close test. The gate is not a low-word
test: offset zero with a nonzero segment is a nonnull far pointer.
FND-EXE-639 replaces the old inventory summary for this width correction.

For a nonnull pointer it calls 182 +0297 with current region DS:140C and
discards that call's argument without testing its returned value. It then
rereads DS:140C, zero-extends it, and requests GMAP with that number through
38FF:04AB, passing the far address DS:0538 as the in/out destination slot.
There is no intervening explicit pointer reset or own capacity argument.
FND-CONFIG-151/152 and FND-COMBAT-120 retain the full-resource transfer,
provider, allocation, failure and external I/O qualifications.

Nonzero reader AX skips bitmap application but still reaches cleanup.
Zero AX reaches both complete grid loops; afterward the local result byte
becomes one. At cleanup a nonzero numeric archive handle DS:145A is passed
to 38FF:02B5; its returned AX is ignored and DS:145A is cleared regardless
of that result. The result byte is returned in AL. It does not combine
the region opener's or closer's results and does not normalize all of AX.

### Region wrapper prepares another archive request

The complete 182 +0297 body selects the RGNXX.GFF or RGN0XX.GFF template
from DS:068F and copies it to a 32-byte local buffer. A nonzero existing
numeric archive handle DS:145A is requested closed and then cleared,
without testing closure's result. The body requests radix-16 formatting
of its word argument and an uppercase transformation through runtime
helpers. When the second formatted byte is NUL it moves the first byte
to the second position and puts ASCII zero first. It copies those two
local bytes to template positions chosen from fresh DS:068F reads.
Formatter/string-helper bounds, success and alias preservation are not
established by those calls alone.

It explicitly zeros DS:145A before requesting local +0000 with the near
constructed name, output far address DS:145A, option word 0A and reserve
04D4. FND-COMBAT-110/127 describe that wrapper and resident publication
contract: a successful new open can publish a selected archive and advance
the issuance counter. Zero returned AL calls the diagnostic through
overlay 180 +0867. Normal diagnostic return/termination is not established
here, and +0297 has no explicit normalization of a final result after it.
FND-EXE-640 preserves the earlier inventory while correcting the distinction
between DS:145A's stored numeric handle and the address of its output slot.

### Bitmap traversal and exact leaf-store footprint

The outer coordinate runs from zero through 127 decimal, the inner from
zero through 97. With x the outer and y the inner coordinate, the running
bitmap bit is t = 98*x + y. Each plane reads byte floor(t/8) and tests
bit t modulo eight. The bit/byte counters continue across outer iterations;
there is no padding at the end of each 98-cell column. Each plane therefore
uses 1568 bytes for 12544 cells, conditional on completing the fixed loops
with valid storage and preserved local state.

The first plane rereads the far pointer DS:55B8 for each cell and requests
25AF:017F for a set bit or 25AF:01DD for a clear bit. The second rereads
DS:55B4 and requests 25AF:01AE or 25AF:020C respectively. Byte indexing adds
to the pointer's offset at word width without a far-segment carry. The
16-bit sign extension of a bitmap byte does not change any tested bit:
the mask is only one of bits zero through seven.

Each of the four leaf setters has no child call. It independently rereads
the full map pointer DS:0538 and rejects null, x unsigned at least 128,
or y unsigned at least 98. Other cases address one byte at that map
pointer's segment with offset low16(base_offset + 128*y + x):

| Leaf | Own byte update |
| --- | --- |
| 25AF:017F | Set mask 40 |
| 25AF:01DD | Clear mask 40 |
| 25AF:01AE | Set mask 80 |
| 25AF:020C | Clear mask 80 |

These stores preserve the other bits of the addressed byte. Coordinate
checks do not validate map capacity, prevent base-offset wrapping, or
establish nonaliasing with the bitmap planes, globals or effect state.
Fresh pointer reads can observe changes caused by aliases or other child
effects. The helper has no own explicit effect-row/count validation or
clamp; GMAP transfer, actual map/bitmap destinations and other helper
effects must be admitted before its writes can be called disjoint from them.

## Interpretation

This removes the map-helper own gate, result/cleanup order, fixed bitmap
traversal and four leaf-store footprints from unread work under
Q-COMBAT-066. It also identifies a conditional open/close route compatible
with FND-COMBAT-146's archive-counter change; it does not attribute that
native change to a particular open without a corresponding runtime trace.

Under admitted stable, adequately sized, nonoverlapping storage, the
successful GMAP route maps the two bitmap planes to bits 40/80 of each
cell. Null map skips everything and returns zero; transfer failure skips
application and returns zero after the conditional cleanup request. These
local branches do not prove rollback, unique source identity or successful
native reconstruction. The parent reader retains its own transfer flag
rather than combining all reconstruction results.

Actual path/region/state at the child call, full GMAP length/provider and
destination extent, bitmap lifetime and aliases, runtime/string/diagnostic
effects and close/open outcomes remain. Shared archive questions stay in
Q-COMBAT-054/064 and storage questions in Q-COMBAT-065. No complete-reading,
rule or parity status is promoted; no queue item closes.

## Alternatives

- Null offset alone skips map reconstruction: the gate tests the complete
  four-byte pointer, so a nonzero segment remains admitted by this test.
- Success means the region open and close both succeeded: their results
  are not combined into the returned local application byte.
- Bits restart on each outer iteration: bit/byte counters are initialized
  only once and advance across all 12544 cells.
- Coordinate bounds establish map capacity or separation from effect state:
  the leaf address still uses word-offset arithmetic and a runtime pointer.

## How to reproduce

Resolve the fingerprinted installed DSUN.EXE through tools/game-dir.mjs.
With the project-local evidence Python and `-I`, run overlay_listing.py
for descriptor 187 with half-open bounds 0x71B1E 0x71C08, descriptor 182
with 0x68AE7 0x68BB5, and resident_listing.py with
25AF:017F..25AF:023B. The committed readers check the installed executable
identity and relocation/fixup mapping. Retain original-derived listings
only in GAME_DIR's approved local analysis store, never Git.
Follow the full-pointer gate, GMAP result branch, persistent bit counters,
the four complete leaf bodies and unconditional handle clearing after the
conditional close request. Reuse the existing shared reader/opener findings
instead of assuming external calls succeeded or repeating their local graphs.
