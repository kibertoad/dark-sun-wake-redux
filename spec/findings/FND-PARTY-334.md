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
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:0000..1000:0163
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:01B0..1000:0220
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:2877..1000:290A
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:2932..1000:298E
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:29AA..1000:2A07
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 15F3:06B6..15F3:0851
tool: tools/research/exec-census/resident_listing.py at dd0ce3fe and 52a64a27; registered source mapping through loaded_headers.py at 448da523; locked Python 3.12.10 with Capstone distribution 5.0.9; cache interfaces in FND-EXE-235/236/239/240/241/259/563/564 and notification in FND-PARTY-333
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

### Retained direct target-slot candidates outside the manager

FND-EXE-582 retains nine direct-store candidates outside manager CS
4AE5 for offsets 0080..0088. This connected classification reuses that
positive candidate set and FND-EXE-236's known manager writers. It is
not a new negative census: indexed, adjusted-pointer, string/block,
alternate-entry, interrupt and other excluded forms remain excluded.

| Retained sites | Meaning on the established stream | Effective segment condition |
|---|---|---|
| 1000:01DD and 01E1 | Save BX and ES returned by DOS get-vector 3506 into words 0080/0082 | Current DS after the native query; startup supplies the game data segment before its call, but native DS preservation remains a condition |
| 1000:2981 and 2987 | Publish argument count and near argument-vector pointer into 0084/0086 | DS reloaded from CS:2875, the entry DS captured by 1000:2877 |
| 1000:2A03 | Publish the constructed environment array's near root from DS:3920 into 0088 | Constructor's current DS after its allocations and restored source-copy segment; it is not forced to manager DS by this store |
| 15F3:073F, 0743 and 074B | Fill request fields with BX, ES and DI at 0084/0086/0080 | DS is set from CS at 06C0/06C2; a prior odd-byte driver call can intervene |
| 15F3:0821 | Fill the second request's low count word at 0088 | DS is set from CS at 0775/0777; prior odd-byte driver calls can intervene |

The ordinary startup call at 1000:0025 follows loading DS from the
relocated game-data literal, before manager setup. Within 01B0 the
four get-vector requests precede installing the interrupt-zero vector.
There is no own DS assignment before the two retained stores; saving
DS on entry restores it only at exit, not after each native query.
Therefore these stores are not an unconditional manager-hook update,
but their native segment condition cannot be erased by the startup label.

The argument constructor explicitly captures incoming DS in CS:2875.
After its parsing/copy branches it reloads that captured segment before
the two publication stores. FND-RNG-066 binds its known startup-record
entry; FND-RNG-019 supplies the separate environment constructor and
allocation interfaces. A changed capture slot, wrong incoming segment,
changed allocator effects or bypass entry retains its own condition.
FND-RNG-020's historical constructor observations are not new runtime
admission under current pins.

For either 15F3 routine, a retained nonzero even count reaches the bulk
stores without an intervening child call after setting DS from CS. Under
that normal mapped entry and unchanged segment state, these stores
therefore target its self-segment request records, disjoint from manager
55CE at the same offsets. Zero count skips the bulk stores. On an odd
count, the documented staging branches can call through DS:00B8 first;
there is no DS reload before the later bulk stores. A final POP DS at
routine exit does not prove the segment used by those intermediate
stores. Driver DS and returned count/control state remain required for
those branches. FND-EXE-397 supplies the complete staging behavior.

All nine retained candidates now have a named destination role and
segment condition. This removes their unexplained equal-offset status;
it does not prove global absence of manager-slot writers. Native/driver
segment changes, different incoming or captured DS, alternate execution
and the original census's unsearched mechanisms remain Q-PARTY-137.
The measured default notification in FND-PARTY-335 is a point binding,
not proof that these conditions hold throughout the first handoff.

### Scalar cache metadata has a finite separation contract

FND-EXE-239/240/241 identifies the allocator's own scalar destinations:
words +08/+0A/+0E of the selected fixed cache record, and words
+0E/+14/+16/+18 of the supplied or walked overlay headers. FMT-EXE-003
registers all 49 source header segments, from 565C through 57DD.
The existing fingerprint-checked registered mapping confirms that full
set; it does not infer header registration from a live link value.

For these scalar writes, the conservative header-field envelope is
565CE..57DEE, covering offsets +0E through +1D in every registered
header. The two fixed sixteen-byte cache records have the combined
envelope 55E10..55E30. The queue/kind span is 4DA10..4DA70.
All ranges are half-open. Both metadata envelopes are strictly above
the queue and below the beginning of the SS window when SS=57E0.
No choice of a saved word's offset within that admitted SS window can
make one of these scalar destinations its storage.

Uniform relocation preserves these comparisons while the segment and
physical-address ranges do not wrap. For the observed delta -0A46:

| Region | Runtime linear envelope |
|---|---|
| Registered-header cache scalar fields | 4C16E..4D98E |
| Fixed cache records | 4B9B0..4B9D0 |
| Queue and dispatch kinds | 435B0..43610 |
| Observed SS window | 4D9A0..5D9A0 |

This whole-set exclusion does not require a particular cache-chain order
or every node to have the same contents. It requires that each reached
header destination belongs to the registered set, the supplied header is
registered, the cache record retains its fixed segment, and SS/queue
retain the mapped identities. Own relinking among those headers and the
fixed record keeps the scalar destination class inside these envelopes.
A cycle can prevent return without making an in-class scalar store an
alias of the queue or stack. A null/foreign header used as a writable
node, a changed segment or an adjusted out-of-class destination cannot
use this exclusion.

In particular, under those input and no-interference conditions the
allocator's own scalar writes cannot overwrite its saved BP at entry
SS:(S-4), its saved DS, or the handler/runner's saved words. The allocator
and its three direct children contain no native request or external
callback; native/notification activity in the outer EMS routine occurs
after allocator return. This closes the own-scalar-write alias case for
that admitted registered graph, without proving graph termination or
actual reflection-time registration and segment preservation.

The frame walker's SS-relative stores/exchange, trampoline string writes,
code moves/fixups, DOS reads, EMS page copies and external/asynchronous
writers are different mechanisms. They are not bounded by these metadata
envelopes. Retain their actual destination/count/frame conditions rather
than reusing a generic cache-metadata warning for this classified case.

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

For the retained outside-manager candidates, reuse FND-EXE-582's nine
positive sites and FND-EXE-236's manager controls rather than count a
new negative census. With resident_listing.py at 52a64a27 read the six
additional resident ranges listed above. Use FND-EXE-397 and
FND-RNG-019/066 for their connected request/constructor entry context.
Distinguish entry DS, an explicit later reload, and DS after a reached
native or driver call. No original game ran for this classification.

For the scalar separation result, use FMT-EXE-003's full registered
header set and the fingerprint-checked registered() mapping in
loaded_headers.py at 448da523. Take the minimum header segment times
16 plus 0E and maximum times 16 plus 1E, then compare the fixed cache,
queue and admitted SS intervals above. Apply one common relocation
delta to segment identities, not to offsets. Reuse FND-EXE-239/240/241
for the complete own scalar-write set. No new original run, byte-pattern
negative search or assumed native target is involved.
