---
id: FND-EXE-209
title: Two ordinary construction callers prepare the same callback with distinct metadata addresses
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x006D6C70..0x006D6CA3
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x005FE630..0x005FE669
tool: scientific-method-engine 13.5.0 and Ghidra 12.1.3 PUBLIC
environment: null
---

## Observation

The two ordinary callers in FND-EXE-050 directly prepare these four-byte
fields before their setup calls. F is each caller's own conventional frame;
the two frames and their local records are distinct.

| Caller entry | Record address | Offset 24 | Offset 28 | Offset 32 | Offset 36 | Offset 40 |
| --- | --- | --- | --- | --- | --- | --- |
| `0x006D6C70` | F -124 | `0x005F50A0` | `0x006EE868` | F -24 | `0x006D6D20` | F -136 |
| `0x005FE630` | F -64 | `0x005F50A0` | `0x006EF6D3` | F -12 | `0x005FE6A0` | F -72 |

Each offset-24 word is FND-EXE-165's callback address. The metadata
words at offset 28 differ from each other and from that callback's own
nested-record metadata address `0x006EE824`. No metadata byte content
or common schema is inferred from these pointer stores.

In the first caller, three saved-register pushes and a 124-byte reservation
leave ESP at F -136. That value is saved at record offset 40 before
twelve additional outgoing bytes are reserved. The local address F -124
is pushed as setup's argument; the call at `0x006D6C9E` then puts its
return address below that argument. The offset-24 and offset-28 stores
at `0x006D6C85` and `0x006D6C8F` are each full-word immediate stores.
There is no intervening call or direct rewrite of these fields before setup.

In the second caller, three saved-register pushes and a sixty-byte reservation
leave ESP at F -72. That value is saved at record offset 40 before the
twelve-byte outgoing reservation. Its offset-24 and offset-28 immediate
stores occur at `0x005FE639` and `0x005FE64F`. It pushes F -64 and
then writes offset 36 before calling setup at `0x005FE664`; that late
field write does not replace the pushed record address. There is no earlier
call in the cited prefix and no direct rewrite of either target field.

These are the direct pre-setup writers. FND-EXE-167 and FND-EXE-188
still govern setup's mode-dependent link publication and intervening calls.
They do not admit these fields as unchanged after setup or prove that
FND-EXE-053's traversal later selects either local record. The record
pointer's stack storage, target field and metadata field are separate values.

## Interpretation

This supplies concrete offset-24 callback producers and additional offset-28
metadata producers behind FND-EXE-050's ordinary construction callers.
Q-EXE-009 must establish selection, preservation, source extent and metadata
consumption for each admitted record origin. FND-EXE-197's five-byte
reading applies to its own metadata address; it cannot discharge these two
distinct producer inputs. No complete-reading or format-status promotion follows.

## Alternatives

Equating every record exposing the callback with that callback's own nested
record would incorrectly equate three different metadata pointer values.
Equating offset 24 with offset 36 also loses the separate stored targets.
The fixed local fields do not prove disjointness from pointer-based setup
writes, segment-base identity, exceptional transfers or external effects.

## How to reproduce

Use shipped interpreter XXH3-128 `09861838aa3018346f9f15c9a4f5925c`.
In the saved PE project, read-only with analysis disabled, run
ReportInstructionWindow at `0x006D6C70`, count 28, and `0x005FE630`,
count 26. Restrict this finding to the cited prefixes ending immediately
after their setup calls. Trace every saved-register push, local reservation,
field-relative store, address formation, outgoing reservation and argument
push. Compare the stored callback with FND-EXE-165 and the distinct metadata
address with FND-EXE-197. Preserve setup and selection obligations from
FND-EXE-167, FND-EXE-188 and FND-EXE-053. Keep rich reports in the
licensed-source store, outside Git. Execute no original program.
