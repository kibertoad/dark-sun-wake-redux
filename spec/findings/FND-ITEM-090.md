---
id: FND-ITEM-090
title: Bitmap backing transfers split word counts across linked blocks and return the first error without rollback
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1425:1954..1425:1B0B
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1425:1B0B..1425:1CC2
tool: tools/research/exec-census/resident_listing.py at 30c0cc63; Python 3.12.10, Capstone 5.0.7; composition with FND-COMBAT-041 and FND-ITEM-086/087
environment: null
---

## Observation

Q-PARTY-116 retains backing-store transfer effects for bitmap load/reload.
FND-COMBAT-041 reads these routines' prefixes and argument widths. This
extends that reading through both complete local transfer loops. Addresses,
field offsets and error codes below are hexadecimal; byte counts are decimal.

### Shared admission

1425:1954 reads backing data into a supplied far buffer; 1B0B writes from
one. Both consume a two-word handle, full offset dword and count word.
Buffer and offset argument positions are reversed between the two entries.
Zero DS:00C4 returns AX 0003. Otherwise they call 1425:0FA3 with the handle's
low word and a far output pointer to a local selected index. A nonzero
result is propagated. The selected 14-byte record under DS:3EEC must have
word +2 equal to the handle's high word and word +0 other than one;
otherwise AX 000B is returned. No additional local index-capacity gate is
added before those record reads.

Both call 15F3:012E with the buffer and raw count word; a nonzero result
returns 0009. They interpret count zero as 65536 bytes, other words as
their unsigned byte count. The selected record's size dword +4 must be
unsigned at least low32(offset+effective_count), or return 0009. The
addition has no separate overflow rejection. Child validation and actual
handle/buffer identity are separate from this local comparison.

### Offset traversal and dispatch

They call 1425:060B with the selected record's first word and far output
pointers to two locals. Nonzero AX is propagated. While the remaining
offset exceeds 16383, they use those locals to follow a word link under
DS:39CE+264*local_index+4*local_subindex through another 060B call,
then subtract 16384 from the remaining offset. The later copy phase uses
the low offset word within that block. There is no own cycle or metadata
capacity check; traversal and child output preservation need admission.

Each chunk is the smaller of remaining effective count and
16384-within_block_offset. A word at DS:39CC+264*local_index+
4*local_subindex supplies its high nibble as backend index and low twelve
bits as backend block number. A 14-byte backend record at DS:3F42 supplies
a handle word at +0C and a far callback at +0 for reads or +4 for writes.
The indirect call receives backend handle, block/offset, current ordinary
buffer pointer and chunk word, in the direction-specific argument order.
No own callback-target, backend-index or full-transfer check supplements
the callback's returned AX. Installed targets and actual service effects
remain unresolved obligations, not arbitrary callable inputs.

Nonzero callback AX returns immediately. Zero resets within-block offset
to zero, subtracts the chunk from the remaining dword count, and advances
only the ordinary buffer pointer's offset word by the chunk. Its segment
word is retained; no offset carry is added to the segment. If bytes remain,
the routines follow the next link through 060B before another chunk.
Every link error is propagated, including after earlier chunks succeeded.

When the remaining count reaches zero they return AX zero. SI/DI/BP are
restored on all exits. There is no own data rollback, transferred-byte
count output, cache residency store or proof that zero callback AX means
the requested bytes were actually copied. Native source/destination spans,
nonwrapping offset steps, block chains and backend contents remain required.

### Bitmap caller composition

FND-ITEM-086 passes only size's low word to 1B0B and ignores its result
before publishing the cache count/flags. FND-ITEM-087 passes that low word
to 1954, sets residency only on AX zero, and returns AL zero on an error;
its provider hit caller still ignores that AL. A low size word of zero
requests 65536 bytes, not no work. Nonzero low size words request precisely
that many bytes at this layer, regardless of the caller's allocation size.
Neither call proves complete original-resource content without matching
size, buffer, offset, block-chain and backend evidence.

## Interpretation

This removes the two transfer routines' local count/error/range/traversal,
chunk dispatch and cursor/failure ordering from Q-PARTY-116. Remaining
dependencies are 0FA3/060B selector/chain state and producers, 15F3:012E
validation, installed backend callback targets/effects, valid ordinary
buffer spans and backing-store initialization/lifetime. No native partial
transfer, complete backend reading or parity promotion follows.

## Alternatives

Treating zero count as zero bytes contradicts its expansion. Treating the
size comparison as overflow-safe adds a carry gate. Treating buffer advance
as normalized far-pointer arithmetic adds a segment update. Treating an
error as no prior write ignores completed earlier chunks. Existing prefix
findings remain valid bounded readings; no native defect is inferred.

## How to reproduce

Use licensed DSUN.EXE length 634416, XXH3-128
e296af55ba2ecde7e77f555c90f33d0b and project-local evidence Python -I
with resident_listing.py on the two half-open location ranges in order.
Under GAME_DIR/analysis/item-removal-20261104/,
effect-backing-transfer-contract.txt and its -repeat pair both have SHA-256
7826326c997033f8dcc52a92697d65bd5af397039f053f906881302602940592.
Keep reports local. No original game or isolated function was executed.
