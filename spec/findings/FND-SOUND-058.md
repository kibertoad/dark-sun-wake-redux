---
id: FND-SOUND-058
title: DIGPAK capability gates use a cached interrupt result rather than a locally fixed mask
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4842:0A23..4842:0B06
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4842:13DA..4842:145A
tool: window_operands.py and resident_listing.py at c8e79f06; Python 3.12.10, Capstone distribution 5.0.9, xxhash 4.0.1; FND-EXE-585, FND-SOUND-052/057 and FND-PARTY-319
environment: null
---

## Observation

Installed DSUN.EXE is 634416 bytes, XXH3-128
e296af55ba2ecde7e77f555c90f33d0b. Addresses below are hex.
FND-SOUND-057 identifies CS:0E2A bit one as the direct-submission
gate. This reading identifies the known producer and caching rule;
it does not assign the external handler's actual returned mask.

### Installation and initialization epoch

FND-EXE-585 establishes the DIGPAK signature/install route within
4842:0A23. On that route, 0AC5 first rejects a nonzero CS:0E22.
Otherwise it sets that word one, stores the selected slot at 0E20,
and constructs the image pointer at 0E26/0E28 by adding 0100 to
the held offset and subtracting 0010 from its segment. It publishes
the resident table 0D7E in the selected slot and clears CS:0E24
at 0AFD. These are word operations; actual image bounds, slot
admission and offset-wrap conditions remain required. The common
flags-restoration transfer uses the entered helper form discussed
by FND-PARTY-319, not the sequential decompiler view alone.

The known table's selector 0064 reaches 13DA. When CS:0E24
is zero, it loads image pointer 0E26, constructs a far target by
adding 0100 to its offset, stores that target at CS:0E1C/0E1E
and calls it. After ordinary return it requests interrupt 66 with
AX 068C and writes returned AX to CS:0E2A. It tests bit three
of that stored word: when clear it writes FFFF to CS:0DC8;
when set it writes zero there. It then sets CS:0E24 one.
No local success-result test guards that cache publication.

When 0E24 was already nonzero, the function skips the image call,
interrupt, capability store and 0DC8 update. Both routes then load
the image pointer again and copy up to 4E printable bytes starting
at image +0C into CS:0DCC, stopping for a byte below 20. They
write two zero terminators, publish CS at 0DBE and return descriptor
pointer DX:AX = CS:0DB4 under intact ordinary frames. Image contents
and lifetime remain separate from capability-mask evidence.

Thus the known initialization establishes a cache epoch but does
not locally force capability bit one on or off. Later queries under
a retained nonzero flag do not refresh the mask. Earlier image-call
or interrupt effects and modified state may change that history.

### Controlled operand census and limits

The inventory query 0E27..0E2E reports 21 listed operands. The
known same-CS capability store at 1405 and the bit-one read at
0F98 are positive controls. Other CS operands in this window
include the installation pointer word at 0E28, the submission
pointer at 0E2C, bit-three tests in 0E7C/0EB3 and 13DA, and
the bit-one test in 1303. The inventory query reports no string
stores in those matching sound routines.

The report also includes DS-relative accesses in 1BF3 routines,
which are not automatically the same backing bytes. It retains
36 raw candidates outside established instruction starts, including
an alternate dword decoding at 4982:0004 and DS-relative aliases.
None is promoted to a capability writer from a raw decoding alone.
The census does not exclude computed/indexed stores with other
displacements, overlapping writes starting outside the window,
changed segments, external writes, alternate entries or modified code.
The known literal CS-relative producer is not an exhaustive runtime
writer claim, and there is no negative actual-capability conclusion.

## Interpretation

Q-RNG-040 and Q-SOUND-002 can replace the unread known capability
producer with an explicit image-call/interrupt/cache contract. The
no-interrupt submission case in FND-SOUND-057 remains conditional
on the actual cached bit, selected driver and preserved epoch.
External 068C behavior, image-target effects, excluded writers and
subsequent reseeding routes remain evidence obligations. No complete
reading, native mask, audible output or whole question closure follows.

## Alternatives

- The resident initialization fixes the submission gate locally:
  it stores an external interrupt's AX result.
- Every query refreshes the capability mask: nonzero 0E24 skips
  both the request and store.
- Equal displacements identify the same capability storage:
  the census includes DS-relative accesses with separate bindings.

## How to reproduce

Run window_operands.py with the approved executable,
coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv and window 0E27..0E2E.
Inventory LF SHA-256 is
820b317d56cb74114699a8e295b0e93e961900a4b2d7d493fb2240b4daa6afa2.
Retain sound-capability-window.txt and the two location ranges
from resident_listing.py as sound-capability-initialization.txt under
GAME_DIR/analysis/handler-census-20261106/. Review both positive
controls and all exclusions. Reuse the entered flags-helper contract
rather than inferring return behavior from an adjacent linear decode.
No game run, emulator session or emulated call supplied evidence.
