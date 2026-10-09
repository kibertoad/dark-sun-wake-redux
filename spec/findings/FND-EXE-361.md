---
id: FND-EXE-361
title: Sound utility formatting destination advances after an unchecked bounded source scan
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1000:1487..1000:14BB
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1000:3AE4..1000:3B03
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1000:30EB..1000:310F
tool: Capstone 5.0.7 and xxhash 4.0.1
environment: null
---

## Observation

FND-EXE-358's 05F5 path calls 1487 with a destination pair followed
by a source pair. With its selected all-zero inputs, the outer helper
substitutes current DS:EDEC for the destination and DS:DDC2 for the
source. These are conditional segment bindings, not admitted capacities.

The 1487 helper saves BP/SI and calls 3AE4 with the source pair,
removing four argument bytes. It stores returned AX in SI, increments AX
as a low word, and calls 30EB with destination, source and incremented
count. It removes ten bytes, reloads the destination segment/offset from
its incoming arguments and adds current SI to the offset without segment
adjustment. It restores SI/BP and far-returns without incoming cleanup.
It does not test either callee's result, allocate storage or test capacity.

The 3AE4 helper saves BP/DI and loads ES:DI from its argument. An
all-zero pair returns AX zero without scanning. Any other pair clears the
direction flag and scans for a zero byte with initial CX=FFFF. It computes
AX as the complemented remaining count minus one. It does not separately
test whether the scan found a terminator before CX reached zero. It restores
DI/BP and far-returns; ES remains the loaded source segment.

If a terminator is the Nth scanned byte, for N from one through 65535,
the computed value is N minus one. A scan exhausted without a terminator
also computes FFFE, indistinguishable here from finding a terminator at
the last scanned byte. Both paths make 1487's incremented copy count FFFF.
The scan can traverse offset wrap; it does not adjust the segment or admit
the source range. The all-zero source shortcut instead makes that copy
count one, with no local rejection of the source pair before copying.

The 30EB helper saves BP/SI/DI and retains incoming DS in DX. It loads
ES:DI from the destination and DS:SI from the source, then loads the count
word. It halves that count, clears the direction flag and copies that many
words. The carry from the halving selects a further byte copy for an odd
count; the intervening direction operation and word copies do not change
that carry. It restores DS from DX, reloads the destination pair into DX:AX,
restores DI/SI/BP and far-returns without incoming cleanup.

This local copy produces exactly the unsigned count's byte quantity on
ordinary completion, up to 65535 bytes. It tests no source/destination
capacity, overlap, offset wrap or copy result, and makes no calls or interrupts.
Its SI save/restore is the local path by which 1487 retains the scan result
for its final offset addition. Writable aliases of the frames remain unadmitted.

## Interpretation

This resolves 1487's local construction of the pair passed to the numeric
formatter in FND-EXE-359. It is the supplied destination with a low-word
offset advance, not a new allocation or a validated writable extent. In the
selected caller it follows a copy from the conditional DS:DDC2 source into
DS:EDEC before the numeric word is written at the resulting pair.

The scan limit bounds local work but does not prove a terminator exists.
Source admission must establish termination, readable extents and offset
behavior; destination admission must cover the preceding copy, subsequent
numeric output and later append, including aliases of arguments and saved
registers. A returned pair alone proves none of those conditions.

Q-EXE-007 retains the substituted storage's contents, capacities, writers,
actual DS, aliases and lifetime, other cleanup helpers and later consumers.
No complete-reading promotion, successful native operation or launch exclusion
follows. FND-EXE-359's digit bound remains separate from destination capacity.

## Alternatives

Treating the return as newly allocated storage ignores the supplied destination
and absent allocation. Treating scan exhaustion as an error ignores the unchecked
count conversion and following copy. Treating the zero-pair shortcut as a safe
no-op ignores the increment to a one-byte copy count. Treating an offset advance
as a far-pointer carry ignores the unchanged returned segment.

## How to reproduce

At revision 73e266f require FND-EXE-350's SOUND_DS.EXE identity:
length 204593, XXH3-128 236c2dc23c071eca421eb5b427caee57.
With locked Capstone 5.0.7 in sixteen-bit mode decode shipped half-open
0x00002887..0x000028BB at IP 1487,
0x00004EE4..0x00004F03 at IP 3AE4, and
0x000044EB..0x0000450F at IP 30EB, modeled CS 1000,
MZ header size 1400. Bind FND-EXE-358's ordered pairs, trace scan
exhaustion separately from a found zero, and retain the halving carry through
the word copy to the odd-byte decision. Track count cleanup, DS restoration,
SI restoration and the final low-word offset sum independently of capacity.
Original bytes and reports remain outside Git. No original process,
DOSBox, interrupt thunk or emulated call is executed.
