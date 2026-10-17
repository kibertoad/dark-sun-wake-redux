---
id: FND-PARTY-220
title: The inventory recipient-space check selects the first unused position from 14 through 25
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00074F09..0x00074F34
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00075099..0x000751CB
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x00075508..0x0007552C
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x00075554..0x00075560
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007C8A3..0x0007C93B
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:323F..1000:3280
tool: tools/research/exec-census/overlay_listing.py, resident_listing.py and trampoline_target.py; tools/evidence/report.mjs table; Python 3.12.10, Capstone 5.0.7, xxhash 4.0.1
environment: null
---

## Observation

FND-PARTY-217 identifies the inventory opener's handler as descriptor
189 +0559, file 74F09. This entry decrements its event word, accepts
unsigned indices zero through five, doubles the index and jumps through
six code-segment targets at +0BA4, file 75554. Event words one through
six select +0B4E, +06E9, +0B44, +0B44, +062A and +0584 respectively.
Other words reach the common +0B44 continuation. This is a finite
selector reading, not a native input-device identification.

### Character-recipient branch

Event two reaches +06E9. After a conditional external call, it searches
nine word keys at +0B58 and jumps through their paired word targets,
eighteen bytes after each key. Keys 11300..11303 all select +0725;
11304 selects +0B44, and 13300, 13302, 13303 and 13304 select
+0A21, +0968, +0A91 and +0AAE. Unmatched keys take +0AC2.
FND-UI-030 independently binds 11300..11303 to the four character
boxes in WIND 13500. The other targets are not assigned new meanings
by this reading.

The +0725 path has an earlier special branch for nonzero DS:43F7
with an unsigned word at SS:BP+18 below eight. That branch calls
descriptor 193 +2290 and later joins another continuation. The
ordinary recipient-space route below does not establish that branch's
effects or its native input meaning.

For the later route, let recipient be the low word of the control key
minus 11300. When recipient equals current 4E71:0B44 and the unsigned
BP+18 word is below eight, it returns through the common continuation.
Otherwise it calls 572F:0025. A nonzero AL, recipient different from
current 4E71:0B44, a nonzero word +0E in recipient's 66-byte details
record through DS:19C5, and unsigned BP+18 at least eight reach the
space check. Other outcomes take the separate +081B route. These gates
do not themselves identify the original device/button contract.

The space call passes recipient to 572F:003E, resolved as descriptor
191 +06B3, file 7C8A3. AX equal to FFFF calls the message window with
the short diagnostic "BACKPACK FULL". Any other AX calls 572F:0048,
resolved as descriptor 191 +074B. The branch does not pass the earlier
space value to +074B; that callee makes its own later space request
(FND-PARTY-221). This first check does not reserve a slot.

### Local space selection

191 +06B3 allocates 812 local bytes: twelve occupancy bytes followed
by an 800-byte graph buffer. It passes the near occupancy offset,
value zero and count twelve to 1000:3265, then passes the object
argument and the graph's SS far address to 2D40:2196. It keeps the
returned count in DX.

From graph index zero, while the signed index is below that count,
it processes only entries whose kind word +2 equals one. Their record
word +4 selects a 23-byte record through DS:19C1. Sign-extend byte
+13 hexadecimal from that record. Values 14 through 25 inclusive
store byte one at occupancy[position minus 14]; every other signed
byte is ignored by this local marker. Repeated records at one position
mark the same byte; they do not consume additional positions. This
loop contains no child calls.

It then scans occupancy bytes from index zero through eleven and
returns the first index whose byte is zero plus fourteen. If all twelve
are nonzero, it returns FFFF. A nonpositive signed graph count skips
the marking loop; there is no separate rejection for that count.

The near fill wrapper 1000:3265 calls 1000:323F with the offset,
count and byte value. The primitive sets ES from incoming DS, clears
the direction flag, fills forward by an optional leading byte, words
and a trailing byte, and restores DI. It does not turn the near offset
into an SS far pointer. Its ES remains the selected DS and its
direction flag remains clear on return.

Consequently the intended twelve-byte occupancy clear and subsequent
SS-based scan need DS/SS identity and admitted distinct storage. The
graph's near indexed reads likewise use DS even though the collector
received an SS far destination. The primitive's own clear direction
does not establish all earlier or later segment and storage contracts.

## Interpretation

The code directly associates the twelve position codes 14..25 with
recipient backpack capacity on the reviewed inventory route. Combined
with FND-PARTY-217's position-plus-four projection, these codes map to
logical draw indices 18..29. This does not assign each code a physical
rectangle or establish the meanings of positions outside this range.

This answers the local first-free capacity scan part of Q-PARTY-085.
All callers, record and graph admission, field/pointer producers,
aliases, mode/device semantics, later writes and final presentation
remain open. The full inventory event handler was not read and no
original function or native input was executed.

## Alternatives

- A count of twelve graph entries is not the capacity test: only
  kind-one entries with positions in the twelve-code interval mark
  occupancy, and duplicates at one position mark one byte.
- Treating the earlier precheck as a reserved position omits +074B's
  independent later query and intervening children.
- Inferring a right-button event solely from the threshold eight adds
  a device interpretation not established by the event-delivery reading.
- Calling a BP-derived near pointer an SS write omits the fill
  primitive's explicit ES selection from DS.

## How to reproduce

Use installed DSUN.EXE, size 634416, XXH3-128
e296af55ba2ecde7e77f555c90f33d0b, and the locked evidence Python.
Read overlay_listing.py descriptor 189, 74F09..74F34 and
75099..751CB; descriptor 191, 7C8A3..7C93B; and resident_listing.py
1000:323F..1000:3280. All ranges are hexadecimal and half-open.
Resolve 572F:003E and 572F:0048 with trampoline_target.py.

Use report.mjs table with the same source/hash and width-two fields
at offset zero. Read start 75554, count/limit six, stride two for
event targets; start 75508, count/limit nine, stride two for control
keys; and start 7551A with the same nine-word layout for targets.
The decrement/unsigned-five guard bounds the first table. The second
selector explicitly loads nine into CX, advances two and uses LOOP.
Use the independently identified character controls as positive
controls. Preserve the earlier special branch, unmatched control
route and full-versus-free result branches as distinct cases.

Follow the space helper from its declared entry and inspect the signed
position/count comparisons, duplicate marker writes and first-zero
scan. Apply the graph contract in FND-PARTY-218, not an assumed
successful collector. Reports and query configurations remain under
GAME_DIR/analysis/party-record-heads/transfer-20261018/ and
GAME_DIR/analysis/party-record-heads/traversal-20261018/.
