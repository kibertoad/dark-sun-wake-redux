---
id: FND-EXE-026
title: A compiled append loop derives its slot from a signed byte and initializes after iteration
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x006B0C0E..0x006B0C6F
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x006B0D4F..0x006B0D77
tool: Ghidra 12.1.3 PUBLIC, pinned bounded instruction and reference reporters
environment: null
---

## Observation

A direct append call to the routine read in FND-EXE-023 occurs in the
bounded block at `0x006B0C0E`. This is not a recovered whole-function reading:
parent-frame ownership, earlier guards, local producers and exceptional
entry paths remain unread. Stack offsets below are relative to the unchanged
stack pointer within this block, conditional on normal callee restoration.

The block sign-extends the byte at stack offset 1027 to 32 bits, initializes
the index word at offset 48 to zero, subtracts 65 from the sign-extended
value at 32-bit width, and stores that derived slot at offset 808. There
is no slot-range or byte-normalization guard in this bounded sequence.
It does not by itself establish that the byte is an admitted drive letter.

At each loop test it reloads the end word at offset 1140 and base word at
1136. It subtracts base from end at 32-bit width and arithmetically shifts
right by two to compute a count. It compares that full result with the
index word at offset 48, exiting to `0x006B0D4F` when the computed count
is unsigned less than or equal to the index. Thus the shift's signed
interpretation and the following unsigned comparison are distinct; a
negative shifted value is not rejected as a negative count by this guard.
Producer evidence still has to decide reachable differences and alignment.

On the admitted arm it reads the 32-bit word at base plus four times index.
It places the previously stored slot as the first outgoing 32-bit argument
and the fetched word as the second, with an intervening local state-word
write of 131 at stack offset 1072. It calls `0x004A8540`, then increments
the index word at 32-bit width and jumps back to the loop test. No branch
tests an append return value. The callee's direct normal paths and its
growth boundary are in FND-EXE-023 and FND-EXE-024. The earlier base/end
values are not kept as a fixed count: both are reread after each normal
append. No claim that those stack locals cannot alias or change through
unread callees is made.

The exit block rereads and sign-extends the same byte, writes 131 to the
local state word, subtracts 65 again at 32-bit width, and passes this newly
derived slot to `0x004A83F0`. That callee's direct installation and shared
word publication are read in FND-EXE-022. This call follows both the initial
empty-range exit and an exit after any normally completed append iterations.
There is no local predicate requiring at least one append before it, and no
local branch tests its return before the subsequent continuation. This
finding stops at that call's continuation; later effects are not described.

## Interpretation

The append caller provides a direct link between a local word range and the
records used by FND-EXE-022's pointer installation. It neither establishes
object construction nor proves that a record began empty. The zero index,
reloaded count, unsigned exit, append-before-increment ordering and subsequent
initializer are direct observations, conditional on valid readable locals
and normal callees. A valid stable list would admit its words in index order;
record and list invariants, byte admission, aliasing and whole-function
continuations remain open under Q-EXE-009. The analyzer reference list is
not treated as a complete caller census.

## Alternatives

A zero-extended selector byte, a signed count-exit comparison, a fixed count
captured before the loop, incrementing the index before append, or skipping
the initializer solely because no iterations occurred are ruled out by this
bounded sequence. Earlier validation might exclude invalid selector bytes
and malformed ranges; its existence and scope need producer evidence and
are not inferred from the local subtraction by ASCII A.

## How to reproduce

Use FND-EXE-011's verified PE and image base. Query references to
`0x004A8540` using ReportReferences' fixed 200-reference cap as a lead,
without claiming a complete caller set. Read 22 instructions from
`0x006B0C26`, 24 from `0x006B0BF0` and 24 from `0x006B0D4F`. Restrict the
reading to the cited ranges; exclude preceding unrelated branches and the
later continuation. Verify sign extension, all argument widths and last
writers, zero index initialization, arithmetic shift, unsigned guard,
base/end reload on re-entry and append/initializer ordering. Track the
unchanged local stack pointer and FND-EXE-023's normal restoration rather
than assigning these offsets to a guessed parent structure. Cover initial
zero count, a normally admitted iteration and subsequent exit conditional
on producer state; do not infer a reachable negative-count case. Keep rich
reports local and execute no interpreter or game.
