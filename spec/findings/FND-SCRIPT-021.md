---
id: FND-SCRIPT-021
title: Cache replacement selects the first largest signed age and invalidates four fields
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 172C:07BB
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 172C:0537
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 172C:07A7
tool: Python 3.14.7 and Capstone 5.0.7 entry-based bounded 16-bit instruction checks and declared MZ relocation mapping
environment: null
---

## Observation

The complete near helper 172C:07BB occupies file offsets
`0x0000CC7B..0x0000CD0E`, ending with the near return at
`0x0000CD0D`. Its six declared state-segment relocation
operands resolve to 4C13. It has no argument read, call,
interrupt or indirect transfer.

The helper initializes a local selected index to zero and
visits indices zero through 15. It compares each age byte
at 4C13:0239 with the selected slot's age. A signed
less-or-equal branch retains the selected index; only a
strictly larger signed age replaces it. Thus the first
slot with the largest signed age wins, including ties at
slot zero. A negative age does not outrank a nonnegative
age because of its unsigned representation.

After the scan it writes the selected slot's start word
at 0219, end word at 01F9 and resource-number word at
01D9 to FFFF, in that order. It then writes the age byte
at 0239 to FF (signed minus one), zero-extends the index
into AX and returns it. It does not clear the selector
array at 01B9, the current pair, code start or script
buffer bytes. It does not validate occupied slots before
comparing ages or treat a free marker specially.

FND-SCRIPT-019's fill body reaches this helper at 0537
only after its scan found no FFFF start. The returned AX
becomes the selected slot index. The following comparison
uses that slot's resource-number word, which the helper
has just set to FFFF, against the requested number. The
fill entry has already rejected a requested FFFF number.
Under unchanged ordinary array state, this replacement
route therefore proceeds to the resource-size query;
its selected-number bypass cannot take that equality.
The first-free-slot route can still have that bypass.

The allocator's known call at 07A7 ignores the returned
index, resets its local candidate offset to zero and
returns to its room-check loop. Its complete allocation
contract, arithmetic, termination and callers remain a
separate dependency; this helper itself does not move
or compact script bytes.

## Interpretation

The local replacement contract is a signed-age selection
followed by four invalidating writes. It is not an
unsigned-age selection, a last-tie policy or script-buffer
compaction. The loader's full-cache route changes state
before requesting a new resource. Subsequent I/O and
error handling do not acquire a rollback guarantee from
this helper.

## Alternatives

Q-SCRIPT-003 retains age and cache writers, valid state,
allocation and error helpers, archive selection and I/O.
One reading supplies ordinary nonnegative occupied-slot
ages; another supplies negative, tied or changed ages.
The same signed comparison and first-tie rule apply, but
actual inputs determine the chosen slot. The caller's
state can also be invalid or aliased; no universal valid
cache invariant follows from the fixed-index local loop.

Q-SCRIPT-007 retains emulator cases for distinct maxima,
ties, mixed signed ages and all-negative ages, including
the four writes and fields left unchanged. The harness
does not yet exist, and no native run or emulated result
is claimed here.

## How to reproduce

Read 07BB from its entry through 084D, verifying the
16-iteration loop, signed conditional branch and strict
replacement of the local index. Resolve all six declared
state-segment relocations. Record each final field store
and the zero-extended return separately. Read fill 04CF
from its entry through the call at 0537 and comparison
at 054A, retaining the earlier requested-number filter.
Read allocator 0698 from its entry to locate its 07A7
call and the following local reset and loop continuation.
Keep that allocator's full contract and every cache-input
producer outside this helper's bounded claim.
