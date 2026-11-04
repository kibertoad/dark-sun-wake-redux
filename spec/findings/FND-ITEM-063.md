---
id: FND-ITEM-063
title: The canonical equipment area pass selects at most one filtered effect-object target without proving a removable row
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00091890..0x00091C06
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000921C3..0x000923FD
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00080B94..0x00080CDB
  - build: BLD-GOG-EN-1.1
    file: RESOURCE.GFF
    offset: 0x00012CE2..0x00012D2B
tool: tools/research/exec-census/overlay_listing.py and tools/evidence/report.mjs table at a1904835; composition with FND-ITEM-062 and FND-PARTY-262
environment: null
---

## Observation

FND-ITEM-062 identifies one canonical equipment candidate for the post-hit
area pass, selector D=42. FND-PARTY-262 corrects its target child's zero-count
unload branch. This follows the selected list construction and tests whether
selection itself proves a positive removal count. It does not settle actual
target-state, child effects or the DUAL routes of Q-PARTY-077.

### Canonical branch and count cap

Canonical DATA/42 is 73 bytes, with byte +17 equal to FF, byte +0C equal to
seven and word +1A equal to 0202 hex. The call from 193 +003C supplies owner,
D, the owner's derived center, mask FFFF, the near output list and final
byte one. Mask FFFF does not select the ordinary combatant scanner here.

208 +0000 first reads caster/script state, snapshots DATA and calls +0AD2
to obtain a range and potentially revised coordinates. It then tests the
snapshot's +17. Retained FF takes +02A3. Final byte one chooses
193 +28A4(D, revised x, revised y, half of the signed range truncated toward
zero, output list), rather than 173 +39CC's combatant-area scan.

The returned count is capped at one when greater than one, before the
final +0301 filter. This is a first-candidate cap, not a search for the first
candidate that passes the filter. With an admitted count/frame/list and
returning filter predicate, the result is zero or one. A first candidate
rejected by the predicate leaves zero; the caller does not retry another
candidate omitted by the cap. No target or nonempty list is established
by the canonical source record alone.

### Selected effect-object scan

The complete +28A4 body reuses DATA D to capture its +1A mask 02 and a
separate D=231 flag. It walks the eight-byte table at DS:67B7 in current
table order, stopping at the current unsigned DS:264E count or 64 accepted
distinct slots. It rejects a row with a negative slot word. For other rows,
let b be byte +0F of that slot's 37-byte object record. It uses b-1 as the
effect-row index and rejects b below two. For D=42's retained mask 02 it
requires bit 40 hex of byte two of that eighteen-byte effect row. The
D=231/bit-80 test and D=181's further DATA test are not taken for D=42.

The scan excludes a slot already in the output and requires the distance
from its input position, as returned by 2D40:382F, to be no greater than
the signed range argument. It appends only the target word at offset two
of each four-byte output entry. It does not write a distance word, sort
by distance, require state two or test a hostility mask. Thus the candidate
surviving the outer cap is the first qualifying distinct slot in table
order, not a proved nearest combatant. Actual table count/capacity, slot
and effect-row indices, distance semantics, DS/SS list identity and retained
storage remain prerequisites.

The outer +0301 helper calls 208 +0F07 on each retained target with owner
and D, deleting rejected list entries and decrementing its byte count.
Its predicate and compaction child effects remain separate contracts;
neither canonical DATA presence nor passing the preceding scan proves
that final predicate will accept the candidate.

### Selection does not establish a removal

+28A4 does not test bit 20 hex of the eight-byte row's byte five. The
371D:050F removal loop requires that bit and a matching slot. A selected
row and b>=2 therefore do not by themselves guarantee that the later
removal count is positive. The target's b excludes the simple b=0 early
skip only if preserved to the child, but not a scan with no flagged match.

FND-PARTY-262's zero-count unload branch remains relevant. Proving its
exclusion on this route requires a producer/lifetime invariant tying every
selected target to a retained flagged row, including changes by the final
filter and earlier per-target work. A matching row without the required
flag, mutation/removal before use, changed base/count or aliases remain
unsettled alternatives. No actual zero-count unload was observed.

### Range setup remains a distinct prerequisite

+0AD2 chooses the script-level byte while DS:45AA is set. Otherwise D=42
uses 177 +07AF(owner, D) and forwards its low byte to +0933. This is not
the constant-six cast-level argument of FND-ITEM-061. The range calculator
and its children must be admitted independently of that cast-level binding.

+0933 snapshots DATA and forms an effect-row pointer from signed byte +17
before +0000's later FF test. For retained DATA/42, this is row minus one,
offset FFEE in segment 51F1; its byte-six accesses address 51F1:FFF4.
There is no local FF early return in +0933. This is not an admitted member
of the canonical 112-row effect table. Readability, actual memory contents,
caller/child/frame preservation and whether any resulting value affects a
later consumed coordinate/range must be checked; no canonical row value or
native failure is inferred from this address calculation. The selected
+0000 branch does not consume the returned angle word locally.

## Interpretation

Q-PARTY-109 no longer treats the extra pass as an unrestricted combatant
area list: its canonical D=42 branch uses the effect-object scanner,
caps before filtering and yields at most one admitted target. It retains
actual range/coordinate setup, predicate/compaction, table and flagged-row
production/preservation, target-child effects and native ownership. The
same slot-lifetime prerequisites matter to Q-PARTY-077; this source-specific
composition does not restore the withdrawn universal DUAL exclusion.
No confidence, rule, parity or complete-reading promotion is claimed.

## Alternatives

A first qualifying row need not be the closest. Mask FFFF does not bypass
the selected effect-row bit test. A nonzero b does not prove a flagged
row survives to removal. The cast's level six is not automatically the
range setup's level. Forming a row-minus-one pointer is not evidence for
a documented row at that location or an observed native crash.

## How to reproduce

Use installed DSUN.EXE length 634416, XXH3-128
e296af55ba2ecde7e77f555c90f33d0b. Run overlay_listing.py for descriptor 208,
91890..91C4A and 921C3..9241C hex, and descriptor 193, 80B94..80CDB.
The retained placement-area-list.txt covers +0000, +0301 and two later
wrappers; placement-area-range.txt covers +0933, +0AD2 and an adjacent
wrapper, whose extra instructions are not part of these claims.
placement-area-object-scan.txt covers the complete +28A4 body.
placement-area-filter.txt is a partial diagnostic listing of +0F07, not a
complete predicate reading. All remain under
GAME_DIR/analysis/item-removal-20261104/.

For RESOURCE.GFF length 5724669, XXH3-128
92fe031616f36062f99484b0f728210e, run report.mjs table at decimal
73960+42*73=77026, count one, stride 73, fields effectRow byte 23,
targetKind byte 12 and typeMask word 26. Independent DATA catalog identity
and width are FND-ITEM-057. placement-area-data42-controls.json and
placement-area-data42.json/-repeat.json remain beside the listings.
The two reports match byte-for-byte, SHA-256
759bb3ec9a94f1b55b0744fc2d527d5bac36cb843e3a3d4b01ac8d1b58fdbda5.
Reuse FND-ITEM-062's caller pushes and FND-PARTY-262's removal flag/count
gates. No original code, memory, asset or fixture is committed or executed.
