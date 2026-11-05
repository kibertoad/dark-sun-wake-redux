---
id: FND-ITEM-079
title: Later unload cleanup removes linked flagged rows before kind-specific movement and the final cleanup request
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 31E0:07CE..31E0:0AD1
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 31E0:05C3..31E0:05DB
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 31E0:0697..31E0:06B1
tool: tools/research/exec-census/resident_listing.py at d21a4b93; Python 3.12.10, Capstone 5.0.7; composition with FND-ITEM-073/076/077/078 and FND-PARTY-213
environment: null
---

## Observation

Q-PARTY-116 retains later unload cleanup after FND-ITEM-078's entry and
flagged residual gate. This reads the remaining body's own loops, stores
and requests, without treating child calls as harmless or declaring a
complete release. Addresses, fields and masks are hexadecimal; record
strides/counts are decimal.

### Optional additional row removal

For saved slot s nonnegative, 07CE replaces its local flag with object
record byte zero AND 02. If nonzero, it scans the current counted
eight-byte placement prefix from index zero, matching row flag bit 20
and slot word +6 equal to s. Nonmatches advance index and pointer.

Each match optionally prepares redraw state when the third argument
byte is nonzero. This includes the DS:143F presentation-child gate and
clear, then geometry derived from the current row/object. The two
conditional cell clears use the effect index saved earlier at BP-16,
not a newly selected effect index per matched row. That index starts
FFFF at entry and, for nonnegative s, is set to unsigned object byte +0F
minus one; FFFF skips these cell calls. FND-ITEM-076 supplies the leaf
contracts, while actual effect/map/row state remains an admission condition.

For a matching row whose slot word is nonnegative it updates the two
object neighbor links as in the initial removal, gated by their FFFF
sentinels. Then it requests 289B:0094 with the current index and the same
table/count/presentation arguments as FND-ITEM-073, rereads count/base
and clears the new boundary row's flag byte. A nonzero third byte also
requests 31E0:128C afterward with the prepared bounds and fourth argument
as page. FND-ITEM-077 retains that child's own gates and external effects.

The matching branch does not advance its index or saved row pointer,
so it examines the replacement after compaction. It has no ten-removal
cap, unlike 371D:050F. Under retained valid table state, successful local
compaction and no intervening insertion/writer, each match reduces count;
the original finite counted prefix bounds this pass. This does not prove
native child preservation or termination when children change that state.

### Kind-specific continuation

09A8 returns directly for negative saved s. Otherwise it reads the slot
kind byte at 4F49:0C33+3*s, with no own upper slot bound, then:

- Kind one requests 56BD:008E with object word +1B. A nonzero AL requests
  56BD:0093(s,0). Both the word's producer and child effects remain open.
- Kind two requests 25AF:071F(s) when signed s<4. If object byte zero
  has bit 20 it also calls 31E0:1808(s), then rereads object word +16.
  Positive values other than 00FF enter the frame walk below. It then
  requests 2D40:3388 with s, its two coordinate words shifted unsigned
  right four and the embedded diagnostic arguments.
- Other kind values skip those branches.

All of these continuing paths reach the common 0AC9 call to 31E0:044C
with s, the low second/third argument bytes and the fourth word. The
negative-slot return and FND-ITEM-078's positive flagged residual exit
are separate routes that do not reach this own call. FND-PARTY-213
already establishes further DS:265B and child gates before deletion and
release in 044C; reaching 0AC9 alone is not successful release.

### Frame walk does not have an independent bound

The kind-two frame walk indexes the descriptor at DS:657A+9*object_word_16
and its far frame pointer with eight times the unsigned object byte +18.
It first tests frame byte +2 bit 02. While clear, it increments that
object byte at byte width, fetches the next frame, sign-extends its bytes
+3/+4 and adds them to object coordinate words +0A/+0C at word width.
It then tests the current frame's bit again. A terminating frame reached
by this advance contributes its offsets before the loop stops; a frame
already marked at the initial test contributes none.

There is no own frame-count or byte-wrap test. Valid descriptor/frame
spans, a reachable terminal bit and child/resource preservation are
requirements, not inferred from a positive descriptor index. The local
coordinate additions can wrap. No native frame walk or object movement
was executed in this batch.

## Interpretation

This removes later unload's own additional-row scan, compaction/redraw
ordering, kind dispatch, frame-walk arithmetic and common cleanup-call
argument binding from Q-PARTY-116. Native row/slot/neighbor/metadata/frame
identity, frame termination, graphics/movement/script children and 044C's
complete effects remain. The local body still does not establish that
an earlier constructed row was counted or retained until this operation.

## Alternatives

Applying the outer removal's cap of ten to this pass omits its different
loop. Recomputing an effect index from each matched row invents a writer
absent here. Assuming a positive animation index guarantees termination
ignores frame and byte-wrap admission. A final cleanup request is not
proof of freed game storage or a party-slot change. No native defect,
complete reading or parity promotion is claimed.

## How to reproduce

Use approved DSUN.EXE length 634416, XXH3-128
e296af55ba2ecde7e77f555c90f33d0b. Run resident_listing.py with -I and
the three location ranges above in order. Under
GAME_DIR/analysis/item-removal-20261104/, effect-unload-later-contract.txt
and effect-unload-later-contract-repeat.txt both have SHA-256
9343eaa06728f09bcd8c160af0425e059bc845cb4d264752cb69387b8bc6372e.
Check the saved effect-index lifetime, match/nonmatch successors, kind
gates, initial versus advanced frame offsets and final argument widths
separately. Keep original listings and memory only in local approved stores.
