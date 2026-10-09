---
id: FND-EXE-284
title: Relocated return-wrapper call census adds a gated cleanup caller
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 444C:01A4..444C:0203
tool: executable-reader 2.5.0, Capstone 5.0.7 and xxhash 4.0.1
environment: null
---

## Observation

The relocation-aware incoming query for shipped target 0x0000669B
returned two far-call byte candidates, at 0x000397A6 and 0x000398AD,
with result limit 100, no truncation and no unresolved candidate.
Both target operands are declared MZ relocations resolving to 1000:149B.
The independent known-call control at 0x00040DA9 resolved to 1000:15A5.
FND-CONFIG-165 supplies the first candidate's bounded caller reading.
This is a candidate census, not proof that no other kind of caller exists.

The second candidate lies on the instruction path beginning at
444C:01A4. That body saves DS and flags, disables maskable interrupts,
and loads DS from a relocated immediate resolving to 57E0. It tests
DS-relative word 0x33CC for exact equality with one. Other values write
0x000D to DS-relative word 0x33BE, set AX to 0xFFFF and reach the
flags/DS restoration and far return.

Equality clears AX and DS-relative words 0x33CC, 0x33C2, 0x33C0,
0x33C4 and 0x33C6, in that order. It then makes two far calls whose
target contracts are unread here. After those calls it tests then-current
DS-relative word 0x33CE for exact equality with one. Equality pushes
then-current DS-relative words 0x33CA and 0x33C8, in that order,
and calls 1000:149B through the relocated target operand. It removes
four argument bytes and reaches the common restoration and far return.
Inequality bypasses this call and reaches that same suffix.

There is no local AX test after the return-wrapper call and no local
restoration of the preceding cleared words. On the admitted arm there
is no common AX normalization before returning: the last reached callee
can determine it. The post-call state accesses use DS then in force,
not proven preservation of the initially loaded 57E0 segment.

## Interpretation

This establishes an additional direct caller path for FND-CONFIG-167's
return wrapper. Its high argument comes from word 0x33CA and its low
argument from word 0x33C8, subject to the unresolved earlier callees and
the exact-one gate. Those words' writers, lifecycle and admitted segment
range remain unknown in this reading. Clearing state before the calls
does not establish completed cleanup, successful release or rollback.

Q-EXE-001 and Q-EXE-010 retain the two earlier far targets, state writers,
DS preservation, interrupt behavior and all excluded caller kinds.
The census excludes near calls, computed calls, unrelocated pointers
and instruction-boundary verification; the second hit's local path was
checked separately. The positive control covers an MZ-relocated call,
not a separate FBOV-fixup control. No absence or complete-reading claim
is made from this search.

## Alternatives

Treating either gate as generic nonzero ignores its equality-to-one test.
Treating the saved pair as coming from known initialized storage assumes
unread state writers and callee preservation. Treating two relocated hits
as every possible caller ignores the query exclusions. Treating state
clearing as successful cleanup ignores the subsequent unchecked calls.

## How to reproduce

At revision 116c73b, require installed DSUN.EXE's XXH3-128
e296af55ba2ecde7e77f555c90f33d0b recorded in FND-EXE-176. Run the
incoming report with loadSegment 0x1000, target 0x0000669B, limit 100
and controls [0x00040DA9]. Use the committed evidence-report wrapper.
With locked Capstone 5.0.7 in sixteen-bit x86 mode decode shipped
0x00039864..0x000398C3 at initial IP 0x01A4. Resolve operand sites
0x00039868 with targetOffset zero and 0x000398B0 with targetOffset
0x149B using the operand report. Follow both gates, ordered stores,
outgoing pair and restoration. Cross-check FND-CONFIG-165/167 and
FND-EXE-271/272's known-call control. Source, configurations and reports
stay outside Git; no original execution or complete caller search is claimed.
