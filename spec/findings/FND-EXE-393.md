---
id: FND-EXE-393
title: Game type-two slot clamps a queried span and its cleanup forwards to a far no-op
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1425:0329..1425:041A
  - build: BLD-GOG-EN-1.1
    file: CD:DSUN.EXE
    address: 1425:0329..1425:041A
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1425:031B..1425:0329
  - build: BLD-GOG-EN-1.1
    file: CD:DSUN.EXE
    address: 1425:031B..1425:0329
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 15F3:036F..15F3:0371
  - build: BLD-GOG-EN-1.1
    file: CD:DSUN.EXE
    address: 15F3:036F..15F3:0371
tool: Capstone 5.0.7, Python struct and xxhash 4.0.1
environment: null
---

## Observation

FND-EXE-391's type-two dispatcher branch calls 1425:0329. Its
incoming SS:BP+6 far pointer names an index word, SS:BP+0A
and +0C hold record words two and four, and SS:BP+0E
is a far quantity pointer. The helper saves BP, SI and DI and
holds the two record words in SI and DI. SI below 0040
returns AX two. Nonzero DI at most SI unsigned also returns two.
DI zero bypasses that latter check.

The continuing path calls far 15F3:03B3 and raises SI to returned
AX when that word is unsigned greater than SI. It next calls far
15F3:0371, shifts returned AX right four and copies it into DX.
DX greater than 0400 goes to a zero-AX exit without later
quantity/slot/index writes. Otherwise AX is increased by 0040; that word
at most current SI takes the same zero exit.

DI zero is replaced with DX+0040. Nonzero DI greater than
DX+0040 is also replaced; at most that sum is retained. DI
below current SI is then raised to SI. It forms DX as DI-SI
at word width and returns zero without later publication when DX is below
four. Native query effects, return meaning and SI/DI preservation remain
unadmitted. With preserved held registers, these local checks make SI at least
0040 and strictly below the queried sum, at most 0440, and make
the admitted span at least four. They are not bounds on native inputs or
proof of actual available storage.

The helper next reads the quantity through its incoming far pointer. If that
word is unsigned below the span it replaces DX with the quantity; otherwise
it retains the span. DX below four is then raised to four. Reloaded
quantity below DX is set to zero; otherwise DX is subtracted from it.
Quantity zero is not locally rejected and can therefore select four and be
set to zero on this path.

It shifts current SI left six at word width, loads the far index
pointer, reads its word through ES and multiplies it by fourteen at word
width. It writes the shifted SI into current DS's indexed cleanup argument
field, 3F4E installed or 3EC2 on disc. There is no local test
of that transformed word for zero. Each following pair publication reloads the
index through the same ES and incoming offset before multiplying by fourteen:

| Order | Installed indexed field | Disc indexed field | Modeled value |
| --- | --- | --- | --- |
| 1 | 3F44 | 3EB8 | Segment 1425 |
| 2 | 3F42 | 3EB6 | Offset 02E1 |
| 3 | 3F48 | 3EBC | Segment 1425 |
| 4 | 3F46 | 3EBA | Offset 02FE |
| 5 | 3F4C | 3EC0 | Segment 1425 |
| 6 | 3F4A | 3EBE | Offset 031B |

MZ relocation index 3 at 0425:03DC, index 2 at 0425:03F1
and index 38 at 0425:0406 each hold shipped segment 0425,
giving 1425 with modeled load segment 1000. After the stores it
reloads the incoming index offset and increments that word through current ES.
It clears AX, restores DI, SI and BP and returns far without incoming
cleanup. All zero-AX bypasses use that same register cleanup. No local bound
limits the supplied index to sixteen, and stable index/segment and disjoint
storage are conditions for all pairs to land in one intended slot.

For a conditional arithmetic case, incoming SI 0400 and DI zero,
first query AX at most 0400, second query AX 4000, preserved
SI/DI and an admitted quantity four pass the local checks. The queried
end is 0440, the span 0040 and the selected quantity four.
The quantity becomes zero, while SI shifted left six becomes zero at word
width and is published as the cleanup argument. The helper increments the
index and returns zero. FND-EXE-559's consumer skips zero argument words.
This is an instruction-level case, not evidence that the shipped game admits
those inputs or query results at runtime.

The published cleanup target 1425:031B saves BP, pushes its incoming
word at SS:BP+6 and calls far 15F3:036F. MZ relocation
index 6 at 0425:0324 holds shipped segment 05F3. That
callee consists of a no-op followed by a far return without incoming
cleanup. The wrapper removes the forwarded word by popping CX, restores BP
and returns far without incoming cleanup. It makes no native request and
does not inspect or modify the forwarded word. The two other published
targets, 1425:02E1/02FE, remain outside this reading.

The preliminary call segments have MZ relocations too: index 5 at
0425:034A and index 4 at 0425:0357 hold shipped 05F3,
giving modeled 15F3. Their return contracts and native dependencies remain
open; no fixed response, driver model or harmless-return substitute is admitted.

## Interpretation

This supplies the second type-selected producer and its cleanup target, including
the distinct early AX-two rejection and later AX-zero bypass/publication paths.
Q-EXE-007 retains preliminary query contracts, other producer types and
published targets, all caller/writer coverage, aliases and input/segment/storage
admission. No complete slot contract, resource availability or game launch
exclusion is claimed.

## Alternatives

Treating zero AX as publication contradicts the later bypasses. Rejecting a
zero quantity invents a guard before its clamp to four. Treating shifted SI
as always nonzero ignores word-width wrap in the recorded arithmetic case.
Calling the cleanup target a checked release contradicts its far no-op callee.
Treating all published fields as one stable slot ignores repeated index reads
and unresolved aliases. Treating query results as native capacity facts would
go beyond the local arithmetic and unresolved native contracts.

## How to reproduce

At revision c172a376 require both identities from FND-EXE-350. Use MZ
header size 5200, modeled load segment 1000 and relative code segments
0425 and 05F3. Decode the Locations ranges separately in sixteen-bit
mode, ending after each final far return. Track the initial unsigned guards,
both query returns, right shift, end/span and quantity clamps, every zero
exit, word-width left shift, repeated index reads and ordered stores. Check
the conditional SI/DI/query/quantity case above without assuming its runtime
admission. Read header relocation count at 0006 and table offset at
0018 and check indices 2 through 6 and 38 with the operands
above. Use FND-EXE-391 for type selection and outgoing widths, and
FND-EXE-559 for the consuming argument test. No negative caller/writer census
is claimed. Licensed bytes stay outside Git; no game process, DOSBox or
emulated call runs.
