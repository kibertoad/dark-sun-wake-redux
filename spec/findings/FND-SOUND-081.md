---
id: FND-SOUND-081
title: Four raw snapshot aliases pair call and stack-test bytes while local-number cleanup explicitly clears the current pointer
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000633C5..0x00063606
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000577BB..0x0005783A
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000579A2..0x00057A39
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00057AB6..0x00057AD2
tool: alias_far_words.py, segment_offset_refs.py and overlay_listing.py at fbcb4d90; Python 3.12.10, Capstone distribution 5.0.9, xxhash 4.0.1
environment: null
---

## Observation

This audit specializes FND-SOUND-080's saved local-number-nine word at
4E71:0216..0218 and current local-number pointer 4C13:0337. Installed
DSUN.EXE is 634416 bytes, XXH3-128
e296af55ba2ecde7e77f555c90f33d0b. It is a bounded reference audit,
not an exhaustive alias or writer exclusion.

### Four nearby word-pair candidates do not form snapshot pointers

The relocated/fixup nearby-pair query processes 12965 readable segment
words with zero unreadable words. For target 4E71:0216 it returns four
paragraph-alias candidates, each pairing segment 3EBE with offset FD46.
The arithmetic alias is valid, but the instruction context does not form
those parts into a pointer:

| Segment word file position | Actual far call starts | Following stack-byte test starts |
|---|---|---|
| 634EE | 634EB, overlay 178 +043B | 634F3, +0443 |
| 63515 | 63512, +0462 | 6351A, +046A |
| 635B3 | 635B0, +0500 | 635B8, +0508 |
| 635DA | 635D7, +0527 | 635DF, +052F |

Each segment word belongs to a far call to 3EBE:071F. The candidate
FD46 word six bytes later spans the addressing bytes of a subsequent
BP-relative byte test. It is not an encoded near pointer operand.
FND-EXE-604 establishes the containing +0040 window/action routine and
its discontiguous ranges; the checked range starts at file 633C5.
These four raw pairs therefore supply no snapshot-word binding on the
recorded streams. Other incoming streams are not excluded.

The companion target 4E71:0204, the known number-block snapshot base,
returns the two canonical segment/offset argument pairs at file 8DD31
and 8DD98 in the save/restore helpers already read by FND-SOUND-080.
They are positive controls for the query's relocated/fixup pair mechanism.
The query covers nearby offset words within twelve bytes of declared
segment words, not distant pairs, runtime-built addresses, unrelocated
segments, arbitrary computed copies or whole pointer lifetimes.

### Current local-number pointer has an explicit cleanup invalidation

The segment-offset query for 4C13:0337 returns eleven candidate contexts:
four resident and seven overlay contexts. The allocation, local clear,
release, save-table and backup/restore uses match FND-SCRIPT-026's
recorded roles. This query follows at most six decoded instructions from
a literal segment move; it does not by itself establish every candidate
or cover other access mechanisms. The two known pointer stores provide
controls and are read from their containing established entry ranges.

In overlay 169 +023B's allocation path, +0298 pushes a 32-bit request
of 64, calls the existing +0552 allocator wrapper and publishes returned
DX and AX to 4C13:0339 and 0337 at +02A6/+02AB. This is the allocation
request and returned-pointer publication already recorded by FND-SCRIPT-026;
it does not independently prove native capacity or allocation success.

In release entry +0422, +049D pushes current 4C13:0337 to local helper
+0536 at +04A5. On return, +04B0/+04B5 publish DX:AX to the same
pointer. The complete +0536 body checks its incoming far pointer for zero,
conditionally calls 444C:0092, then explicitly clears DX and AX and returns.
Thus both pointer words become zero on this intact returning cleanup path,
regardless of the release service's returned pointer. Reaching the service
call alone does not prove release success, return or subsequent publication.

FND-SOUND-080's restore subsequently uses the current pointer and has no
null check. A cleanup path that reaches this publication invalidates the
previous current-pointer binding; restoring snapshot bytes does not undo
that publication. Actual reachability of release between save and restore,
other writers and physical aliases remain open.

## Interpretation

No unclassified candidate remains in this particular nearby snapshot-word
pair query. The pointer-lifetime obligation is concrete: bind allocation
and exclude or account for the +0422 cleanup publication before the restore,
beside other excluded writers and aliases. Q-SOUND-004 retains those actual
histories, the saved-word backing, native/callback effects and continuation.
No universal no-write result, native invalid copy or complete reading follows.

## Alternatives

Treating the four arithmetic alias pairs as usable pointers conflates a
far-call segment with addressing bytes in a different instruction. Treating
cleanup as preserving the original pointer contradicts the explicit zero
return and publication. Treating the query as a complete memory-write census
would omit computed/bulk accesses and the declared exclusions above.

## How to reproduce

With tools/research/exec-census/ at fbcb4d90 and installed DSUN.EXE, run
alias_far_words.py targets 4E71:0216 and 4E71:0204, and
segment_offset_refs.py segment 4C13 offset 0337. Retain
music-health-snapshot-far-bindings.txt and
music-health-local-number-pointer-refs.txt under
GAME_DIR/analysis/handler-census-20261106/.

Use overlay_listing.py descriptor 178 file 633C5..63606 for
music-health-snapshot-alias-context.txt; descriptor 169 ranges
577BB..5783A, 579A2..57A39 and 57AB6..57AD2 for
music-health-local-number-allocation.txt, music-health-local-number-release.txt
and music-health-local-release-return.txt. Preserve the known copy-base
controls, instruction boundaries and exclusions. No original game or
emulated call supplied this result.
