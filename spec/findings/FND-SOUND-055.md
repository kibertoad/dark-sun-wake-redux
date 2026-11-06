---
id: FND-SOUND-055
title: File-sound refill debits requested bytes before accepting non-error short reads
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4611:0051..4611:0177
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4611:032A..4611:03A5
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4842:0C64..4842:0C76
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4842:1685..4842:169A
tool: resident_listing.py at 7280071c; Python 3.12.10, Capstone distribution 5.0.9, xxhash 4.0.1; FND-CONFIG-166, FND-SOUND-053/054, FND-EXE-646 and FND-COMBAT-082
environment: null
---

## Observation

Installed DSUN.EXE is 634416 bytes, XXH3-128
e296af55ba2ecde7e77f555c90f33d0b. Addresses below are hex.
Reuse FND-CONFIG-166's early zero gates for 4611:0051:
configuration word +14 and DS:3434 must both be nonzero to
enter the active branch. The first gate tests the whole word,
not merely bit zero as the start entry does (FND-SOUND-054).

The active branch initializes SI zero and loops while signed SI
is less than two. Under retained SI and ordinary child returns
it visits slots zero and one. Each requests 4842:0C70 with
DS:3436 and SI. Only result three and nonzero dword DS:A438
enter refill. Other results advance to the next slot.

### Requested length and publication order

Refill loads dword 4E71:0B63, shifts it right once logically,
then sign-extends its low word to a double word. It compares
that value with DS:A438 unsigned. If smaller it reloads 0B63
and repeats the shift/sign-extension; otherwise it loads A438.
Thus a stable-input description is an unsigned minimum of the
remaining count and that sign-extended low word, not an ordinary
32-bit half-size minimum. Bits discarded before sign extension
and changes between the two loads cannot be silently ignored.

It writes the selected value to DS:A42C, subtracts it from
DS:A438 at double-word width, then XORs word DS:A43C with one.
Zero after XOR selects local 032A with zero and pointer DS:342C;
nonzero selects it with one and pointer DS:3430. FND-SOUND-054
initializes A43C zero, so its first unchanged refill selects the
second pointer. An arbitrary starting A43C is not normalized to
a Boolean: only zero and one alternate through zero under XOR one.

A zero result from 032A calls stop 03A5 and returns AX zero.
The length debit and toggle precede that call and have no local
rollback. Otherwise the selected pointer is reloaded after the
read and published at DS:A428. The body requests 4842:0C64 with
DS:3436, SI and DS:A424, then 0C6A with DS:3436 and DS:A424.
Their results are not checked. The pointer used for the read
and the pointer reloaded for publication require actual retention.
The DS:A424 descriptor was prepared by FND-SOUND-054; its local
refill fields include the pointer at +4 and requested count at +8.

### Read result classification

032A requests 44DE:01AF with current DS:349E, the selected
far pointer and dword DS:A42C. It sign-extends only returned AX,
ignoring returned DX, then checks the reloaded handle against
FFFF and that sign-extended result against FFFFFFFF. Either
comparison failing its validity condition returns zero; otherwise
it returns one. It does not compare the actual count to A42C,
replace A42C with the count, or restore A438's debit. Zero and
short non-FFFF results are accepted locally. Any result with low
word FFFF takes the error route irrespective of its high word.
FND-COMBAT-082 supplies the bounded actual-count/error behavior
of the underlying reader; larger counts and native outcomes still
need their own admitted contracts.

### Final status and conditional DIGPAK binding

After visiting the slots, 0051 requests 4842:0C88 with DS:3436.
If current A438 is nonzero it returns AX one. If zero, it queries
slot zero through 0C70; only result three triggers a slot-one
query. Both equal to three invoke 03A5 and return zero; any other
result returns one. Reuse FND-CONFIG-166 for the stop clear and
release ordering, not an assumed successful device stop.

The three stubs select AX 0079 (0C64), 0086 (0C6A) and 007A
(0C70), then tail-jump to dispatch 03F4. FND-EXE-646 supplies
the table/target conditions. Under its unchanged DIGPAK table,
007A reaches 1685: it doubles incoming word BP+8 with word
wrapping and returns CS:[0E58 + doubled index], restoring its
saved DS/SI/DI. The caller's retained zero/one indexes therefore
read 0E58/0E5A. FND-SOUND-053 records known initialization and
stop writes of three to these words. Their other writers and the
0079/0086 children remain obligations; no driver request proves
actual output or successful consumption of the descriptor.

## Interpretation

Q-SOUND-002 can remove the unread active refill/control-flow and
local read-classification contract. The function is a service step,
not just a passive hardware query. Requested-count bookkeeping,
short-read acceptance, buffer toggle and final state tests now have
finite local descriptions. Actual counts/capacities, buffer lifetime,
state producers, descriptor consumers, child/frame retention, external
requests and native playback remain unresolved. No complete reading,
whole queue closure or reachable native defect is claimed.

## Alternatives

- Remaining length tracks bytes actually received: it is decremented
  by the requested count before the read and never reconciled here.
- A successful read helper guarantees a full buffer: zero and short
  non-error results are accepted without an equality check.
- The size computation halves a full unsigned dword: it sign-extends
  the shifted low word before the unsigned comparison.
- Returning one means a sample is audibly playing: it can follow
  skipped refills and unchecked driver requests.

## How to reproduce

Read the four location ranges with resident_listing.py at the revision
above. Retain sound-file-refill-contract.txt under
GAME_DIR/analysis/handler-census-20261106/. Track repeated size loads,
MOVSX width, unsigned versus signed branches, debit/toggle ordering,
AX-only read-result classification and post-read pointer reloads.
Reuse prior gate, setup, stop and table findings. No original game,
emulator session or emulated call supplied this evidence.
