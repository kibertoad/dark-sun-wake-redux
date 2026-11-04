---
id: FND-COMBAT-130
title: Archive handle issuance is word-modular and its known output producers do not independently prove live handle separation
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 38FF:000E..38FF:0066
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 38FF:0256..38FF:02B5
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00068AE7..0x00068BB5
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00069D75..0x00069E27
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00067321..0x00067899
tool: tools/research/exec-census/window_operands.py, resident_listing.py and overlay_listing.py at 3bf37d55; committed DSUN.EXE function inventory
environment: null
---

## Observation

Q-COMBAT-064 retains DS:145A versus working-handle identity before
the close in FND-COMBAT-128. FND-CONFIG-067 already establishes the
numeric/far-pointer distinction. This inventories the explicit handle
and counter operands and derives the conditional issuance relation,
without claiming actual counts, live membership or absence of aliases.
Offsets below are hexadecimal unless stated otherwise.

### Controlled literal operand inventory

window_operands.py searches the committed
coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv at 3bf37d55 for three half-open
data-offset windows. Its source check is DSUN.EXE length 634416 and
XXH3-128 e296af55ba2ecde7e77f555c90f33d0b.

| Window | Listed-start operands | Separate overlapping candidates |
| --- | --- | --- |
| 144E..1452, working saved handle | 13 | 12 |
| 145A..145E, region saved handle | 16 | 15 |
| 9D8E..9D90, issuance counter | 4 | 1 |

Every separate candidate is marked inside a listed instruction; none
is admitted as another instruction or producer. Most are alternate
decodes after an operand-size prefix. The four counter operands are
the initializer's word-one store at 38FF:005F, the opener's record and
optional-output reads at 0283 and 0298, and word increment at 02A7.
All four independently read controls occur. The known handle-output
address pushes are also present: 187 +1BF8 for DS:144E and 182 +0347
for DS:145A. These are address-taking controls, not direct value stores.

The listed direct working-handle store is startup's dword-zero store
at overlay 180 +02FA, file 674DA. Other listed uses read it or push
its address for opening. Region-handle direct stores clear it at
182 +02D0, +0337, +1643 and +1981, and 187 +1DC8. Its one listed
address push is the output passed by region opening at 182 +0347.
FND-CONFIG-181 supplies that complete filename/open route: it requests
the wrapper with option 000A, K = 04D4 and output DS:145A.
FND-COMBAT-110 supplies the working opener's option 0002 and same K.

This is an encoded displacement/immediate search, not every writer of
the storage. Computed and bulk writes, a write spanning a window from
a different starting displacement, changed segment identity, unmodeled
runtime code and unresolved boundaries remain outside its conclusion.
Hit-bearing working-handle routines include string operations; their
presence is retained, not silently treated as nonaliasing. Candidate
and inventory passes do not establish complete caller reachability.

### Reset and issue sequence

After its guard, initializer 38FF:000E compares global DS:9DA7 with
its expected internal signature. Equality returns without resetting
the counter. Inequality writes the signature, clears head DS:9DA3,
active DS:9D9F, count and accounting, initializes other archive globals
and stores counter one. Its own body does not clear DS:144E or DS:145A
or close/release the formerly named records. This is its local contract,
not evidence that reinitialization occurs during a native load.

On the opener's successful publication ending, current counter word
becomes a zero-extended dword at record +28. A supplied output receives
the same counter low word and zero high word. The counter then increments
as a word, with no collision scan or zero-skip branch. There is no child
call on this ending. Valid distinct stable record/output/global storage
is required to compose these writes (FND-COMBAT-127).

Under unchanged counter state apart from those increments, if a region
handle H was issued earlier and a working handle W is issued n successful
publications later, their low words satisfy:

`W = (H + n) mod 65536`, with both high words zero.

For decimal 1 <= n <= 65535, preserved H and W are distinct. Equality
requires n divisible by 65536 under these conditions. A counter reset,
changed saved output, altered record handle, alias or asynchronous change
invalidates that inference. Merely finding two different output addresses
does not establish the conditions or actual n. The reuse branch of
FND-COMBAT-110 issues no new handle and cannot use a fresh-issue argument.

Zero is not skipped: an admitted publication while the counter is zero
writes zero to its record/output before incrementing. Region helpers'
nonnull tests would then skip their explicit close request for a preserved
zero field despite that publication. Conversely counter FFFF produces
0000FFFF, not close-all's FFFFFFFF input (FND-COMBAT-096). Neither case
is asserted to occur during a native session.

### Close and saved value have separate lifetimes

The complete region teardown 182 +1525 requests close of nonzero
DS:145A at +1559, ignores AX and has no own DS:145A clear before
return. It has other children and stores display state zero at the end;
their preservation is not assumed. This differs from FND-COMBAT-128's
two close-and-clear branches and FND-CONFIG-181's close/reopen helper.
Thus a retained numeric field does not alone prove a still-open record,
and a clear does not prove successful closure.

With a valid fresh working record retained as list head, a nonzero
equal region handle makes close's first-match resolver select that head.
Distinct preserved numeric handles exclude that particular same-record
match while its record +28 remains W. Actual handle values, graph state
and preservation are still required to choose either case; this finding
does not establish native accidental closure or a complete alias census.

## Interpretation

The generation relation now makes handle separation a checkable remaining
obligation: admit current values directly, or establish preserved issuance
provenance and a non-repeating interval without reset. Q-COMBAT-064 need
not rediscover the local generator or literal output producers. Shared
graph/counter and caller admission remains Q-COMBAT-054; actual paths,
storage, aliases, external outcomes and resource providers remain open.
No question closes or rule/parity status changes.

## Alternatives

- Distinct global locations do not prove distinct numeric values.
- A matching initializer signature prevents its own reset on that call.
- Closing a record need not clear its saved numeric handle.
- Modular issuance is not a runtime uniqueness guarantee.
- Handle FFFF and the full-width close-all sentinel are different inputs.

## How to reproduce

Use locked evidence Python and DSUN.EXE resolved through tools/game-dir.mjs.
Run window_operands.py with the named inventory and windows 144E..1452,
145A..145E and 9D8E..9D90. Check all counter and output-address controls;
keep the 28 overlapping candidates separate. Read all listed bodies from
their established starts; the opener tail is reached in FND-COMBAT-127.
Follow the signature bypass, counter reset/increment, full output widths
and teardown's returning continuation without an own saved-handle clear.
Compare FND-CONFIG-067/181 and FND-COMBAT-096/110/127/128.
Retain restore-handle-identity-operands.txt, restore-handle-identity-bodies.txt
and restore-handle-startup.txt only under
GAME_DIR/analysis/hit-incoming-20261104/. No original listing or memory
image is committed, and no game or emulated call is run.
