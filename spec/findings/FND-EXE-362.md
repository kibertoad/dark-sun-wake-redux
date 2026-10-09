---
id: FND-EXE-362
title: Sound utility final cleanup request restores DS before mapping a carry-set result
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1000:0EF5..1000:0F0D
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1000:04CE..1000:0507
tool: Capstone 5.0.7 and xxhash 4.0.1
environment: null
---

## Observation

FND-EXE-358's final cleanup passes the pair returned by 05F5 to
0EF5. The latter saves BP and DS, sets AH=41 without initializing AL,
loads DS:DX from SS:BP+06/08 and executes interrupt 21. It restores
DS before testing carry. Carry clear selects AX=0000. Carry set pushes
post-interrupt AX and near-calls 04CE. Both paths restore BP and
far-return without incoming argument cleanup. There is no local save of
SI, DI or ES, or test of the incoming pointer pair or its extent.

The error helper forms BP, saves SI and loads its incoming word into SI.
A signed nonnegative input at most 0058 is used directly as an index.
A larger signed nonnegative input is replaced by 0057. On that path it
stores the selected index into current DS:DD66, reads a byte at the
low-word offset index minus 2298 (equivalently DD68 plus the index),
sign-extends that byte into AX and transfers the resulting word to SI.

A signed negative input is negated at word width. If the signed result
is greater than 0030, it joins the index-replacement path at SI=0057.
Otherwise it stores FFFF to DS:DD66 and retains the negated word in SI.
Thus inputs FFD0..FFFF produce one through 0030 on this local path.
The minimum signed input 8000 remains 8000 after word negation and
also passes the signed greater-than test; it is not converted to a positive
magnitude or sent to the replacement index by this code.

The common suffix stores current SI to DS:007F, sets AX=FFFF,
restores saved SI/BP and near-returns removing two argument bytes.
It has no further calls or interrupts. The reachable table indices are zero
through 0058 inclusive; this reading establishes their construction and
local bound, not the table bytes, capacity or actual DS at invocation.

Returning ordinarily through 0EF5's carry-set branch therefore supplies
AX=FFFF and the error helper's data-segment writes. The caller in
FND-EXE-358 removes four argument bytes without testing AX, reloads
its record pointer, clears record word sixteen and returns current SI.
The error helper's local SI restoration does not establish interrupt SI
preservation or preservation by other cleanup helpers.

## Interpretation

This resolves the local final-request and error-mapping bodies reached by
FND-EXE-358. Carry clear and carry set select distinct local AX returns;
the latter is not a transparent return of the post-interrupt error word.
The caller's word-sixteen clearing is not conditional on a zero AX result.
None of this proves that the external operation occurred or succeeded.

The data-segment restoration precedes the error-helper call, so its stores
use the restored incoming DS on ordinary continuation. That order does
not by itself admit the segment, table or globals. The signed-negative
minimum-word case differs from a mathematical absolute-value reading.

Q-EXE-007 retains interrupt arguments/results and register preservation,
the mapping table and state writers, incoming DS, pointer/string admission,
other cleanup callees, aliases and lifetime. The meanings of the two
error-state words and the native selector are not assigned here. No
complete-reading promotion, native outcome or launch exclusion follows.

## Alternatives

Treating returned AX as the unchanged native error ignores the fixed FFFF
suffix. Treating the final record clear as success-only ignores its caller's
absent result test. Treating every negative word as a bounded positive
magnitude ignores word-width negation and signed comparison at input 8000.
Treating local SI restoration as interrupt preservation conflates two contracts.

## How to reproduce

At revision a87c581 require FND-EXE-350's SOUND_DS.EXE identity:
length 204593, XXH3-128 236c2dc23c071eca421eb5b427caee57.
With locked Capstone 5.0.7 in sixteen-bit mode decode shipped half-open
0x000022F5..0x0000230D at IP 0EF5 and
0x000018CE..0x00001907 at IP 04CE, modeled CS 1000,
MZ header size 1400. Follow the saved DS, pointer load, carry branches,
word-width signed comparisons, table indexing and common stores. Distinguish
input 8000 from other negative words. The extension at IP 04E9 is the
sixteen-bit instruction despite Capstone's wider printed mnemonic. Compare
FND-EXE-358's ignored result and later word-sixteen clear.
Original bytes and reports remain outside Git. No original process,
DOSBox, interrupt thunk or emulated call is executed.
