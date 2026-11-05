---
id: FND-ITEM-103
title: Temporary-slot handoff copies the initialized entry index and normal loader completion clears direction flag
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2D40:0461..2D40:048D
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2D40:0C3F..2D40:0C92
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 31E0:10E2..31E0:1137
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 31E0:1241..31E0:1262
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:0452..1000:046E
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:3F7E..1000:3FC1
tool: tools/research/exec-census/resident_listing.py at a294b0ce; Python 3.12.10, Capstone 5.0.7; FND-COMBAT-149/161, FND-PARTY-119 and FND-ITEM-096/097/102 composition
environment: null
---

## Observation

Q-PARTY-121 retains first-allocation copies and DF before ordinary
publication. FND-COMBAT-161 reads these complete resident helpers for a
flags-zero call set; their local copy/pop/fill contracts also apply when
selected by the unchanged mode-one route. This binds their widths and
writes to the initialized slot index, keeping actual admission separate.
Addresses and field offsets are hexadecimal; sizes and indices are decimal.

### First replacement allocation

2D40:0461 examines its argument's low byte. Nonzero first pops head
DS:56ED; zero skips that head. A retained result other than 9999 returns
it, otherwise it pops DS:56F1 and returns that result. The pop at 0C3F
has no child: head 9999 returns 9999 without mutation; another head s
becomes its returned word, the head is replaced with the paired word at
4F49:0C34+3*s, and that paired word is cleared. Popping DS:56F1 also
increments DS:55C8 at word width; the helper has no saturation test or
own slot-range validation. It does not itself change the popped kind.

FND-COMBAT-149 binds the loader's argument to whether temporary slot
319's current kind is two, not its category argument. Thus category five
alone does not show which head is popped. On a first result other than
9999, retry and eviction are skipped. Assume the actual result is an
admitted replacement s in 5..318, with intact distinct source, destination,
head, counter and frame backing. No allocator/pop store then targets the
temporary presentation index or placed count DS:264E. No inference from
a legal result word to an intact native free-list history follows.

### Fixed handoff and index identity

At 31E0:10E2 the loader copies three bytes from temporary row 319 of
the kind/paired-word table to row s. It then copies all 37 presentation
bytes from DS:95D6 to DS:67BB+37*s. Both use 1000:0452. This helper
loads distinct far source and destination arguments, saves/restores DS,
SI, DI and BP, clears DF, copies floor(CX/2) words and then CX's original
odd tail byte, and removes the two far arguments with RETF 8. Lengths
three and 37 copy their entire odd final bytes; the index word at
presentation +1 crosses two copied word units and both bytes are carried.
No child or result-dependent short-copy path exists in that helper.

Let e be the initializer's retained entry-index argument recorded in
FND-ITEM-097. Under unchanged temporary source/frame and the valid s
above, the replacement record's word +1 is e after the handoff. The
paired word cleared by the pop is separately overwritten by the first
copy's temporary paired word. It is not the presentation index. The
loader then writes s into placed entry e's word +6 at 1127. Slot s is
at least five, so the below-four script-chain call is skipped here.
No call intervenes between the two fixed copies and that row-slot store.

This proves a conditional handoff at that point, not preservation through
subsequent bitmap refresh. It also does not prove that e equals the
current placed count after the earlier resource request or later children.

### Normal-return DF and the ordinary caller

Every normal loader completion reaches 1241's 37-byte FF fill of the
temporary presentation record through 1000:3FA2. Its complete wrapper
calls only 1000:3F7E. The leaf executes CLD before its alignment and
byte/word fill, including an empty-count route. Neither the wrapper nor
the loader's remaining kind-clear, result move and epilogue changes DF.
Thus a normally completed valid fill leaves DF clear at the return,
regardless of DF left by earlier returning refresh children. It clears
the temporary record, not a valid distinct replacement's index.

FND-ITEM-096's ordinary caller admits only a non-FFFF loader return.
Between this normal return and insertion it reaches only the optional
coordinate helper and 31E0:444A; FND-ITEM-102 and FND-ITEM-096 give their
complete local bodies, neither changes DF. Its local arithmetic and
stores do not change DF either. In FND-ITEM-097's retained normal-return
domain, the insertion wrapper therefore receives DF clear. That local
boundary-copy precondition is supplied by this path, not an assumption
about the machine's DF before the original placement call.

## Interpretation

This removes unread first-success pop/copy/index handoff and normal
loader-return DF from Q-PARTY-121. The remaining question is actual
free-head/slot/source/frame admission and e/count identity through the
resource request and refresh, plus other allocation/retry outcomes and
the later immediate/cell/queue calls. The placement-index proof remains
conditional on these inputs and disjoint backing. No original constructor
call, complete reading, defect or parity promotion is established.

## Alternatives

- A pop's cleared paired word clears the presentation index: those are
different tables, and fixed handoff overwrites the former separately.
- Both copies lose the last byte because they use words: an explicit
odd-tail copy follows the word loop.
- The ordinary boundary copy always requires an unknown initial DF:
the normal lower-loader fill clears it after the earlier child calls.
- An observed free-list snapshot admits this constructor: FND-COMBAT-169
records a distinct paused endpoint, not the lifetime at this call.

## How to reproduce

Use licensed DSUN.EXE length 634416, XXH3-128
e296af55ba2ecde7e77f555c90f33d0b and project-local evidence Python -I.
Run resident_listing.py on the six location ranges in order. Retained
reports under GAME_DIR/analysis/item-removal-20261104/ are
effect-slot-copy-df-contract.txt and its -repeat pair, both SHA-256
a6afdc34c7b579fa6a8f74395566858780f347d04e3c51f5feeacc88d9ca6a33.
Reuse FND-COMBAT-149's full loader body, FND-COMBAT-161's complete
helper call set and FND-ITEM-096/097/102 for initializer/caller/insertion
conditions. Keep original-derived reports local. No original game or
isolated function ran.
