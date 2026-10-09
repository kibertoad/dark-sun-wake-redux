---
id: FND-EXE-557
title: Game cleanup registration rejects only count 32 and stores the segment before the offset
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:030B..1000:0338
  - build: BLD-GOG-EN-1.1
    file: CD:DSUN.EXE
    address: 1000:030B..1000:0338
tool: Capstone 5.0.7, Python struct and xxhash 4.0.1
environment: null
---

## Observation

The resident far routine 030B saves BP and sets its frame from SP.
It compares current DS count word 3572 installed or 34E6 on disc
with 0020. Equality returns AX one without the later table/count stores.
Every other count takes the storing path; there is no unsigned greater-than
or signed range test at this guard.

The storing path reloads the current count into BX, sets CL two and
shifts BX left two at word width. It loads SS:BP+8 into AX
and SS:BP+6 into DX. The four incoming bytes are read before
either target-word store. It first writes AX through DS at offset
BX+A448 installed or BX+A392 on disc, then writes DX through
DS at BX+A446 installed or BX+A390 on disc, with word-width
offset arithmetic. Thus the incoming high word is stored before the low
word, and FND-EXE-555's far-call consumer interprets them as segment and
offset respectively. There is no local null-pointer or target check and no
relocation of the incoming segment word.

After both stores it increments the current count word in memory, clears
AX to zero, restores BP and returns far without incoming cleanup. The
increment rereads that memory word; it does not use the count held in BX.
The helper makes no calls or native requests and does not locally change
DS, SS or ES. It does not save BX, DX or CX. The rejected
path does not assign DX, while the storing path leaves DX as the
incoming low word. These observations describe the helper, not any caller's
result consumption or argument cleanup.

With an intact frame and admitted DS storage, stable count zero selects
the first pair, count 31 selects offsets A4C2/A4C4 installed or
A40C/A40E on disc, and count 32 rejects. Stable count 33
selects A4CA/A4CC installed or A414/A416 on disc instead of
rejecting. Counts 4000 and 8000 both shift to zero at word
width and select the first pair. These arithmetic paths are not evidence
that such counts occur during the game.

The accepted arithmetic can also name the count word itself. In the
installed edition, count 244B shifts to 912C: the segment store reaches
DS:3574 and the offset store reaches DS:3572, the count word.
The subsequent increment therefore uses the incoming offset word just stored,
not the original count 244B. On disc, count 2455 shifts to 9154:
the segment store reaches DS:34E6, the count word, and the offset
store reaches DS:34E4. The subsequent increment uses the incoming segment
word just stored. Both then return AX zero. This is a local alias
consequence under admitted storage/frame binding, not a claim that runtime
initializers or callers admit those counts.

FND-EXE-555 records the shipped count zero and the cleanup consumer's
decrement before far dispatch. The registration helper adds a positive writer
of both target words and the count. It does not establish every producer,
caller, storage extent or reachable target. In particular, zero initialization
plus this equality guard alone is not proof of an invariant across unknown
writers, aliases and callback effects.

## Interpretation

This resolves the local registration path feeding general cleanup, including
its exact rejection condition, paired-word publication order and count reload
after the stores. Q-EXE-007 retains caller/input and return-consumption
coverage, all count/table writers and aliases, actual segments, extents and
callback lifetimes, and the other stored general cleanup targets. No complete
registration contract or game launch exclusion is claimed.

## Alternatives

A general count-at-least-32 rejection contradicts the equality branch. Treating
the count increment as always original-count-plus-one ignores the table/count
aliases. Publishing offset before segment contradicts the store order. Treating
the rejected path as returning a null pointer ignores its unassigned DX.
Treating an incoming segment zero as automatically relocated ignores the absent
adjustment. Treating these static arithmetic cases as observed game states would
go beyond the evidence.

## How to reproduce

At revision 3b141a26 require the installed and disc identities from
FND-EXE-350. With MZ header size 5200 and modeled load segment
1000, decode shipped file offsets 550B..5538 in sixteen-bit mode for
both files. Trace the equality branch, fresh count load, two incoming word
loads, ordered stores, fresh memory increment and far return. Check counts
0000, 001F, 0020, 0021, 4000, 8000 and the alias
counts 244B installed and 2455 on disc using sixteen-bit shift/add
arithmetic. Use FND-EXE-555 for the shipped count and consuming far-call
path. No negative writer/caller census is claimed; overlapping decodes from a
search are not admitted instruction boundaries. Licensed bytes stay outside Git;
no game process, DOSBox or emulated call runs.
