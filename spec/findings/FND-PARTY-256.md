---
id: FND-PARTY-256
title: Base ability scores feed the current-score rebuild, whose item selectors adjust six scores or movement and whose earlier 24 branch is a lower bound
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008527B..0x00085776
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00085776..0x00085786
    kind: file-data
  - build: BLD-GOG-EN-1.1
    file: CHARSAVE.GFF
    offset: 0x53A..0x540
  - build: BLD-GOG-EN-1.1
    file: CHARSAVE.GFF
    offset: 0x571..0x577
tool: Python 3.14.7 with Capstone 5.0.9 and xxhash 4.0.1 (tools/research/exec-census/field_reads.py, overlay_listing.py, direct_callers.py; a scratch script reading the CHAR resources through the GFF directory as details_chunks.py does)
environment: null
---

## Observation

Overlay offsets are written `descriptor +offset`; file offsets are those of the installed
`DSUN.EXE`. A load copies a CHAR record's bytes `0x23` to `0x28` to `+0x19` of the character's
FMT-COMBAT-001 record and bytes `0x5A` to `0x5F` to `+0x15` of its FMT-COMBAT-002 record
(FND-PARTY-049, FND-PARTY-051). The generation finish copies the six score bytes at `+0x19` of
the combatant record to `+0x15` of the details record (FND-PARTY-070), and the scores it copies
are the rolled ones with the origin modifiers added (FND-PARTY-071).

**The shipped records.** In 18 of the 19 `CHAR` resources of the installed `CHARSAVE.GFF` the
bytes at `0x5A` to `0x5F` equal those at `0x23` to `0x28`. In resource 50 (`Ar'Anda`) the
strength at `0x23` (file `0x53A`) is 19 and the byte at `0x5A` (file `0x571`) is 18; the other five
agree. Resource 40, also
`Ar'Anda`, holds 18 in both.

**`field_reads.py <dsun> <inventory> --es --table 19c5 15 16 17 18 19 1a`** finds reads of those
details bytes in overlay 176 `+01FD`, overlay 197 `+1ED2`, `+1F9A` and `+1FB1`, overlay 204
`+1226` and `+1262`, overlay 209 `+17FB` and overlay 210 `+0D86` and `+0DA8`.

**Overlay 197 `+1E7B`** (trampoline `575A:0039`) copies the six bytes at `+0x15` of a character's
details record into two arrays of six words at `bp-0x33C` and `bp-0x348` (`+1EC2..+1EF4`). It then
goes through the 10-byte entries at `4E71:0251`, dispatching on the byte at `+2` of each through a
table at `cs:2386`; the cases set or raise words of the arrays, such as raising the first word of
`bp-0x33C` to at least 24, using a signed comparison (`+1FC8..+1FF4`) and adding the entry's byte at `+5` to the
first word of `bp-0x348`, kept within 3 and 24 (`+1FFD..+2035`). For each of the six positions it
stores a byte from one of the two arrays, or 3 for positions 3 and 4 under a flag, at `+0x19` of
the character's combatant record (`+22B0..+233D`), and then tests the stored byte against 0x19
(`+234F`). `direct_callers.py <dsun> 197+1E7B` finds it called from overlays 179, 189, 193 and
195.

## Interpretation

### Item-selector extension and clamp correction

The later reading from the same established entry follows the graph
branch at `+2056` and the complete local item cases through the return.
The owner resolver `2D40:3E64` must first return nonzero AL; its two
output words supply combatant and details record indices. A local
permission byte starts at one and is cleared when `193 +1C95(owner, 4)`
returns nonzero. After the earlier effect scan, the routine requests
the owner's graph from `2D40:2196` into an SS-based local buffer.

The signed graph-index loop keeps kind-one entries only, requires
`173 +2DD0` to return 16, and requires that permission byte nonzero.
FND-PARTY-249 describes this ancestor query, including its root-index
exception: for a root, 16 may be an object number rather than a field.
Graph, parent termination, metadata and DS/SS identity remain prerequisites.

For each kept record the word +0x10 minus 320 is tested unsigned
against seven before indexing eight words at overlay `cs:2376`.
This admits exactly source values 320 through 327:

| Word +0x10 | Reached local case |
|---|---|
| 320 | Ability position zero, strength. |
| 321 | Position one, dexterity. |
| 322 | Position two, constitution. |
| 323 | Position three, intelligence. |
| 324 | Position four, wisdom. |
| 325 | Position five, charisma. |
| 326 | Advance to the next graph entry without an item adjustment. |
| 327 | Movement-byte adjustment. |

The first six table words target `+20CF`, the seventh `+22A5` and
the eighth `+21D5`. Two word arrays, A at BP-33C and B at BP-348,
started from the six zero-extended base-score bytes. For ability
position k, let n be the item's unsigned word +0x0E:

- n below 16 adds n to B[k] at 16-bit width.
- n from 16 through 31 sets the candidate-selection flag and replaces
  A[k] with the unsigned larger of A[k] and n.
- n at least 32 subtracts n-31 from B[k] at 16-bit width.

These item branches do not clamp the word result to 3..24. That bound
belongs to one different earlier effect case, as retained above.
The arrays are later read using near addresses derived from BP, so
their agreement with the earlier stack-relative writes remains a
storage-identity prerequisite.

Selector 327 instead adjusts combatant byte +0x13, already initialized
from details byte +0x22. FND-PARTY-146 identifies this as current
movement. With the 16-bit absolute value of item object word +0x00
equal to 804 or 803 it subtracts two; with 802 it subtracts three;
otherwise it adds the low byte of item +0x0E. The byte arithmetic
wraps modulo 256. No ability-array operation occurs in this case.

The final six-position loop first forces positions three and four
to three when its separate earlier flag is set. Otherwise, with the
candidate-selection flag set it selects the signed larger of A[k]
and B[k]; with the flag clear it selects B[k]. It writes the selected
low byte to combatant +0x19+k, then compares that stored byte unsigned
with 25 and replaces larger values with 25. This is byte truncation
before the upper clamp, not a clamp of the full intermediate word.
For example a hypothetical word FFFF would store FF and then 25;
word 0100 would store zero and remain zero. Native admission of such
intermediates is not established here.

The earlier `+1FE0..+1FF4` branch tests A[0] signed against 24:
values at most 24 become 24 and larger values remain unchanged.
Thus FND-PARTY-142's description of this as a cap at 24 was wrong.
It is a lower-bound operation at that local stage, followed later by
the separate byte-write/25 upper clamp. The full effect-code table,
its producers and the owner resolver remain outside this extension.

The current inventory row for this entry counts 710 bytes in five
fragments, while its linear span through the return is 1275 bytes.
The item adjustment case bodies are among omitted intervals. This
reading leaves that provenance/ownership gap explicit and makes no
qualified complete-reading or inventory-repair claim. The shared
hole-report request is toolkit issue 477 (also FND-PARTY-255).

### Retained base-score interpretation

`unk_5a` holds the character's base scores: the six scores as generated, origin modifiers
included. The scores at `0x23` are the current ones, which overlay 197 `+1E7B` rebuilds from the
base scores whenever the entries at `4E71:0251` that it applies change; resource 50's strength
of 19 over a base of 18 is such a raised score. A save stores both, so a score raised by one of
those entries keeps its base beside it.

## Alternatives

- A cap at 24 in the corrected earlier branch is ruled out by its
  signed branch direction; neither it nor the final byte cap can be
  substituted for the other. This finding supersedes FND-PARTY-142
  while retaining its base/current field evidence and shipped comparisons.
- These local item cases do not establish admitted owner/record/value
  domains, graph and effect-list construction, DS/SS preservation,
  arithmetic edge reachability or complete mutation ordering across
  children. Those remaining questions are Q-PARTY-111; graph admission
  and field-16 gameplay naming also remain Q-PARTY-099 and Q-PARTY-082.

- What the entries at `4E71:0251` are, and which of their codes raise or set which score, was not
  read case by case; their word at `+0` is compared with 0xEB, the first psionic power number
  (FND-PARTY-108), which suggests active spells and powers.
- The reads in overlays 176, 204, 209 and 210 were not read for what they do with the base scores.
- Resource 50 may differ for another reason, such as an edit of the shipped save.

## How to reproduce

From the commit that adds this finding, with the locked evidence Python, `<dsun>` the installed
`DSUN.EXE` and `<inventory>` `coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv`: in
`tools/research/exec-census/`, run `field_reads.py <dsun> <inventory> --es --table 19c5 15 16 17
18 19 1a`, `overlay_listing.py <dsun> 197 8527B 85770` and `direct_callers.py <dsun> 197+1E7B`.
Read the directory of the installed `CHARSAVE.GFF` as `details_chunks.py` does and compare bytes
`0x23` to `0x28` with `0x5A` to `0x5F` of each `CHAR` resource.

For this correction and extension, use the unchanged committed
`overlay_listing.py <dsun> 197 8527B 85776` at commit 3c6e2941
and read the eight little-endian target words at file 85776. Check
the subtraction/bound before the table, each n interval, the
movement object cases, the array-selection flags and the final
byte-store-before-clamp ordering. The source length and XXH3 checks
are enforced by the script. No original function or game process ran
for this reading.
