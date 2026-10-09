---
id: FND-EXE-371
title: Sound utility buffer quantity helper separates exact-fit and split segment returns
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1000:1BD6..1000:1C5D
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1000:1BB3..1000:1BD6
tool: Capstone 5.0.7 and xxhash 4.0.1
environment: null
---

## Observation

FND-EXE-357 passes current SI to far-returning entry 1BD6, removes
its argument word and tests the returned pair. This entry sets DX zero and
loads AX from SS:BP+06. Alternate entry 1BE0 loads both AX and DX
from incoming words. Both join a common path that saves SI/DI and writes
incoming DS to shared CS:1992 before testing the initial quantity for zero.
An all-zero quantity selects the suffix with AX/DX zero, but still performs
that shared saved-DS store.

For nonzero input it adds 0013 to the AX/DX pair with carry. A carry
out of DX or any high-word bit in FFF0 selects a zero-pair return. Otherwise
it shifts AX right by four, shifts DX left by four and combines the resulting
low byte into AH. The resulting AX is the accepted sum divided by sixteen.
For a nonzero input N accepted by this local gate, N is at most 000FFFEC
and the encoded unit count is floor((N + 19) / 16), from one through FFFF.
The selected caller's initial quantity 0200 yields unit count 0021.
This arithmetic does not establish the downstream allocation unit or capacity.

Current CS:198C zero calls near 1AF5. Otherwise CS:1990 zero calls
near 1B59. For nonzero CS:1990 it keeps that starting segment in BX
and loads DS from the current candidate DX. It compares candidate word
zero unsigned with the unit count. A smaller value loads the next segment
from current DS:0006 and compares it with the retained starting segment.
Inequality repeats with that next segment; equality calls 1B59. There is
no independent iteration count, pointer-extent check or rejection of a cycle
that does not return to the starting segment.

A candidate word zero equal to the requested units calls 1A6C, whose
local body FND-EXE-367 reads. On ordinary return it reads current DS:0008
and writes that word to DS:0002, sets AX=0004 and keeps the selected
segment in DX. A larger candidate selects near helper 1BB3 instead.
The exact-fit comparison flags are used again to choose that larger branch;
there is no intervening result-producing call before this choice.

The split helper keeps the selected segment in BX and subtracts requested
AX from its word zero. It adds that remaining word to DX at word width
and loads DS from the resulting segment. It writes requested AX to this
segment's word zero and the selected segment to word two. It computes
another segment from DX plus the current word zero, loads DS from it and
writes DX to that segment's word two. It sets AX=0004 and near-returns.
It does not change DX after selecting the split segment, perform native
requests or test any result. Both segment sums wrap and admit no extent.

Thus, on ordinary noninterfering continuation, the exact-fit path returns
selected-segment:0004 and the split path returns split-segment:0004.
The split field updates precede its returned pair. Its current DS on return
is the later computed segment, not necessarily the returned DX segment.
The common outer suffix reloads DS from CS:1992, restores DI/SI/BP
and far-returns without incoming argument cleanup. The 1AF5 and 1B59
growth-helper contracts are not established by this finding.

## Interpretation

This resolves the local quantity conversion, candidate search and two fitted
returns reached by FND-EXE-357. The requested quantity, rounded units,
selected segment, remaining field and returned offset are distinct values.
A returned offset four does not by itself establish header semantics or
available bytes for the caller's later writes.

The search's start-segment equality is a local stopping condition, not a
validated topology or a total iteration bound. Shared DS restoration also
requires CS:1992 writers and re-entry admission. The local zero-pair result
is not a general unchanged-state result, since its saved-DS publication occurs
before both zero-input and arithmetic-rejection decisions.

Q-EXE-007 retains 1AF5/1B59 and their lower helpers, encoded-unit and
extent admission, candidate/link and shared CS-state writers, aliases, lifetime
and re-entry. No complete-reading promotion, successful native allocation
or launch exclusion follows.

## Alternatives

Treating all fitting entries as the same return path ignores the exact/split
branch and different DX segment source. Treating the rounded unit count
as verified writable capacity assumes the remaining helper contracts. Treating
start equality as a bound on any linked structure ignores other cycles.
Treating zero return as no publication ignores the prior CS:1992 store.

## How to reproduce

At revision fd970fa require FND-EXE-350's SOUND_DS.EXE identity:
length 204593, XXH3-128 236c2dc23c071eca421eb5b427caee57.
With locked Capstone 5.0.7 in sixteen-bit mode decode shipped half-open
0x00002FD6..0x0000305D at IP 1BD6 and
0x00002FB3..0x00002FD6 at IP 1BB3, modeled CS 1000,
MZ header size 1400. Bind FND-EXE-357's argument, track the initial
zero test and saved-DS store, double-word addition and bit gate, unit packing,
retained start segment, reused comparison flags and ordered split stores.
The extension at IP 1C41 is the sixteen-bit instruction despite Capstone's
wider mnemonic. Keep growth helpers, topology and returned capacity open.
Original bytes and reports remain outside Git. No original process,
DOSBox, interrupt thunk or emulated call is executed.
