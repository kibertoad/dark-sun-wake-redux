---
id: FND-SOUND-026
title: Setup count allocations zero-extend and round requests, and group clearing covers the requested span
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1000:04CE..1000:0507
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1000:1A6C..1000:1A95
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1000:1AF5..1000:1C5D
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1000:1DBE..1000:1E32
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1000:1E73..1000:1EFE
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1000:25FD..1000:2619
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1000:310F..1000:3152
tool: tools/research/exec-census/resident_listing.py at b5bd8e13; Python 3.12.10, Capstone 5.0.7
environment: null
---

## Observation

FND-SOUND-024/025 bind installed count conversion to the allocation and
clearing calls. This reads the requested-size treatment, normal returned
offset and exact clearing span. It does not establish the native heap or
DOS memory service outcome. Offsets and selectors are hexadecimal; sizes
and counts are decimal unless explicitly prefixed.

### Request and normal-return paths

At 1000:1BD6 the one-word request is loaded into AX after DX is cleared.
It is therefore zero-extended, including when the caller supplied a low
word derived from a signed negative count. The common path saves SI, DI
and the caller's DS, and returns a zero far pointer for a zero request.
For a positive request it adds 19 to the double word, rejects overflow or
values outside its paragraph representation, then shifts to obtain the
paragraph count floor((request+19)/16).

| Call input | Requested bytes | Paragraphs | Block bytes minus four-byte prefix |
| --- | ---: | ---: | ---: |
| 13 records times 254 | 3302 | 207 | 3308 |
| 13 labels times 80 | 1040 | 66 | 1052 |
| Low word of -1 times 254 | 65282 | 4081 | 65292 |
| Low word of -1 times 80 | 65456 | 4092 | 65468 |

Each request passes the local representation checks. Neither of the last
two requests is treated as negative or zero by this entry.

With an existing free-list root, the allocator follows the circular list
looking for a block at least the requested paragraph count. An exact-size
block is unlinked through 1A6C; a larger block is split through 1BB3.
Both normal paths return AX=4 and the selected segment in DX. Without a
suitable free block it attempts heap growth through 1B59; without an
initial heap root it uses 1AF5. Those paths also return offset four on
their normal successful continuations and a zero far pointer on their
explicit growth-failure branches. This is conditional on well-formed heap
links and valid service results; malformed cycles and all heap producers
are not admitted by this reading.

The growth child 1E73 checks the proposed break against its recorded
bounds and returns FFFF:FFFF on its local failure branches. Its adjustment
child 1DBE either updates the stored break within the current allocation
unit or calls 1000:25FD. That routine issues interrupt 21 with AH=4A,
ES from its segment argument and BX from its size argument. Clear carry
maps to AX=FFFF; set carry saves returned BX, calls error helper 04CE
with returned AX and then returns the saved BX. The error helper records
error state and removes its one argument with RET 2. 1DBE distinguishes
that FFFF success convention and
updates its break or limit fields accordingly. Thus neither a large
request nor the local rounding calculation proves native success.

### Exact group clearing

The group reader passes its returned far pointer, zero and the low-word
requested length to 1000:3133. That wrapper forwards the byte value and
length to 310F, then returns the original pointer. The worker sets ES:DI
to the destination, duplicates the fill byte into AX and clears direction.
For an odd destination it writes one byte only when the count is nonzero,
then reduces the count. It writes count/2 words and the remaining odd byte.
There are no nested calls, interrupts or ports in this worker.

For a normal allocator result at offset four and each tabled length, this
clears exactly the requested bytes, does not wrap the offset and does not
touch the four-byte allocation prefix or the rounded trailing slack.
The worker saves/restores DI and BP, leaves SI and DS untouched, and
clobbers ES; the wrapper returns the original segment/offset in DX:AX.
No extra guard is provided for an invalid pointer or an arbitrary wrapping
length. Valid, disjoint allocation and stack spans remain required.

## Interpretation

This removes the allocator's local signed-rejection alternative and the
group clearing's length/loop uncertainty for the installed-count requests.
Successful clearing does not manufacture a music-only record: the group
reader still skips record reads under the negative signed count, and the
label builder still produces no label (FND-SOUND-024).
Q-SOUND-010 retains native heap/break admission, state retention and actual
chooser/child behavior. A zero-count group would request zero here and
fail its caller's nonnull requirement; it is not a native empty-list success
contract. No setup process or DOS service was executed.

## Alternatives

A malformed or insufficient heap, a failed service request, changed counts,
reentrancy affecting the allocator's saved-DS slot, or later child mutation
can prevent the stated normal continuation. The local representation check
cannot decide among those native states. A universal nonwrapping allocator
or clearing contract is not claimed from these particular lengths, and
the remaining heap/error paths are not a complete-reading promotion.

## How to reproduce

Use SOUND_DS.EXE length 204593, XXH3-128
236c2dc23c071eca421eb5b427caee57 and resident_listing.py's fixed load
model. Retain source-derived output only under
GAME_DIR/analysis/disc-adv-survey-20261104/.
setup-allocation-and-clear.txt covers the allocation entry and clearing
wrapper; setup-allocation-helpers.txt covers the free-list/split/growth
helpers and exact 310F..3133 worker. The words before 1998 are allocator
state, not instructions. setup-allocation-break.txt and
setup-allocation-break-boundaries.txt cover the break adjustment and its
arithmetic boundaries; setup-allocation-dos-resize.txt gives 25FD..2619.
setup-allocation-error-helper.txt covers 04CE..0507 through RET 2.
Read only the named reached entries and returns; later linear-listing text
is not part of this claim. Derive the table with zero-extension, addition
of 19 and division by sixteen, keeping native success separate from arithmetic.
