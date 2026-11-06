---
id: FND-SOUND-053
title: Known DIGPAK software-state writers distinguish start admission from stop and parser termination
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4842:1107..4842:1285
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4842:1468..4842:14CD
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4842:169A..4842:1740
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4842:186B..4842:1906
tool: window_operands.py and resident_listing.py at a38e0bf4; Python 3.12.10, Capstone distribution 5.0.9, xxhash 4.0.1; FND-SOUND-052 and FND-EXE-585
environment: null
---

## Observation

Installed DSUN.EXE is 634416 bytes, XXH3-128
e296af55ba2ecde7e77f555c90f33d0b. Addresses below are hex.
FND-SOUND-052 identifies CS:0E5E as the software state returned
by the known DIGPAK selector 007C. This finding classifies its
literal writer component, not every possible state producer.

The inventory-based window 0E5B..0E62 reports seventeen operands:
eleven name 0E5E, five name adjacent 0E5C and one names 0E60.
The eleven comprise eight literal writes and three reads. The known
stop clear at 18B9 and query read at 18FD are positive controls
from FND-SOUND-052. Listed routines have zero string stores.

| State writer | Local condition and value |
|---|---|
| 1120 | The 1107 helper returns zero when CS:0E58 is zero, otherwise one when CS:0E5A is zero. When neither is zero, it stores three at 0E5E and returns FFFF. |
| 1158 | The 112E stream-processing helper calls 0F31. Result zero or four reaches the store of three and common return. Other results can loop, change the stream pointer/repeat fields, or request processing; those child outcomes remain separate. |
| 14AC | Initialization 1468 calls 077F and 13DA, then clears 0E6A and 0E5E, sets 0E5C to one, and sets 0E58/0E5A to three. Child outcomes and actual initialization admission remain required. |
| 16C4 | Selector 007B's 169A body first requests the stop body 18B3, clears 0E5C, holds its incoming far pointer and stores three before deriving the stream pointer. |
| 1732 | In that 169A body, incoming word at BP+0C equal to FFFF takes the clear directly. Otherwise a loop requests 0F31, exits without this clear when AX is zero, or requests 0F40 and 10B2 until the held 0F40 result matches the incoming word; the match reaches the clear. |
| 188A | Selector 007D's 186B body, with 0E5C nonzero and 0E5E unequal to two, calls 1107. Result other than FFFF publishes two, then passes that result to 1285. |
| 18A3 | In 186B, with 0E5C zero and 0E5E zero, publishes two before calling 112E. Nonzero 0E5E skips this branch. |
| 18B9 | Selector 007E's stop body clears state before the interrupt requests and later software writes recorded in FND-SOUND-052. |

Selector-to-body correspondence uses FND-EXE-585's unchanged
DIGPAK table. In the 0E5C-nonzero start branch, an existing state
two returns without calling 1107. In the zero branch, 112E can
replace the newly published two with three at its 1158 store.
The start body does not normalize AX to a shared success result.
These local writes do not establish which stream or hardware state
was actually admitted, nor retention across interrupts or children.

The census also reports 32 raw candidates outside its established
instruction starts: 31 inside listed instructions and one outside,
494B:0004, a CS-relative read of adjacent 0E60. Many interior hits
are alternate decodings that omit a prefix. These counts classify
report candidates, not alternate-entry reachability. No candidate
is promoted to a real writer from its decoding alone.

## Interpretation

The known literal writer component of Q-SOUND-002 now has finite
conditions: zero, two and three are explicitly stored, and a query
of two reflects the current software word. Calling start does not
prove that its final state remains two; calling stop does not prove
physical silence. This does not assert an exhaustive runtime value
domain: overlapping or indexed writes with other displacements,
computed pointers, segment aliases, external handlers, modified code,
alternate entries and actual child/frame preservation remain open.
The adjacent mode and buffer-state histories also remain required.
No complete reading or whole question closure follows.

## Alternatives

- Start always leaves state two: the zero-mode branch calls 112E
  after publication, and its zero/four result branch stores three.
- State two is a hardware observation: its known producers are
  software stores and its known query is a memory read.
- Inventory operands prove all writers: the report preserves raw
  candidates and excludes computed/address-alias mechanisms.

## How to reproduce

Run window_operands.py with the approved executable,
coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv and window 0E5B..0E62.
The inventory's LF-normalized SHA-256 is
820b317d56cb74114699a8e295b0e93e961900a4b2d7d493fb2240b4daa6afa2.
Review both positive controls and all reported operands, retaining
sound-digpak-state-window.txt under
GAME_DIR/analysis/handler-census-20261106/. Read the four location
ranges with resident_listing.py, retaining
sound-digpak-state-producers.txt and reusing FND-SOUND-052 for
18B3..1906. Do not infer a negative runtime claim from candidates.
No original-game run, emulator session or emulated call was made.
