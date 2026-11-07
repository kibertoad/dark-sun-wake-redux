---
id: FND-SOUND-085
title: Literal sequence-handle writers separate initial sentinels, unchecked start publication and stop clearing
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x00050440..0x00050444
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x00050464..0x00050468
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4A32:0185..4A32:0338
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 47E5:0296..47E5:02B6
tool: window_operands.py and resident_listing.py at a3e74539; pinned evidence Python with Capstone 5.0.9; Ghidra 12.1.3 ReportDataBytes.java and ReportDecompileWindow.java; FND-SOUND-068/069/082/083/084 and FND-CONFIG-236
environment: null
---

## Observation

This assessment addresses the literal getter-handle producer component of
Q-CONFIG-014, using installed DSUN.EXE (634416 bytes, XXH3-128
e296af55ba2ecde7e77f555c90f33d0b). Addresses are hexadecimal.
FND-EXE-269 establishes the relocated startup data-segment immediate 57E0
and its file base 0004D000. This does not establish preservation of that
segment throughout execution.

### Initialized data and the direct writer component

The source-initialized words at 57E0:3440, 3442, 3464 and 3466 are all
FFFF. These are file contents, not observations after startup, a saved-game
load or any sound callback. The getter reads 3464, not 3440 or 3442
(FND-CONFIG-236).

The current installed function inventory supplies the decoding ranges for
window_operands.py. Windows 3440..3444 and 3464..3468 report respectively
23 and 38 encoded operands in those ranges. Their direct DS-relative
write component is finite:

| Destination | Listed instruction sites | Local value source |
| --- | --- | --- |
| 3440 | 47E5:02A5 | AX returned from sequence setup. |
| 3442 | 4654:08CA, 0990 | AX returned at the two alternate setup sites. |
| 3464 | 4A32:01F6, 021D | FFFF on the stop paths in FND-SOUND-068. |
| 3464 | 4A32:0333 | Current word 3440, reloaded after the start request. |
| 3466 | 4A32:0258, 02CE | FFFF on the stop paths in FND-SOUND-068. |

Independent positive controls are the known start copy from FND-SOUND-069,
stop clears from FND-SOUND-068 and alternate result stores from
FND-SOUND-084. All occur in the report with their expected read/write roles.
The report's second pass retains three non-start candidates per window:
for 3440..3444, one inside a listed instruction and two outside listed
decoding; for 3464..3468, three inside listed instructions. All six decoded
candidates are reads. They remain candidates, not new established entries.

The first pass is inventory-range decoding, not a reachability proof. Zero
reported string stores in the routines with hits does not exclude copies
in other routines. Register-computed accesses without these literal offsets,
implicit operands, aliases, overlapping writes based outside these windows,
unlisted or differently entered code, external effects and changed segment
bindings remain outside this direct-writer component. No global absence of
other writers follows.

### Publication reloads state after the child

The complete local body 4A32:0310..0338 returns without publication when
configuration word +08 equals 71. Otherwise it requests 4842:0CF4 using
current DS:343E and DS:3440, removes four argument bytes, reloads DS:3440
at 0330 and stores that word to DS:3464 at 0333. It neither tests the
child's returned status nor copies returned AX. Ghidra's full ten-line
decompilation agrees with that ordering; its inferred extra call argument
is not used as argument evidence.

The later load matters: a child changing 3440 changes the value published
to 3464. Assuming unchanged storage across that call would lose part of
the actual interface. The routine does not locally reject source FFFF.

FND-SOUND-083's missing installed song-tag route returns before the setup
store to 3440. FND-SOUND-069 shows that this local failure does not itself
set the caller-tested failure flag and does not clear an old 3440. Thus a
caller that continues to 0310 can publish a previously retained handle,
conditional on its gates and child effects. This is not evidence of a
new registration or successful playback.

For a source-faithful initial FFFF at 3440, no intervening writer and an
unchanged value across the returning start request, that copy leaves 3464
FFFF; FND-CONFIG-236 then gives getter result 255 through its sentinel
exit. With a previously changed 3440, the same local copy can instead
forward the changed value. Actual histories are not established here.

## Interpretation

Q-CONFIG-014 can remove unread literal handle writers and initialized
source values from its discovery work. The remaining claim-relevant
obligations are whether actual histories preserve or replace those words,
including computed/block/alias writes and reached child effects, and which
driver/sequence binding the getter then uses. Repeating the literal scan
with unchanged inputs cannot settle those mechanisms.

This is not a complete reading, an answered queue deletion, an observed
stale-handle fault or a universal default of 255. It does not establish
audible output. No original game or emulated function was executed.

## Alternatives

- Start publishes the child's AX result: it reloads 3440 instead.
- Failed acquisition clears every handle: the known failure leaves 3440
  untouched, and publication is separate from successful acquisition.
- Initialized FFFF proves the handle always stays unavailable: later
  registration and unchecked publication can replace it; other mechanisms
  remain unexamined.

## How to reproduce

Run the pinned evidence Python with -I and window_operands.py, passing the
licensed installed DSUN.EXE, coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv and the
two half-open windows above. Preserve the entire report and candidates.
Use resident_listing.py for 4A32:0310..0338 and 47E5:0296..02B6.
Reports are retained in GAME_DIR/analysis/getter-handle-producers-20261107/
as handle-window.txt and admitted-producer-context.txt.

Process the existing INST-handler-20261106 project in
GAME_DIR/analysis/runtime-error-inventory-20261107/projects read-only with
no analysis. Run ReportDataBytes.java for 57E0:3440 and 57E0:3464, four
bytes each, and ReportDecompileWindow.java for 4A32:0310, lines 1..60.
initial-and-start.log retains those outputs. producer-context.txt includes
an initially requested offcut window at 47E5:0290; its initial decoding is
rejected context, replaced by the admitted 0296 window. No source-derived
listing, byte dump or decompiler output is committed.
