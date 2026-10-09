---
id: FND-EXE-518
title: Game allocator selected cleanup updates globals before a stack-margin setter can reject
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:20A3..1000:20FA
  - build: BLD-GOG-EN-1.1
    file: CD:DSUN.EXE
    address: 1000:20A3..1000:20FA
tool: Capstone 5.0.7 and xxhash 4.0.1
environment: null
---

## Observation

FND-EXE-512's far callee 20A3 saves SI and DI, copies SP
into SI and reads word SI+8 through current DS into BX. With
admitted equal DS/SS and intact frame this is the outgoing source word;
without those conditions it remains a DS-based read at a stack-derived
offset. It subtracts four from BX at word width. Borrow skips both
later calls and returns without replacing AX locally.

Otherwise it compares BX with current DS:390E installed or DS:3882
on disc. Equality calls near 20C0; inequality calls near 20FA,
whose body is outside this reading. It then restores DI and SI and
returns far without incoming cleanup. There is no result test after either
call. The wrapper's saves restore SI/DI locally conditional on intact stack
and callee preservation of frame/segments.

Near 20C0 first compares BX with current DS:390C installed or
DS:3880 on disc. Equality clears the three shared words at installed
390C/390E/3910 or disc 3880/3882/3884 before continuing.
Inequality loads SI from current DS word BX+2 and tests low byte
DS:SI for bit one. Set stores SI into the second shared word,
leaving BX unchanged, then continues. Clear compares SI with the first
shared word. Equality sets BX to SI and clears all three shared words.

With clear bit one and SI different from the first shared word, it
sets BX to SI and calls FND-EXE-514's near 2133. That helper
locally preserves BX while changing links and the third shared word. After
return it reloads word DS:BX+2 into AX and stores AX into the
second shared word. The next shared-state update therefore follows the
link helper and can observe its stores under aliases.

All branches then push current BX into FND-EXE-517's near 0F46,
remove two argument bytes into BX and return near without incoming cleanup.
Thus the outgoing setter offset is either the incoming BX or the selected
SI according to the paths above. The setter's returned AX is not tested
or replaced locally. If its margin check rejects, the earlier shared-word
or link changes remain; no local rollback restores them. The preserved
argument pop restores BX, rather than retaining the setter's DX result.

The selected helper contains no local interrupt and does not locally change
DS or SS. Actual segments, block/link extents, aliases and lifetime remain
unadmitted. Both editions share local control flow with the distinct shared
offsets listed above. The other 20A3 branch through 20FA remains unread.

## Interpretation

This supplies a concrete setter caller reachable from diagnostic initialization
when the record's flag-four path selects 20A3, conditional on state
admission. It separates mutations before setter rejection from the setter's
own local nonmutation of DS:009C on failure. Q-EXE-007 retains
20FA, remaining setter callers and offset producers, block/shared-state writers,
actual DS/SS, storage and frame extents, aliases and lifetime, other
initialization helpers and broader startup/launch coverage. No complete cleanup
or writer census is claimed.

## Alternatives

Treating the wrapper's argument as admitted stack storage ignores its DS
read. Treating setter rejection as transactional ignores earlier global/link
stores. Treating every setter argument as the incoming BX ignores the
SI-selected paths. Treating 20A3 as returning one fixed success/failure
contract ignores its unmodified-AX early exit and unread alternate helper.

## How to reproduce

At revision a2a5f29 require both DSUN.EXE identities from FND-EXE-350.
With header size 5200 and modeled load segment 1000, decode shipped
72A3..72FA in sixteen-bit mode. Track the DS-based SI+8 read,
subtraction borrow, shared-word comparisons, low-byte bit test, ordered global
stores and 2133's preserved BX into the subsequent field reload.
Follow every BX producer into the setter argument and its cleanup pop.
The positive lead search tests E8 rel16 targets in resident segment-zero
offsets 0000..FFFF for target 0F46, including the independently known
0F99 wrapper's call at 0F9F. It supplies candidates 20F5 and
22A2; bounded decoding from 20A3 admits the former, while the
latter and other transfer forms remain outside this reading. No absence
claim relies on this search. Licensed bytes stay outside Git; no game
process, DOSBox or emulated call runs.
