---
id: FND-COMBAT-161
title: An admitted flags-zero temporary load with successful allocation and a low image word has a closed resident call set until its record is cleared
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 31E0:0E1B..31E0:128C
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 31E0:2670..31E0:271E
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2D40:0461..2D40:048D
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2D40:0C3F..2D40:0C92
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:0452..1000:046E
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:3F7E..1000:3FC1
tool: tools/research/exec-census/resident_listing.py at d1ee2c51; Python 3.12.10; Capstone 5.0.7
environment: null
---

## Observation

Q-COMBAT-045 asks whether a save can contain a populated temporary
presentation slot. FND-COMBAT-149 accounts for the loader's complete
own body. This reading bounds one synchronous lifetime class; it does
not establish the state of every original-made save.

### Scope of the selected continuation

The requested slot is FFFF, mapped to 319 by 31E0:0EFF. The admitted
state has resident DS, nonzero DS:265B, intact frames/flags, valid disjoint
OJFF/local/presentation/object/placed/free-list storage and a non-FFFF
entry index. The low flags byte remains zero. The first allocation request
returns a valid replacement slot from 5 through 318, and the image word
passed to 31E0:2670 is unsigned at most 8000 hexadecimal. Placed-table
counts/extents and the relinking loop are admitted and stable. These are
conditions of the reading, not observations of a populated native slot.

The interval starts with the no-child-call 31E0:0E1B initializer writing
the temporary presentation record, and ends when the loader fills that
record with FF at +124B. The preceding OJFF resource request is outside
the interval; a previously populated incoming temporary record would
require a different lifetime analysis.

### Reached calls and excluded branches

Flags zero skips the RDFF request after initialization. The first
2D40:0461 allocation succeeds, so the parent's eviction/retry branches
are skipped. Its complete allocator body calls only 2D40:0C3F, first for
the preferred head when requested and then for the fallback head if
needed. The complete 0C3F body has no child call: it pops a three-byte
object-table link, updates the appropriate counter and clears the popped
link word. It neither polls input nor dispatches a callback.

The two loader copies use 1000:0452 with fixed lengths three and 37.
Its complete body is a forward word copy plus an optional final byte,
with no child call. The replacement slot is at least five, excluding
the parent's below-four script-chain insertion branch.

31E0:2670's flag-one and flag-two child branches are both skipped. Its
remaining image store has no child call: zero image becomes 8000, and
other images are negated at word width. Every input from 0000 through
8000 therefore produces a negative signed image word. The parent's
subsequent kind-two animation branch requires that word to be signed
greater than FFFF (at least zero), so it skips 31E0:271E in this class.
No lazy-image or animation child is reached here.

The parent's remaining placed-entry relinking is local code without a
child call. It then requests a 37-byte FF fill of the temporary record
through 1000:3FA2. That complete wrapper calls only 1000:3F7E, whose
complete body performs byte/word stores with no child call. It returns
the supplied pointer in DX:AX, and the loader clears the temporary
three-byte-table kind after the fill.

Thus the selected interval's complete reached call set is initialization,
free-list allocation/pop, fixed copies, the flags-zero image-store branch
and fixed fill. None contains an indirect call, input dispatch, window
processing or a call to a save writer. They are resident functions, so
this selected chain adds no overlay call boundary inside the interval.

## Interpretation

This removes synchronous save/input re-entry through the selected normal
flags-zero, first-allocation-success, low-image continuation from
Q-COMBAT-045's unread lifetime cases. It gives a finite distinction from
the paths that can reach resource loading or eviction while temporary
presentation state is populated.

It does not admit flags with RDFF enabled, allocation failure/eviction,
an image above 8000 that can make the stored word nonnegative, changing
gates/flags, invalid or aliased metadata, a populated incoming temporary
record, or asynchronous behavior outside this call-chain reading. Those
cases and save-call timing on other routes remain open. The image range
is a sufficient branch condition, not a census of shipped OJFF values.
No universal absence of a populated saved temporary slot, runtime
observation, complete-reading promotion or validated parity follows.

## Alternatives

Flags zero alone is insufficient: it does not exclude eviction or the
high-image animation branch. Successful allocation alone is also
insufficient if an intervening gate, argument or storage alias changes
the selected path. Conversely, the presence of a lazy-image child in
the whole function does not make it reachable in this admitted class.

## How to reproduce

Resolve the licensed DSUN.EXE through tools/game-dir.mjs and use the
project-local evidence Python with resident_listing.py on all complete
half-open ranges in Locations. Approved readers verify size 634416 and
XXH3-128 e296af55ba2ecde7e77f555c90f33d0b and apply declared mappings.
The combined listing is retained at
GAME_DIR/analysis/hit-incoming-20261104/temporary-normal-callset.txt.
Trace from the initializer through flags, first allocation, copies,
image store/animation gate and temporary fill. Include allocator/pop
and copy/fill bodies, not merely their names. Reuse FND-COMBAT-149,
FND-PARTY-119 and FND-CONFIG-176 for prior contexts and limits.
No original ran; original-derived output remains only in GAME_DIR.
