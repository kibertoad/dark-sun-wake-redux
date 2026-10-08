---
id: FND-EXE-199
title: Zero-mode setup admission excludes some incoming-slot overlaps when duplicate callback words are nonzero
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x006008F0..0x00600924
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x00600AF2..0x00600B25
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x005F50A0..0x005F50D6
tool: Ghidra 12.1.3 PUBLIC
environment: null
---

## Observation

FND-EXE-198 places the incoming sixth slot at C +28 and its pointed-to
selected local at C +76, where C is FND-EXE-165's conventional callback
frame. The callback supplies setup record R = C -108. On setup's
nonnull-shared path, its two mode reads are full dwords at P +48, where
P is the loaded shared base. A negative first read or nonzero second
read leaves the direct zero-mode path. There is no call between these
reads on that path. It next loads the old dword at P +40, writes that
value at R, then writes R at P +40 before restoring its saved registers
and returning. The old link is read before either publication.

Consider valid stack extents, equal admitted DS/SS bases, unchanged
incoming callback words through this direct prefix and no external
intervening writer. Four-byte stores can overlap protected four-byte
slots at unaligned starts; equality of their starting addresses is not
the only overlap. The shared-field store overlaps the sixth slot when
P is one of C -15 through C -9, inclusive. It overlaps the selected local
when P is one of C +33 through C +39. R's own first-dword store has a
fixed distinct numeric interval and does not overlap either slot under
this admitted geometry. These candidate sets do not prove any P occurs.

FND-EXE-053's two earliest outgoing pushes both use the same EAX word V.
After the callback establishes C, they occupy its seventh and eighth
slots at C +32..C +36 and C +36..C +40. V retains the current record
address's upper 24 bits; its low byte is replaced by the equality result,
zero or one. A current record address at least 256 consequently makes
V nonzero, regardless of that equality result. A merely nonzero record
address alone does not establish this condition. Before the mode guard,
the cited callback prefix and setup prologue write their own saved-frame,
saved-register and local slots, all numerically below C +32. They make
no direct store to either duplicate incoming word. On this no-call route,
their fixed frame writes therefore preserve those two intervals under
the admitted segment/stack model and absence of an external writer.

For candidate shared bases P = C -15, C -14, C -13 or C -12,
the full mode read at P +48 begins at C +33, C +34, C +35 or C +36.
Each of those four-byte windows lies wholly within the two repeated V
words. Its bytes are a rotation of V's bytes, so a nonzero V makes
every such mode word nonzero. Those candidates cannot reach the direct
zero-mode publication under the stated preservation/segment conditions.
This includes the exact shared-field/sixth-slot alias P = C -12
considered conditionally in FND-EXE-167.

The remaining incoming-slot candidates P = C -11, C -10 or C -9
read mode windows that extend beyond C +40. The duplicate words alone
do not establish those windows. They can affect trailing bytes of the
sixth slot even though their shared-field store starts after its first
byte. The selected-local candidate set also has no mode-zero exclusion
from these duplicate words. A full-word preservation claim therefore
cannot follow from excluding the exact alias alone.

## Interpretation

This combines a concrete writer's admission guard with the caller's
last-written argument bytes. It narrows, but does not eliminate, the
shared publication's possible aliases on the no-call zero-mode route.
Q-EXE-009 retains P's actual provenance, V's admission and preservation,
remaining partial overlaps, selected-local aliases, segment identity,
setup's other paths and external/exceptional effects. SRC-WIN32-X86-ABI
is an external contract only. No native corruption, pointer preservation
or complete reading is established.

## Alternatives

Checking only equal destination starts would miss partial writes to
argument bytes. Conversely, admitting every geometric overlap without
checking the mode read would retain cases that this branch guard rules
out when V is nonzero. With V zero, the duplicate-word exclusion does
not apply; no shipped input establishing that alternative is claimed.

## How to reproduce

Use shipped interpreter XXH3-128 `09861838aa3018346f9f15c9a4f5925c`.
In the saved project, read-only with analysis disabled, run
ReportInstructionWindow at `0x006008F0`, count 25; at `0x00600AF2`,
count 27; and at `0x005F50A0`, count 24. Restrict claims to the cited
spans and the specified direct route. ReportCitationBoundaries for
`006008F0..00600924:return` and `00600AF2..00600B25` confirms their
decoded endpoints, not interior completeness or reachability.

Use FND-EXE-198's frame mapping. Enumerate four-byte overlap starts
relative to protected slots C +28 and C +76 and subtract shared-field
offset 40; retain the inclusive integer candidate sets above. For each
of the four excluded candidates, add mode offset 48 and compare its
four byte positions with repeated V at offsets 32 and 36. Each byte
position modulo four occurs once in each such window. Follow the full
mode tests before either store, rather than treating geometric overlap
as an admitted execution. Keep rich reports local and execute no
original program.
