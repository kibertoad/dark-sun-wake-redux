---
id: FND-EXE-407
title: Manager table writers call mutable slot targets and retain an unbounded stored-link traversal
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1425:0591..1425:060B
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1425:076B..1425:08E4
tool: Capstone 5.0.7, Python struct and xxhash 4.0.1
environment: null
---

## Observation

FND-EXE-406 calls 0591 with words one and zero and calls 076B
with a quantity difference, slot index and current DS:00C6 far pointer.
Both helpers save BP, SI and DI; neither establishes DS locally.

### Indexed callback helper

0591 takes its incoming word into SI and forms SI times 0108
at word width. It reads current DS word indexed from 39C6, shifts
that word left two and reads another word indexed from 39CC. There
is no local input-index or first-table-index bound. From the second word
it derives DI as its high four bits and local BP-2 as its
low twelve bits. Thus DI is locally zero through fifteen, independently
of the unbounded first lookup and storage validity.

It reads DS word indexed from 39C8 with the original SI stride,
shifts it left eight at word width, and stores it at BP-4.
It pushes word 0100, that shifted word, the low-twelve-bit word,
current DS, the offset 39CC plus SI times 0108, and the current
slot argument at DI times fourteen plus 3F4E. It calls the far
pair at the same slot's 3F46/3F48 and removes twelve outgoing bytes.
No null-target or validity test qualifies that call.

Returned AX is copied into DX and tested whole. Nonzero skips the
store; zero writes zero at current DS:39C4 plus current SI times
0108. The helper does not locally save SI or DS around the target,
so a returning target can alter the indexed destination. It then returns
DX in AX, restores saved DI/SI and discards the local frame. The
clear after a zero result is not an unchanged-segment or successful-native-effect
contract. The full body is 122 bytes and 48 instructions.

### Table-producing helper

076B's incoming far pointer is at SS:BP+6, slot word at +0A
and quantity word at +0C. It starts CX zero, derives local BP-2
as the slot word shifted left twelve at word width, and starts DI
zero. If the pointed-to word is zero, it writes FFFF to five
39C6 fields with stride 0108, sets DS:3ACE one, 3AD0 zero
and 3AD2 two, and writes FFFF to indexed 3AD6 words two
through 63. It seeds the first two 3AD4/3AD6 pairs with the
shifted slot value and its word-width successor, each with second word zero,
then sets DI two. This prefix occurs before comparing DI with quantity:
quantities zero or one do not suppress the two seed pairs or initialization.

It obtains SI from current DS:3AD2 masked to six bits. While
unsigned DI is below the currently read incoming quantity, it writes the
shifted slot value plus DI to SI-indexed 3AD4 and one to
SI-indexed 3AD6, then increments SI and DS:3AD2. Each table
offset is formed at word width. Quantity is re-read, not captured as
an invariant count across callback effects.

When the incremented 3AD2 has low six bits zero, value 0040
additionally clears 39C4, copies 3ACE/3AD0 into 39C6/39C8,
sets 39CA one, and copies 64 pairs of words from indexed
3AD4/3AD6 to 39CC/39CE. Every such boundary clears SI,
calls 0591 with one, removes the outgoing word and copies returned
AX into CX. Nonzero CX returns immediately, without the final pointed-to
quantity addition. Earlier writes remain; no local rollback occurs.

A zero callback result clears 64 3AD6 words to FFFF. If
3AD0 is unsigned below 003F it increments that word. Otherwise it
sets 3AD0 zero and begins a stored-link walk with DX one.
The walk examines DS:39CE plus DX times four. A nonzero word
becomes the next DX and repeats; zero ends the walk. There is
no local cycle detection, maximum index or iteration bound. A cycle among
nonzero links therefore has no locally selected exit. Validity and termination
require evidence about writers and admitted storage, not the sixty-four-entry
copy's bound.

After finding zero, it stores DX plus one into that indexed link,
clears the next indexed link and increments 3ACE, using word arithmetic.
It then increments DI and re-tests the current quantity. With stable
quantity, preserved DI and terminating callbacks/link walks, the ordinary writes
use DI zero..quantity-1 for nonzero initial pointed-to word, or DI
two..quantity-1 after the zero prefix. Those conditional indices do not
bound prefix outputs, link traversal or native effects.

On ordinary completion it reloads the incoming far pointer, reads the
current incoming quantity and adds it to the pointed-to word at word
width. It returns current CX in AX, restores DI/SI and discards
the frame. Aliases can affect quantity, local saves and current DS fields;
the returning callback's zero result does not admit those inputs. The full
body is 377 bytes and 131 instructions, ending at 08E3.

## Interpretation

This supplies the two direct manager helper bodies and writers of the
packed lookup fields and links consumed by 0591. Q-EXE-007 retains
complete callers/writers, DS and input provenance, aliases/extents, slot-target
preservation and native effects. No unconditional quantity-loop, link-walk or
output bound is established, and no complete reading is declared.

## Alternatives

Bounding the first lookup by the derived slot's four bits reverses the
order of operations. Treating zero callback results as preserved SI/DS adds
an unsaved contract. Skipping the seed prefix for small quantities changes
the observed order. Treating callback failure as rollback removes already-issued
writes. A sixty-four-word table copy does not imply a sixty-four-step stored-link
walk; the latter follows unbounded current link words.

## How to reproduce

At revision ccecc948 require installed DSUN.EXE length 634416 and
XXH3-128 e296af55ba2ecde7e77f555c90f33d0b. With header 0x5200,
relative segment 0x0425 and modeled segment 0x1425, decode both
Locations spans in sixteen-bit mode. Require 122/377-byte coverage and
48/131 instructions. Follow all shifts, multiplication widths, lookup producers,
paired targets, outgoing arguments, full-word results and conditional stores.
For 076B distinguish the zero-pointer-word prefix from the quantity test,
the 0040 boundary from later multiples, early failure from ordinary addition,
and fixed table-copy loops from the stored-link walk. Use FND-EXE-406
for caller bindings and FND-EXE-391/395/397/399 for published targets.
Retain preservation and storage gaps; no negative whole-program writer claim
is made. Licensed bytes remain outside Git; no original execution occurs.
