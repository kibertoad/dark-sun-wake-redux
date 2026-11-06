---
id: FND-EXE-649
title: The recorded break publisher graph preserves canonical offsets and conditionally excludes fresh heap headers from the resident overlay domain
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:1528..1000:1582
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:16EA..1000:1702
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:19A9..1000:19C9
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x0004D09E..0x0004D0A0
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x0004D0A2..0x0004D0A4
tool: Pinned Python 3.12.10, Capstone module 5.0.7 (distribution 5.0.9), executable-reader 4.3.0; direct_callers.py and resident_listing.py at 1922a19b; retained CONFIG-211/212/225/226/231 and EXE-548/549 interfaces
environment: null
---

## Observation

This composes the recorded fresh-allocation address path for
Q-EXE-033 and FND-EXE-648's concrete resource allocation producer.
It does not reopen the free-list or coalescing bodies as unknown helpers.
The shared allocator assessment remains Q-CONFIG-013; this result is
not its complete input, membership or backing proof.

**Direct publisher/caller graph.** The fingerprint-checked caller reader
reports these direct near sites in segment 1000:

| Callee | Reported direct call sites | Retained/new argument classification |
| --- | --- | --- |
| 177C | 1821, 18AB | 17F2 forwards its selected pair; 1831 passes its normalized candidate |
| 17F2 | 13C3, 16F7 | The terminal release cases and terminal resize branch supply offset zero |
| 1831 | 14CE, 14E7, 1500, 153A, 156E, 19BC | First query, alignment and paragraph increments; diagnostic 19BC supplies zero |

FND-CONFIG-231 supplies all four 13C3 terminal argument choices.
The 16EA branch pushes its adjusted segment and an explicitly zero
low word before 16F7, as retained in FND-EXE-296 and checked here.
19A9's diagnostic branch pushes zero twice before 19BC, consistent
with FND-CONFIG-228. The two 177C callers and their branch meanings
are retained in FND-EXE-548/549 and FND-CONFIG-167.
The direct census is not an exclusion of computed or alternate entries.

FND-CONFIG-226 assigns the explicit current-pair stores and the
startup publications. Source offset words at modeled DS:009E and
DS:00A2, file 4D09E and 4D0A2, are both zero. No two-byte MZ relocation
intersects either word. Under that finding's selected startup and
unchanged-shipped-quantity conditions, lower and current segment words
are assigned the same DS+DI, with B4F <= DI <= 1000 hexadecimal.

**Conditional own-publication invariant.** Let the expected data-field
identity be preserved, let the lower offset remain zero, and admit that
startup state. Restrict subsequent current-pair publications to the
reported direct graph and the explicit stores already audited. Require
intact, separate frames and retained candidate arguments across calls,
with no outside, computed, bulk or asynchronous mutation of those fields.
On those conditions:

- Startup has current offset zero and current address equal to the lower
  address L. The source zero alone does not admit a live startup state.
- A committing 1831 path supplies the candidate produced by 060B,
  whose offset is 0..15 (FND-CONFIG-212). It passes the lower comparison
  before 177C. With the lower offset zero, that canonical candidate is
  numerically at or above L. A returned rejection makes no own current
  pair publication through that call.
- A committing 17F2 path receives offset zero at the two recorded sites
  and passes its own lower comparison before 177C. The resulting current
  pair is likewise canonical and at or above L.
- 177C commits the incoming pair on its recorded success paths; its
  failure branch updates the upper bound but not the current pair.
  Native backing and the sentinel ambiguity in FND-EXE-549 remain open.

Thus this own-publication graph preserves current offset O in 0..15
and numeric current address B=16*S+O at or above L. It does not prove
that every actual writer/caller belongs to this graph or has its bindings.

**What fresh allocation can inherit.** 1831 checks the normalized next
candidate, but on nonzero updater result returns a separately captured
previous pair. A checked next pointer is not by itself a normalized
previous pointer. The invariant above supplies that missing previous
address-form condition for the recorded own-publication history.

The admitted nonzero allocator paragraph count p is 1..FFFF and its
fresh increment is D=16*p. FND-CONFIG-212 shows the preliminary gate
rejects sums above FFFFF for these nonnegative forms. For a successful
fresh request from the conditional state, the captured previous segment
S therefore has 16*S >= L: O<16 and paragraph-aligned L make that follow
from 16*S+O >= L.

First acquisition 14C4 writes its header through the final returned
segment. Its earlier zero/alignment queries are not assumed to succeed;
any own current publication they do make still uses 1831's canonical
candidate, under the same conditions. Growth 1528 uses the returned
segment when its offset's low nibble is zero. For an unaligned result it
requires the subsequent alignment request to return before accepting,
then increments the saved previous segment by one. The preceding
positive paragraph request excludes previous segment FFFF on an accepted
path, so that increment does not wrap. Either fresh helper's own header
stores therefore start at or above L under this invariant. This is an
address-domain relation, not proof of extent ownership or acquired RAM.

For the modeled DS=57E0 and the retained startup DI range, L lies from
632F0 through 67E00. FND-EXE-003/004's 49 resident overlay headers start
between 565C and 57DD; their 32-byte field regions have exclusive
upper bound 57DF0. Even the
conservative envelope including 53 trampolines after the last start ends
at 57EF9. It lies below the minimum L. The selected fresh header stores
are therefore linearly disjoint from these resident headers on the named
bindings. Relocated actual bindings need their own no-wrap relation.
This does not classify free-list exact/tail returns as fresh requests.

**Why the previous pair matters.** The growth body tests AX against
FFFF, then masks it with 000F before choosing its aligned branch. It does
not normalize a noncanonical captured pair into DX. For illustration,
raw previous pair 57DD:B520 denotes numeric 632F0, so a positive
16-byte request can have normalized next candidate 6330:0000 within
lower 632F:0000 and a suitable upper bound. If the updater returns its
success indication and the captured pair is retained, growth sees zero
low nibble and writes its header at segment 57DD, not 632F. Those are
ordinary word/address calculations for an explicitly noncanonical
incoming pair. That pair violates the own-publication invariant above;
it was not seeded, observed or established reachable in the game.
The example rules out inferring header disjointness from next-bound
checks alone, not the admitted recorded producer history.

## Interpretation

The fresh segment's linear header domain has a concrete conditional
producer proof. Remaining admission is the actual data-field identity,
startup/lower binding, excluded writers and incoming frame/candidate
preservation, rather than an unspecified normalization helper. Free-list
membership/extent, cached object history, physical backing and later
header-relevant effects remain separate Q-EXE-033 conditions.

## Alternatives

- A noncanonical current pair, changed lower bound, unexpected writer or
  alternate/computed entry invalidates the stated induction unless its
  publication is separately classified. The example is not an original
  game bug or a claim of such a live state.
- Free-list exact and tail-split allocations have distinct input/header
  requirements (FND-CONFIG-227/230). This result does not admit them by
  applying the fresh-request bound checks they do not execute.
- A disjoint fresh header store does not prove the wrapper's repeated
  fills, marker, release, saved pointers or callbacks preserve every other
  live object. Direction, offset/chunk progression, header mutation,
  reentry and backing retain their specific contracts.
- Computed/bulk/physical aliases, changed DS/SS, native argument effects
  and unresolved callers remain excluded. No complete-reading declaration,
  negative whole-program census, queue closure or parity result follows.

## How to reproduce

Use direct_callers.py with 1000:177C, 1000:17F2 and 1000:1831 on the
licensed installed DSUN.EXE, size 634416 and XXH3-128
e296af55ba2ecde7e77f555c90f33d0b. Retain all reported sites and the
reader's direct-call limits. Check resident_listing.py ranges
1000:16EA..1000:1702 and 1000:19A9..1000:19C9; compare terminal arguments
with FND-CONFIG-231. Read 1000:1528..1000:1582 to distinguish the low
nibble test from normalization of the returned segment.

Use the checked readMz API with loadSegment 1000 to resolve data
57E0:009E and 57E0:00A2. Read each unsigned word and check that no
relocation word interval intersects it. Reuse the eight bound/current
store classifications in FND-CONFIG-226 and the sixteen root/saved-DS
store classifications in FND-CONFIG-225; do not silently turn either
narrow census into all-writer coverage.

Compose FND-CONFIG-211/212 and FND-EXE-548/549's argument, normalization,
comparison, capture and commit contracts under the explicit invariant
conditions above. Verify the numeric intervals and the deliberately
noncanonical example independently; none needs execution of the original.
The retained direct-caller report is
GAME_DIR/analysis/heap-break-canonical-callers-20261107.txt.
