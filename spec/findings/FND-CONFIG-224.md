---
id: FND-CONFIG-224
title: The shared fatal diagnostic selects runtime exit rather than an ordinary caller return even if the terminate interrupt returns
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000675FD..0x0006765C
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00067A47..0x00067A83
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4448:002C..4448:0048
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:0388..1000:0400
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:019E..1000:01B0
tool: overlay_listing.py and resident_listing.py at d04333a6; Ghidra 12.1.3 read-only INST-overlay-20261029g; FND-EXE-551/555/601 cleanup and alternate-exit interface reuse
environment: null
---

## Observation

This replaces FND-CONFIG-062. Its startup archive selection and termination
request are retained, but a returning DOS termination interrupt does not
by itself reach the diagnostic caller's fall-through. The connected
runtime instructions supply another termination path instead.

Installed DSUN.EXE is 634416 bytes, XXH3-128
e296af55ba2ecde7e77f555c90f33d0b. Current licensed installation and
BLD-GOG-EN-1.1's manifest retain RESOURCE.GFF and no RESFLOP.GFF.

### Retained startup failure and shared entry

Overlay180's startup selects RESOURCE.GFF for nonzero DS:14E3 and
RESFLOP.GFF for zero, then calls the archive opener. A zero AL at
file offset67625 enters the formatting branch for the selected name
and game directory, then calls local +0867, before the later graphics
initializer. FND-CONFIG-039 supplies the archive-open interface and
FND-CONFIG-157 its earlier OBJEX.GFF attempt. The file-presence result
does not replace the opener's complete search/selection contract.

The complete shared +0867 entry is file67A47..67A83, exported through
56B2:0034. It conditionally requests mode three from 1BF3:2973 when
word4E71:0001 differs from FFFF, then publishes FFFF to that word.
After this conditional arm it requests mode three again, clears byte
DS:1462, and passes its incoming near message pointer to 4448:002C.
The latter calls 1000:3603 for output and then 1000:03DF with status one.
Output and mode-helper effects or termination are not inferred from
their calls. Ghidra confirms the main-loop-flag clear and shared call;
instruction checks establish the exact branches and argument widths.

### Exit has a return predicate, but this wrapper supplies zero

1000:03DF pushes three words for near helper0388: the status, zero
as its second argument, and zero as its third. FND-EXE-555 reads
the selected zero-third-argument callback route. The callback count,
priority dispatch, mutable stored targets and their effects remain
conditions of reaching the later instructions with intact state.

At0388's +03BF, a nonzero second argument branches to the normal
near-return suffix. The fixed wrapper's zero second argument instead
continues through the remaining cleanup calls to019E. Under preserved
argument/frame bindings, there is no ordinary return branch selected
by this invocation. A child that does not return also supplies no
ordinary continuation back to the diagnostic caller.

019E requests INT21h/AH4C, with AL from the status argument, here one.
It contains no return instruction. If that interrupt returns normally
to the following instruction, +01A7 prepares another diagnostic and
jumps to +02B3, which reaches the alternate exit wrapper03EE. That
wrapper supplies second argument zero and third argument one to0388,
skipping the optional callback routes but retaining the termination
branch. FND-EXE-551/601 establish that alternate path. Interrupt
return is therefore not a return through03DF or4448:002C.

### Restore allocation failure reaches the same diagnostic

FND-SCRIPT-063 records the bitmap allocation's null result and the
reader call through56B2:0034 with argument1693. Its existing statement
about continuation *if the diagnostic returns* remains conditional.
This connected reading excludes an ordinary return through that call
under the preserved frame/argument conditions above. It does not prove
that callbacks preserve those conditions, that output/cleanup terminates,
or that the operating system actually ends a process in a native run.

## Interpretation

The missing-alternate-archive startup path and restore allocation-failure
diagnostic request runtime exit. A returning terminate interrupt alone
does not admit later graphics initialization or the restore loop. Any
such caller continuation needs a distinct mechanism, such as changed
exit arguments/frames or a nonlocal transfer, rather than an assumed
normal error-message return. Actual native process termination remains
separate from this code-decided exclusion of the ordinary return edge.

Q-SCRIPT-025 need not treat a normal returning diagnostic as an admitted
bitmap-allocation-failure path. Earlier resource-transfer errors that
return through the numbered reader are different: their path can reach
reverse conversion as FND-SCRIPT-062 records. Q-CONFIG-008 and the
existing callback/runtime questions retain target, state, frame and
external-effect admission. No complete reading or queue closure follows.

## Alternatives

FND-CONFIG-062's implication that returning from INT21h/AH4C permits
the code after the diagnostic call is replaced: the actual successor
is +01A7's alternate exit route. Conversely, absence of an ordinary
return does not establish every callback, successful output, finite
cleanup or observed OS termination. A modified second argument would
select a different0388 branch and is not silently excluded.

## How to reproduce

Use the pinned evidence interpreter and supported source. Retain
overlay_listing.py descriptor180 windows675FD..6765C and67A47..67A83
as trigger-diagnostic-startup-control.txt and
trigger-restore-diagnostic-entry.txt under
GAME_DIR/analysis/handler-census-20261106/. Read resident4448:002C..0048,
1000:03DF..0400,1000:0388..03DF and1000:019E..01B0, retaining
trigger-restore-diagnostic-runtime.txt and trigger-restore-exit-continuation.txt
there. Require the zero second-argument pushes, return predicate, exit
call and post-interrupt jump, not an assumed library convention.

In read-only INST-overlay-20261029g, program INST-MAPPED.EXE, run
ReportDecompileMatches.java at71FE:0867 with patterns1462 and002c.
Require its function marker and calls; retain
trigger-restore-diagnostic-decompile.log in the same local store.
Reuse FND-EXE-551/555/601 for callback and alternate-exit details.
Check the two archive paths and manifest accounting again for the
retained installation claim. No original process or emulated call ran.
