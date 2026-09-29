---
id: FND-CONFIG-135
title: The iterator flag is written by a registered resident helper and an overlay 188 clear path
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2D40:37F3
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 5702:00B1
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 5702:00D4
tool: Python 3.14.7 and Capstone 5.0.7 bounded 16-bit disassembly; declared MZ relocation and FBOV fixup mapping
environment: null
---

## Observation

Resident 2D40:37F3 begins at file offset `0x00025DF3` and
returns at `0x00025E2E`. It loads its word argument into SI
and stores it at DS:55C4. When SI equals 49 and byte DS:143D
is nonzero, it calls overlay entry 576C:0039 before continuing.
That call's effects, including its handling of SI, remain unread.

The continuation compares SI with 51. Equality writes byte one
to 4F49:000A; inequality writes zero. MZ relocation operands
at `0x00025E15` and `0x00025E22` hold raw segment 3F49,
mapped to 4F49 with resident load segment 1000. An argument
of 51 reaches the one assignment without taking the earlier
call branch. Arguments other than 49 or 51 reach the zero
assignment without taking that call branch. For input 49 with nonzero DS:143D, the unread callee's effects
precede the flag assignment.

Overlay 188's setup entry in FND-CONFIG-144 stores a far pointer
to this helper in DS:02F6 and DS:02F8 at
`0x00073BBA..0x00073BC6`, on the DS:193E zero branch.
The offset is 37F3; the segment operand at `0x00073BBE`
is a declared FBOV fixup holding raw 00C8, descriptor 25,
mapped segment 2D40. This proves a pointer assignment, not
that a dispatcher invokes it or when that happens.

Overlay 188 entry 5702:00B1 maps to code offset 1901,
file offset `0x000747A1`, returning at `0x000747CD`.
It calls two helpers around an optional message branch gated by
DS:143C, then clears byte 4F49:000A at `0x000747C6`.
The clear lies on the common returning path after those calls.
Its segment operand at `0x000747C2` is a declared FBOV fixup:
raw 0388 names descriptor 113, mapped segment 4F49.
This reading does not establish the calls' return or state effects.

The flag's byte in the resident shipped load image is zero,
at file offset `0x0004469A`. This is initial file data,
not an observation of its value when rest processing begins.
FND-CONFIG-133 directly reads this flag before returning the
stored index. No exhaustive flag-writer inventory is claimed here.

## Interpretation

The iterator flag has a registered local producer that distinguishes
51 from other retained SI values, and a separate clear path.
It is not established as permanently zero or permanently nonzero.
The registration and these local assignments do not establish
which player input supplies the helper argument, nor their ordering
relative to rest processing or table updates.

## Alternatives

FND-SCRIPT-023 subsequently reads the error entry's stop assignment,
and FND-CONFIG-161 bounds 576C:0039's local body. External effects,
DS/SI preservation and return outcomes remain open in Q-CONFIG-008.

FND-CONFIG-136 reads a script-opcode dispatch consumer and its
argument producer. Q-CONFIG-008 retains other callers, pointer
replacement and invocation timing, the
input-49 callee effects, other flag writers and the clear entry's
incoming routes. On the registered script route, 49 and 51 are
opcodes 0x31 and 0x33 (FND-CONFIG-136); other routes remain unread. In particular, an argument-only formula covering the
input-49 call path would require proving the callee's handling
of SI; the branch comparison alone does not provide that proof.

## How to reproduce

Read the complete bounded resident helper
`0x00025DF3..0x00025E2F`; verify the two listed MZ operands
and relocate the earlier call segment to 576C. Read the setup
pointer stores from FND-CONFIG-144's entry and verify their
declared fixup. Resolve overlay 188 trampoline 00B1 to code
1901 and read `0x000747A1..0x000747CE`, tracking the
optional message branch, later call and common clear. Verify
the clear's fixup before labelling its segment. Read the single
initial byte using MZ header size 5200 and mapped segment 4F49;
keep initial file data distinct from reachable runtime state.
