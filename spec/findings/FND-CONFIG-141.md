---
id: FND-CONFIG-141
title: The expression seed wrapper searches active slots through the same selector lookup
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2D40:0AB6
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2D40:0D94
tool: Python 3.14.7 and Capstone 5.0.7 bounded 16-bit disassembly; declared MZ relocation and callee-graph inspection
environment: null
---

## Observation

FND-CONFIG-139's below-32768 seed branch calls resident
2D40:0AB6. The complete wrapper occupies
`0x000230B6..0x000230E5`, ending before the adjacent
routine. It initializes a local cursor word to zero and its
retained result to 9999, then calls local 2D40:0D94 with
a far pointer to that cursor and its input word. A result other
than 9999 replaces the retained result; it returns that word.
There is no other callee or explicit nonlocal data write.

Helper 0D94, `0x00023394..0x0002341D`, retains the
supplied search word and initializes its result to 9999.
A nonnegative search word is returned directly and the supplied
cursor is assigned 320. Negative search words take the scan
branch from the supplied cursor. The additional sentinel and
upper comparisons before that scan do not admit another
ordinary nonnegative input from the entry's earlier signed gate.
This reading does not assign another public entry to those
internal branches.

The scan continues while its index is signed less than 320.
A zero byte in the three-byte table at 4F49:0C33 skips
the slot. For a nonzero byte, it calls 1AA0:0009 with the
index and selector zero, then compares returned AX with the
search word. A match retains the index and assigns index plus
one to the supplied cursor; no match returns 9999 and assigns
320 to the cursor. No lower-bound check is present for a
caller-supplied cursor, although the 0AB6 wrapper supplies zero.

The table load operand at `0x000233D6` is a declared MZ
relocation mapping raw 3F49 to 4F49. The lookup operand
at `0x000233E8` maps raw 0AA0 to 1AA0. Both bodies
have no opcode-dispatch or flag-clear call. Their shared lookup
callee is read in FND-CONFIG-140; its verified children do
not call back into this search. This is converging call flow,
not recursive search.

The metadata byte for selector zero at DS:60ED is zero in
the shipped resident load image, file offset `0x000531ED`.
If unchanged, that lookup skips its metadata-at-least-ten table
writer. This is an initial data observation, not a runtime
invariant or proof that the table is immutable.

## Interpretation

The seed path has a concrete bounded scan for the wrapper's
zero cursor and negative search word. It can repeatedly invoke
the same lookup used by the later selector chain, so its global
lookup outputs also matter before the expression root's pointer
check. The wrapper does not independently establish flag or
table preservation; FND-CONFIG-140's metadata and slot-range
conditions apply to each lookup.

## Alternatives

FND-CONFIG-142 records the initial slot word, guarded setup
assignment and qualified literal-writer inventory. It does not
establish a complete slot range or immutable metadata.
Q-CONFIG-008 retains selector-zero metadata writers, active-slot
producers, lookup input validity, returned-value provenance and
reachable expression inputs. A reading that this seed helper
recurses through the chained wrapper is ruled out by the
complete callee bodies. A reading that shipped-image metadata
alone proves every later scan read-only remains unproven.
Arbitrary caller cursors and malformed pointers are outside the
wrapper's zero-cursor scan claim; no gameplay meaning is
assigned to the returned index or search word.

## How to reproduce

Read the complete wrapper and search helper at the stated bounds.
Separate the signed nonnegative gate from the negative scan,
track its zero-cursor caller, active-byte skip, selector-zero
lookup, AX comparison and both cursor results. Verify both
named MZ operands before applying load segment 1000.
Read the single initial DS:60ED byte using DS segment 57E0
from FND-SCRIPT-005. Compare FND-CONFIG-139 and
FND-CONFIG-140 without treating shared callees as recursion
or initial file values as established live invariants.
