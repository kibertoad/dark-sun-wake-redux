---
id: FND-CONFIG-092
title: Overlay 175 registers a separate frame handler whose value-32 branch enters item feedback
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 5689:0057
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 5689:005C
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 5689:0043
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 3F96:02AB
tool: Python 3.14.7 FBOV trampoline and fixup inspection; Capstone 5.0.7 bounded 16-bit disassembly
environment: null
---

## Observation

Overlay 175's setup entry `5689:0057` starts at file offset
`0x0005FC0C` and returns at `0x0005FE10`. The following entry,
`5689:005C`, has a separate prologue at `0x0005FE11` and returns
at `0x00060327`. The near call at `0x000601BE` belongs to this
second entry, not to setup as FND-CONFIG-091 claimed.

The guarded overlay 178 calls at `0x00063415` and `0x00063CD9`
still target setup. Their local guards are respectively an OR of two
local bytes equal to `0x40`, and bit `0x08` of a selected 15-byte
record's byte `+0x0B`. They do not directly enter the frame handler.

Setup requests resource `0x34BD` (13,501) through overlay 182's
`56BD:0048`, passing callback `5689:0052`, and stores the returned
far pointer at `DS:4214` and `DS:1431`. It then loops identifiers
`0x2BCD` through `0x2BD2` (11,213 through 11,218). At
`0x0005FD71`, each iteration passes the window pointer, identifier
and callback `5689:005C` to resident `3F96:02AB`. The setter looks
up an `APFM` and writes the callback field when lookup succeeds
(FND-CONFIG-074). The next call at `0x0005FD8A` passes the same
window and identifier to `3F96:02F8` with mask `0x0066` and
operation one, requesting an OR into the event mask (FND-UI-007).
The loop ignores both return values. This is attempted six-frame
registration, not evidence that all six records exist.

The frame handler subtracts `0x2BCD` from its second word argument
and retains that difference locally. It obtains a local record through
`3F96:01A6` with the window pointer and identifier. It compares its
third word argument with the four values at overlay offset `0x0748`:

| Argument value | Branch file offset |
|---:|---|
| 2 | `0x0005FE6F` |
| 4 | `0x0005FFFC` |
| 32 | `0x00060155` |
| 128 | `0x0006020A` |

An unmatched value goes to the zero-result exit at `0x00060322`.
The value-32 branch clears `DS:0DA4`. A nonzero far pointer at raw
segment `04E8:0000` selects two additional calls; a zero pointer
skips them and rejoins at `0x0006017F`. After further presentation
calls, the branch passes the identifier difference to the near call
at `0x000601BE`, targeting helper `5689:0043` at `0x00060898`.
There is no conditional jump between that rejoin and the helper call;
the intervening callees' effects remain unread.

The helper's two message paths remain conditional: the no-effect path
calls the shared entry at `0x00060959`; the alternative calls local
helper `5689:003E` at `0x0006097F`, and that helper contains the
money-message call at `0x00060EF3` (FND-CONFIG-052). The separate
registered `0052` callback at `0x00060741` has guarded calls to
local `0048`, not a direct call to either message helper.

## Interpretation

The incoming chain is setup, attempted frame registration, separately
dispatched handler with argument value 32, then locally guarded
feedback helper. Calling setup alone does not establish a message.
The earlier reading mistook physical adjacency for a direct function
call chain and is superseded by this finding.

## Alternatives

The shipped matching frame records, producer and enabled mask of the
value-32 event, successful acquisition and lookup, and later callback
changes remain unread (Q-CONFIG-008). A registered path may be
reachable under live conditions, or earlier state may prevent dispatch.
The ordered frame graph and its incoming event route would distinguish
the code-decided parts. Computed calls may provide another route.
This reading proves neither a visible message, a completed item action
nor successful `WIND/10501` setup.

## How to reproduce

Resolve overlay 175's stubs `0057`, `005C`, `0043`, `003E`, `0048`
and `0052` to offsets `0x0005FC0C`, `0x0005FE11`, `0x00060898`,
`0x00060BAB`, `0x000607E5` and `0x00060741` using its declared
trampoline table. Inspect setup windows `0x0005FD3A..0x0005FD5E`,
`0x0005FD60..0x0005FD99` and return `0x0005FE0D..0x0005FE11`.
Inspect the handler prologue and dispatch at `0x0005FE11..0x0005FE6F`;
decode only four value words and four target words at overlay offsets
`0x0748` and `0x0750`. Read its value-32 branch at
`0x00060155..0x000601C2`, and return `0x00060322..0x00060328`.
Map descriptor 46 to resident `3F96` for the setter reading in
FND-CONFIG-074. Resolve the two declared setup-call fixups from
overlay 178 and inspect the guards at `0x00063415` and
`0x00063CD9`. FND-CONFIG-052 gives the bounded message-site readings.
