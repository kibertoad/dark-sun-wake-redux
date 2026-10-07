---
id: FND-EXE-128
title: Full-width nonzero-mode lookup retries retained entry offsets through a fresh backing pointer
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x0068987D..0x00689909
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x006899F0..0x00689A33
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x00689C3A..0x00689C41
tool: Ghidra 12.1.3 PUBLIC bounded full-width lookup and missing-entry reading
environment: null
---

## Observation

FND-EXE-126 grounds full-width target `0x00689860`; FND-EXE-127
records its retained full address A, page index and mode branch. Byte
`0x01B7BB14` nonzero reaches the lookup described here. It reads
full `0x0075B6C8`, shifts it left twelve and adds four times A
logically shifted right twenty-two, all at thirty-two-bit width, to form
first offset P. It separately retains I as (A logically shifted right
twelve) AND 1023 in local current ESP plus `0x10`.

It reads backing pointer B from full `0x01D4A380`, loads full W0
at B plus P and stores it at current ESP plus `0x18`. W0's low
byte mask one gates progression. A present bit reaches the second-level
lookup. An absent bit takes the first missing-entry path, which reads full
`0x0075B144` and tests its intersection with current full
`0x0075B140`. Zero intersection selects full third input zero;
nonzero selects full four. It calls `0x00417CF0` at
`0x00689A0F` with retained A, retained P and that selected third
input in its outgoing slots.

After an ordinary first helper return, it freshly reads full
`0x01D4A380`, reloads W0 at that fresh base plus retained P,
updates local `0x18`, and tests mask one again. Present rejoins the
second-level lookup; absent supplies pointer `0x0071F414` to
`0x0058F890` at `0x00689A33`. This reading stops that failure
path at the call, without assigning a return/exception contract. The
retry does not recompute P from a fresh `0x0075B6C8` or fresh A,
and no helper EAX is tested to decide whether the retry succeeded.

The second-level lookup forms Q as (retained W0 AND `0xFFFFF000`)
plus four times retained I. It reads full W1 at the currently retained
backing base plus Q and stores it through a retained pointer to current
ESP plus `0x14`. W1's low byte mask one present proceeds to
`0x00689909`, whose later permission/publication branches remain
unresolved here. If absent, it separately reads current full
`0x0075B144` and `0x0075B140` and selects third input zero or
four from their intersection, as above. It calls `0x00417CF0`
at `0x006898EF` with retained A, Q and that selected input.

After an ordinary second helper return, it freshly reads full
`0x01D4A380`, adds it to retained Q, reloads full W1 there and
updates local `0x14`. It tests mask one again. Present proceeds to
`0x00689909`; absent supplies pointer `0x0071F433` to
`0x0058F890` at `0x00689C41`. This bounded reading stops at
that call; the following instruction gap does not prove nonreturn.
The second retry reloads neither W0 nor the root word used to form P,
and does not recompute Q from a new first-level word. A helper may change
backing storage or globals beyond what this caller reloads; those effects
and preserved-register/local-frame conditions remain unresolved.

Each local missing-entry route therefore makes one helper call followed
by one reload and bit test before either progressing or handing off to
the shared transfer. It does not locally poll until the entry is repaired,
check a helper failure return, or replace the saved entry offset with a
returned value. Both first-level and second-level accesses precede any
claimed backing-storage validation; the bit gates test loaded values,
not pointer validity. I and A shifted right twenty-two are each at most
1023, but that bounds only two index components. The shifted root word,
masked W0 base, wrapping sums, backing extent, aliasing and lifetime still
need their producers and callee contracts.

## Interpretation

The nonzero-mode full-width reader's two-level entry admission is now
recorded, including which values survive each helper call and which base
is reloaded. A fresh backing pointer does not imply a fresh first-level
selection on the second retry. Q-EXE-009 in FMT-EXE-006 remains open for
helper effects, shared-transfer completion, backing/root/entry producers,
permission and publication continuations, cleanup targets, caller admission
and the remaining selected-callee branches. This is not a complete mapping
model or an established description of actual PATH behavior.

## Alternatives

- The helper's EAX does not decide retry admission; the reloaded entry's
  low mask-one bit does.
- Retrying through a fresh backing pointer does not recompute the saved
  offset. The second-level retry also retains the earlier W0 selection.
- The third helper input is selected freshly at each missing-entry gate,
  so the first and second calls need not see the same global intersection.
- These are single local retries, not proof of repair, complete failure
  handling, valid backing storage or normally returning transfer calls.

## How to reproduce

Verify FND-EXE-011's executable identity, FND-EXE-099's six physical
controls and FND-EXE-126's full-width target slot. Use the saved Ghidra
program with -noanalysis and ReportInstructionWindow.java at
`0x00689860` limit 65, restricting the lookup through
`0x00689909`. Read `0x00689991` limit 60 only for its separate
missing-first-entry branch `0x006899F0` through `0x00689A33`;
FND-EXE-127 already records the zero-mode branch. Follow the second-level
failure edge with a window at `0x00689C3A` limit eighteen, restricting
claims to the first two instructions and excluding the following word
reader. Track component widths, wrapping arithmetic, local offsets,
backing reloads, both outgoing argument last writers, each fresh global
intersection, retained W0/Q and the mask-one retests. Stop shared transfer
paths at their calls and keep all helper/register/alias conditions explicit.
No native or emulated execution is part of this observation.
