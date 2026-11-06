---
id: FND-SCRIPT-057
title: Trigger-pool pointer publication and save-table binding are separate phases
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00057580..0x000575DB
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000576F4..0x000577BB
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00070B4A..0x00070BFD
tool: window_operands.py at e29f07ba with installed inventory; FND-CONFIG-156, FND-SCRIPT-054 and FND-SAVE-021/023 interface reuse
environment: null
---

## Observation

This assesses concrete pointer producers and the saved-game destination
binding for Q-SCRIPT-019. Source DSUN.EXE is634416 bytes, XXH3-128
e296af55ba2ecde7e77f555c90f33d0b. The inventory's LF-normalized SHA-256
is820b317d56cb74114699a8e295b0e93e961900a4b2d7d493fb2240b4daa6afa2.

The window40B0..40D0 query emits154 listed operands and101 raw
candidates outside those instruction starts. Among listed direct,
unindexed stores, four overlap the pool pointer's offset/segment words:

| Overlay169 site | Pointer word | Source |
| --- | --- | --- |
| +0045 | DS:40C2, segment | Allocation's returned DX |
| +004A | DS:40C0, offset | Allocation's returned AX |
| +0218 | DS:40C2, segment | Release helper's returned DX |
| +021D | DS:40C0, offset | Release helper's returned AX |

FND-CONFIG-156 and FND-SCRIPT-054 independently establish their containing
entries, boundaries and ES binding to57E0, supplying both producer controls.
The release helper explicitly returns zero; allocation publishes its actual
pair, including zero on its failure path. Segment and offset are separate
stores, in that order, not an atomic far-pointer publication. No intervening
observer is asserted to occur. Neighboring dword stores at40BC end before
the pointer and status-word stores at40C4 begin after it.

Four raw variants start inside those ES-prefixed producer instructions.
Five unindexed word-write candidates at51BF:0009,51C2:0009,51C2:000F,
51C4:0007 and51C5:000F use displacement40C3 outside inventoried starts.
They lie in descriptor121's flags-zero span at loaded segment50C3,
offsets zero through4831 decimal, in FND-SCRIPT-053's complete table report.
This does not establish them as instructions or prove a reachable entry.
Other raw indexed/interior forms remain candidates. Indexed destinations,
computed segments and pointer/block aliases are not excluded by a direct
operand census. No all-writer absence claim follows.

### The save destination is a separate pointer copy

FND-SAVE-021 reads preparation entry overlay187 +0D1A, which copies
the current pool pointer into save-table entry38 at DS:1612. Both writer
and reader call that preparation before their resource loops. This slot
is separate storage, not a new allocation or a publication back to40C0.

FND-SAVE-015 supplies the recorded2600-byte SAVE/38 length and selector-
zero binding. FND-COMBAT-035 reads the reader's indexed destination and
whole-resource request; the recorded length is not an enforced capacity
bound, and failed reading need not leave the destination unchanged.
FND-SAVE-023's converters instead reload the live pool pointer while
walking200 records. The prepared transfer destination and later live
conversion pointer belong to different reads. Their equality at use
needs preservation evidence.

If cleanup reaches its zero publication after preparation, the preparation
instructions do not automatically clear the copied slot. Rebinding the
save table does not allocate or validate the live pool. These are conditional
field relationships, not evidence of native interleaving.

FND-COMBAT-152 supplies one actual bounded observation: a reverse-conversion
boundary captured the current pointer and2600 bytes and matched predicted
contents before release and at reader return. Reuse that sample within its
scope; it establishes neither every future binding nor all failed restores.

## Interpretation

Q-SCRIPT-019 has explicit allocation/cleanup publications and a separate
saved destination binding to audit. Remaining mechanisms include actual
phase/order, pointer-word preservation, copied-slot lifetime, computed/
indexed/bulk aliases and transfer/converter buffer identity. The earlier
Ghidra reference result in FND-SCRIPT-014 was not a complete
overlay writer census. No queue closure, status promotion or whole-entry
complete reading follows.

## Alternatives

No write in one reference view does not exclude known overlay stores.
A saved table pointer is not an ownership transfer or a lease on live
storage. Raw decoding outside inventoried starts does not establish a writer.

## How to reproduce

Run window_operands.py on the supported source and installed inventory with
window40B0..40D0. Retain trigger-pool-pointer-operand-window.txt under
GAME_DIR/analysis/handler-census-20261106/. Require the four independent
producer controls,154 listed operands and101 raw candidates; classify
neighboring bytes and retain the raw ownership limits. Use the existing
allocation/cleanup contexts and FND-SCRIPT-053's table report for row121.
Reuse the save preparation, reader and converter findings above rather
than treating a pointer copy as preservation evidence. The native sample
remains FND-COMBAT-152; no new original run or emulated call supplied evidence.
