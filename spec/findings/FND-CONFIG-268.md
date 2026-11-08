---
id: FND-CONFIG-268
title: Post-dispatch setup writes group indices by fixed membership priority rather than selected or marked card
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 17FB:03D3..17FB:0668
tool: Ghidra 12.1.3 ReportFunctionSummary.java; tools/research/exec-census/resident_listing.py
environment: null
---

## Observation

Q-CONFIG-005 retains03D3 effects after FND-CONFIG-267's producer dispatch.
This reads that complete local root, reusing FND-CONFIG-259/262's
membership/index helper contracts. The source is SOUND_DS.EXE,204593 bytes,
XXH3-128236c2dc23c071eca421eb5b427caee57. All numbers below are hexadecimal.
Actual group/count/record backing and preservation across repeated child
calls remain conditions. No original or emulated call was run.

The recognized caller1ABD:00A1 invokes17FB:03D3 after the dispatcher
result's conditional DS:D470 store, regardless of that result. Both routes
then continue to the later setup choices (FND-CONFIG-267,FND-SOUND-021).
03D3 has no supplied argument and no explicit local initialization of its
destination words DS:EBCE and DS:EBD0.

It calls17FB:02CD with these literal card numbers in order, stopping at
the first nonzero result:

66,67,73,7A,79,6A,7C,7D,81,82.

02CD supplies any-group membership in MainGroup, DigitalGroup or
MusicOnlyGroup, under its existing signed-count/first-match conditions.
If all ten results are zero,03D3 reaches its far return without assigning
either destination word. The priority list is local literal control flow,
not a comparison with the dispatcher's selected word or DS:ECCE.

For the first admitted card number C,03D3 performs the same two independent
steps in every arm:

| Gate and child | Explicit assignment |
| --- | --- |
|17FB:0346(C) nonzero, then17FB:01B0(C) | DS:EBCE receives the MainGroup index returned in AX. |
|17FB:03A4(C) nonzero, then17FB:020C(C) | DS:EBD0 receives the DigitalGroup index returned in AX. |

Each zero gate skips only its corresponding store. Both steps complete
before the root returns; it does not resume the later card-number tests
when either or both gates are zero. The second gate is not conditional
on the first gate succeeding. The index helpers return the first match
or zero, not a not-found sentinel, so repeated membership/index stability
is required to infer that a stored index still identifies C.

The twenty direct word-store sites are independently present in the
instructions, two per admitted card arm. In priority order their offsets
within17FB are:

| C | EBCE store | EBD0 store |
| --- | --- | --- |
|66 |03F9 |0412 |
|67 |043B |0454 |
|73 |047D |0496 |
|7A |04BF |04D8 |
|79 |0501 |051A |
|6A |0543 |055C |
|7C |0585 |059E |
|7D |05C7 |05E0 |
|81 |0609 |0622 |
|82 |064A |0663 |

There is no own record EC test or extended-field assignment, no own
DS:ECCE or DS:D470 store, and no own read of the previous dispatcher
result in this root. It does not call the producer or marker functions.
Its reached membership/index children have the existing read-only local
contracts in259/262; no new device/file operation is introduced here.
It returns through0666's shared epilogue without assigning a normalized
AX result; its recognized caller does not test that register afterward.

## Interpretation

This settles the local03D3 transformation left by FND-CONFIG-267.
Under stable admitted group state, the first listed card present anywhere
controls both gated stores. If that card is present only in MusicOnlyGroup,
both stores are skipped and later cards are not considered. If present
in only one destination group, the other word retains its incoming value.
This is not evidence that such a state occurs in the installed profile.

Q-CONFIG-005 retains incoming EBCE/EBD0 and record histories, actual
group/count/index/backing lifetime, aliases and excluded entry mechanisms,
their later consumers and game-side settings/driver behavior. This root's
priority does not establish the eventual constructor's source record.
No complete-reading declaration, status promotion or parity validation is
claimed.

## Alternatives

Treating this as the producer-selection result contradicts its independent
priority list and lack of an input argument. Treating membership as an
EC marker test contradicts the actual02CD calls. Skipped assignment does
not mean zero initialization, and a first candidate missing a destination
group does not cause fallback to the next literal card number.

## How to reproduce

Use the verified read-only SETUP-blocks project in
GAME_DIR/analysis/config-block-construction-20261108/ and summarize
17FB:03D3 into post-dispatch-state-ghidra.log. Generate
post-dispatch-state-instructions.txt with resident_listing.py for
17FB:03D3..17FB:0668. Check all ten literal tests, both stores per arm,
the joins to0666 and caller continuation in CONFIG-267's retained
producer-selection-instructions.txt. Reuse the unchanged helper readings
in FND-CONFIG-259/262. Retain all original-derived reports only in GAME_DIR.
