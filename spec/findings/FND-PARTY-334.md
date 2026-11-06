---
id: FND-PARTY-334
title: Overlay cache hooks select one copy backend and expose specific header and native-transfer bindings
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4AE5:0D11..4AE5:0D27
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4AE5:09CB..4AE5:0A4B
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4AE5:0BCF..4AE5:0BFE
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4AE5:0C2E..4AE5:0D11
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4AE5:11A9..4AE5:1257
tool: tools/research/exec-census/resident_listing.py at dd0ce3fe; locked Python 3.12.10 with Capstone distribution 5.0.9; cache interfaces in FND-EXE-235/236/239/240/241/259/563/564 and notification in FND-PARTY-333
environment: null
---

## Observation

Q-PARTY-137 retains cache-hook effects during the overlay return route
in FND-PARTY-333. Existing FND-EXE-235/236 identifies the near hook
word at manager-data +80: its default is the one-byte near-return leaf
4AE5:1256; known setup writers select 4AE5:0D11. This reading follows
that dispatcher and both copy branches, retaining actual target/segment
admission rather than declaring the hook universally harmless.

### Dispatch and allocation interface

With manager DS55CE, the dispatcher first tests bit one (mask 1) of
byte +10. When set it calls the extended-memory branch at +0BCF and
returns immediately. Only when clear does it test mask two and optionally
call EMS branch +09CB. Thus both bits set select extended memory alone;
they do not cause two copies. Both clear select no copy. These are local
branch results, not a census of live flag combinations.

Both branches call +0C2E with a fixed cache-record segment, 55E1 for
extended memory or 55E2 for EMS, and the selected header in ES.
FND-EXE-239/240/241 supplies the complete allocation interface and its
chain validity, wrap arithmetic and alias conditions. The reread confirms
that its ordinary encoded return clears carry; its linked walks may fail
to terminate or encounter invalid backing, but there is no separate
ordinary carry-set capacity-rejection return in that body. Caller carry
gates do not themselves prove an admissible failed-allocation outcome.

Its own writes update the cache record's next-position pair +08/+0A
and its +0E chain head,
selected header words +14/+16/+18 and +0E, and walked-header +0E/+18
links or loader selectors. Their actual cache/header graph identities
are needed to exclude queue aliases. No game callback occurs in those
three leaf walkers. The scalar/header write set is separate from the
payload copy that follows. Wrapped size input FFFF becomes zero in
+C22; this is not an unbounded mathematical round-up (FND-EXE-240).

After allocation, both copy branches save their working registers and
header ES, call the far notification with AX1, then restore those saved
values. Under the unchanged default target, FND-PARTY-333's RETF leaf
adds no explicit data write. Changed notification pointers and aliased
save slots retain their own effects.

### EMS copy window

The EMS branch derives a logical page and initial within-page offset
by dividing its allocated position by 4000 hexadecimal. It requests
AH47 through INT67 using manager handle word +32. Its explicit failure
gate tests the carry produced by shifting returned AH left, so the
high bit of the status byte controls that gate.

For each intended chunk it forms the smaller of the remaining code bytes
and the bytes before the 4000 boundary, then requests AX4400 through
INT67 for the current logical page/handle. On its admitted continuing
status branch it reloads the physical page-frame segment from manager
word +30, sets DS from selected-header word +10, and performs forward
REP MOVSB into that page frame. Native preservation of CX, pointers,
segments and stack remains necessary: the copy uses the post-interrupt
register state, not an independently restored chunk count.

It increments the logical page, clears the destination offset and
continues while remaining bytes are nonzero. It saves its local carry
outcome around an AH48 INT67 page-map restoration request. The tested
failure paths transfer to the known 1000:02DF error route; no successful
return or unchanged partial payload is implied for them.

The payload destination is therefore a conventional-memory page-frame
window, not merely an abstract remote cache. To exclude queue/index
writes through this branch requires the actual +30 segment, handle/page
mapping, count and register binding. Fixed cache-record addresses alone
do not exclude such an alias. The overlay source is likewise the actual
selected load segment +10, not a source-file offset.

### Extended-memory copy and descriptor writes

The extended branch forms the source linear address as selected-header
load segment +10 multiplied by 16, and calls +11A9 with that source,
allocated destination and rounded count. The complete move helper saves
DS/ES, rounds the two-word count to even and clears direction, then
reloads DS through CS word five. It uses manager word +47 to choose an
XMS far call or the BIOS INT15 AH87 path (FND-EXE-259).

The XMS branch writes the count and source/destination descriptor words
at manager offsets +4D..+5D, then calls through the pointer at +43 with
AH0B. The BIOS branch instead updates its descriptor fields at +5F/+61
and +67/+69 and supplies ES:SI and a word count to INT15. Actual handle,
address encodings, callback target and native outcomes are not established
by the presence of those requests. The helper converts its native success
or failure to a carry result; the outer copy branch transfers on carry
set to the same error route. Descriptor stores and the native payload
write are distinct mechanisms with distinct backing requirements.

## Interpretation

The selected overlay hook is a no-op or one of two specific copy paths,
under its target and flag conditions. Remaining queue preservation work
names cache/header link destinations, the EMS physical page frame and
post-service registers, or the extended-memory descriptors/driver target
and destination, plus actual frame and buffer backing. It need not treat
these as unspecified game callbacks or infer two copies from both mode
bits being set.

Neither an ordinary carry-clear allocation return nor a source-level
cache target proves native copy success, valid linked metadata or queue
separation. Q-PARTY-137 retains those actual bindings, changed targets,
interrupt effects and other return roots. No complete reading, runtime
cache invocation, status promotion or queue closure is claimed.

## Alternatives

- Both enabled mode bits cause two copies: the first branch returns
  immediately after the extended-memory call.
- The allocation body's carry gate proves a normal rejection exit:
  its admitted ordinary return clears carry; linked-state failures
  and external changes must be distinguished from that result.
- EMS cache copies cannot touch conventional queue storage: the actual
  payload copy targets a physical page-frame segment and needs its bound.
- Fixed manager fields prove all payload destinations: the native move
  consumes separately bound source/destination/handle state.

## How to reproduce

Use licensed DSUN.EXE size 634416 and XXH3-128
`e296af55ba2ecde7e77f555c90f33d0b`, locked evidence Python and
`tools/research/exec-census/resident_listing.py`. Read
`4AE5:0D11..4AE5:0D27`, `4AE5:09CB..4AE5:0A4B`,
`4AE5:0BCF..4AE5:0BFE`, `4AE5:0C2E..4AE5:0D11`,
`4AE5:11A9..4AE5:1256` and `4AE5:1256..4AE5:1257`.
The existing reporter checks identity and applies relocations. Follow both
mode tests, each branch's notification/save sequence, the complete copy
loop and native result gates. Distinguish the shifted-AH gate from testing
native carry itself.

Reuse FND-EXE-239/240/241 for full cache allocation/link semantics,
FND-EXE-259 for native move descriptor arithmetic, FND-EXE-563/564 for
known setup producers, and FND-PARTY-333 for the default notification.
Retain all actual input, graph, target, memory and asynchronous conditions.
No original game ran and no original-derived listing or payload is committed.
