---
id: FND-PARTY-318
title: Existing sound-record seed and driver write domains narrow timer backing admission
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x00047103..0x00047109
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4654:09A9..4654:0A96
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4842:03BC..4842:040C
tool: window_operands.py at 350ed79d; Python 3.12.10, Capstone 5.0.9, xxhash 4.0.1; existing FND-RNG-081/082/093, FND-EXE-646 and FND-PARTY-316/317
environment: null
---

## Observation

This is an existing-evidence review for Q-PARTY-136's selected sound
record and driver effects. Installed DSUN.EXE is 634416 bytes,
XXH3-128 e296af55ba2ecde7e77f555c90f33d0b. Offsets are hex.

FND-RNG-081 already records the shipped pointer of entry F1 in
the six-byte table at 50C3:0D2D: after relocation it names
50C3:0000. The entry itself is 50C3:12D3; its pointer words
are 12D5 and 12D7. These follow 0D2D + 6*00F1, with no
word wrap for this index. They are not newly discovered storage.

With this unchanged pointer, FND-PARTY-317's record writes are word
50C3:000B and byte 50C3:0002. They do not overlap the timer's
captured words 4842:03B6/03B8, its private stack's admitted
nonwrapping region below 4842:03B6, or cursor storage
57E0:9B86..9BCF and number word 57E0:9BE2. This numerical
separation does not establish the actual interrupted SS frame or prove
that the pointer remains unchanged. The post-driver reload of the saved
local record pointer separately requires that local frame to survive.

The new overlap census uses windows 0D2A..0D34 for the encoded
indexed fields and 12D0..12DA for the final entry address. The first
has 60 established operands and 95 raw candidates outside established
starts. Its only established write operands are byte flag stores at
4654:06D5, 0A83 and 0D41, all displacement 0D2D. There is
no established pointer-field store in that result. The final-address
window has zero operands and zero raw candidates. The known callback
flag clear at 0A83 and pointer read at 0A27 are independent positive
controls for this source and indexed mechanism. The final-address
window has no positive hit and alone supplies no absence proof.

This result does not exclude a wider store, another displacement,
adjusted pointer, different index/segment, register-only/string destination,
table copy, dynamic code or alternate instruction stream. It replaces
an unspecified literal-pointer-writer question with those exclusions and
the three classified flag stores. It does not promote the shipped pointer
to an invariant.

FND-RNG-082 already classifies the write domains of functions 67,
C1 and B5 in the ten shipped ADV drivers with timer function 67.
For its completely followed ADV 11 path, relevant writes are frame
words, driver CS, registered sequence blocks and the named game callback
fields. Its C1 body writes frame words and CS variables. Those paths
are not wholly unread driver effects. Other drivers retain its stated
unfollowed external calls/interrupt and less complete segment-producer
readings; an arbitrary changed driver is outside that evidence.

FND-RNG-093 qualifies allocation backing: startup's modeled far-heap
base is at least A6F0 beyond DS:0000 under its explicit stable-field,
segment and nonwrapping assumptions. Valid later heap links, extents,
callback routes and pointer lifetimes remain necessary. It supersedes
FND-RNG-083; its conditional heap result does not prove separation
from an arbitrary interrupted stack. FND-EXE-646 and FND-PARTY-317
separately exclude C1 invocation through the unchanged internal DIGPAK
table, rather than treating every driver as following the ADV 11 route.

## Interpretation

The unchanged shipped sound-record case and known driver write domains
can be removed from unspecified timer writers. Q-PARTY-136 retains
actual pointer/table history through excluded mechanisms, local and
interrupted-frame backing, relevant driver/sequence-block extents and
lifetime, excluded driver transfers and BIOS-chain effects. No complete
retained-number contract, complete reading or queue closure follows.
FMT-PARTY-007 remains supported.

## Alternatives

- The selected record has no known seed: FND-RNG-081 supplies it.
- No indexed literal pointer store means the pointer never changes:
  copies, adjusted addresses and other excluded mechanisms remain open.
- Known ADV services are wholly unread: FND-RNG-082 supplies bounded
  write domains, with explicit remaining external and backing conditions.
- Heap separation from resident data proves separation from every stack:
  FND-RNG-093 does not establish the interrupted frame's identity.

## How to reproduce

Review FND-RNG-081/082/093 before new callback or allocator reading.
Run window_operands.py with installed DSUN.EXE, the committed installed
DSUN inventory, 0D2A..0D34 and 12D0..12DA. Preserve access widths,
the indexed callback controls, all raw candidates and excluded mechanisms.
The retained report is getter-sound-record-writers.txt under
GAME_DIR/analysis/handler-census-20261106/. The exploratory
getter-sound-record-activation.txt is not needed for this assessment.
No original game, emulator session or emulated call supplied new evidence.
