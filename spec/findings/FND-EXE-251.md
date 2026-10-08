---
id: FND-EXE-251
title: Vector initializer stores the DOS open result as the transfer handle without checking carry
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4AE5:0140..4AE5:0193
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4AE5:00E7..4AE5:00F6
tool: executable-reader 2.5.0 and Capstone 5.0.7
environment: null
---

## Observation

FND-EXE-176 reads the initial vector procedure only through `4AE5:016B`.
The full candidate at `4AE5:0140` contains thirty-three instructions and
eighty-three bytes, ending exclusively at `4AE5:0193`. It contains four
DOS interrupt sites, no calls and a far return without argument cleanup.
The two vector-service interrupts precede the file-handle paths; none has
an explicit carry test before the next procedure step.

After saving BP and DS, the procedure loads DS through CS-relative word
five, as recorded in FND-EXE-176. It obtains the old vector, saves its ES
and BX and the state DS, forms the replacement vector through state words
two and four, and requests its installation. It restores the saved state
DS and stores the retained old offset and segment to words two and four.
File-handle processing follows these publications, rather than being a
prerequisite for the vector requests or old-pointer stores.

At `4AE5:016B` it compares current DS-relative word `0x0128` against zero.
The nonzero path reloads that word into BX, selects DOS service `0x3E`,
interrupts at `4AE5:0178`, then explicitly stores zero to word `0x0128`
and jumps to restoration. No test of the close result intervenes before
that store. The zero path forms DX as offset `0x008C`, selects service
`0x3D`, loads its mode byte into AL from DS-relative byte six and
interrupts at `4AE5:018B`. It immediately writes returned AX to word
`0x0128`, without testing carry or distinguishing an error result from a
successful handle in its explicit instructions.

The final path restores DS and BP and returns far. It contains no explicit
carry normalization after either file interrupt. The source's lack of a
carry test is not proof of a successful operation or a native register
preservation contract. State-slot identity after interrupts and saved-stack
integrity remain conditional. The mode byte and name buffer are inputs,
not independently admitted complete strings or supported modes here.

FND-EXE-175's loader root selects service `0x3E` and interrupts at
`4AE5:00E9`, then clears DS-relative word `0x0128` without testing that
close's carry. At `4AE5:00F2` it pushes CS and at `4AE5:00F3` makes a near
call to `4AE5:0140`. These steps form the segment and offset return words
consumed by the initializer's far return. The root does not check the
initializer's carry before its bounds-setup continuation at `4AE5:00F6`.
Native return-frame integrity, live CS and DS bindings and DOS effects
remain unproved.

FND-EXE-248 later loads BX from current DS-relative word `0x0128` before
its seek request. That establishes a concrete consumer of this slot,
but not that the consumer sees this initializer's successful open result:
other writers, state-segment identity, intervening operations and the
actual DOS outcome still require admission.

## Interpretation

This extends the vector-prefix reading with the full file-handle branch
and an actual caller's return-frame construction and flag consumption.
Neither the stored open result nor the zeroed close state is proof of
successful native resource acquisition or release. A failure-path reading
must retain vector requests and pointer publications already performed
before the later file operation.

Q-EXE-001 and Q-EXE-010 retain handle writers and consumers, mode/name
producers, DOS results and register effects, effective state identity,
return-frame aliases and live vector/callback admission. No complete_reading
or replacement inventory is established.

## Alternatives

A carry-gated handle store is contradicted by the immediate AX publication.
A close-success-only clear is contradicted by the unconditional zero store.
A file operation preceding vector replacement is contradicted by their
order. Treating the initializer as returning near ignores the caller's
extra segment push and the far return encoding.

## How to reproduce

At revision `c3e75e0`, use FND-EXE-236's source identity, original-source
region and default x86-bounds limits. Set entry and sole entries to
`0x00040190`, with no seeds or summaries. Check interval
`0x00040190..0x000401E3`, thirty-three instructions, no calls and far
return. The interrupt sites are `4AE5:014E`, `4AE5:0160`, `4AE5:0178`
and `4AE5:018B`; the report assumes each returns. Its complete field
is not a Standard complete reading.

Independently decode that interval from the shipped source in sixteen-bit
mode with Capstone and compare FND-EXE-176's vector prefix. Decode the
caller interval `0x00040137..0x00040146` from `4AE5:00E7`, following the
close, handle clear and return-frame construction. Check the slot consumer
in FND-EXE-248 separately and keep identity and intervening-writer admission
open. Source, configurations and listings remain in GAME_DIR.
