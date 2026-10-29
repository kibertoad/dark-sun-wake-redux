---
id: FND-COMBAT-088
title: Intact zero-image APFM setup writes placement and a parent link while preserving the callback region
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 3F96:000B..3F96:0068
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 3CFA:0006..3CFA:00DA
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 3A8E:0514..3A8E:0537
tool: tools/research/exec-census/resident_listing.py at 93609f94; Capstone 5.0.9
environment: null
---

## Observation

FND-PARTY-044 and FND-CONFIG-031 locate the APFM child-registration
branch of resident 3A8E:02B3. At 0514 it pushes the child entry's
placement words +0E and +0C, the locally retained window far pointer,
and the loaded child far pointer, then calls 3F96:000B. It compares the
full returned AX with FFFF; equality returns registration failure, while
other values continue the child loop. This is the APFM branch, not the
adjacent BUTN setup that writes differently overlapping offsets.

3F96:000B reads its placement words at BP+0E and BP+10 into DX and CX
before its stack-limit guard. With that guard bypassed, call these words
x and y. Its own writes through the child far pointer are:

| Child offset | Width | Assigned value |
| --- | --- | --- |
| 54 | 4 | Far argument at BP+0A, the window pointer in the named parent branch |
| 10 | 2 | x |
| 12 | 2 | y |
| 14 | 2 | x + child word 28 - 1, in word arithmetic |
| 16 | 2 | y + child word 2A - 1, in word arithmetic |

It then passes the child far pointer advanced by 0C to 3CFA:0006.
Nonzero returned AX becomes FFFF; zero becomes zero. There is no own
store at child offsets 58 through 73. The supplied child pointer is not
tested for null in this helper; the parent tests its acquired child pointer
before tag dispatch (FND-PARTY-044).

3CFA:0006 has its own stack-limit guard. It tests a double word at
argument-relative 20, then one at 28. For this APFM call these are child
offsets 2C and 34. Each nonzero field requests BMP acquisition through
38FF:0438, stores the returned pointer at argument-relative 24 or 2C
(child offsets 30 or 38), and may call 39D1:0947 on that result. Missing
images or rejected results reach FFFF, including diagnostic calls on
missing pointers. These nonzero-image children are not characterized here.

When both image-number fields are zero and both stack-limit guards are
bypassed, there are no other calls on this setup path. 3CFA:0006 reaches
AX zero without any own memory store; 3F96:000B therefore returns zero.
FND-UI-005 records that all 97 shipped APFM resources have zero bytes
at both image fields and throughout 5A through 73. For intact resources,
valid disjoint frame/stack storage, no address wrap and the stated guard
bypasses, this branch preserves those callback bytes and the mask at 58.
Placement arithmetic may wrap as a word; that is distinct from address
wrap. No native guard, allocation, alias or loaded-resource identity is
assumed from the shipped bytes alone.

## Interpretation

The named APFM constructor branch does not manufacture a nonzero +5A
or +5E callback from an intact shipped frame. Its concrete mapped effects
are the parent link and placement endpoints; its zero-image path has no
resource or presentation child after the guards. This removes that local
construction branch from the unresolved callback-byte writers, under the
explicit storage and input conditions. It does not prove preservation
through all other children of window registration or through later use.

## Alternatives

FND-CONFIG-032 previously bounds the image-free case for the message
window's APFM/11270. This reading binds the shared helper's exact APFM
offsets and complete local write footprint, using FND-UI-005's all-resource
zero-field observation. It does not replace that finding or extend its
native acquisition claims.

Q-COMBAT-049 retains actual resource acquisition, the hit/lookup record
origins, other construction/copy/write routes, alias/storage admission,
guard outcomes, later modes and callback-target identity. A modified or
unrelated record could enter the nonzero-image branch; the zero-resource
case cannot settle that input. FND-EXE-588's broader census limitations
remain. No whole-program no-writer or whole-load health claim follows.

## How to reproduce

Use resident_listing.py on the licensed installed DSUN.EXE for
3F96:000B..3F96:0068, 3CFA:0006..3CFA:00DA and
3A8E:0514..3A8E:0537. Bind the four constructor arguments from the
parent's pushes, distinguish child-relative offsets from helper-relative
offsets after the 0C advance, and enumerate every own memory store.
Trace both zero-field jumps through the helper's zero return. Compare
FND-UI-005's shipped fields instead of assuming runtime zeros from size
or tag alone. Keep the guard, address/storage and resource-origin
conditions separate from the local byte-preservation result.
