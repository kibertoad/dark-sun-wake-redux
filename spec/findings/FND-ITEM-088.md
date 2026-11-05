---
id: FND-ITEM-088
title: Bitmap reclamation ranks resident ages and counts requested-release sizes before unconditional consolidation
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 31E0:3A66..31E0:3C02
tool: tools/research/exec-census/resident_listing.py at f93d99aa; Python 3.12.10, Capstone 5.0.7; composition with FND-ITEM-074/087 and FND-COMBAT-125
environment: null
---

## Observation

Q-PARTY-116 retains reclamation after FND-ITEM-087's pool allocation
reading. This reads the complete local 31E0:3A66 body and reuses the
established mover/cursor contracts. Addresses, offsets and masks below
are hexadecimal; counts and indices are decimal.

### Resident scan and candidate ordering

The routine clears dword DS:655F, its accumulated byte total. A local
three-byte candidate list begins with one record: word index 301 and
age byte FF. It scans indices from zero below unsigned DS:265D, rereading
that count at loop conditions, through 16-byte records at 52A2:0CA8.
Only records whose byte +0E has mask two set enter candidate handling.
No enable-byte gate or own cache-capacity check precedes this scan.

For each resident record, if its age byte +0F is greater than the last
candidate's age, it skips insertion. Otherwise it finds the first candidate
age greater than or equal to the record's age. It inserts that cache index
and the pre-decrement age there, shifting three-byte records with
1000:32C9. List count grows up to ten; a full insertion drops its last
record. Equal ages insert before earlier equal ages, so later scanned
indices precede earlier ties under stable admitted state.

The shift is three times the old count minus insertion index while growing,
or three times that difference minus one record when already full. The
destination is one record above the source. FND-ITEM-074 supplies the
overlap mover's odd/even behavior; valid disjoint stack locals, DS/SS
relationship and spans are still required. Under those conditions the
sentinel and at most nine real records grow to a ten-record list; after
further replacements it can contain ten real records.

Every resident record's age is decremented at byte width after candidate
handling, including records skipped by the last-age test. Zero wraps to
255. Nonresident records receive no age change. Ranking uses each record's
old age while later cache state holds the decremented byte.

### Release requests and accounting

It walks the candidate list while its unsigned accumulated dword is less
than the requested dword and the list has entries. Candidate words greater
than 300 are skipped, excluding initial sentinel 301. Other candidate
words, including 300, are passed to 31E0:3D2A without a local comparison
against the current cache count. It ignores the result, rereads that
record's size dword +2 and adds it to DS:655F at doubleword width.

If the accumulated total is still below the request, it scans current
cache indices from zero below DS:265D. For each still-resident record it
requests 3D2A(index), ignores its result and adds that record's current
size. This fallback stops at the count or the requested total. A failed
first-pass release that leaves residency set can consequently be selected
again; the local body supplies no success accounting or deduplication gate.

Regardless of whether the total reached the request, it finally calls
31E0:38EC. There is no own normalized success result or branch on that
child's result. SI/DI/BP are restored; no broader register preservation is
asserted. Native candidate/cache/count identity, arithmetic wrap, release
and consolidation effects remain independent of the accumulated total.

### Existing pointer-update evidence

FND-ITEM-087's selected pool path calls 1000:0586 with the far address of
its pool pointer field and the request as displacement. FND-COMBAT-125
already records that complete helper: it reads that pointer variable,
performs signed displacement arithmetic and writes normalized offset and
segment back. No new reading is needed. With admitted distinct pool fields
and preserved caller state, this advances the selected pointer before the
allocator subtracts size and returns the saved old pointer. The helper
does not supply allocation, capacity or physical-memory admission.

## Interpretation

This removes reclamation's local resident/age/candidate selection, release
request accounting and unconditional consolidation ordering, and imports
the existing selected-pool pointer-update contract into Q-PARTY-116.
DS:655F measures requested-release sizes reread after calls, not proven
free memory. Actual cache/pool producers, valid stack and record spans,
3D2A release, 38EC consolidation and backing-store/reset children remain.

## Alternatives

Treating the list as ten guaranteed real candidates ignores the sentinel.
Treating the total as freed bytes assumes success the caller never checks.
Treating age decrement as saturation changes zero's result. Treating pool
pointer arithmetic as an unread child repeats FND-COMBAT-125's existing
answer; valid pool state remains a different question. No native failure,
complete-reading declaration or parity promotion follows.

## How to reproduce

Use licensed DSUN.EXE length 634416, XXH3-128
e296af55ba2ecde7e77f555c90f33d0b and project-local evidence Python -I
with resident_listing.py on the half-open location range. Under
GAME_DIR/analysis/item-removal-20261104/,
effect-bitmap-reclamation-contract.txt and its -repeat pair both have
SHA-256 9e8a72d20b3e86a27db70c24edaa360d87e091b4ec42caed222a8ce53a1c4591.
Reuse FND-ITEM-074 and FND-COMBAT-125 for mover/cursor controls and limits.
Keep reports local. No original game or isolated function was executed.
