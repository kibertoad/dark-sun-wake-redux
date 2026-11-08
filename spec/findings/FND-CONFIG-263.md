---
id: FND-CONFIG-263
title: Card 6A copies interrupt-result IRQ and DMA words while retaining its existing extended addresses
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 17DA:000B..17DA:0218
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1000:310F..1000:3152
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1000:20DF..1000:21A9
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1000:0DEF..1000:0E0F
tool: Ghidra 12.1.3 ReportFunctionSummary.java; resident_listing.py; retained FND-CONFIG-259 EC census; FND-SOUND-044 error-normalization contract
environment: null
---

## Observation

FND-CONFIG-262 leaves eight literal EC writers. Two occur in17DA:000B's
conditional partial update for card6A; they have no corresponding EE
store. This reads the complete local body and its register-buffer binding.
SOUND_DS.EXE is204593 bytes, XXH3-128236c2dc23c071eca421eb5b427caee57.
Addresses, offsets, vector and register values below are hexadecimal;
sizes and counts are decimal. Data segment is1E36.

### Declared source request and result gate

The caller reserves a16-byte local buffer. It passes that far stack
pointer, byte value0 and count16 to1000:3133. The wrapper forwards the
arguments to310F, which clears direction and fills exactly that admitted
span, using an initial odd-address byte, words and an optional final byte.
3133 restores its frame and returns the destination pointer; the caller
does not use that result. This is a local buffer initialization, not a
zeroing of all CPU or segment registers.

The caller next writes BC04 to buffer word0 and passes vector2F with the
same far pointer as both input and output to1000:20DF. Its segment-record
helper0DEF captures the active ES, wrapper return CS, SS and DS into four
words. 20DF forwards that record with the vector and two buffer pointers
to2110. The selected vector2F takes its ordinary path, distinct from its
special25/26 paths. The instruction reading establishes these word bindings:

| Buffer offset | CPU word loaded before and stored after the selected interrupt |
|---|---|
| 00 | AX |
| 02 | BX |
| 04 | CX |
| 06 | DX |
| 08 | SI |
| 0A | DI |

The wrapper also stores flags, including a carry-only word at+0C.
For returned carry set, it can call04CE after writing the output record;
reuse FND-SOUND-044's bounded normalization/global-write contract.
Actual interrupt handling, result values and preservation through that
boundary are not established by this static binding. No interrupt is
executed or stubbed, and no generic call runner is added.

After the wrapper returns,17DA tests the high and low bytes of output
word0 against4D and56 respectively: returned AX must be4D56. A mismatch
takes its local AX0 return without its own group-record stores. It does
not separately test the output carry word before the signature test.
For a matching signature it retains output word+4 in DI and word+2 in SI,
therefore the returned CX and BX words respectively.

### Conditional paired partial update

The body independently tests MainGroup and DigitalGroup membership for
card6A. On each nonzero result it repeats that group's index lookup for
every word store, using the existing254-byte record-address arithmetic.
Reuse FND-CONFIG-259's group pointers, first-match lookup and signed-count
contracts. Under retained valid disjoint group/record/frame/segment state,
both groups receive this same local mapping:

| Record fields | Source |
|---|---|
| F0,F8 | Returned CX, buffer+4 |
| F2,F4,FA,FC | Returned BX, buffer+2 |
| EC, after those six stores | 0001 |
| EE,F6 | No local assignment |

Thus this producer does not replace the two extended address words before
setting EC. Their earlier backing/writers remain independent obligations;
the later nonzero-EC constructor can consume that mixed state.
The body does not locally populate MusicOnlyGroup, replace card IDs or
assign DS:ECCE. Repeated index calls are not silently treated as one
immutable target when group data can change.

After both group tests, a nonzero caller argument requests a notice via
190F:0000 and discards its return. The admitted matching-signature route
then assigns AX1, even if both groups were absent. The notice/helper result
does not become a success check. Input/state admission and actual child
return remain conditions, not observed native settings.

Ghidra and instructions agree on buffer clearing, actual argument offsets,
the selected register loads/stores, signature bytes, field sources and
store order. Register roles come from the verified SOUND_DS wrapper body,
not an assumed identity with a same-address routine in DSUN.EXE.

## Interpretation

This assigns two more sites of FND-CONFIG-259's controlled EC census to a
partial register-result producer. Q-CONFIG-005 retains six other literal EC
and six EE writers, excluded computed/block/alias mechanisms, actual reply
and source/group/frame lifetime, existing EE/F6 values and downstream
application. The local branch can be documented without claiming an actual
device reply, completed original run, complete reading or format promotion.

## Alternatives

A matching signature is not proof that every extended word was refreshed.
The sixteen cleared bytes are the local input/output frame, not the group
record. A local AX1 result is not proof that a group member existed or a
file write/device operation succeeded. Carry alone is not this caller's
record-update predicate. Other prior address writers remain possible;
omitting an assignment is not evidence that the retained addresses are zero.

## How to reproduce

Use the verified SETUP-blocks project under
GAME_DIR/analysis/config-block-construction-20261108/. The fourth body in
remaining-extended-producers-ghidra.log is17DA:000B.
partial-register-binding-ghidra.log covers20DF/3133;
partial-register-transfer-ghidra.log covers0DEF/2110/310F.
Regenerate partial-register-producer-instructions.txt with resident_listing.py
for17DA:000B..0218; partial-register-binding-instructions.txt for
1000:20DF..2110 and310F..3152; partial-register-transfer-instructions.txt
for1000:0DEF..0E0F and2110..21A9. Read the selected2F path's field bindings
without executing its interrupt or extending special25/26-path claims.

Reuse the unchanged EC census from FND-CONFIG-259; file sites92C1 and938A
are this body's two verified marker stores. Keep all original-derived
reports local. No original executable or emulated call was run.
