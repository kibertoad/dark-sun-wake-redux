---
id: FND-EXE-021
title: Compiled filename components shorten names and retain output mutations on failure
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x004B51B6..0x004B53EC
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x004B5140..0x004B515C
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x004B5400..0x004B547B
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x004B54AD..0x004B54CE
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x004B5507..0x004B5515
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x00601D40..0x00601D46
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    kind: file-data
    offset: 0x00322BA0..0x00322BA2
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    kind: file-data
    offset: 0x003595E6..0x003595EE
tool: Ghidra 12.1.3 PUBLIC and independent physical PE import/header reading
environment: null
---

## Observation

This reads component handling after FND-EXE-020's normalized local input and
output-prefix setup. The output name is the caller buffer retained at frame
offset 44; the retained separator index is at 40. A component buffer starts
at 128, the input index starts at zero and the terminal flag at 39 starts
clear. Backslash and NUL end a component. NUL also sets the terminal flag.
Other bytes are copied to the component buffer one at a time. At a boundary,
the component is NUL-terminated before its branch is selected.

An empty component resets its counter and advances past that boundary.
A nonempty component equal to dot followed by NUL does the same after
clearing the component's first byte. The two-byte literal comparison includes
NUL; its physical literal is at the file-data location above. If the terminal
flag is set after either path, the helper assigns 32-bit one and enters the
shared frame-restoration return described in FND-EXE-017. Otherwise it reads
the next normalized byte. Consecutive separators and a trailing separator
therefore do not create a nonempty component to append on these paths.

Other nonempty components are measured by a local four-byte NUL scan, with
byte selection in its final word. A second loop checks every component byte
for dot. An all-dot component of length greater than one selects backward
output scanning, with a removal counter equal to its length minus one.
That scan begins at output strlen minus one, decrements its signed index,
and counts a backslash or index zero as a boundary. Each counted boundary
updates the retained separator index; exhaustion of the removal counter or
a negative index ends the scan. It writes NUL at the retained index, resets
that index to zero and recomputes the last backslash with a nonzero following
byte by scanning the shortened output. It then clears the component and
continues. This is the direct procedure, including what happens when the
requested removal count outlasts the output; no filesystem parent operation
is called in this component branch.

A component containing a non-dot byte takes the ordinary append path:

- The helper measures the current output, retains that length as the
  separator index and, if it is nonzero, writes backslash then NUL at its
  end. It does this before testing the component's dots or append length.
- It searches for the first dot through the strchr thunk identified in
  FND-EXE-015. With no dot, it writes NUL at component index 8, retaining
  at most its first eight bytes as a visible string.
- With one dot, it searches again starting one byte after that dot. A second
  dot fails: a terminal component takes the value-2 failure path, and a
  nonterminal component takes value 3. Both paths use the shared word setter
  and return zero in FND-EXE-017. The earlier output-separator write remains.
- With only one dot, it writes NUL four bytes after the dot, limiting the
  visible extension to three bytes when it was longer. It measures the
  component and the substring beginning at that dot and subtracts those
  lengths to obtain the base length. A base length greater than 8 selects
  an imported memmove of exactly five bytes, from the dot's address to
  component index 8. This copies all five bytes, including bytes after an
  earlier NUL when the extension is short; only the resulting visible
  string is used by the subsequent length and append operations. A base
  length at most 8 skips that move.
- It measures the current output again and the transformed component,
  adds the two lengths at 32-bit width and compares that sum unsigned with
  79. A larger sum takes value-3 failure after the earlier output mutations.
  An admitted sum calls the strcat thunk identified in FND-EXE-015, clears
  the component and continues or returns one at a terminal boundary.

The five-byte move uses the thunk at `0x00601D40`, which jumps through
`0x024319E0`. Ghidra's exact slot reference and an independent physical
import-descriptor reading identify it as `msvcrt.dll::memmove`. The imported
name is at the second file-data location above. The other string imports
are recorded in FND-EXE-015. Statements about their ordinary string effects
are conditional on valid, nonoverlapping caller storage where required and
normal CRT behavior; this is not a reading of the loaded CRT implementation.

## Interpretation

Failure can leave the output name with an added separator, prior components
already appended or a prior truncation. The helper has no local rollback
path for those writes. Its append limit applies after prefix initialization
and separator insertion and uses a 32-bit sum; it does not establish the
prefix's maximum length, rule out sum wrap for arbitrary unread inputs, or
prove a transactionally unchanged output on failure. The filename helper's
component procedure is now directly described, but its prefix producer,
caller storage/alias contracts and subsequent drive-object targets remain
open in Q-EXE-009.

## Alternatives

Preserving every long base and extension, accepting multiple dots in an
ordinary component, treating only exactly two dots as parent handling, and
checking the append length before adding its separator are ruled out by the
local branches. A generic host path-normalization function is not equivalent
evidence for the all-dot procedure or its retained index. No claim is made
that every successful return identifies an existing file or that these
branches establish the complete wrapper continuation.

## How to reproduce

Use FND-EXE-011's verified PE and image base and FND-EXE-020's admitted local
input. Read 160 instructions from `0x004B51B6`, 40 from `0x004B5400`, four
from `0x004B5140` and the retained 130-instruction tail window from
`0x004B53A8`; interpret only the cited component intervals. Follow the
terminal flag, component reset and shared return. Check the local NUL scan's
four-byte stepping and final byte selection before using its length, then
trace every all-dot and ordinary-component branch. Keep the retained
separator index separate from the backward scan index and removal counter.

Verify the two bytes at `0x007259A0` and their physical mapping. Read the
move thunk's exact instruction and query memmove symbol references; match
its slot independently against the physical PE import directory. Trace the
move's source, destination and full five-byte count, including short
extensions. Compare terminal and nonterminal multiple-dot failures and the
32-bit append sum's two outcomes. Check static paths for empty, dot, all-dot,
no-dot, one-dot short/long bases, short/long extensions and multiple-dot
components, conditional on valid prefix and normal imports. Retain rich
reports locally; run no interpreter or game.
