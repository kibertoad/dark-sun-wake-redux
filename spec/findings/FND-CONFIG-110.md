---
id: FND-CONFIG-110
title: Overlay 208 selector caller has a guarded entry route and a conditional repeated-call route
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 57A6:006B
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 57A6:007F
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 57A6:0084
tool: Python 3.14.7 and Capstone 5.0.7 bounded 16-bit disassembly; FBOV fixup and trampoline mapping
environment: null
---

## Observation

Two 16-bit relative near-call candidates in overlay 208's declared
code range target entry 57A6:007F at `0x00092FF1`
(FND-CONFIG-108). Both are instructions within distinct exported
entries, with no preceding return between their entry and call:

| Caller | Entry file offset | Call file offset |
|---|---|---|
| 57A6:006B | `0x000929DA` | `0x00092A6E` |
| 57A6:0084 | `0x0009304D` | `0x00093083` |

The 006B entry adds DS:1408 and DS:140A to its first two word
arguments and initializes local byte BP-1 to zero. Byte offset 12 of
the record pointed to by DS:9BCF selects an explicit eight-target
table after subtracting one. Values one, three, four, five and six
call the coordinate helper at `0x00092A0F`; its byte return becomes
the local byte. Values two and eight use a different branch: if
DS:9BE4 is zero, another call to the same helper can set DS:9BE4
without setting the local byte; if DS:9BE4 is nonzero, the helper
at `0x00092A4C` supplies the local byte. Value seven and values
outside one through eight go directly to the final gate.

That gate invokes 007F if the local byte is nonzero. With zero local
byte, it calls a resident helper and invokes 007F only when the
helper's returned word has at least one of its low three bits set
and DS:143C is nonzero. Otherwise the entry returns without that
selector-caller invocation. This reading does not assign a physical
input meaning to those tests.

The 0084 entry clears DS:9BE4 and requires byte offset 12 of the
stored record to equal seven. It then uses DS:9BE0 to index the
37-byte coordinate records at DS:67C5 and DS:67C7, copying their
two words into DS:9BD8/DS:9BDC and DS:9BD6/DS:9BDA, and calls
007F. Its fixed stores do not change DS:9BD3 or DS:9BE2.

Setup 0066 calls 0084 (FND-CONFIG-109). In addition, 007F itself
calls 0084 when its selector returns a nonzero byte. Thus a nonzero
selector result followed by stored record byte 12 still equal to seven
can produce another invocation. The zero-result branch instead calls
local entry beginning `0x00092F2E`.

The declared FBOV direct-call inventory finds no incoming overlay
far call to 007F; selecting exact direct MZ relocated calls to its
resident trampoline also finds none. These are encoding-specific
negative results, separate from the two verified local near calls.

## Interpretation

The stored selector inputs can be consumed through a guarded local
route or through the conditional 0084 route, including a cycle after
a nonzero selector result. The fixed coordinate stores in 0084 do
not themselves reset the gate or code, but selector and helper effects
could change the state that controls repetition.

## Alternatives

The 006B incoming routes, helper results, record-byte producers,
selector state changes and termination of the repeated-call route
remain unread (Q-CONFIG-008). Neither a completed visible action
nor unbounded recursion is established by this local call cycle.
Computed, aliased or unrelocated incoming calls remain possible.

## How to reproduce

Resolve 006B, 007F and 0084 with FMT-EXE-002 through FMT-EXE-004.
Inspect `0x000929DA..0x00092A73` and
`0x0009304D..0x00093088`; stop at returns before the intervening
inline data. Decode exactly eight target words at
`0x00092A73..0x00092A83`, using overlay code base
`0x00091890`. Compare 007F's result branch at
`0x00093037..0x0009304D`. Search 16-bit near-call candidates
only inside the declared code-and-data range and verify each selected
instruction from its exported entry. For direct overlay calls, select
FBOV fixups whose decoded descriptor is 208 and preceding far-call
offset is 007F, using FMT-EXE-005's shifted index. For MZ calls,
select relocated segment words with raw segment 47A6 and call offset
007F; do not extend these negatives to other call encodings.
