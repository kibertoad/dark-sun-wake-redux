---
id: FND-EXE-244
title: Loader-entry candidate initializes segment bounds from SS-relative inputs before a wrapped difference check
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4AE5:00F6..4AE5:0126
tool: executable-reader 2.5.0 and Capstone 5.0.7
environment: null
---

## Observation

FND-EXE-175's entry candidate forms BP from SP after saving incoming BP,
reserves twenty local bytes and loads DS from CS-relative word five. The
BP-based accesses below default to SS, not the state segment loaded into DS.
Native incoming frame contents and preservation through earlier callees remain
unread.

After its call to FND-EXE-176's vector procedure, the entry reads SS-relative
word BP plus `0x000C` into AX and increments it at sixteen-bit width. It stores
that AX into current DS-relative word `0x0124` at `4AE5:00FA` and word
`0x0120` at `4AE5:00FD`. It then reads SS-relative word BP plus `0x000A`
into BX and stores BX into current DS-relative word `0x0126` at
`4AE5:0103`. These are direct initialization writers of the bounds consumed
by FND-EXE-242 and the state word used by FND-EXE-228. An input word 65535
would increment to zero; that is an arithmetic consequence, not an admitted
native input.

It calls `4AE5:029B` before checking the resulting range. After the call
it rereads current DS-relative word `0x0126`, subtracts word `0x0124` at
sixteen-bit width, and compares the retained BX against word `0x011A`
using an unsigned at-least branch. A smaller BX sets CX to `0xFFFB` and
branches to the earlier error path at `4AE5:0050`. The other arm shifts
BX right twice and stores it into current DS-relative word `0x0118`.

The subtraction's borrow is not tested: the following comparison determines
the branch. There is no explicit upper-not-below-lower check in this sequence.
A wrapped difference can therefore satisfy the comparison for some possible
word values, unless actual input/state admission excludes those cases.
The computed difference and later store come from post-call reloads, not
necessarily the values initialized before `4AE5:029B`.

The cited sequence has sixteen instructions and 48 bytes under source
decoding. The wider entry traversal and its calls, interrupts and far-return
frame limitations remain as recorded in FND-EXE-175. These stores do not
prove initialization is reached before any particular header validation.

## Interpretation

This supplies missing direct writers for the lower bound, upper bound and
current segment, including their SS-relative input origin and exact widths.
It distinguishes pre-call initialization from the post-call difference check
and records that the check does not independently prove bound ordering.

Q-EXE-001 and Q-EXE-010 retain native entry and SS-frame admission, each input
word's last writers, preservation of BP/DS through earlier calls and the vector
procedure, the effects of `4AE5:029B`, other field writers, error-path effects,
invocation order, aliases and interrupt-enabled changes. No complete_reading
or replacement inventory is established.

## Alternatives

DS-relative input arguments are contradicted by the BP accesses' SS default.
Unbounded increment or difference arithmetic ignores their sixteen-bit widths.
An ordering check inferred from the range-size comparison ignores the discarded
subtraction borrow. Treating the checked words as immutable initialization
values ignores the intervening call and explicit reloads.

## How to reproduce

At revision `a232590`, repeat FND-EXE-175's original-source entry traversal
and FND-EXE-236's source hash guarding. Decode only file interval
`0x00040146..0x00040176`, initial IP `0x00F6`, with Capstone 5.0.7 in
x86 sixteen-bit mode. Inspect the sixteen instructions and all widths,
segment defaults, stores, returning-call dependency and branches above.
Use the wider entry interval `0x00040060..0x00040190`, initial IP `0x0010`,
only for its frame formation and the preceding vector-call order; retain
FND-EXE-175's one-byte-hole and native-entry limitations. Read FND-EXE-176
separately rather than assuming its call preserves state. Sources,
configurations and listings remain in GAME_DIR.
