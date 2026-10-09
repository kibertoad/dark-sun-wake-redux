---
id: FND-EXE-272
title: Loader allocation callee rounds byte requests to paragraphs and traverses mutable segment links
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:15A5..1000:1622
tool: Capstone 5.0.7 and xxhash 4.0.1
environment: null
---

## Observation

The selected far callee establishes BP from SP and reads the incoming
request's low and high words at SS-relative BP plus six and eight into
AX and DX. It saves SI and DI and publishes incoming DS to CS-relative
word `0x1361`. An all-zero request reaches restoration without allocation.
A nonzero request adds nineteen to DX:AX with carry, rejects carry out
of the high word, and rejects any set bit in DX masked by `0xFFF0`.
It then shifts AX right four and DX left four, combining DL into AH.
The resulting AX is the unsigned adjusted byte request divided by sixteen,
not the original byte count.

It tests CS-relative word `0x135B`. Zero calls `1000:14C4` and takes
the restoration suffix. Nonzero reads CS-relative word `0x135F`.
A zero latter word calls `1000:1528`. Otherwise it retains that segment
in BX, loads DS from it, and compares DS-relative word zero with the
paragraph count unsigned. A smaller word follows DS-relative link word
six into DX, repeating while that link differs from retained BX. Closing
that cycle calls `1000:1528`. There is no independent local iteration limit
or validation of each linked segment's initialized extent.

A larger available count calls `1000:1582`. Equality calls `1000:143B`,
then reloads current DS-relative word eight into BX and stores it into
current DS-relative word two, and sets AX to four. Thus this exact-size
path has a post-callee state read and publication; pre-call DS preservation
cannot be assumed for that access. All called helpers' result and storage
contracts remain unread in this finding.

Every ordinary suffix reloads DS from the shared CS-relative slot `0x1361`,
restores DI, SI and BP and far-returns without argument cleanup. The local
rejection path zeros AX and sign-extends its word value into DX before
that suffix. The shared DS slot is not a private stack save; intervening
callee or reentrant writes would affect the restored segment.

## Interpretation

FND-EXE-271's wrapper supplies a byte request equal to sixteen times its
saved word count. For a nonzero saved count through `0xFFFE`, this callee's
conversion requests one additional paragraph. Saved count `0xFFFF`
produces byte request `0x000FFFF0`, whose adjusted value `0x00100003`
fails the high-word mask. Zero requests bypass the helpers. These local
arithmetic cases do not establish an allocator unit/header contract,
actual returned pointer, initialized allocation extent or safe native bounds.

Q-EXE-001 and Q-EXE-010 retain the four helper effects, linked-state writers
and lifetime, shared saved-DS writers, actual returned DX:AX and the root's
derived segment bounds. The wrapper's zero-pair test cannot supply those
missing contracts. No complete_reading or replacement inventory follows.

## Alternatives

Calling AX a byte length after conversion ignores the paragraph shift.
Treating saved DS as protected by the stack ignores its shared code-segment
slot. A circular-list exit test does not prove every admitted link chain
closes or that no callee changes it. The exact-size path's AX four is only
the local offset publication; DX and the helper's actual storage meaning
must be traced independently.

## How to reproduce

At revision `c54331b`, verify the installed source identity in FND-EXE-236.
Decode shipped `0x000067A5..0x00006822` in sixteen-bit mode, initial IP
`0x15A5`, model segment `0x1000`, MZ header `0x5200`. Use FND-EXE-271's
far call and push/pop sequence as the conditional incoming-frame provenance.
Verify that call's segment operand at shipped `0x00040DAC` using the
operand wrapper with targetOffset `0x15A5`: its MZ relocation resolves
to `1000:15A5`, shipped `0x000067A5` under load segment `0x1000`.
Follow zero and nonzero requests, both overflow checks, the word-width
conversion, list re-entry and each helper continuation independently.
The sixteen-bit sign-extension instruction is the same decoding distinction
controlled in FND-EXE-258: Capstone's printed cdq name does not widen AX
to EAX in this mode. Retain helper effects and the shared saved-DS lifetime
as open obligations. Source and reports stay in GAME_DIR.
