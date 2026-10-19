---
id: FND-PARTY-255
title: Overlay 204 splits a created item's word 0x10 into three request fields and a bit-7 guard inside an omitted inventory fragment
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008D0E2..0x0008D307
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:0452..1000:046E
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0004F8D2..0x0004F8DA
    kind: file-data
tool: tools/research/exec-census/overlay_listing.py and resident_listing.py at commit c8246483; Python 3.14.7, Capstone 5.0.7
environment: null
---

## Observation

This follows actual local instructions from established entry
`204 +1322`, including a reached fragment absent from its inventory
row. No original code was executed. Arguments, capacities, resource
results and native DS/SS identity remain qualified.

### Setup and item fields

The function copies eight bytes from DS:28D2 to an SS-based local
far pointer through `1000:0452`. The copy helper clears the direction
flag, copies the requested byte count using word and optional byte
repetitions, restores DS and returns while removing its two far-pointer
arguments. The four shipped source words are 1, 2, 16 and 128.

It applies a signed absolute-value operation to its first word argument
at 16-bit width. It divides its next signed doubleword by 1000 and
keeps the low words of quotient and remainder. Their shifted-by-four,
plus-eight forms are retained as coordinates. It requests
`31E0:0121` with the resulting first-word object, slot FFFF, those
coordinates, zero, six and seven.

A return FFFF jumps to final `173 +24C7` and return processing.
Another return is retained as a slot. A metadata kind other than one
skips the item-field work. For kind one it obtains the record index
and reads its full word +0x10 three times:

| Source bits | Local value and consumer |
|---|---|
| 0..2 | Word masked with seven; passed to `173 +2A5D` with doubleword six in the later per-target loop. |
| 3..4 | Unsigned shift right three, masked with three; passed to target request `173 +0000`. |
| 5..6 | Unsigned shift right five, masked with three; indexes one of the four copied words, later passed to `179 +0C7F`. |

The table copy uses a far SS destination, while the later indexed
local-table read uses a DS-based near address derived from BP. Treating
the indexed value as the shipped 1/2/16/128 therefore requires admitted
DS/SS storage correspondence and an unchanged local copy. The index
itself is locally bounded from zero through three.

The target request receives the low quotient/remainder words, -1,
the bits-3..4 value, a near local output buffer and one. Its returned
word controls signed target loops. This reading does not establish the
buffer's capacity, meaning or successful population.

### Conditional guard and bit 7

A local byte starts at one. When bit 0 of the function's following
doubleword argument is clear, it skips the guard loop and retains that
byte. Otherwise it searches the returned target entries, stepping four
bytes at a time, until a signed word at entry +2 is below four. For
larger words it advances while the signed index is below the returned
count. No selected entry leaves the local byte unchanged.

For the first selected entry it requests the near function at file
8C5CB with word 0x7FFE, doubleword two and doubleword zero. A returned
word one clears the local byte and exits the guard. Otherwise it
requests `193 +1C95` with the object word at `4C13:0369` and word 26.
Zero AX exits the guard with the byte still one. Nonzero AX then
tests the item word +0x10 with mask 0x80. Bit 7 set retains one;
clear sets zero. Both paths leave the guard immediately.

If the local byte is zero, later per-target requests are skipped.
Otherwise it requests presentation using the low byte at +0x44 of
the slot's 72-byte entry, then `206 +0986(slot, 1)`. That returned
word replaces the retained slot. For each target, it requests
`173 +2A5D` using the bits-0..2 field, retains its result in a word,
then requests `179 +0C7F` with, in order, the target entry's word +2,
the current slot, -1, the table-selected word, an SS-based far pointer
to that retained result, twice the result, word zero, doubleword zero,
word 99, word one and word 11. These are
request bindings, not established damage or visual outcomes.

The common non-FFFF path requests `31E0:05BB` using the current slot's
word +1 and three words one. Because the slot may have been replaced,
this is not unconditionally a request for the original created slot.
It then calls `173 +24C7`. The return conversion produces zero for
local byte one and one for local byte zero. FFFF creation also returns
zero because the byte began at one; that return alone is not proof
of successful creation or an applied effect.

### Inventory denominator gap

The committed row at file start 8D0E2 lists 456 body bytes in
8D0E2..8D201 and 8D25E..8D307. The conditional branch at 8D1FF
has a fall-through path into 8D201..8D25E; the loop there also branches
back to 8D201. Its 93 bytes include the bit-7 read at 8D24A.
No other current inventory row owns that interval. The reached local
span is consequently 549 bytes, while the authoritative inventory
still counts 456. This finding does not change or reconcile its
inventory provenance and does not claim a qualified complete reading.

The matching shared report is toolkit issue 477; this case was added
at https://github.com/kibertoad/refurbished-dinosaurs-toolkit/issues/477#issuecomment-6256765999.
Earlier inventory-only operand censuses exclude this fragment. A result
from those censuses must not be treated as all word-0x10 consumers.

## Interpretation

The local overlay-204 consumer uses item word +0x10 as a packed request
configuration: three bounded selectors and a conditional permission
bit. It is neither the door-flags interpretation nor overlay 189's
one-based effect selector. Q-PARTY-020's known overlay-204 sites are
answered locally, including the newly reached bit-7 site. Admitted
script inputs, target buffers, local storage and child-effect meanings
remain Q-PARTY-110; inventory ownership remains an explicit tooling gap.

## Alternatives

- Skipping the inventory hole would miss a reachable bit-7 test. It
  cannot be dismissed as data merely because the analyzer omitted it.
- The local byte is not an unconditional success flag: creation FFFF
  retains one and returns zero, like a path that reaches later requests.
- No field is named as damage count, radius or damage kind solely from
  its downstream request. Child readings and original input producers
  must establish those meanings. DS/SS, resource and target admission,
  high argument values and lifetime remain unresolved.

## How to reproduce

Use locked evidence Python and installed `<dsun>` with
`tools/research/exec-census/overlay_listing.py <dsun> 204 8D0E2 8D307`
and `resident_listing.py <dsun> 1000:0452..1000:046E`. The scripts
verify source length and XXH3. Read four little-endian words at
shipped offset 4F8D2, the MZ header size plus the DS:28D2 load-image
displacement. Compare the current inventory's two ranges and trace
the 8D1FF fall-through and guard-loop back edge through their hole.
Separate FFFF/non-kind-one, the three fields, each guard return and
local-byte result before interpreting any effect request.
