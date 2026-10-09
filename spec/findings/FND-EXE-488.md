---
id: FND-EXE-488
title: Sound utility input modes return row indices or transformed bytes and use an unadmitted scan sentinel
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1A7C:022A..1A7C:03D7
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1A7C:0058..1A7C:00B2
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1000:1F8C..1000:1FA5
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    kind: file-data
    offset: 0x00000C72..0x00000C7A
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    kind: file-data
    offset: 0x00000C8A..0x00000C92
tool: Capstone 5.0.7 and xxhash 4.0.1
environment: null
---

## Observation

FND-EXE-487's downstream 1A7C:022A saves BP, SI and DI and
reserves six local bytes. Incoming word one selects a byte path; every
other word selects a coordinate path. The byte path calls 1000:1F8C
and retains only AL. That helper clears current DS:DF08 and reads
DS:DF09 when the flag byte is nonzero; otherwise it supplies AX 0700
to interrupt 21. It then clears AH and returns far. Native interrupt
behavior and actual DS remain unadmitted.

The byte path substitutes 7F for a zero byte, sign-extends the retained
byte into AX and calls FND-EXE-377's 1000:0EC9 classifier. It removes
two incoming bytes and retains returned AL. It scans SI from zero while
less than current DS:18F8 signed, comparing the transformed byte with
the byte at current DS:DF3C plus low signed SI-times-nine. Equality
returns current SI immediately; otherwise SI increments and the limit
is reloaded. Exhaustion returns the transformed byte sign-extended to AX.
Thus an unmatched byte with high bit set can return a negative word,
including FFFF, also used elsewhere as a no-result value.

The coordinate path calls local 0058 and 0085 separately. Each reserves
0020 local bytes, writes word SS:BP-10 as three, and passes
SS:BP-20, SS:BP-10 and 0033 to 1000:20DF. After ten-byte
cleanup, 0058 reads BP-1C and 0085 reads BP-1A, each adds eight
at word width and returns AX. Other local-record bytes are not initialized
here. The calls do not establish an atomic sample or native result validity.
The consumer sign-extends each returned word into DX:AX, divides signed
by eight and stores the quotients as its two local coordinates.

It scans SI from zero against current DS:18F8 signed. A row uses
low signed SI-times-nine added to current DS bases DF34, DF38,
DF36 and DF3A in that order. Signed comparisons require the first
coordinate between the DF34 and DF38 words inclusive and the second
between the DF36 and DF3A words inclusive. Every field recomputes
the low product and reloads state. No capacity, high-product or wrap
check is performed.

A match saves SI in DI and assigns SI 61A8; the common increment
then makes SI 61A9. The loop still compares that value with the current
signed count. After loop termination, only SI equal to 61A9 selects
candidate processing. Other SI values call 00EA repeatedly while full AX
is nonzero, then return FFFF. Candidate processing rechecks the same
inclusive comparisons at current DI against the held coordinates; failure
returns FFFF without that readiness loop. A passing candidate calls
00EA repeatedly until full AX zero, resamples both coordinates through
0058 and 0085 and rechecks the same row bounds. Passing returns
current DI; failing returns FFFF. Native preservation and state lifetime
are needed to identify DI and the fields across those calls.

The numeric sentinel has no independent found flag. Under admitted stable
state and a positive signed count below 61A9, a match ends the scan at
that sentinel while exhaustion ends below it. Count 61A9 allows exhaustion
to reach the same sentinel without assigning DI. A count greater than
61A9 allows the post-match sentinel to re-enter the scan as an ordinary
index; a subsequent match can reset it again. These conditional cases do
not establish native reachability, table capacity or incoming DI validity.

All returns restore DI and SI, reset SP from BP, restore BP and return
far without incoming argument cleanup. Far calls at 0238 and 024F
have relocated encoded-zero segments in records 782 and 781; local
0058 and 0085 calls at 0071 and 009E use records 788 and 787.
They bind to modeled segment 1000 at load segment 1000. Other local
far-returning calls supply CS through push-CS/near-call.

## Interpretation

This follows both downstream modes without assigning device or key meanings.
Row-index results and unmatched transformed-byte results share the same word
return channel. Coordinate selection depends separately on sampled values,
signed row bounds, readiness and an unadmitted count/sentinel distinction.
Q-EXE-007 retains 00EA and 20DF effects, native inputs and preservation,
actual DS, count/table and flag producers, extents, aliases and re-entry,
other callers and the local 08E5 interface. No complete input reading,
successful native selection or launch exclusion is claimed.

## Alternatives

Treating every returned word as a row index ignores unmatched byte returns.
Treating both coordinate samples as one snapshot ignores separate calls.
Treating sentinel equality as proof of a match assumes a missing count bound.
Treating candidate rechecks as atomic ignores readiness and sampling calls
between distinct field reads.

## How to reproduce

At revision 0e27fcd require FND-EXE-350's SOUND_DS.EXE identity:
length 204593, XXH3-128 236c2dc23c071eca421eb5b427caee57.
Use Capstone 5.0.7 in sixteen-bit mode, MZ header size 1400 and
modeled load segment 1000. Decode shipped BDEA..BF97 at IP 022A
and BC18..BC72 at IP 0058, CS 1A7C; and 338C..33A5
at IP 1F8C, CS 1000. Verify opcode 98 at 024D/027D and
opcode 99 at 0287/0294/0367/0374 as sixteen-bit sign extensions.
In the relocation table at 003E with 958 records, inspect records
782/781/788/787 at call starts 0238/024F/0071/009E.
Track argument-one selection, AL versus AX returns, signed division and
inclusive field comparisons, repeated count reads, DI assignment and sentinel
61A8 plus one. Contrast stable counts 61A8, 61A9 and 61AA without
assuming those counts are admitted native inputs. Licensed bytes stay outside
Git; no original process, DOSBox or emulated call runs.
