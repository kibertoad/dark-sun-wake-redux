---
id: FND-EXE-269
title: Declared MZ startup publishes the filename source-segment slot from incoming storage
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
    offset: 0x00000014..0x00000018
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:0000..1000:0028
tool: executable-reader 2.5.0, Capstone 5.0.7 and xxhash 4.0.1
environment: null
---

## Observation

The shipped MZ initial IP and CS words are both zero. Under load segment
`0x1000` and header size `0x5200`, the declared entry is `1000:0000`,
shipped offset `0x00005200`. This is a source-declared executable entry,
not a function entry selected from analyzer ownership.

Its first instruction loads DX through an MZ-relocated immediate at
shipped site `0x00005201`: raw `0x47E0`, loaded segment `0x57E0`, whose
shipped base is `0x0004D000`. It publishes DX to CS-relative word `0x02C4`,
sets AH `0x30`, and invokes interrupt `0x21`. After that interrupt it
reads BP from current DS-relative word two and BX from current DS-relative
word `0x002C`. Only then does it load DS from current DX.

Through that resulting DS it publishes AX to word `0x0092`, ES to word
`0x0090`, BX to word `0x008C`, and BP to word `0x00A8`, before calling
`1000:01B0`. Thus the source of the word `0x008C` is an explicit pre-switch
DS-relative read, not an inferred constant or analyzer global name.

FND-EXE-264 and FND-EXE-265 load their source segment through word
`0x008C` in the segment formed from a relocated `0x47E0` immediate.
Under preservation of startup DX across its interrupt and admission of
the shared relocated segment, this startup store supplies a concrete
initial producer for that source-segment slot. It does not supply the
external string contents or their length bounds.

## Interpretation

Q-EXE-001 and Q-EXE-010 can now trace this input back to a declared startup
path and an explicit incoming-storage read. The pre-switch DS and later
DS need not identify the same storage; the publication crosses that
segment change. What provides the incoming DS:002C value, preservation
of DX/DS/ES across the interrupt, other slot writers, startup callee effects,
native loader entry and the source strings' initialized extent remain
open. This does not establish a platform input contract, complete_reading
or replacement inventory.

## Alternatives

Treating word `0x008C` as having no producer ignores the declared startup
store. Treating it as a constant source-segment value ignores its pre-switch
read and the intervening interrupt. Equal offsets in the incoming and
relocated DS segments do not make them the same storage. Native arrival
at the later loader root is not proved merely by a startup input producer.

## How to reproduce

At revision `dacd7c5`, verify the installed source identity in FND-EXE-236.
Read little-endian initial IP and CS words at shipped offsets `0x14` and
`0x16`, and the header paragraph count at offset eight. Decode the shipped
interval `0x00005200..0x00005228` in sixteen-bit mode, initial IP zero,
model segment `0x1000`. Restrict this reading to the prefix before the
first near-call continuation; no callee effects are assumed established.

Run the committed operand wrapper with sourceKind mz, site `0x00005201`,
targetOffset zero and the same hash-guarded source. Require the MZ
relocation and loaded target `57E0:0000`, shipped base `0x0004D000`.
Follow the incoming DS reads and subsequent DS switch in execution order,
retaining the interrupt-preservation obligation. Compare the filename
helpers' explicitly recorded segment loads in FND-EXE-264 and FND-EXE-265.
Repeat the operand query at their immediate sites `0x00040258` and
`0x00040220`, targetOffset zero, requiring the same relocated target.
Source and local reports remain in GAME_DIR.
