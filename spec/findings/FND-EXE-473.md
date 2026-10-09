---
id: FND-EXE-473
title: Sound utility zero-read classifier compares final returned pair after held-pair request
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1000:27FC..1000:2873
tool: Capstone 5.0.7 and xxhash 4.0.1
environment: null
---

## Observation

FND-EXE-379 calls far-returning 27FC after its lower read returns AX
zero. Returned AX exactly one selects its bit-0020 flag continuation;
every other value selects bit 0010. This helper reserves four local bytes
and requires its incoming word unsigned below current DS:DD38. Failure
passes error word 0006 to 04CE and returns its FFFF result; FND-EXE-362
reads that mapper's state changes and two-byte incoming cleanup.

An admitted word is doubled at word width to index current DS at
displacement DD3A. Bit 0200 set returns AX one without an interrupt.
Otherwise the helper sets AX=4400, loads BX from the incoming word
and executes interrupt 21. Carry set passes returned AX to 04CE.
Carry clear with returned DL bit 80 set returns AX zero. A clear bit
continues into three further interrupt requests; no native service meaning
or preservation guarantee is assigned by this local reading.

The first later request sets AX=4201 and CX/DX zero. Carry set maps
returned AX and exits. Otherwise it holds returned DX then AX on the
stack. The second sets AX=4202 and CX/DX zero, executes the interrupt
and stores returned AX/DX into SS:BP-04/-02 before testing carry.
It restores the held first low word into DX and held first high word into
CX. These stores and pops do not change the second request's carry,
which then selects mapped failure or the final request.

The final request sets AX=4200 and retains CX/DX as that held first
pair. It executes interrupt 21; carry set maps returned AX and exits.
Carry clear compares the final returned DX/AX pair unsigned with the
second returned pair saved in the frame. A smaller high word, or equal
high word with smaller low word, returns AX zero. All other pairs,
including equality, return AX one. The final pair is not checked for
equality with the held pair supplied as its input.

BX is not reloaded between the first AX=4400 request and any of these
later requests. Identifying each later BX with the incoming word therefore
requires preservation through every intervening interrupt. The helper does
not save DS, SI, DI or ES. Its local frame accesses require admitted BP/SS
and stack preservation; the mapper's current DS state also remains dependent
on native preservation. Every suffix restores SP from BP, restores BP and
returns far without incoming argument cleanup.

No local rollback occurs after any carry failure. A second-request error
still stores that request's returned pair into the local frame before exit,
but does not make the final request. A final-request error follows both
earlier successful-carry continuations. The helper makes no direct record
flag store; FND-EXE-379's caller changes its record after this result.

## Interpretation

This resolves the auxiliary classifier left open by FND-EXE-379. Indexed
flag admission, native carry, returned-bit classification and final unsigned
pair comparison are distinct ways to reach its result. Only exact word one
selects the caller's corresponding flag path; mapped FFFF is not propagated
as a distinct reader result by that caller.

The held first pair is an input to the final request, while the final
returned pair is the comparison value. Neither establishes actual position
restoration without native argument/result and preservation admission.
Q-EXE-007 retains limit/index/flag writers, actual DS, all native request
contracts and register preservation, frame aliases, caller record state and
later pathname continuation. No complete reading or execution exclusion follows.

## Alternatives

Treating word one as proof of a native comparison ignores the immediate
indexed-flag return. Treating the held pair as the comparison result ignores
the final interrupt's returned pair. Treating the last request as checked
restoration assumes equality with an input that is not tested. Treating all
errors as pre-operation failures ignores the earlier requests and state.
Treating later BX as freshly loaded overlooks its uninterrupted register chain.

## How to reproduce

At revision d1b4fbf require FND-EXE-350's SOUND_DS.EXE identity:
length 204593, XXH3-128 236c2dc23c071eca421eb5b427caee57.
Decode shipped half-open offsets 0x00003BFC..0x00003C73 at IP 27FC,
modeled CS 1000 and MZ header size 1400, with locked Capstone 5.0.7
in sixteen-bit mode. Follow the index gate, bit path, four request selectors,
BX lifetime, held first pair, second-result local stores and preserved carry,
final CX/DX inputs, unsigned comparison and shared mapper/frame cleanup.
Keep native contracts and caller admission open. Original bytes stay outside
Git; no original process, DOSBox, interrupt thunk or emulated call runs.
