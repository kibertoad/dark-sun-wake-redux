---
id: FND-EXE-294
title: Allocator list selection connects DX to the selected header segment
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:15A5..1000:1622
tool: Capstone 5.0.7 and xxhash 4.0.1
environment: null
---

## Observation

FND-EXE-272's allocator first loads DX from CS:135B. For a nonzero
value it replaces DX with CS:135F. A nonzero latter value is copied
to BX, then loaded into DS before comparing word DS:0000 with the
converted request in AX. Each smaller-count iteration reads DS:0006
into DX; unless that equals the retained initial BX, it re-enters at
the DS-from-DX load before the next header comparison. There is no
instruction modifying DX between that load and either successful-count
branch. Thus each selected comparison reaches its exact-count or
larger-count helper with DS and DX containing the same segment word.
The reads still require initialized, admitted storage.

On equality it calls FND-EXE-273's unlink helper, whose ordinary returns
restore the selected DS and preserve DX. It then copies selected word
eight into word two and sets AX=0004. On the larger-count branch it
calls FND-EXE-274's split helper. Under the just-described local equality
of DS and DX, the helper computes its first adjusted segment from the
selected segment plus the available count minus the requested count,
all at word width. That adjusted segment becomes returned DX; its
word zero is written with the requested count and AX becomes 0004.
Its later neighbor access and physical aliases remain separate obligations.

For FND-EXE-293's request 00000401, adding nineteen yields 00000414;
the allocator's division by sixteen yields paragraph count 0041. No
overflow rejection is selected by this arithmetic. If the chosen header
retains count 0041 through the relevant calls and stores, the wrapper's
returned offset 0004 minus four selects offset zero in returned DX.
Its header-derived shift then gives 0410 and its byte write selects
offset 040F in that segment. Relative to the returned offset 0004,
this is byte displacement 040B. These are conditional address calculations,
not a claim that the segment contains a valid allocation.

## Interpretation

This settles the local DS/DX relation left open by FND-EXE-274 and
FND-EXE-273 for the allocator's admitted list-selection instruction path.
It does not validate the list, establish its termination, exclude other
entries into helpers or protect header counts against aliases and later
writers. The first-allocation and fallback paths retain their distinct
callee and interrupt obligations in FND-EXE-275 and FND-EXE-280.

Q-EXE-010 retains actual segment bounds, header initialization and lifetime,
physical aliases, count stability, all state writers and native interrupt
effects. A count-derived address lying within a nominal paragraph count
does not establish that those bytes were acquired and remain writable.
No complete-reading promotion or initialized-extent claim follows.

## Alternatives

Leaving DX unrelated to DS at the selected comparison ignores its explicit
DS-from-DX load on every list iteration. Treating that equality as valid
storage ignores the source link's admission. Treating the count calculation
as proof of an extent ignores allocation effects and overlapping stores.
Treating this list path as every allocation path ignores the separate
initial and fallback calls.

## How to reproduce

At revision 8fd14ac require FND-EXE-176's installed DSUN.EXE identity,
XXH3-128 e296af55ba2ecde7e77f555c90f33d0b. With locked Capstone
5.0.7 in sixteen-bit x86 mode decode shipped half-open
0x000067A5..0x00006822 at IP 15A5, modeled CS 1000. Follow
each DX writer, the loop's DS load and both count branches; compare
FND-EXE-273, FND-EXE-274 and FND-EXE-293's separately recorded
bodies. Check the stated word-width arithmetic for input 00000401
and retained header count 0041. Source bytes and reports stay outside
Git. No original execution or complete writer search is claimed.
