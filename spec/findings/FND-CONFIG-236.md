---
id: FND-CONFIG-236
title: Music-level readback has explicit sentinel exits and its setter clamps a signed word before dispatch
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4A6E:0064..4A6E:00D7
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4A6E:011F..4A6E:0187
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 5736:0000
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 5782:0000
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4842:0D06..4842:0D1E
tool: resident_listing.py and overlay_listing.py at 636538f8; pinned evidence Python and Capstone 5.0.9; Ghidra 12.1.3 ReportDecompileWindow; reused FND-EXE-646 and FND-RNG-082
environment: null
---

## Observation

The load routine in overlay 192 passes the byte at `DS:26B4`, copied from
`PREF/100` offset `0x02`, to `4A6E:0064` at
`DSUN.EXE+0x0007D9FB` (FND-SAVE-005). The far-call fixup names descriptor
81, resident segment `4A6E` (FMT-EXE-002, FMT-EXE-005).

The routine at `4A6E:0064` checks the card ID at offset `0x08` of the
`SOUND.CFG` record (FMT-CONFIG-006). When it is 113, it returns the byte at
`4E71:0C14` without applying the argument. Otherwise it compares its argument as a signed 16-bit word: values above
100 become 100 and values below zero become zero, in that order. It reloads
the configuration pointer, and a word at `+0x32` equal to 3 adds an upper
cap of 90. The signed branches are at `4A6E:0083`, `008E` and `00A4`;
negative word inputs therefore differ from an unsigned upper-only cap. It stores
the capped byte at `4E71:0C14`, calls `4842:0D18` with that byte and driver
handles, and, after the call, rereads the stored byte and zero-extends it into AX.
The store precedes driver dispatch; no dispatch-success test rolls it back.
A changed stored byte across that call can change the return, so unchanged
callee effects cannot be assumed. The installed `SOUND.CFG` has card ID
122 and word `0x32` equal to 1.

On entry to the Preferences routine at `DSUN.EXE+0x0008B240`, overlay 203
calls `4A6E:011F` and writes its returned byte to `DS:26B4` when the master
sound byte `DS:13F7` and music-enable byte `DS:1436` are nonzero. The getter
returns AX=255 when the first configuration card-word read equals 113,
when `4E71:0B51` is 3 or 5, or when `DS:3464` is FFFF. Otherwise it passes
the two current words `DS:343E` and `DS:3464` to `4842:0D06`. A returned
word equal to zero or two also selects AX=255. Other returned words lead
to `4842:0D0C` with those words loaded again. The final result is forwarded
in AX without a local 0-to-100 clamp; Preferences stores AL, so its
saved byte is the low eight bits. The getter tests and joins are
`4A6E:0127..0187`; this does not establish the reachable driver results.

Overlay 203 passes `DS:26B4` when music is enabled, or zero when it is not,
alongside `DS:26B6` to a drawing path at `DSUN.EXE+0x0008B4AB`. On a
music-enable change, its state routine passes `DS:26B4` to `4A6E:0064`;
when the returned value is smaller, it copies the unmodified `DS:26B4` to
`DS:26B6` (`DSUN.EXE+0x0008B81B`). The save-load path uses the same
comparison and copy (FND-SAVE-005). The music player also passes
`4E71:0C14` back to `4A6E:0064` when it starts a song (FND-SOUND-013).

## Interpretation

The connected scope is Preferences entry/readback and its music-toggle
setter/denominator consumer, with the shared driver interface reused rather
than re-read as a new entire subsystem. Installed source identity is 634416
bytes, XXH3-128 e296af55ba2ecde7e77f555c90f33d0b, modeled load 1000.
Ghidra's complete local decompilations agree with the branches, but their
inferred extra call parameters are not argument evidence: Preferences pushes
one zero-extended byte word to the setter at file 0x0008B81B..0x0008B827,
and the caller removes two bytes. The callee reads SS:[BP+6] after a far
return frame. Its driver request pushes four words and removes eight bytes.
Configuration fields are read through the far pointer loaded from DS:3411;
fixed library state is addressed through explicitly loaded ES=4E71. Actual
DS/frame/pointer admission and preservation remain conditions.

The dispatch wrappers at 4842:0D06, 0D0C and 0D18 select function words AE,
AF and B1 respectively, then tail-transfer to 4842:03F4. Reuse FND-EXE-646
and FND-RNG-082 for slot/table lookup and the far-return dispatch interface.
Those interfaces do not establish a current slot binding or the driver
function's return and writes. The old finding's 3842 call labels omitted
the load-segment adjustment; the corrected relocated instructions target 4842.
The initial attempted 3842 wrapper window is retained as rejected context,
not as evidence of the driver's instructions.

`PREF/100` offset `0x02` is the requested music level that the game submits
to its sound library. The setter uses signed lower and upper clamps before dispatch, with the
additional 90 cap for the separately read configured type. `PREF/100` offset `0x04` participates in music
display and receives the request when the library returns a smaller level.
FND-CONFIG-013 supplies its scaling-denominator role. Neither byte has a
direct store in the inspected Preferences arrow instructions described by
FND-CONFIG-010; callee and indirect effects remain unexcluded.

## Alternatives

This whole-entry replacement corrects FND-CONFIG-012's call labels and omitted
signed lower clamp; it also resolves the previously unspecified local getter
fallback branches. No complete reading, whole queue answer or audible-output
validation follows. Reuse the known lookup interface, but retain actual
configuration/slot/sequence state, aliases and interval-wide preservation.

The selected driver bodies beyond `4842:0D18` and `4842:0D0C` were not
read here, so their physical effect on loudness and the getter's reachable
returned range are not established. Their local dispatch wrappers were read.
FND-UI-065 supplies the distinct full-redraw and music-toggle bar consumers;
this finding does not establish native painted frames. A music
volume key or another control outside the ordinary Preferences arrows has
not been ruled out. This reading does not settle the new-game initialization
of either byte or whether indirect writes exist.

## How to reproduce

Resolve the far-call fixup at `DSUN.EXE+0x0007D9FB` through descriptor 81 of
the segment table at file offset `0x4B080`, then disassemble
`4A6E:0064..00D7` and `4A6E:011F..0187`. In overlay 203 inspect bounded
windows at file offsets `0x0008B240..0x0008B281`,
`0x0008B490..0x0008B4C0` and `0x0008B7F0..0x0008B845`.
Search the shipped file for the little-endian displacements `B4 26` and
`B6 26`, and check instruction alignment at the reported code locations.

Fresh reports are under GAME_DIR/analysis/music-level-contract-20261107/:
preferences-connected.txt, music-library.txt, music-driver-wrappers.txt and
library-decompile.log. Regenerate the first with overlay_listing.py for
203 8B240 8BB80, the resident services with resident_listing.py for
4A6E:0064..4A6E:0187, and the corrected wrappers for
4842:0D00..4842:0D40. Decompile the two service entries with
ReportDecompileWindow.java, lines 1..80, in read-only INST-handler-20261106
from the isolated runtime-error-inventory project. Both complete outputs
fit that window. Check signedness, frame cleanup, relocation and AL/AX use
in the instructions rather than inferred decompiler parameters.
