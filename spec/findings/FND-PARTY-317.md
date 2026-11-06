---
id: FND-PARTY-317
title: Timer capture-word census and known callbacks narrow saved-stack alias obligations
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4842:040C..4842:04DD
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4654:09A9..4654:0A96
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 31E0:31A7..31E0:31F7
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4842:0519..4842:0520
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4842:03BC..4842:040C
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4842:0D72..4842:0D78
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008337B..0x00083384
tool: window_operands.py, resident_listing.py and overlay_listing.py at bf131518; Python 3.12.10, Capstone 5.0.9, xxhash 4.0.1; FND-PARTY-316, FND-TIME-012/013/014 and FND-EXE-646
environment: null
---

## Observation

Q-PARTY-136 retains the timer's captured-stack and callback-memory
conditions after FND-PARTY-316. Installed DSUN.EXE is 634416 bytes,
XXH3-128 e296af55ba2ecde7e77f555c90f33d0b. Offsets are hex.

### Capture-word literal window

The operand window 03B3..03BA covers possible displacement overlaps
of the captured words 03B6/03B8 for operands up to four bytes.
Across inventoried linear ranges it has seven operands: the two capture
stores, their two reloads, the private-SP immediate, another immediate
03B8 at 1BF3:298D, and an indexed CS:03B9 jump-table read at
2D40:00E9. Only the capture stores have write or read-and-write access.
Their independently read instructions are positive store controls; both
reloads are read controls. No absence claim is made for other write forms.
There are also 36 raw candidates outside the established starts.

The two other matching routines are not proved capture-word writers by
their numeric operands. In particular the 2D40 jump uses another CS.
The report retains two string stores in 1BF3:2973; its immediate match
does not determine their destination. Different/adjusted displacements,
register-only destinations, wider operands, string/block copies, physical
aliases, alternate streams and indirect transfers remain outside this
classification. The result replaces an unspecified literal-writer question
with the two concrete handler stores, not a whole-memory writer proof.

### Callback effects relevant to the captured and interrupted stacks

FND-TIME-012 supplies registration and slot meaning; this checks the
local memory effects of its fixed callbacks. Under modeled DS 57E0,
the game tick 31E0:31A7 writes byte 265A, dword 145E and byte
1424, and conditionally dword 4C13:035B. It has no child call.
The FLI callback, overlay 196 +040B, increments word DS:21E0
and has no child call. These fixed destinations do not overlap captured
words 4842:03B6/03B8 or the cursor copy 57E0:9B86..9BCF
under those bindings. Actual interrupted-frame backing remains separate.

4654:09A9 first clears a four-byte local and updates fixed words
4E71:0C15, 0C23, 0C19, 0C1B, 0C1D and 0C20. It then tests
4E71:0B54. Zero skips the record path. Otherwise it starts SI at
00F1, tests byte 50C3:(0D2D+6*SI) for one, and, when selected,
loads the far pointer from the following two words. It subtracts ten
from that pointed record's word +0B. A positive signed result skips
the later record effects; other results call 4842:0D72 with the
record's word +0 and zero-extended byte +2, then decrement
4E71:0B54, copy record word +9 to +0B, set byte +2 to 11
and clear the selected table byte. Source/pointer reloads follow the
child call; intact local saved state and aliases remain conditions.

SI increments and a signed comparison repeats while SI is at most
00F1. With the ordinary child's SI unchanged, this examines one
entry, index 00F1, not entries zero through 00F1. The known local
lookup/dispatch instructions do not change SI; an invoked driver may.
Fixed counter writes have separate modeled backing from capture words,
but the pointed record's +0B/+2 writes require actual pointer admission.
Its local stack accesses likewise require private-stack bounds.

0D72 selects driver function 00C1 and tail-jumps to 03F4. That
dispatcher reads its slot argument through SS, uses FND-EXE-646's
03BC lookup, and either returns directly on a null pair or pushes the
target and transfers by RETF. The unchanged internal DIGPAK function
table recorded in FND-EXE-646 has no C1 pair. Under that table's
binding and intact lookup, this request therefore invokes no driver
function. Other admitted tables or changed lookup/slot state remain
indirect targets; C1 must not be treated as an unconditional no-op.

The BIOS-chain callback 4842:0519 pushes flags and calls the stored
old vector at CS:011A. Its memory and continuation contract remains
external evidence, not the local fixed-field exclusions above. The
installed-launch observation in FND-TIME-014 covers its stated interval;
it is not a proof of all later callback/table histories.

## Interpretation

Q-PARTY-136 no longer asks what the fixed game-tick/FLI writes or
the sound callback's local writes are. It retains actual DS and saved
frame backing, the selected sound record pointer and private-stack
extent, changed driver/table/slot admission, BIOS-chain effects, capture
word/nesting writers through excluded mechanisms and alternate transfers.
These are the mechanisms that can change the retained number or the
timer's restoration. No complete retained-number contract, queue closure
or complete reading follows; FMT-PARTY-007 stays supported.

## Alternatives

- The sound callback only increments counters: its selected record path
  writes through a far pointer and can dispatch driver function C1.
- The loop starts at zero: its initializer is 00F1.
- The unchanged internal DIGPAK table supplies C1: its thirteen recorded
  entries contain no such selector; other tables remain conditional.
- An empty extra literal-writer set excludes aliases: the census explicitly
  leaves indirect, adjusted-pointer, copied and alternate-stream writes open.

## How to reproduce

Run window_operands.py with installed DSUN.EXE, the committed installed
DSUN inventory and 03B3..03BA. Check both capture stores and reloads
against FND-PARTY-316. Use resident_listing.py for the resident locations
above and overlay_listing.py for descriptor 196, 8337B..83384.
Reuse FND-EXE-646's table rather than decoding its data as instructions.

Retained reports under GAME_DIR/analysis/handler-census-20261106/ are
getter-timer-capture-writers.txt, getter-timer-known-callbacks.txt,
getter-timer-fli-callback.txt, getter-timer-sound-child.txt,
getter-timer-sound-dispatch.txt and getter-timer-driver-lookup.txt.
The sound-dispatch report's exploratory suffix decodes table data;
that suffix is not instruction evidence. No original game, emulator
session or emulated call supplied new evidence here.
