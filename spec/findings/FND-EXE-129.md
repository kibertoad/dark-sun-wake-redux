---
id: FND-EXE-129
title: Full-width reader combines mode-dependent entry-bit gates into four local selector values
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x00689909..0x0068998C
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x00689A38..0x00689A70
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x00689B20..0x00689B4F
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x00689B8B..0x00689BC0
tool: Ghidra 12.1.3 PUBLIC bounded full-width entry-bit selection reading
environment: null
---

## Observation

FND-EXE-128 records the two-level lookup's retained words W0 at current
ESP plus `0x18` and W1 at plus `0x14`, reaching
`0x00689909` when both low mask-one gates pass. This continuation
initializes full local selector C at current ESP plus twelve to zero.
It separately derives P0 and P1 as zero or one from each retained word's
low-byte mask four, then reads full mode M from `0x006F0090`.

It forms wrapped M minus 64 and compares unsigned with sixteen. An
in-range value selects a bit in full mask `0x00010021`, identifying
M equal to 64, 69 or 80. Those three values use predicate P0 zero OR
P1 zero. Every other full M, including values outside that range, uses
predicate P0 zero AND P1 zero. The default path tests the OR of the
two zero-or-one inputs for zero. The special path separately forms their
zero tests, ORs them and masks to full one; partial AL/CL writes cannot
contribute upper bits to the resulting predicate.

Predicate false jumps directly to `0x00689A50` with C still zero,
without reading the following global intersection. Predicate true reads
fresh full `0x0075B144` and `0x0075B140`, ANDs them as H and
tests full equality with three. H exactly three sets C to three and
joins `0x00689A50`. This is equality, not merely presence of both
low bits: H seven does not take that edge. Otherwise M in the set
48, 64, 69 and 80 sets C to one; every other M leaves C zero. The
mode classification uses equality/unsigned branch edges, not a broad
numeric interval. There are no intervening calls in these gates.

The first selector stage is therefore:

| Predicate | H / retained M condition | C at the first join |
| --- | --- | --- |
| false | H not read | zero |
| true | H exactly three | three |
| true | H not three, M in 48, 64, 69, 80 | one |
| true | H not three, all other M | zero |

At `0x00689A50` it tests W1's low-byte mask two, then W0's
low-byte mask two when the first test passed. Both present go directly
to `0x00689A70`. Either absent takes `0x00689B20`, which reads
C and immediately rejoins `0x00689A70` if C is nonzero. Only C
zero performs another classification of retained M. M in 48, 64, 69
or 80 sets C to two and rejoins; other values leave C zero. Setting
two also replaces full EBX with two, while the other local selector
assignments above leave EBX holding M. This is an observable difference
for later code and is not silently treated as preserving the mode register.

Thus the second stage changes only an incoming C zero, only when at
least one mask-two bit is absent, and only for the stated four modes.
Earlier C one or three is not overwritten with two. Both mask-two bits
present preserve even C zero. These gates consume retained local entry
bytes; they do not reload backing entries or call a helper. The later
branch at `0x00689A70` reads C again, and its mapping/read/cleanup
continuations require further evidence. C is a local selector, not a
proved success flag, permissions API or returned value.

## Interpretation

The full-width reader's local branch table distinguishes two entry-bit
pairs, a mode-dependent predicate, an exact global-intersection case and
ordered selector precedence. Q-EXE-009 in FMT-EXE-006 remains open for
mode/entry/global producers, downstream selector meanings, mapping/read
and cleanup effects, missing-entry helper effects and remaining
selected-callee branches. These observations do not establish a complete
permission model or actual PATH behavior.

## Alternatives

- The special predicate applies only to modes 64, 69 and 80, not every
  value from 64 through 80. Mode 48 belongs to later classification but
  uses the default first predicate.
- An intersection containing three plus other bits is not H equal to
  three. The full equality has precedence over classification as one.
- The second stage does not replace an earlier selector one or three,
  and both mask-two bits present leave selector zero unchanged.
- Local selector two changes EBX as well as C; full selector assignments
  and return meaning must not be conflated with partial-register tests.

## How to reproduce

Verify FND-EXE-011's executable identity, FND-EXE-099's six physical
controls and FND-EXE-126's full-width target slot. Use the saved Ghidra
program with -noanalysis and ReportInstructionWindow.java at
`0x00689909` limit 33, `0x00689980` limit five,
`0x00689B40` limit twelve, `0x00689B20` limit twenty,
`0x00689B8B` limit twenty-one and `0x00689A38` limit three.
Read `0x00689991` limit sixty only for the already reported
`0x00689A50` through `0x00689A70` gates. Restrict observations
to the declared selection ranges and exclude following cleanup/read
paths. Track local byte provenance, zero-extension before AL/BL shifts,
SETZ partial writes, the final low-bit mask, unsigned wrapped mode index,
full equality with three, flags through direct jumps and last writers
of both C and EBX. Check the four P0/P1 combinations in default and
special modes, mode 48 versus 64/69/80, H three versus seven and C
zero/one/three entering the second gate. These are instruction-reading
controls, not native observations or evidence that those inputs occur.
No native or emulated execution is part of this observation.
