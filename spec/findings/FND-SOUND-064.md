---
id: FND-SOUND-064
title: Music-selector computed list stores use DS for BP-derived offsets and table indexing wraps within the held segment
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2834:000C..2834:043B
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00067206..0x0006729C
tool: resident_listing.py, direct_callers.py and retained overlay_listing.py report at c92fb4a0; Python 3.12.10, Capstone distribution 5.0.9, xxhash 4.0.1; FND-SOUND-011/012/061/063
environment: null
---

## Observation

Installed DSUN.EXE is 634416 bytes, XXH3-128
e296af55ba2ecde7e77f555c90f33d0b. Addresses below are hex.
This reading assigns explicit address calculations to the known
selector's computed accesses for Q-SOUND-011. It does not exclude
other code's computed accesses to the table or its state.

### Record address calculation

2834:000C reserves 7E bytes and saves SI/DI. Its record loops
hold a word index at SS:BP-2, multiply that index by six with
word-width IMUL and add the product to the table pointer's near
offset. LES loads the segment from DS:424E for the accesses
identified by FND-SOUND-061. Other reads at 00F7, 010B, 0153
and 01FD reload only the near offset and reuse held ES; no local
child call intervenes between each preceding LES and these reads.
Actual pointer stability and asynchronous effects remain separate.
The addition does not carry into ES or normalize the far pointer.

The region byte is sign-extended for comparisons. Song and chance
bytes are zero-extended when passed to their children. Index bytes
from candidate lists are sign-extended before multiplying by six.
Count at DS:4252 is likewise sign-extended before signed loop tests.
Under retained count 38 and an intact loop index, the scan visits
indexes 0..37. Its largest field position is 37*6+5 = E3. With
that admission, a table near offset at most FF1C avoids local
word-offset wrap through the last song byte. Actual allocation,
segment/offset correspondence and capacity are not inferred from
this arithmetic. Changed counts or list bytes need separate bounds.

### Candidate-list writes and backing distinction

The selector initializes list counters and control bytes, but does
not clear the three candidate-list regions. Their near bases are
BP-7E, BP-58 and BP-32, spaced by 26 bytes (38 decimal).

| Store site | Computed destination under unchanged BP/counter | Recorded index |
|---|---|---|
| 016A | DS:(BP-7E + first counter) | Low byte of current SS:BP-2; mode-one any-region candidate |
| 0215 | DS:(BP-7E + first counter) | Low byte of current index; matching mode with band one |
| 023A | DS:(BP-58 + second counter) | Low byte of current index; matching mode with band two |
| 025F | DS:(BP-32 + third counter) | Low byte of current index; matching mode with band three |

Each store forms a near offset with LEA and ADD and writes through
BX without a segment override; it does not write through SS merely
because BP contributed to the arithmetic. Counters and control
locals use BP-relative SS addressing. Under DS=SS, unchanged
count 38, counters beginning zero and retained frames, each list
can receive at most 38 index bytes. Their intended intervals are
BP-7E..BP-59, BP-58..BP-33 and BP-32..BP-0D inclusive.
Those intervals precede controls beginning at BP-0C and fit in the
reserved 7E bytes. This is conditional capacity, not proof of an
actual frame's backing or exclusion of the music globals.

The mode-one selected-list read at 0188 instead uses SS via
BP+SI-7E. Mode-three list reads at 02BD/02DF/037C/03A9/03C4/
03F1 use DS via computed BX from BP-7E plus 26 times the list
selector plus an index. Thus even an intended stack-local list
has distinct DS/SS access mechanisms; actual equal backing or
non-overlap must be shown rather than assumed.

For health eight, selector/last-index bytes retain their initialized
zeros before scanning list zero through index zero. List bytes
themselves were not initialized at entry. For the other selected
bands, the last-index byte derives from the list count minus one.
Whether the selected list is populated, the random-index contract
and retained list contents remain required before claiming every
chosen record is initialized and in bounds. This specializes the
health-eight route already recorded by FND-SOUND-012.

### Direct caller component

The direct-call query reports one resident caller of 000C at
28C9:2346, matching FND-SOUND-012. Loader 043B has two overlay
180 sites, +0097 and +00AA, both established in the retained
startup stream of FND-SOUND-011. They are alternate branches,
not two mandatory consecutive loads: current DS:14E3 selects
the first route with 4734:0128 argument zero or the second with
argument one, then that route calls 043B. Failure joins the common
message continuation. The controls are the prior findings' actual
caller sequences; indirect calls, jumps, changed code and alternate
entries are not covered by the direct-call matcher.

## Interpretation

Q-SOUND-011 can replace the unknown local selector-addressing
component with four exact computed writers, the DS/SS list read
mechanisms, word-offset record calculation and conditional capacity.
It also has the direct loader/selector caller component. Actual
BP/frame versus DS backing, changed count/list bytes, empty-list
admission and unrelated computed/indirect accesses remain open.
These conditions can affect field interpretation and selected extent;
they are not generic unrelated playback obligations. No complete
reading, global alias exclusion or whole question closure follows.

## Alternatives

- LEA from BP makes the later BX store SS-relative: the final
  unoverridden BX memory operation uses DS.
- Every field read reloads a complete far pointer: four reload
  only the offset and reuse ES within the local no-child interval.
- Count 38 proves all list reads initialized: entry does not clear
  list bytes and actual selected-list population still matters.
- Two direct loader sites prove two loads per startup: they are
  on alternate DS:14E3 branches.

## How to reproduce

Read the full selector range with resident_listing.py, retaining
music-selector-address-contract.txt under
GAME_DIR/analysis/handler-census-20261106/. Reuse the controlled
field census and retained sound-preference-startup-caller.txt;
compare the startup prefix's two alternate loader calls. Run
direct_callers.py for 2834:000C and 2834:043B as
music-table-procedure-callers.txt. Track final memory operand segment,
not the register originally used to form an address. No game run,
emulator session or emulated call supplied evidence.
