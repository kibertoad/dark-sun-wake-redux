---
id: FND-PARTY-235
title: The CHAR writer's chunk-header word 0x06 is the field the graph walk reached each record through, so a chunk the game writes names fields 2, 4, 5, 15, 16 or 17 and never field six
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2D40:2196..2D40:2410
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006FFBF..0x000701CA
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2D40:00C9..2D40:0300
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000704C9..0x0007054D
tool: tools/research/exec-census/resident_listing.py, overlay_listing.py, direct_callers.py, far_pointer_words.py and trampoline_target.py at commit da45e6e1; Python 3.14.7, Capstone 5.0.7, xxhash 4.0.1
environment: null
---

## Observation

FND-PARTY-233 and FND-PARTY-234 leave one computed field source for the
generic setter: the CHAR loader's call at 2D40:01F5, whose field is the
word at +06 of the current chunk header, for CHAR data the game writes
at run time. This finding reads where the writer gets that word.

### The loader's use of the word

In the chunk walk (2D40:00C9..2D40:0300) the setter call at 2D40:01F5
is reached from the type-2 handler (2D40:012F, joining at 2D40:01EB) and
the type-3 handler (2D40:018C, falling through). Both push the header's
word at +06 read through the chunk pointer at [BP-0C] as the field. The
type-1 handler (2D40:00EE) and the type-4 handler (2D40:0200) reach no
setter call, and any other type clears the data pointer at 2D40:02AF.
Nothing in the walk writes header word +06.

### Where the writer takes the word

Overlay 187 +018F (FND-PARTY-054) fills a 0x320-byte stack array through
2D40:2196 and, for each entry, calls +0303, which copies the 10-byte
entry unchanged and then the record's bytes into the output buffer
(+0317..+0345). Between the 2196 call and the copies the writer stores
only to the handle table at 4F49:0C33 (+01F9, +0217); its test at
+0237..+0246 compares the same entry word with 4 and then with 6, so it
skips no entry. The end header is the first entry again with byte 0 set
to 0xFF and the length word set to 0 (+0295..+02B5).

2D40:2196 writes entry word +06 from SI at 2D40:2295, its only store to
that word in an accepted entry; 2D40:21DF writes CX there only on the
path for a slot byte of zero or above five, which then emits no entries
and ends with the 0xFF header. SI starts as the input slot (2D40:21B0),
so the first entry, always type 1, carries the slot. After each accepted
entry the routine calls 1AA0:0566 with mode word 0 (push at 2D40:22C4),
pops a pending handle through 1AA0:051C, and at 2D40:23B7..2D40:23C3 sets
SI to the word at state offset 0x42 plus twice the decremented count,
the selector 1AA0:05DC stored for that same pending handle
(FND-CONFIG-145). With mode 0, 1AA0:0566 takes its selectors only from
the per-kind lists at DS:5ECB, skipping zero words. Overlay 188 installs
those lists (FND-PARTY-233): kind 1 gives 5, 2 and 4; kind 2 gives 15,
16, 17 and 4; kind 5 gives 2; kinds 3 and 4 give none. No list holds 6.

The chunk type follows from the same selector (2D40:21EB..2D40:2210):
type 4 when the selector's FNFO 2 type byte is 8 (field 2), else type 3
when the handle is the one in DS:60EB, which the getter returns for a
type of 10 or more (field 15, type 12; field 5, type 13), else type 2.
A handle of kind 4, which field 5 yields, is passed over unless the first
entry's kind is 5 (2D40:2329..2D40:23A0).

| Chunk type | Header word +06 in a chunk the game writes |
|---|---|
| 1 | the slot passed to the writer (not read by the load) |
| 2 | 4, 16 or 17 |
| 3 | 15, or 5 under a kind-5 first entry |
| 4 | 2 (not read by the load) |

### Other resource writes

The direct-call search finds 14 calls of the resource write routine
37FC:00E8 and no others: overlay 187 +0386 inside the chain writer above;
171 +1032 and 184 +151B with the tag CACT; 186 +01F2, +0234 and +026F
with SPST, PSST and PSIN; 187 +082A with ETAB; 187 +0A4B with STXT;
187 +1436 with RGTP; 192 +0599 and +05D1 with PREF and GREQ; 187 +09D3,
which writes memory blocks named by the far-pointer table at DS:157E
under the tag in its [BP-8], set to SAVE at +08FB; and 187 +0791 and +088F, which write one
37-byte record from DS:67BB. +0791 takes its tag from 187 +0699, which
builds it from two letters of `RDOJRGOT` and two characters that
resident routines form from a number; +088F (187 +0840) takes it from its caller, and both direct
callers, +108C and +13E8, pass the doubleword 3. None of these other
sites builds a chunk header.

The chain writer copies each 23-byte record whole, its word at +0C
included. The load's kind-1 copy (overlay 187 +04F8, FND-PARTY-051)
stores zero at +0C after copying, so the +0C word a CHAR resource holds
never reaches the loaded record.

## Interpretation

In every CHAR chunk the game writes, header word +06 is the field through
which the graph walk reached the chunk's record, taken from overlay 188's
field lists. The chunks the load passes to the setter at 2D40:01F5 (types
2 and 3) name field 4, 5, 15, 16 or 17, and the shipped chunks name 15,
16, 17 or 4 (FND-PARTY-052). Neither names field six, so the CHAR load
never writes the +0C bitmap-number cache through the generic setter.
With FND-PARTY-233 and FND-PARTY-234, the only generic accessor requests
that could still name field six are those whose field is a script operand
byte. Saving a character through this writer leaves no +0C value for a
later load: the load replaces it with zero.

## Alternatives

- Reading word +06 of a written chunk as a copy of a stored field value
  fails: 2D40:2196 writes SI there, and SI holds only the starting slot or
  a selector popped from the walk's state.
- The selector state lives in a BP-relative SS frame and 1AA0:05DC writes
  it through DS. This reading takes DS and SS as the same storage, as
  FND-PARTY-234 does; Q-PARTY-089 holds that correspondence.
- A write into the field lists at DS:5ECB after overlay 188 installs them,
  such as a raw block restored from a SAVE resource, would change the
  selectors; FND-PARTY-233's immediate search found no such store, and
  which blocks the DS:157E table names is not read here.
- A CHAR resource copied whole between archives keeps the header words
  the writer or the shipped file gave it; no direct write site builds
  chunk headers other than overlay 187 +018F.

## How to reproduce

Use installed DSUN.EXE, size 634416, XXH3-128
e296af55ba2ecde7e77f555c90f33d0b, and the locked evidence Python. List
2D40:2196..2D40:2410 and 2D40:00C9..2D40:0300 with resident_listing.py,
and overlay 187 file 6FFBF..701CA, 704C9..7054D and 7054D..70898 with
overlay_listing.py. Run direct_callers.py for 2D40:2196, 187+018F,
187+0303, 187+034E, 37FC:00E8, 187+0840, 187+071D and 187+08E3, and
far_pointer_words.py for 2D40:2196. For each 37FC:00E8 call, list the
containing overlay from 0x80 bytes before the call and read the pushed
tag doubleword. Retained listings are under
GAME_DIR/analysis/party-record-heads/char-writer-20261018/. No original
function or game process was run.
