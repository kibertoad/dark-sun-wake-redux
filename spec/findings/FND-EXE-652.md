---
id: FND-EXE-652
title: The packet writer uses at most two fixed-segment copy spans whose header disjointness does not require valid ring indices
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4464:0230..4464:02E4
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 45CD:0000..45CD:001E
tool: Pinned Python 3.12.10 and Capstone module 5.0.7 (distribution 5.0.9); resident_listing.py at eb1bb188; retained INPUT-016, EXE-591/649/651 interfaces
environment: null
---

## Observation

This resolves the local packet-copy mechanism named by FND-EXE-651 for
Q-EXE-033. It does not establish the actual input buffer's allocation,
lifetime or segment history. FND-INPUT-016 retains its queue interface;
FND-EXE-591 independently locates the normalization helper.

The writer at 4464:0230 saves registers and flags, disables maskable
interrupts and selects DS=57E0. If word 33CC is not exactly one it writes
error 000D to 33BE and returns FFFF without copying. Otherwise it calls
45CD:0000 on the incoming packet pointer and replaces its own incoming
argument words with returned AX and DX. The helper returns offset
old_offset AND 000F and segment (old_segment + (old_offset >> 4)) modulo
65536. It has no child calls and restores its saved DS, BX and BP.
Actual frame integrity and writable argument storage are conditions.

The writer reads packet length n from the normalized packet's word +2.
Let f, r, w and c be the current words 33C2, 33C4, 33C6 and 33C0.
It rejects signed16(f)<signed16(n), writing error 0801 and returning
FFFF. This is not an unsigned extent admission test.

For the accepted branch, the first count k is:

- n when signed16(r)>signed16(w);
- otherwise n if signed16(n)<signed16((c-w) modulo 65536), and
  (c-w) modulo 65536 in the other case.

At 0280 the count is retained in AX and subtracted from DX=n with
16-bit arithmetic. The destination pair is loaded afresh from DS:33C8
into ES:DI; its offset becomes (DI+BX) modulo 65536, initially BX=w.
The source pair is loaded from the modified incoming argument. CLD
immediately precedes REP MOVSB. No helper converts a destination offset
carry into a segment increment. Every destination offset increment is
16-bit; ES stays fixed throughout that REP under ordinary instruction
execution without excluded asynchronous or code/register mutation.

After the REP, the body restores DS=57E0, adds AX to its source-offset
argument and word 33C6, and subtracts AX from 33C2. It tests the retained
remainder DX. When nonzero, it clears 33C6, sets BX=0 and CX=DX and
reenters 0280. On that second pass DX becomes zero, so there is no third
copy pass under the intact execution/frame conditions. The second pass
reloads the destination pair from shared memory; it is not necessarily
the pair captured for the first pass. Each source/destination span can
wrap its offset without adjusting its segment. A zero count copies no
bytes. Final success sets AX=0 and clears 33C6 if it equals 33C0, then
restores flags/registers and returns at 02E3.

Thus the exact destination offset set of a pass with segment S, starting
offset o and count k is {(o+i) modulo 65536 : 0<=i<k}, all through ES=S.
This is contained in the full segment window [16*S,16*S+FFFF] in a
linear real-mode mapping. It remains so even when ring metadata is
inconsistent. For example f=c=0400, r=w=0 and n=FFFF pass the signed free
space test and select k=FFFF; the nominal 0400 size alone does not bound
the copy to that many bytes. This is an arithmetic example, not an
observed packet, an executed sequence or an admitted native state.

### Header and control-word separation

Reuse FND-EXE-649's modeled load 1000 and startup lower-segment bound
Ls>=632F. If each pass's captured destination segment S>=Ls, every
linear byte destination is at least 632F0. That is above both the
conservative resident header/trampoline end 57EF9 retained there and
the queue-control interval DS:33BE..33CF, linear 5B1BE..5B1CF.
On a mapping without address wrapping those copies cannot change either
region, regardless of packet length or ring-index arithmetic. To use the
same interval argument on a 20-bit wrapping mapping, additionally require
16*S+FFFF<=FFFFF (S<=F000); this is a sufficient condition, not a claim
that every native buffer satisfies it. No native A20 or load binding was
measured. Other load mappings require their own interval comparison.

For the second pass the segment condition must hold for the pair reloaded
there. The first copy cannot itself change the modeled queue pointer,
flag or controls when this window separation holds, but external writers
and actual intervening effects still need admission. The ordinary control
stores named above do not write 33C8..33CF. Own stack/argument writes,
return storage, code bytes and actual backing must also remain separated
and intact for this execution model; header disjointness is not a general
proof of valid packet access, lifetime or successful return.

If the captured buffer instead aliases a header or shared field, the
absence of a literal address in the copy instruction excludes nothing.
A first pass could then change the pair reloaded by the second. The
actual supplied/allocated pointer history remains the decisive producer
obligation, with FND-EXE-650's free-list domain distinct from the fresh
allocation path. No general all-writer negative result follows.

## Interpretation

The packet writer is now an explicit fixed-segment mechanism, not an
unspecified block-copy helper. For header exclusion its relevant inputs
are each captured segment, the address mapping and frame/code/field
preservation. Complete ring correctness, unsigned packet-length bounds
or every input consumer need not be added independently when a full
segment-window separation already excludes the protected regions.
Q-EXE-033 retains actual producer/lifetime and excluded effects. No
complete-reading declaration, queue closure or parity change follows.

## Alternatives

- The queue size bounds every copy. The signed tests and FFFF arithmetic
  example prevent that inference for unadmitted states.
- Offset wrap advances the destination segment. The destination uses
  REP MOVSB with a fixed ES and 16-bit DI, without such a carry helper.
- Both passes necessarily use one privately captured pointer. The body
  reloads shared 33C8 on each pass.
- A high linear start proves native separation. Actual wrapping/mapping
  and each captured segment must satisfy the stated comparison.
- A disjoint destination proves the whole operation valid. Source reads,
  backing, frames, code, asynchronous effects and lifetime remain separate.

## How to reproduce

Use licensed installed DSUN.EXE, size 634416 and XXH3-128
 e296af55ba2ecde7e77f555c90f33d0b. Run the committed resident_listing.py
for 4464:0230..4464:02E4 and 45CD:0000..45CD:001E. Require the
setup gate, signed free-space branch, both entries to 0280, CLD/REP,
DS reload, second-pass pointer reload and restoring far return in the
connected reading. Compare segment-window endpoints with EXE-649's
modeled bounds; do not promote its conditional allocation history.

Local listings are retained under GAME_DIR/analysis/ as
input-packet-alias-listing-20261107.txt and
input-packet-alias-helper-20261107.txt. The latter includes the restoring
suffix and normalization helper. No original-game run occurred.
