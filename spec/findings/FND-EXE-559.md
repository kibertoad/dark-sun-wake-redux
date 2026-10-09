---
id: FND-EXE-559
title: Registered game cleanup dispatches mutable slots and then uses a carry-clearing native wrapper
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1425:1684..1425:16BF
  - build: BLD-GOG-EN-1.1
    file: CD:DSUN.EXE
    address: 1425:1684..1425:16BF
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 44DE:02A9..44DE:02CC
  - build: BLD-GOG-EN-1.1
    file: CD:DSUN.EXE
    address: 44D3:02AF..44D3:02D2
tool: Capstone 5.0.7, Python struct and xxhash 4.0.1
environment: null
---

## Observation

FND-EXE-558's registered callback first calls 1425:1684 in both game
editions. That far helper saves BP and SI and tests current DS word
00C4. Zero restores SI/BP and returns far without incoming cleanup.
Nonzero clears SI and enters a slot walk; it does not retest 00C4
between slots.

Each pass forms BX as SI times fourteen at word width. It tests
current DS word BX+3F4E installed or BX+3EC2 on disc. Zero
skips the call. Nonzero recomputes BX, pushes that current word, recomputes
BX again and calls the far pointer at current DS words BX+3F4A
and BX+3F4C installed, or BX+3EBE and BX+3EC0 on disc.
There is no local null-target or target-range check. The tested argument is
reloaded for the push; the pointer is loaded after that push, not held
from the argument test. On return it removes the outgoing two bytes by
popping CX and does not test returned AX or flags.

After a skipped or returning call it increments SI and compares it
unsigned with sixteen. Below sixteen repeats. Otherwise it stores zero in
current DS word 00C4, restores incoming SI/BP and returns far without
incoming cleanup. Under preserved SI and DS, the nominal indices are zero
through fifteen and their products range from zero through 210. The helper
does not locally save SI or DS around the indirect call; target effects
can change the next index, table segment and final store. Neither an
unconditional sixteen-pass bound nor successful cleanup follows from this local
walk. It does not clear the per-slot argument words or targets itself.

The helper makes no local AX assignment. The zero-gate and all-skipped paths
therefore retain incoming AX; paths with calls retain whichever AX the last
returning target supplies, unless a later target changes it. This is not a
zero/FFFF result contract. FND-EXE-558's caller does not test that AX
before preparing its next call and uses current DS after this helper.
All target/input producers, preservation, extents, aliases and runtime admission
remain open.

FND-EXE-558's second callee is 44DE:02A9 installed or 44D3:02AF
on disc. It saves BP, DS and DX, sets AH 41, loads
DS:DX from the four incoming bytes at SS:BP+6 and requests
interrupt 21. On native continuation, carry clear selects AX zero. Carry
set instead loads DS from a relocated segment immediate, stores the returned
AX word at DS:33BE installed or DS:3334 on disc, and
sets AX FFFF. Installed relocation index 3913 at 34DE:02BD
holds shipped segment 47E0, modeled 57E0; disc index 3903 at
34D3:02C3 holds 47D7, modeled 57D7, with load segment 1000.
The clear-carry path does not make that error-word store.

Both paths explicitly clear carry, restore DX, DS and BP and return
far without incoming cleanup. Thus native carry is represented in the AX
word and is not passed back as returned carry. The saved incoming DX/DS
are restored under an intact native stack/preservation contract. The wrapper
does not validate the incoming pointer, scan its contents or test a length.
The actual request outcome and native register/stack effects are not established
by this static reading. FND-EXE-558's callback removes four incoming
argument bytes after this wrapper and does not test its returned AX.

## Interpretation

This resolves both immediate callees of the registered callback locally. The
first depends on mutable indirect targets and does not supply a tested success
result; the second maps native carry to an AX word that the callback ignores.
Q-EXE-007 retains the slot gate/table/target producers, complete caller and
writer coverage, aliases and segment/storage admission, native request effects,
and the earlier registration caller's predecessor/error contracts. No complete
callback effects, cleanup invariant or game launch exclusion is claimed.

## Alternatives

Always walking sixteen slots contradicts the initial gate and depends on SI
preservation across callbacks. Treating a cleared gate as proof of successful
slot cleanup ignores the absent result tests. Treating slot arguments as cleared
after dispatch invents stores the helper does not make. Treating this helper's
AX as a success flag ignores its absent assignment. Returning the native carry
contradicts the wrapper's explicit clear; testing returned carry alone would
discard the AX distinction. Treating the wrapper as admitting the pointed-to
contents ignores its absent validation and unresolved native contract.

## How to reproduce

At revision 3bf9adfa require both identities from FND-EXE-350. Use MZ
header size 5200, modeled load segment 1000 and relative code segments
0425, 34DE installed and 34D3 on disc. Decode the Locations
ranges in sixteen-bit mode, ending after each final far return. Follow both
00C4 gate paths, the argument test/reload, paired target load, cleanup,
unsigned continuation and current-segment final store. Track AX without supplying
a default return value. Follow native carry clear/set through the second helper,
the error store and common explicit carry clear. Read header relocation count
at 0006 and table offset at 0018 and check indices/operands 3913
and 3903 above. Use FND-EXE-558 for connected caller inputs and ignored
results. No negative caller/writer census is claimed. Licensed bytes stay outside
Git; no game process, DOSBox or emulated call runs.
