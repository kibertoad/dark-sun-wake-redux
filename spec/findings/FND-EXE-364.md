---
id: FND-EXE-364
title: Sound utility downstream count guard precedes unchecked preliminary and carry-dependent requests
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1000:3B03..1000:3B6D
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1000:3C4E..1000:3C54
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1000:3C54..1000:3C90
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1000:05CC..1000:05F5
tool: Capstone 5.0.7 and xxhash 4.0.1
environment: null
---

## Observation

FND-EXE-363 passes an extended record byte, buffer pair and wrapped
count to 3B03. Its entry reserves 008E local bytes and saves SI/DI.
It loads the first argument into DI and compares it unsigned with current
DS:DD38. An out-of-range word calls 04CE with word six and selects
the common frame-restoration suffix. FND-EXE-362 reads that mapper.

Otherwise it increments the incoming count at word width and compares
the result unsigned with two. Count words 0000 and FFFF therefore
select AX=0000 directly; counts 0001..FFFE continue. The zero return
for FFFF is not an unsigned positive-count admission or a native result.

The continuing path indexes current DS storage at low-word twice DI
minus 22C6. Bit 0800 set causes a call to 05CC with DI, a zero pair
and word two, followed by eight-byte argument removal. It does not test
returned AX or DX before looking up the indexed storage again. That
later lookup uses current DI and DS; their preservation across the native
request is not established by this local caller.

With bit 4000 clear in the later indexed word, 3B03 calls 3C54 with
current DI, the incoming buffer pair and original count, removes eight
bytes and selects the common suffix without changing returned AX. Bit
4000 set reaches a separate byte-processing path beginning at 3B6D;
that path's expansion, batching and result arithmetic are outside this
bounded prefix finding. The shared suffix restores DI/SI, restores the
frame and far-returns without incoming argument cleanup.

The 05CC helper first clears bit 0200 in current DS storage indexed
by low-word twice its first argument minus 22C6. It sets AH=42 and
AL from the low byte of its fourth argument, BX from the first argument,
CX from the third and DX from the second, then executes interrupt 21.
For the prefix's pushed inputs those encoded registers are AX=4202,
BX=current DI, CX=0000 and DX=0000. Carry clear returns the ordinary
post-interrupt pair without a local normalization. Carry set passes AX to
04CE, then sign-extends returned AX=FFFF into DX=FFFF. The helper
restores BP and far-returns without incoming cleanup. It does not save DS,
SI, DI or ES, and does not locally admit its indexed storage or arguments.

The 3C54 helper also indexes current DS storage by low-word twice
its first argument minus 22C6, without an independent range check. Bit
one set calls 04CE with word five and skips its interrupt. Otherwise it
saves DS, sets AH=40 without separately initializing AL, loads BX from
the first argument, CX from the count and DS:DX from the buffer pair,
and executes interrupt 21. It restores DS before testing carry.

Carry set passes post-interrupt AX to 04CE and returns its FFFF result.
Carry clear preserves post-interrupt AX on the stack while setting bit
1000 in the restored-DS indexed word, then restores AX. This path does
not compare returned AX with the requested count. Both paths restore BP
and far-return without incoming cleanup; SI/DI/ES are not locally saved.
Thus the bit-1000 publication is conditional on carry clear, not on an
equal returned/requested count. DX is not a normalized result in this helper.

## Interpretation

This resolves the count guard and direct-request route behind part of
FND-EXE-363's result comparison. A zero returned word may precede any
native request. A preliminary request may leave indexed state changed
and return a failure pair that the caller does not test before proceeding.
The direct route preserves its post-interrupt AX on carry clear, so the
outer full-word comparison remains a separate decision.

The two native wrappers have different DS handling: 3C54 restores its
incoming DS before error mapping and indexed publication; 05CC has no
local DS restoration. Neither establishes native preservation, successful
operation, buffer extent or lifetime. No external service meanings are
assigned to the observed register selectors here.

Q-EXE-007 retains the 3B6D byte-processing path, native arguments/results
and register preservation, indexed-state writers, actual DS, record/buffer
admission, aliases and lifetime. No complete-reading promotion or launch
exclusion follows from this prefix and wrapper reading.

## Alternatives

Treating only zero count as a no-request case ignores word increment wrap
at FFFF. Treating the preliminary result as a gate ignores its absent test.
Treating bit 1000 as proof of an equal-count result ignores the carry-only
condition. Treating both wrappers as preserving DS ignores 05CC's absent
save/restore. Treating the initial DI bound as a bound for every later lookup
assumes preservation across the reached native request.

## How to reproduce

At revision cbe3a05 require FND-EXE-350's SOUND_DS.EXE identity:
length 204593, XXH3-128 236c2dc23c071eca421eb5b427caee57.
With locked Capstone 5.0.7 in sixteen-bit mode decode shipped half-open
0x00004F03..0x00004F6D at IP 3B03,
0x0000504E..0x00005054 at IP 3C4E,
0x00005054..0x00005090 at IP 3C54, and
0x000019CC..0x000019F5 at IP 05CC, modeled CS 1000,
MZ header size 1400. Bind FND-EXE-363's ordered arguments, track
count increment wrap, both flag lookups, the ignored preliminary result,
DS restoration and carry-dependent AX/state paths. The extension at
IP 05F2 is the sixteen-bit instruction despite Capstone's wider mnemonic.
Original bytes and reports remain outside Git. No original process,
DOSBox, interrupt thunk or emulated call is executed.
