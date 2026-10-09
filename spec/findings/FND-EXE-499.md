---
id: FND-EXE-499
title: Both game editions pass the batch-name diagnostic through an overlay message interface
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
    offset: 0x0004D9E7..0x0004DA0B
  - build: BLD-GOG-EN-1.1
    file: CD:DSUN.EXE
    kind: file-data
    offset: 0x0004D949..0x0004D96D
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00067206..0x00067230
  - build: BLD-GOG-EN-1.1
    file: CD:DSUN.EXE
    offset: 0x00067166..0x00067190
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00067A47..0x00067A83
  - build: BLD-GOG-EN-1.1
    file: CD:DSUN.EXE
    offset: 0x000679AA..0x000679E6
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4448:002C..4448:0048
  - build: BLD-GOG-EN-1.1
    file: CD:DSUN.EXE
    address: 443D:0032..443D:004E
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:3603..1000:364E
  - build: BLD-GOG-EN-1.1
    file: CD:DSUN.EXE
    address: 1000:3603..1000:364E
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x0004B248..0x0004B250
  - build: BLD-GOG-EN-1.1
    file: CD:DSUN.EXE
    kind: file-data
    offset: 0x0004B158..0x0004B160
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x00067D64..0x00067E8A
  - build: BLD-GOG-EN-1.1
    file: CD:DSUN.EXE
    kind: file-data
    offset: 0x00067CA6..0x00067DCA
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x00003C7E..0x00003C86
  - build: BLD-GOG-EN-1.1
    file: CD:DSUN.EXE
    kind: file-data
    offset: 0x00003C56..0x00003C5E
tool: Capstone 5.0.7 and xxhash 4.0.1
environment: null
---

## Observation

FND-EXE-350's sound.bat suffix belongs to a longer zero-terminated
diagnostic in each game edition. The diagnostic reports a missing sound.cfg
file and directs the player to run the batch helper. Its complete physical
interval is 4D9E7..4DA0B installed and 4D949..4D96D on disc.
Under modeled DS 57E0 and 57D7 respectively, these starts are
offsets 09E7 and 09D9. Actual DS at the consumers remains unadmitted.
The suffix itself is not the beginning of either message.

In overlay 180, procedure offset 0026 reserves four local bytes and
tests current DS byte 13F7 installed or 1377 on disc. Zero jumps
to continuation 010F, outside this reading. Nonzero calls the fixup-bearing
far target encoded as segment-table index 72, offset 0063 installed or
0067 on disc. It stores returned DX:AX in its two local words and
tests their combined zero value. Only an all-zero returned pair pushes
the diagnostic offset and calls local helper 0867 installed or 086A
on disc through push-CS/near-call. It removes two incoming bytes and
joins continuation 0050; it does not independently test the message result.

That helper accesses a fixup-bearing segment selector encoded as table
index 110, word offset one. If the word is not FFFF it calls the
fixup-bearing index-11 target at offset 2973 with incoming word three,
removes two bytes, reloads the selector and writes FFFF at offset one.
Both paths then call that same target with three again. The helper clears
current DS byte 1462 installed or 13E2 on disc, forwards its incoming
diagnostic-offset word to the fixup-bearing index-57 target at 002C
installed or 0032 on disc, removes two bytes, restores BP and returns
far without incoming cleanup. Selector meaning, target effects and
preservation remain open; encoded selector 0370 is not a native segment.

The index-57 descriptors have flag one and relative segments 3448
installed and 343D on disc. Their modeled resident targets 4448:002C
and 443D:0032 forward the incoming word to 1000:3603 and remove
two bytes. They then call 1000:03DF with incoming word one, remove
two bytes, restore BP and return far. No result is tested locally.
The far segment words for these two calls are MZ relocations
3857/3856 installed and 3847/3846 on disc, encoded zero.

The 3603 body in each edition saves SI and DI and retains its incoming
word in SI. A zero word returns AX zero. Otherwise it calls local
far-returning 37D8 with that word, removes two bytes and holds AX
in DI. It passes SI, that returned word and a third word to near
345B; the third word is 3692 installed and 3606 on disc. If returned
AX differs from held DI it returns FFFF. Equality calls local far-returning
3313 with the same third word and 000A, removes four bytes and
tests full AX against 000A. Equality returns 000A; otherwise FFFF.
All paths restore DI, SI and BP and return far without incoming cleanup.
The deeper callees and near 345B's incoming cleanup remain open.

## Interpretation

This identifies positive consumers for both editions' complete diagnostic,
instead of treating the interior batch suffix as a command pointer.
The local chain conditionally forwards message offsets and tests output-like
results; the diagnostic's instruction to the player is not itself an
observed program launch. Q-EXE-007 retains actual DS and pointer/result
admission, the index-72 and index-11 targets, 37D8/345B/3313/03DF,
other callers and later continuations, indirect effects and launch-capability
coverage. No native display, complete caller reading or launch exclusion
is claimed.

## Alternatives

Searching only for the suffix offset misses its containing message pointer.
Treating the player instruction as an executed command confuses writing
with behavior. Treating encoded overlay selectors as native segments ignores
their fixup entries. Treating the near-call syntax as a near-return contract
ignores the pushed CS and far-returning helper.

## How to reproduce

At revision e30a8fd require both DSUN.EXE identities from FND-EXE-350.
Use Capstone 5.0.7 in sixteen-bit mode with MZ header size 5200 and
modeled load segment 1000. Find the nearest preceding zero within 128
bytes before the recorded suffix and the following zero within 128 bytes;
inspect the complete diagnostic intervals above without retaining its prose.
Use FND-EXE-003's overlay-180 code bases 671E0 installed and 67140
on disc. Decode overlay offsets 0026..0050 and 0867..08A3 installed,
0026..0050 and 086A..08A6 on disc. Check fixup entries 003B,
and helper entries 086B/087C/0880/0890/089E installed or
086E/087F/0883/0893/08A1 on disc against FMT-EXE-005.
Read index-57 descriptors at shipped 4B248 and 4B158, decode their
resident targets and 1000:3603..364E in both sources, and check the
four MZ relocation records above in the table at 003E. Keep ordinary
descriptor segments, encoded fixup indices and modeled addresses separate.
Licensed bytes stay outside Git; no original process, DOSBox or emulated call runs.
