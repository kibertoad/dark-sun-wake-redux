---
id: FND-SCRIPT-019
title: Script-cache paths have distinct age updates and retain writes before a failed transfer
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 172C:0388
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 172C:043F
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 172C:04CF
tool: Python 3.14.7 and Capstone 5.0.7 entry-based bounded 16-bit instruction checks and declared MZ relocation mapping
environment: null
---

## Observation

This replaces FND-SCRIPT-008. The complete local wrapper
0388 occupies file offsets `0x0000C848..0x0000C8FF`,
cache scan 043F `0x0000C8FF..0x0000C98F`, and fill
body 04CF `0x0000C98F..0x0000CB58`. Declared MZ
relocations map the state segments to 4C0E and 4C13,
resource callees to 38FF and error entry to 5702.

The 16 slots use these parallel arrays in 4C13:

| Offset | Element | Observed use |
|---|---|---|
| 01D9 | word | resource number |
| 01B9 | word | selector |
| 0219 | word | start offset, FFFF selected as free |
| 01F9 | word | end offset |
| 0239 | signed byte | age counter |

Wrapper 0388 returns AL zero when byte 0326 equals one.
Otherwise, matching the requested number and selector to
4C0E:0012 and 0014 returns AL one. Both paths jump to
the final register restoration without executing the age
loop or helper 31ED. FND-SCRIPT-008's assertion that every
case ages slots is contradicted by these explicit edges.

Only the remaining path calls 31ED, then scan 043F, then
fill 04CF when the scan returns zero. A nonzero result
updates 4C0E:0012 and 0014. This path subsequently visits
all 16 ages and increments each signed value from zero
through 126, returning the retained AL result. It does
not increment negative ages or age 127.

Scan 043F examines all 16 slots. Every number-and-selector
match assigns the current number 4C13:0197, selector
0195 and code start 0193, resets that slot's age to zero,
and retains success. It does not exit on its first match
or test the start against FFFF. With multiple matches, the
last matching slot supplies the final code start and all
matching ages are reset before the wrapper's later loop.

Fill 04CF rejects number FFFF or a selector other than one
or two. Selector one chooses tag GPL and two chooses MAS.
It selects the first FFFF start, or calls 07BB when none
exists. It compares only the selected slot's number with
the requested number. Equal bypasses resource calls and
slot initialization and reaches the success assignments,
including the requested current selector. Slot validity
and the selector already in that slot are not checked on
this bypass.

Different queries resource length through 38FF:05B5 into
a zero-initialized local double word at SS:BP-8. The
wrapper's double-word output is read in FND-CONFIG-151;
this is not a word-sized length output as stated in
FND-SCRIPT-008. However, the fill body uses only its low
word for the following increment, allocation argument,
end-offset arithmetic and stop-byte address. The increment
and offset additions are word arithmetic.

A zero size-query result calls 0698 with that low word
plus one. A stop byte equal to one after that call returns
zero immediately. Otherwise the body writes the selected
start, then the end as start plus the low length word plus
one. It next increments ages from zero through 126.
These slot and age writes precede the resource transfer.

It forms a destination far pointer from 4C13:0255 plus
the allocated offset and calls 38FF:04AB. That wrapper
requests the complete selected resource length, not the
truncated allocation length (FND-CONFIG-151). On zero
return, the fill body appends byte 31 at the start plus
low length word, assigns the slot number and selector,
and resets the selected age to zero. It then assigns
current number, selector and start and returns one.
A general destination-capacity guarantee is not established
by this call sequence.

A nonzero size-query or transfer result calls 5702:00B1
and, if it returns, exits with the local zero result.
The transfer-failure path has already written slot bounds
and ages. It contains no local rollback before returning;
the error callee's effects remain separate. Therefore a
failed transfer cannot be described as leaving the slot
unchanged solely from this body. The wrapper's later age
loop still executes when the error path returns normally.

A successful new transfer can thus run two age loops:
one before transfer and one in the wrapper. The selected
age is reset between them, so its final local value is one
under unchanged, valid state. A matching cache scan also
resets matching ages before the wrapper's one loop. The
early current-pair and stop paths run neither loop. Error
callees and later writes can affect these conditional results.

## Interpretation

Cache reuse, a new transfer and an early current-pair
return have different effects. Neither an unconditional
age update nor a transactional failed-load guarantee is
supported. Size-output width, allocation arithmetic and
full-transfer length are distinct contracts. This is a
local reading, not evidence that oversized resources or
failed transfers occur in the owner's running build.

## Alternatives

FND-SCRIPT-008 is superseded because it combines the
wrapper's early returns with the age-loop path and names
a word output where the resource wrapper writes a double
word. RULE-SCRIPT-001 is superseded by RULE-SCRIPT-010
because its failed-read edge claim also conflicts with
the direct pre-transfer writes. Their original text is
retained as history.

Q-SCRIPT-003 retains slot-choice and allocation effects,
error handling, valid buffer and cache state, archive
selection and successful I/O. A matched or reused slot
can bypass a transfer; a different selected number can
request a transfer and partially update state before failure.
Actual cache inputs and resource outcomes distinguish
those reachable cases. FND-SCRIPT-020 reads helper 31ED's
bounded reset separately. No complete reading of all
callers, writers or external outcomes is claimed.

## How to reproduce

Read the three named bodies from their verified entries
through their returns. Follow both early wrapper jumps
past the age loop, the full 16-slot scan, selected-number
bypass and each error edge. Resolve the declared segment
operands and inspect the size output and low-word loads
separately. Put each slot, age, transfer, append and identity
write in local call order; do not infer rollback from an
error result. Compare 38FF:05B5 and 04AB with
FND-CONFIG-151. Keep 0698, 07BB, 00B1, buffer provenance
and actual resource I/O as dependencies.
