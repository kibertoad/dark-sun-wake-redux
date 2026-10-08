---
id: FND-EXE-177
title: A resident cleanup candidate conditionally exchanges the vector before two unresolved near callbacks
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4AE5:0193..4AE5:01B5
tool: executable-reader 2.4.0, scientific-method-engine 13.5.0 and Ghidra 12.1.3 PUBLIC
environment: null
---

## Observation

The bounded candidate at `4AE5:0193` saves its frame and DS, reads DS through
CS-relative offset five, and tests the word at state offset `0x0128` against
zero. A zero word bypasses all three calls. On the other arm it first calls
`4AE5:0140`, the vector-exchange procedure in FND-EXE-176. It then calls
through the words at state offsets `0x0084` and `0x0082`, in that order.
Each of the three near calls is immediately preceded by a saved CS word.
Their actual targets, return behavior and state preservation remain separate
obligations. The two computed call targets are unresolved in this reading.

Both local arms converge on restoration of DS and BP and the far return at
`4AE5:01B4`, with no additional argument cleanup. The bounded CFG covers
34 contiguous instruction bytes, `4AE5:0193..4AE5:01B5`, assuming each call
returns. It reports the direct call site `4AE5:01A5` and the computed sites
`4AE5:01A9` and `4AE5:01AE`; it does not establish effects of the callbacks.

## Interpretation

This supplies another conditional consumer of the vector-exchange procedure,
and a concrete cleanup path to include in Q-EXE-001 and Q-EXE-010. The gate
is the state word's value, not evidence of what that word means. FND-EXE-176's
replacement of the stored pointer with the previous vector must be read in
execution order relative to this consumer. Native callers, CS admission,
state-word writers and both callback targets remain unresolved. This is not
a complete reading or a claim that cleanup restores a particular live vector.

## Alternatives

An unconditional vector exchange is ruled out by the zero-state bypass.
A completed cleanup after the vector call is not established: either callback
may change state or fail to return. An empty analyzer reference result cannot
rule out pointer, computed, unrelocated or undisassembled callers.

## How to reproduce

Use the installed original with XXH3-128
`e296af55ba2ecde7e77f555c90f33d0b`, sourceKind `mz`, and default load segment
`0x1000`. Run the committed wrapper's `x86-bounds` command with entry file
offset `0x000401E3` (262627) and one resident region: start `0x00040050`
(262224), exclusive end `0x00041050` (266320), segment `0x4AE5` (19173),
ip zero, entries `[262627]`. Supply no callback summaries or interrupt models.
The bounds result keeps both computed targets unresolved and explicitly
assumes call continuation; its complete flag is CFG coverage only.

In the saved original resident project, with analysis disabled and read-only
mode, run ReportInstructionWindow at `4AE5:0193`, count 20. Restrict this
observation to the first 16 instructions ending at `4AE5:01B4`; the window
then crosses an undisassembled gap and prints unrelated handler instructions.
Do not join those into the cleanup body. Rich reports and configurations stay
in the licensed local store, outside Git.
