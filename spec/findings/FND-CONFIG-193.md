---
id: FND-CONFIG-193
title: The graphics-pool initializer installs two fixed roots and 254 free handle slots
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1BF3:271B
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 56B2:0499
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1BF3:28A9
tool: Python 3.14.7 and Capstone 5.0.7 bounded complete resident 16-bit readings with declared FBOV fixup mapping
environment: null
---

## Observation

The complete resident 1BF3:271B body spans
`0x0001384B..0x000138D7` and has one normal far return.
It saves DS/SI/DI, selects DS=CS, writes the slot and
pool fields below without a branch or callee, then
restores those registers. It does not read the two
stacked zero words supplied at the observed call site.
The fields are CS-relative word addresses unless a
byte access is specified.

| Field | Assigned value |
|---|---:|
| E4C, upper paragraph word | AFFB |
| Slot 0: +4, +204 | A000, 03E8 |
| Slot 0: +404, +604, +804, +A04, +C04 | 0, 0, 319, 199, 0 |
| Slot 1: +4, +204 | A400, 03E8 |
| Slot 1: +404, +604, +804, +A04, +C04 | 0, 0, 319, 199, 0 |
| E4E, next paragraph word | A7E8 |
| 254 following slot flag low bytes, offsets C08..E02 stride two | 80 |
| 118E, 1050 | 0, 0 |

The upper word comes from A000+0FFB. The second root
segment comes from A000+0400, and the next paragraph
from A400+03E8. The free-slot loop starts at slot
offset four and runs FE hexadecimal iterations; it
writes only the low flag byte of each of those 254
slot words. It does not clear their other fields or
their flag high bytes. FND-CONFIG-183's scan tests
bit 0080 of these words, so the low-byte writes make
these slots eligible under unchanged high bytes.

Descriptor 180 maps overlay 56B2 and its code at
`0x000671E0`. At local 0499, that code calls 271B
after two zero pushes and then advances SP by eight.
This local body does not consume the pushed values;
the caller's other stack state remains to be read.
The declared fixup on the call segment
word resolves token 0058 through descriptor 11 to
resident 1BF3. Earlier branches and callees in this
overlay remain separate conditions on whether this
setup call is reached in a native session. This is a
call-site reading, not a claim that startup completed.

The complete following resident 1BF3:28A9 body spans
`0x000139D9..0x000139F4`. It subtracts E4E from E4C
as a word, clears DX and shifts the resulting DX:AX
left four times, then far-returns without a guard or
state write. Immediately after 271B's assignments,
those two words differ by 0813 hexadecimal paragraphs,
so this calculation would return 00008130 hexadecimal
bytes. The arithmetic is still conditional on the
two words' state when 28A9 is called; its caller and
any later writers are not settled here.

## Interpretation

271B supplies a concrete initial pair of roots and an
allocation interval for FND-CONFIG-183's slot allocator
and FND-CONFIG-192's root walks. It does not itself
validate native VGA mapping, segment aliases, later
allocations or the fixed AFFB scratch segment used by
FND-CONFIG-192. The initial root count words and the
paragraph gaps are recorded independently; neither
is silently treated as a complete storage contract.

## Alternatives

Q-CONFIG-008 and Q-SCRIPT-003 retain the setup call's
upstream gates, other direct or indirect initializer
calls, later E4C/E4E and slot writers, accepted allocation
state, root aliases and native hardware behavior. One
reading reaches 271B before graphics requests and keeps
these fields; another reaches a later request under changed
pool or slot state. Complete callers/writers and owner
observations where hardware decides the result would
distinguish those states.

A reading that this initializer clears all 256 slot
records is ruled out by its word stores for slots zero
and one and low-byte-only flag loop for the other 254.
Its fixed root assignments do not by themselves prove
that an accepted 43AE transfer writes expected pixels.
No native or emulated result is claimed.

## How to reproduce

Read resident 1BF3:271B through its far return at 27A7,
including every fixed slot word, the FE-iteration byte
loop, E4C/E4E and the two final zero words. Compare
the scan bit in FND-CONFIG-183 and the root fields in
FND-CONFIG-192. Resolve overlay descriptor 180's code
0499 call and its declared segment fixup through
descriptor 11. Read resident 28A9 through 28C4 to
derive its conditional word-subtraction and 32-bit
shift result. Keep other producers, actual call reachability
and native VGA effects outside this local reading.
