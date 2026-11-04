---
id: FND-ITEM-072
title: Canonical equipment effects always have a nonsentinel sound request although 85 of their 186 requests return at the sentinel gate
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2C5F:0A22..2C5F:0AFA
  - build: BLD-GOG-EN-1.1
    file: RESOURCE.GFF
    offset: 0x000120E8..0x00017C28
tool: tools/research/exec-census/resident_listing.py and tools/evidence/report.mjs table at 5d4df111; controlled item pairing of FND-ITEM-060
environment: null
---

## Observation

FND-ITEM-062 binds the equipment route's two sound arguments to bytes
+0F and +16 of its 73-byte DATA snapshot. FND-SOUND-007 describes the
dispatcher. This checks its complete local body and joins those bytes to
the canonical property-one item candidates, to determine which requests
can be excluded from the sound-child preservation obligation in Q-PARTY-118.

### Local early-return paths

The dispatcher tests DS:13F7 first. Zero jumps to its common leave/return.
Otherwise the argument's low byte equal to FF also jumps there, before
reading DS:1435, the far configuration pointer or calling any child.
For a nonsentinel argument, zero at DS:1435 or configuration word +8 equal
to 113 likewise returns before any child. The first child site is +0A5E,
after these gates. None of these early-return paths writes a DS field,
changes DS or calls another routine. Their stack-frame push/return must
still be admitted as valid and disjoint from the flag.

This supplies a local preservation result for an FF argument, conditional
on the declared segment, argument and stack binding: either the first
global gate returns or the sentinel gate does, with no sound child reached.
It does not require reading the configuration pointer. For nonsentinel
requests that pass all gates, FND-SOUND-007's stop, resource, path-formatting
and playback children remain. Host output being muted by the emulator
does not set any of these guest fields or exclude those calls.

### Canonical source composition

The fingerprinted 320-row DATA table uses the catalog-admitted IDs 0..319,
73 bytes each, starting at file offset 73960. Join FND-ITEM-060's complete
1931 item rows by D=word10-1, retaining ordinary word10 in 1..319 and
DATA[D].byte0 AND three equal to one. This reproduces 93 candidates,
covering 53 distinct DATA IDs. Count the two sound bytes per item row,
rather than counting distinct DATA records as invocations.

| Canonical source measure | Count |
| --- | ---: |
| First requests equal to FF | 70 |
| Second requests equal to FF | 15 |
| Candidates with both requests FF | 0 |
| Candidates with exactly one request FF | 85 |
| Candidates with neither request FF | 8 |
| Total requests equal to FF | 85 of 186 |
| Total requests not equal to FF | 101 of 186 |

Thus every canonical candidate has at least one nonsentinel sound request.
The source data cannot eliminate sound children from the complete equipment
path. It does eliminate children for the 85 sentinel requests under the
stated binding. Whether any remaining request actually passes the guest
sound gates needs use-time state; nonsentinel does not mean audible playback.

## Interpretation

Q-PARTY-118 no longer needs a sound-child preservation reading for correctly
bound sentinel requests. For every canonical candidate it still needs either
an admitted early-return guest configuration or preservation through the
reached nonsentinel dispatch children. The snapshot, actual invocation,
DS/stack identity and preservation before the sound calls remain prerequisites.
No native equipment action, audible result, complete writer census, parity
or complete-reading promotion follows from this source composition.

## Alternatives

Treating FF as a valid playback selector ignores the explicit argument gate.
Treating one sentinel as disabling both calls ignores the independent second
argument. Treating muted host output as disabled guest sound would skip
emulated logic that remains configured. Treating 53 distinct DATA IDs as
53 item candidates would drop repeated source payloads.

## How to reproduce

Use the approved DSUN.EXE fingerprint from FND-ITEM-070 and disassemble
2C5F:0A22..2C5F:0AFA with resident_listing.py. The complete entry-based
listing is equipment-sound-gates.txt under
GAME_DIR/analysis/item-removal-20261104/.

Run report.mjs table twice for RESOURCE.GFF length 5724669, XXH3-128
92fe031616f36062f99484b0f728210e, start 73960, count/limit 320, stride 73,
one-byte fields property at zero, soundFirst at fifteen and soundSecond
at twenty-two. Independently retain FND-ITEM-057's catalog/count admission.
Reuse the controlled item-hook-inputs.json of FND-ITEM-060; filter and join
as above, group the ordered sound pairs, and reconcile the total with 93.

The private configuration, report and repeat use placement-sound-data
stems; the derived counts are placement-sound-source-summary.json.
The two complete reports repeat byte-for-byte, SHA-256
35c83c551fcdff3fe3acced2ddad8bb55ab8e466c231bb678c038383cb3845f1.
No licensed DATA payload or original listing is committed or executed.
