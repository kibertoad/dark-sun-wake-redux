---
id: FND-PARTY-303
title: Cursor SI producers reduce to exact last-cast/list words and an unrefreshed non-list-mode register path
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0005A051..0x0005A27A
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00096F04..0x00097541
tool: overlay_listing.py at 4a1ae4eb, Python 3.12.10, Capstone 5.0.9, xxhash 4.0.1; Ghidra 12.1.3 read-only INST-handler-20261106 with packaged ReportDecompileMatches.java; FND-PARTY-243/279/280/302 and FND-RNG-095
environment: null
---

## Observation

FND-PARTY-302 leaves two retained-SI inputs among the cursor setup's
six known callers: 172+0E45 and 211+173C. This binds both inputs to
their actual source words or ambient register path. Offsets are hex;
target resource number 1001 and numeric domains below are decimal.
The installed DSUN.EXE identity is unchanged from FND-PARTY-301.

### Last-cast selection

On the selected event-five branch, 172+0C21 forms a word index by
subtracting 2BCD from the control argument. At +0C72..+0C7F it
loads SI through current DS at the word-wrapped address
445E + 22 times current member DS:445C + 2 times that index.
A zero word exits; the other path decrements SI at +0C8A.
The eventual +0E45 setup call pushes that retained SI at +0E3F.

At the decrement itself, the result is positive 1001 exactly when
the loaded word is 03EA (1002 decimal). This is a word-width
subtraction; it does not introduce a new selector from the member
or control arithmetic. There is no own bound on the control-derived
index before the source load on this path. Known six-entry table
publications alone do not admit every possible effective source word.

FND-PARTY-243 records the known history: 190+2503 shifts five entries
and stores its retained number plus one; 190+26F8 clears six entries.
The 172 path repeats an existing value rather than inventing a new
number. An unchanged known source history excluding 1001 therefore
excludes 1002 from these encoded publications. Actual table backing,
initial/restored state, member/index admission and wider/copied/aliased
writers still require evidence; the literal writer census is bounded.
Calls intervene between the decrement and setup, so actual retention
of SI and the outgoing frame stays Q-PARTY-136.

### List-window selection

211+10F4 subtracts 2BCD from its control argument in DI. It first
requires signed DS:9D79 above zero, then signed DI below that word.
It does not require DI nonnegative. The current member comes from
4E71:0B44 and selects the mode byte DS:43E8+member.
The source equations below require the loaded SI to survive reached
helpers through the later add/use; Q-PARTY-136 retains that obligation.

| Mode seen by the own selector branch | Reaching number production | Exact source needed for 1001 at that production |
|---|---|---|
| 1 or 2 | SI loaded from word DS:(9C20+2*DI) at +11FB | Word 03E9 |
| 3 | Same word load at +12CF, then word addition of 00EB at +13BC | Word 02FE (766 decimal) |
| Any other mode | No own SI initialization on that branch | Current incoming SI must already be 03E9, or be changed later |

The mode-one path joins the mode-two word load after its presentation
work. The positive 1001 equations use the actual word-sized load/add,
not a level result. The selected route pushes current SI into setup
at +172E/+173C. Other reached helper effects can change it before
that push; those effects remain distinct from its intrinsic production.

FND-PARTY-280's ordinary list-builder writes are below 256, even when
the selected endpoint bytes differ from the shipped level table.
FND-PARTY-243's discipline builder also emits byte-bounded indices.
For a valid emitted entry retained until the load, neither word 03E9
nor 02FE is available. Adding 235 to any such byte-bounded index
is at most 490, without word wrap. Canonical discipline entries give
the narrower DATA domain 235..268. This excludes those known emitted
values, not stale/unwritten or out-of-range reads into another word.

In particular, the signed index test can admit a negative DI. Its
word shift and address wrap need not address an emitted list entry.
Actual registered control delivery and list/count/segment identity
must be admitted before applying the emitted-value exclusion.

### Fresh non-list-mode setup cannot enter this handler body

FND-PARTY-279 reads 211+1A4F as initializing its count to zero and
calling a builder only for modes one, two or three. Its other own
mode paths retain zero for publication to DS:9D79. If that same
member/mode and published count remain current at +10F4, the signed
positive-count gate exits before SI can be used. Thus an arbitrary
incoming SI on a non-list-mode path matters only after an unrefreshed,
changed or differently bound mode/member/count state admits entry.
The code at +10F4 alone does not prove those states impossible.

## Interpretation

The two SI families no longer need an unspecified number-source reading.
Their remaining admission is precise: a last-cast effective source word
1002; list source words 1001 or 766 outside the retained emitted-value
domain; or a positive-count non-list-mode state with an appropriate
ambient/changed SI. Their known producer chains exclude these values
under the stated source/index/freshness conditions.

Q-PARTY-134 retains those actual source and mode/count histories plus
alternate incoming transfers. Q-PARTY-136 retains intermediate SI,
frame and producer-to-publication preservation. This does not establish
all list/table writers or the event router's control domain. It does
not remove an entire getter site, close a queue item or declare a
complete reading. FMT-PARTY-007 remains supported.

## Alternatives

- Fresh mode-zero setup with count zero exits, even if incoming SI is
  1001. A mode-zero/positive-count combination needs separate admission.
- A signed upper bound alone does not establish a valid list index.
- A repeated last-cast word can preserve an earlier exceptional input;
  repeating it is not a new arithmetic producer or proof of valid history.

## How to reproduce

Use overlay_listing.py for descriptor 172 file 5A051..5A27A and
descriptor 211 file 96F04..97541. Verify the source loads, word
decrement/add, both signed count/index branches and all own SI writes
on the selected mode routes. Reuse FND-PARTY-243's publication map
and FND-PARTY-279/280's exact count/builder contracts.

In the read-only Ghidra project, bounded literal matches for 0x2bcd
at fbov211::0a1d04 recover the index/count gate; the same query at
fbov172::064e51 misses its known control and supplies no negative
proof. Original instruction checks decide widths and branch signedness.
Reports cursor-si-lastcast, cursor-si-list and cursor-si-ghidra logs
remain in GAME_DIR/analysis/handler-census-20261106/. No original
game, emulator session or emulated call ran.
