---
id: FND-ITEM-081
title: Animation descriptor acquisition has distinct guarded number-hit, unguarded content-hit and fresh count producers
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 31E0:1893..31E0:1B38
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 31E0:4474..31E0:44C0
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:3F5A..1000:3F7E
tool: tools/research/exec-census/resident_listing.py and window_operands.py at f21d1885; Python 3.12.10, Capstone 5.0.7; composition with FND-SCRIPT-018, FND-EXPLORE-019, FND-CONFIG-184 and FND-ITEM-080
environment: null
---

## Observation

Q-PARTY-116 retains descriptor/count producers after FND-ITEM-080's
cleanup reading. FND-SCRIPT-018 identifies the SCMD acquisition entry
and its callers; FND-EXPLORE-019 identifies the nine-byte animation
descriptors. This reads the complete acquisition body, keeping native
resource, allocation and storage admission separate. Addresses, fields
and sentinel words are hexadecimal; lengths/counts are decimal.

### Enable and number-hit path

31E0:1893 takes a resource-number word and a second selector word. Zero
byte DS:265B or DS:265C reaches its common failure return. Selector 8000
requests an automatic scan of exactly 64 nine-byte records at DS:6577.
For each it compares descriptor word +1 with the requested number before
checking whether its count byte is zero.

A number match rejects count FF through the common failure path.
Otherwise it increments the descriptor byte and returns that index,
without validating the descriptor pointer or requesting the resource.
A zero-count number match can consequently increment to one. For a
nonmatching record with zero count it saves that index as the candidate
and continues. Thus the last encountered free record is retained, not
the first. With no match/free record the selector remains 8000.

A second selector other than 8000 skips that scan. There is no own
0..63 gate before its later word-width descriptor selection. This is
not admission of arbitrary selector values to a native caller.

### Size, staging and second comparison pass

The routine queries SCMD size through 38FF:05B5. A nonzero query result
returns the word negation of the resource number. Otherwise let L be
the size word it reads from the local output. It forms F=low16(L-72),
requests allocations with signed extensions of F and L through 444C:00FA,
and stores logical F>>3 as its prepared frame-count word. No own size-
at-least-72 or word-overflow rejection precedes these operations.

The staged descriptor has count one, the requested number, the frame
allocation pointer and that frame count. It forms a far frame-source
pointer by adding 72 to the temporary resource buffer's offset before
38FF:04AB loads SCMD through the address of that buffer pointer. No far
carry is added. Transfer success is therefore insufficient without
preservation of this buffer identity and readable nonwrapping spans.

After a zero transfer result it scans all 64 descriptors again. For each
it first compares 72 header bytes under DS:6573+72*index with the staged
resource. An equal comparison then requires descriptor word +7 equal to
F, not F>>3. It next compares F bytes from the cached frame pointer and
the preformed temporary frame-source pointer. 4474 returns FF for a
null pointer, one for a mismatch and zero for equal bytes; its pointer
increments keep word width, so span/segment admission is required.

If both comparisons and the word test pass, it jumps directly to the
count increment at 18E6 and returns that existing index. This bypasses
the number-hit path's FF guard: a byte FF would wrap to zero on this
local route. It also bypasses the temporary-buffer cleanup blocks.
These are conditional control-flow facts, not observed wrap, leaked
allocation or a claim that actual cache contents admit this route.

The literal F comparison must not be silently changed to a frame-count
comparison. Fresh publication stores F>>3 at +7, while this comparison
uses F itself. Other descriptor writers and actual resource sizes remain
necessary to determine which native content-hit cases occur.

### Fresh publication and failures

With no content hit, selector still 8000 cleans up the staged allocations
when nonnull and takes the common failure return. Allocation/transfer
failure has its own cleanup paths. FND-ITEM-080 supplies the resource
release wrapper's conditional return versus actual-release distinction.

A retained candidate/explicit selector instead selects its descriptor
with word arithmetic. A nonnull old frame pointer is passed to release
and replaced by the returned pointer before the new descriptor is stored.
The routine then writes staged count one, number, frame pointer and
frame count in that order. These stores precede the 72-byte header copy
and F-byte frame copy through 1000:3F5A. After those requests it releases
the temporary resource buffer and returns the selected index. It has no
own post-copy rollback or result check. Valid separate table/header/frame/
resource/stack spans and child preservation remain requirements.

The common failure return is 8000 when requested number is zero and
FFFF otherwise. It differs from size-query failure's negated number and
from successful index zero. SI/DI/BP are restored on the common exit;
ES and scratch registers are not preservation claims.

## Interpretation

This removes acquisition's local count producers, selector/number/content
gates and publication/cleanup ordering from Q-PARTY-116. Together with
FND-ITEM-080 it records distinct byte increment/decrement paths, rather
than assuming a globally saturating reference count. Native caller
selectors, descriptor/header/frame/count state, transfer/allocation
results, other writers and use-time preservation remain open.

## Alternatives

A universal FF-increment exclusion ignores the second pass's direct
jump. Assuming a free candidate means the first free record changes the
scan result. Equating byte length with stored frame count repairs a test
the original actually makes. Publication before content copies is not
proof of completed content. No original was run, native defect inferred,
complete reading declared or parity status promoted.

## How to reproduce

Use approved DSUN.EXE length 634416, XXH3-128
e296af55ba2ecde7e77f555c90f33d0b. Run resident_listing.py with -I and the
three location ranges above in order. Under
GAME_DIR/analysis/item-removal-20261104/, effect-descriptor-acquire-contract.txt
and effect-descriptor-acquire-contract-repeat.txt have SHA-256
c2a53d1a99d84a8aeff77f44df804408ea7d1d0e06164058fa776f797d21d379.
The inventory-backed window_operands.py report effect-descriptor-operands.txt
uses 6577..6580 to locate candidates, including the known cleanup count
stores as controls; it retains other-segment/interior candidates and is
not a complete writer census. Keep reports/listings/buffers only locally.
