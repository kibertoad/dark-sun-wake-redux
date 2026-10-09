---
id: FND-EXE-368
title: Sound utility alternate buffer path preserves distinct retained segments across field combinations
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1000:19FB..1000:1A95
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1000:1A95..1000:1ACC
tool: Capstone 5.0.7 and xxhash 4.0.1
environment: null
---

## Observation

FND-EXE-367's nonzero incoming segment different from CS:198E
calls 19FB with that segment in DX. This body loads DS from DX and
pushes DS. It loads ES from the current segment's word two, clears
current DS:0002 and writes ES to current DS:0008. These stores occur
before its branch tests. No incoming offset or field extent is checked.

For this description P names the initial DX segment and R the word-two
segment loaded into ES. P equal to CS:198C or a nonzero R:0002 calls
1A95 with DS=P and leaves the pushed P segment in place. Otherwise
it reads P:0000 into AX, pops the retained P into BX and pushes R
instead. It adds AX to R:0000 at word width, retains R in CX, adds
AX to DX at word width and loads ES from that resulting segment.
If this segment's word two is zero it writes R to word eight; otherwise
it writes R to word two. Both routes join the common suffix with different
possible segments retained on the stack.

The common suffix pops the retained segment into ES, adds its current
word-zero value to that segment at word width and loads DS from the sum.
A nonzero word two in this newly loaded segment near-returns immediately.
For zero word two it reads that segment's word zero and adds it to the
retained segment's word zero. It then computes a new ES from current DS
plus the word zero read there again, and writes the retained segment to
this ES:0002. The second read follows the preceding field update; it must
not be replaced by a cached value without establishing aliases.

This zero-word path falls through into 1A6C rather than near-calling it.
FND-EXE-367 reads that link-adjustment body and both of its near-return
suffixes. The fall-through therefore uses 19FB's existing return address;
there is no extra call-frame cleanup on this path. Segment additions throughout
this body wrap at word width and have no local validity or extent checks.

The 1A95 helper reads CS:1990 into AX. Zero publishes current DS to
CS:1990 and writes current DS into its own words four and six, then
near-returns. A nonzero word saves incoming SS in BX, pushes flags,
disables maskable interrupts and loads SS from AX. It reads that selected
segment's word six through an explicit SS access into ES and replaces
the same word with current DS. It writes the temporary SS value into
current DS:0004, restores SS from BX and only then pops saved flags.
It finally writes current DS to ES:0004 and ES to current DS:0006,
then near-returns.

The temporary SS:0006 access is a field in the selected segment, not
an established reference to the caller's ordinary stack. SP is not adjusted
in this helper, and no call or stack pop occurs while SS carries that
selected segment. Its saved-flags pop occurs after SS restoration. This
instruction order does not establish interrupt, exception or hardware outcomes.

The alternate body, its fall-through and 1A95 make no native interrupt
request. They do not assign a normalized AX result. FND-EXE-367's outer
wrapper later reloads DS from shared CS:1992 and ignores the result;
its re-entry and shared-slot admission remain separate obligations.

## Interpretation

This resolves the local alternate body left open by FND-EXE-367.
Its field combinations, retained-segment replacement and later segment
calculation must be read in execution order. The segment used by the common
suffix is not necessarily the original incoming segment, and later field reads
are not necessarily the values read before the preceding writes.

The local SS switch supplies a bounded field-access sequence, not proof
that DS equals the caller's stack segment or that the updates are atomic
under every runtime condition. No valid topology, allocation unit, successful
release or unchanged-state return is established from these local stores.

Q-EXE-007 retains segment/field producers, shared CS-state writers,
extents, aliases, lifetime and re-entry admission, and the separate matching
path's terminal 1E34 contract. No complete-reading promotion or launch
exclusion follows.

## Alternatives

Treating the popped segment as always P ignores the branch replacing it with R.
Treating the common suffix as using original field values ignores its intervening
writes and later reads. Treating the fall-through as another call invents a return
frame. Treating temporary SS fields as caller-stack fields confuses the segment
binding; treating interrupt disabling as complete runtime corroboration goes
beyond the instruction sequence recorded here.

## How to reproduce

At revision 14a182f require FND-EXE-350's SOUND_DS.EXE identity:
length 204593, XXH3-128 236c2dc23c071eca421eb5b427caee57.
With locked Capstone 5.0.7 in sixteen-bit mode decode shipped half-open
0x00002DFB..0x00002E95 at IP 19FB and
0x00002E95..0x00002ECC at IP 1A95, modeled CS 1000,
MZ header size 1400. Bind FND-EXE-367's incoming DX and shared
state, track the pushed/replaced segment on each branch and each DS/ES
load before its field accesses. Preserve the later reread after field combination.
Distinguish fall-through from call and explicit SS accesses from default DS
accesses. Track SS restoration before the saved-flags pop without executing it.
Original bytes and reports remain outside Git. No original process,
DOSBox, interrupt thunk or emulated call is executed.
