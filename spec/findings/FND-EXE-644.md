---
id: FND-EXE-644
title: Nine branch-joined overlay trampoline pairs reach window or per-slot callback arguments without an intervening header-data dereference
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00063541..0x0006354A
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006360C..0x0006361F
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008BF4E..0x0008BFCB
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008C0A8..0x0008C0B9
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008C1E7..0x0008C322
tool: Pinned Python 3.12.10, Capstone module 5.0.7 (distribution 5.0.9); segment_table_readers.py --pointer-sites, overlay_listing.py and trampoline_target.py at a100acba
environment: null
---

## Observation

The optional pointer-site report at a100acba retains the same 153 overlay
fixup/push candidates and 1,929 far-transfer operands as FND-EXE-567.
Nine pointer candidates encounter a jump before a call: one in overlay
178 and eight in overlay 204. The report is a locating aid; instruction
and argument use below comes from the connected branch/join reading.
Overlay entries and consumers are retained in FND-EXE-604, FND-EXE-587,
FND-EXE-586, FND-PARTY-156 and FND-EXE-626. Their exclusions remain.

Overlay 178 pushes the relocated header segment and trampoline offset
`56A2:003E` at `+0491/+0494`, then jumps from `+0497` to `+055C`.
The join loads ES with the data segment `4DA0`, pushes its current
four-byte window pointer at offset zero, and calls `3A8E:0D3C` at
`+0567`. It removes eight argument bytes afterward. Thus the preserved
trampoline pair is the setter's callback argument at BP+0A, after its
window argument at BP+6. FND-EXE-587 reads that setter as storing the
callback value at window +FD. No child call or pointer-data dereference
intervenes between the pair's push and this call. The selected window
pointer is not assumed valid or disjoint from resident headers here.

The overlay-204 legs each push two dword arguments and a far pair from
header `5787`, then join one of these three points:

| Segment push | Trampoline offset | Trampoline target | Branch destination | State supplied at join |
| --- | --- | --- | --- | --- |
| +0203 | 00BB | +259D | +019C | -1 |
| +02F0 | 0039 | +0731 | +019C | -1 |
| +042F | 00AC | +19C1 | +055D | 2 |
| +0475 | 00B6 | +1B7A | +055D | 2 |
| +0486 | 00A2 | +2057 | +055D | 2 |
| +0497 | 009D | +2029 | +055D | 2 |
| +04A8 | 00A7 | +2085 | +055D | 2 |
| +0557 | 00B1 | +214E | +019E | already pushed as 2 at +055D |

For the last row the fall-through first reaches +055D, pushes 2, then
jumps from +055F to +019E; the reported first jump is that final jump.
For the other five state-2 rows +055D performs the same state push and
jump. At +019C the state is pushed as -1. Both routes next push SI at
+019E, push CS and make the near call at +01A1 to +1A68, giving that
callee a far return frame. The callback pair is its BP+0A argument,
after target and state; the two dwords follow. Cleanup removes sixteen
argument bytes. These joins contain no intervening child call or store
through the callback pair. They preserve the callback value under the
ordinary intact-stack/segment interpretation; no actual live frame is
admitted by this static reading.

FND-PARTY-156 reads +1A68's conditional per-slot invocation and count
of nonzero callback results. The present reading adds the exact eight
branch legs' correspondence, not a new interpretation of every callback
body. FND-EXE-626 already reads several of those bodies.

## Interpretation

The nine first-jump candidates have finite local consumers: one window
callback store and eight per-slot callback arguments. The branch itself
is not an unresolved mechanism for reading header fields through these
pairs. This narrows immediate-use classification in Q-EXE-033; it does
not close the header reader/writer census or establish FMT-EXE-003.

## Alternatives

- A changed stack, asynchronous mutation, different entry or incompatible
  frame/segment binding could change the supplied argument. This reading
  does not admit those states or prove every candidate reachable.
- The destination window record could alias a header, or a later callback
  could change a header field or selection. Neither is excluded by proving
  that a callback value itself is not dereferenced along these joins.
- FND-EXE-567's runtime-computed segments and FND-EXE-587's adjusted-base,
  block-copy and saved-history exclusions remain. The other 144 candidate
  first transfers are not newly qualified by this nine-route result.
- The surrounding script dispatch has additional effects. They are not
  needed to identify these local callback argument slots and are not
  characterized as harmless here.

## How to reproduce

Use the committed readers at a100acba and the pinned evidence Python
with the licensed installed DSUN.EXE. The readers check its 634,416-byte
size and XXH3-128 e296af55ba2ecde7e77f555c90f33d0b.
Run `segment_table_readers.py <dsun> --pointer-sites`; retain output only
in GAME_DIR and select the nine records with following_transfer.kind
of jump. Run `overlay_listing.py <dsun> 178 6353E 6354A` and
`178 6360C 6361F`; for overlay 204 read `204 8BF2A 8C322`, following
only the specified legs and joins. Resolve the table's eight offsets
with `trampoline_target.py <dsun> 5787:<offset>` and check the two
stack dword pushes, state and SI pushes, call frame and argument cleanup.
Compare the consumer contracts with FND-EXE-587 and FND-PARTY-156.
The retained local context report is
GAME_DIR/analysis/header-pointer-204-joins-20261106.txt.
