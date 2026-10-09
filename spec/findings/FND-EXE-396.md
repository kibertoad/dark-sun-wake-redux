---
id: FND-EXE-396
title: Game type-three slot publication follows an exact-one native result and a mutable quantity helper
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1425:041A..1425:04FF
  - build: BLD-GOG-EN-1.1
    file: CD:DSUN.EXE
    address: 1425:041A..1425:04FF
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 15F3:0618..15F3:06B6
  - build: BLD-GOG-EN-1.1
    file: CD:DSUN.EXE
    address: 15F3:0618..15F3:06B6
tool: Capstone 5.0.7, Python struct and xxhash 4.0.1
environment: null
---

## Observation

FND-EXE-391's type-three dispatcher calls 1425:0462 with a far index
pointer at SS:BP+0006, record word two at +000A and a far quantity
pointer at +000C, relative to the callee's saved-BP frame. The producer
allocates a local word at SS:BP-2 and saves SI. It reads the quantity
through ES:BX, clamps SI upward to four and copies SI to that local word.
It forwards the local word's SS:offset pointer and the record word to far
15F3:0632, then removes six outgoing bytes.

### Preliminary helper

0632 saves BP, BX, CX, DX, DI, ES and DS, but not SI.
It assigns DS from CS and sets CX four. It first adds four to its own
incoming word at SS:BP+0006, then shifts that stored word left four,
both at word width. These are writes to the outgoing argument copy, not
an established write to the original record. Record values 0000, 0FFC,
0FFD and FFFF become 0040, 0000, 0010 and 0030 respectively.

It loads ES:DI from the incoming far pointer at SS:BP+0008, clears AX
and reads the pointed-to quantity into BX. If any of BX's high four bits
are set it replaces BX with 0FFF. Otherwise it keeps BX. It shifts
BX left four and copies the word into CX. A zero shifted word takes
the AX-zero exit, after the earlier incoming-word mutation but without a probe.

For the connected producer call, the helper's saved-BP frame is sixteen
bytes below the producer's. Its incoming record copy is at producer BP-000A,
while its supplied quantity pointer names producer BP-0002 in the same SS.
Those word locations are distinct even with word-width stack-offset wrap.
Before any native call, the local quantity is therefore the producer's clamped
value, at least four. Values above 0FFF clamp to 0FFF before the shift;
the resulting CX ranges from 0040 through FFF0 and cannot take this
zero-count exit. Other callers and subsequent native effects remain separate.

A nonzero word calls near 15F3:00BC, the installation probe in
FND-EXE-394. Returned AX zero exits. Returned AX one selects a far
indirect call through current DS:00B8/00BA with BX zero and AX 0801.
The probe's initial CS-relative stores, later DS-dependent pointer stores,
and native-preservation dependencies remain as recorded there; the pointer's
validity and target identity are not supplied by this helper.

After that call it subtracts the transformed incoming word from returned DX
at word width. A borrow sets AX zero and exits, using the subtraction's
unchanged carry. Otherwise it keeps the smaller unsigned of returned AX and
adjusted DX, copies that word into DX and compares it with 0044. It
sets AX zero without changing those comparison flags and exits if the word
was below 0044. Current BL equal to 80 or 81 also exits with zero.
No separate success predicate is applied to the first indirect call's AX.

The continuing path reduces current CX to current DX if CX is unsigned
greater, copies CX to DX and sets AX 0900. It makes another far
indirect call through the then-current DS:00B8/00BA. No DS reload occurs
between the two indirect calls. Native register changes can therefore affect
the second lookup and the quantity/count inputs; the initially assigned DS
does not establish their later identities.

It decrements the second call's returned AX, sets AX zero without changing
the decrement's flags, and exits unless the original returned word was exactly
one. On that exact-one path it captures current CX into AX, shifts that
word right four and subtracts it from the word at current ES:DI without
an underflow guard. It then returns current DX in AX. Native ES, DI,
CX and DX preservation/meaning are not checked locally. In particular, exact
one from the second call does not guarantee a nonzero returned DX, and the
pointed-to subtraction precedes the final DX return.

Every exit restores DS, ES, DI, DX, CX, BX and BP and returns far
without incoming cleanup. The saved registers are restored only at final exit;
they do not establish intermediate native preservation. SI is not saved here
or by the near probe, so the producer's held request also needs the reached
native contracts before its later quantity arithmetic can be admitted.

### Producer continuation and publication

The producer copies returned AX to DX and tests that whole word. Zero
goes directly to the common AX-zero exit without its later global quantity,
slot or index writes. This does not undo the preliminary helper's outgoing
argument mutation, possible local-quantity subtraction or native effects.

Nonzero DX subtracts current local SS:BP-2 from current SI at word width.
It reloads the original quantity pointer. If its current word is unsigned
below the resulting SI it sets the quantity to zero; otherwise it reloads
the pointer and subtracts SI. A bound on the reduction requires the helper's
native inputs/results, held SI, storage identity and alias admission; the local
subtraction alone supplies none of those contracts.

It loads ES:BX from the index pointer, reads the index word and multiplies
it by fourteen at word width. It writes DX into current DS's indexed
cleanup-argument field, 3F4E installed or 3EC2 on disc. Each following pair
publication reloads the index through the same ES and incoming offset before
multiplying by fourteen. The ordered stores are:

| Order | Installed indexed field | Disc indexed field | Modeled value |
| --- | --- | --- | --- |
| 1 | 3F44 | 3EB8 | Segment 1425 |
| 2 | 3F42 | 3EB6 | Offset 041A |
| 3 | 3F48 | 3EBC | Segment 1425 |
| 4 | 3F46 | 3EBA | Offset 0437 |
| 5 | 3F4C | 3EC0 | Segment 1425 |
| 6 | 3F4A | 3EBE | Offset 0454 |

Relocation indices 33 at 0425:04C2, 32 at 0425:04D7 and 31 at
0425:04EC each hold shipped 0425, becoming modeled 1425. The producer
finally reloads the incoming index offset, increments its word through ES,
clears AX, restores SI and its frame, and returns far without incoming
cleanup. There is no local bound limiting the index to sixteen. Stable
index/segment and disjoint storage are not established by repeated reloads.

### Published wrappers and cleanup

Targets 1425:041A and 1425:0437 each forward twelve bytes, including an
operand-size-32 push, and call far 15F3:06B6 and 15F3:076B respectively.
They remove twelve bytes and return the callees' AX without testing it.
Their six outgoing words have the same two forwarding layouts recorded for
FND-EXE-395's pair: relative callee offsets 0006, 0008, 000A, 000C,
000E and 0010 come from first-wrapper offsets 0006, 0008, 000A, 0010,
000C and 000E, or second-wrapper offsets 0006, 000C, 000E, 0010,
0008 and 000A. The bodies at 06B6/076B remain outside this finding.

Cleanup target 1425:0454 forwards its incoming word to far 15F3:0618,
removes it by popping CX and returns without an AX test. The callee saves
BP, BX and DX and calls the same near probe. AX zero skips the native
target and returns zero. AX one loads DX from its incoming SS:BP+0006,
sets AX 0A01 and calls the far pointer through explicit CS:00B8/00BA.
It restores DX, BX and BP and returns the native AX unchanged. It makes
no result check or local slot-field clear. The explicit CS lookup differs
from 0632's default-DS lookups.

Relocation indices 37 at 0425:0430, 36 at 0425:044D, 35 at
0425:045D and 34 at 0425:0485 hold shipped 05F3, becoming modeled
15F3. Both editions agree on all these relocation values. Their wrappers
contain 11, 11 and seven instructions; the producer has 52 in each edition,
and helpers 0618 and 0632 have 14 and 64. The wrappers and both
helpers have identical bytes between editions. The producer's corresponding
instructions differ in its seven indexed field offsets as listed above.

## Interpretation

This supplies the type-three producer, its preliminary helper and cleanup
route, including exact-one admission, word-width quantity arithmetic and ordered
publication. The producer's zero AX does not distinguish a skipped slot from
a published slot. A preliminary exact-one result can precede a local quantity
write and a returned zero DX, conditional on admitted native effects.
Q-EXE-007 retains native pointer identity, semantics and preservation, input and
state writers, quantity/index aliases, complete callers, the other published
callee bodies and type four. No usable native storage or game launch exclusion
is claimed, and no complete-reading declaration is made.

## Alternatives

Treating record zero as a zero transformed input ignores the add before the
shift. Treating the transformed word as a widened product loses the stored
word wrap. Accepting arbitrary nonzero from the second indirect call would
replace an exact-one test. Equating that test with a nonzero final helper
return would replace current DX with the tested AX. Assuming the quantity
subtraction always addresses the caller's local word would supply unverified
ES/DI preservation. Using one data-segment assumption for both native lookups
and cleanup would ignore the cleanup's explicit CS prefix. Treating producer
AX zero as proof of publication would erase its preliminary-zero bypass.

## How to reproduce

At revision f7bcf3b4 require the two identities in FND-EXE-350. Use MZ
header size 5200, modeled load segment 1000 and relative segments 0425
and 05F3. Divide the first Locations range at 0437, 0454 and 0462,
and the second at 0632. Decode each body separately in sixteen-bit mode,
checking complete byte coverage, instruction counts, the listed edition-specific
fields and relocation indices 31 through 37. Header 0006 gives relocation
count and 0018 its table offset.

Follow the dispatcher binding from FND-EXE-391, the local quantity pointer,
both incoming-word writes and every helper exit. Check the four transformed
record cases above at word width. Keep the first indirect result's arithmetic,
the second result's exact-one flags and the final current-DX return separate.
Track current versus saved segments/registers, all repeated index reads and
ordered stores. Use FND-EXE-394 for the near probe and FND-EXE-559 for
the cleanup-argument consumer. No negative caller or writer census is claimed.
Licensed bytes remain outside Git; no game, DOSBox or emulated call runs.
