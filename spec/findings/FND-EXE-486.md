---
id: FND-EXE-486
title: Sound utility optional consumer polls and transforms selection words before matching row results
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 190F:04CA..190F:0730
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 190F:02C6..190F:02C9
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    kind: file-data
    offset: 0x000009F6..0x00000A16
tool: Capstone 5.0.7 and xxhash 4.0.1
environment: null
---

## Observation

FND-EXE-485's continuation at 04CA calls local interface 08E5 with
the same ten-word row pattern, but indexed by current SS:BP-4 and
using words eight and ten from the second incoming object instead of
four and six. It removes twenty bytes, ignores the result and jumps to
06EE. The row selector is not locally checked against the row count.
At 06EE, current BP-1C equal to 000D selects 06F7; otherwise
the caller calls 1A7C:0164 and enters the polling path at 054B.

The poll passes current DS:18F6 as one incoming word to 1A7C:03D7,
removes two bytes and stores returned AX in BP-1C. FFFF immediately
repeats the poll. Any other word sets BP-24 to one. If current
BP-1C is 004D or 0009, the caller increments current BP-4 in AX,
sign-extends that word into DX:AX, divides signed by BP-22 and
stores the remainder DX into BP-1C, then clears BP-24.
There is no local zero-divisor or quotient-range check.

It next tests current BP-1C against 004B and 000F. These tests
use the potentially transformed word, not a retained original poll result.
On equality it decrements current BP-4 in AX. The signed branch
uses the flags from that decrement: one path stores BP-22 minus one
into BP-1C, the other sign-extends a freshly decremented BP-4,
divides it signed by BP-22 and stores the remainder. Both clear
BP-24. The stages are sequential and not mutually exclusive input cases.
Under an admitted positive count and selector within zero through count
minus one, each stage considered separately computes a forward or backward
wrapped selector. Their composition can transform a forward result again
when it equals 004B or 000F. The code does not independently admit
that domain or input meanings.

The caller resets SI zero and scans while SI is less than current
BP-22 signed. Every selected iteration first calls 1A7C:0195,
then compares current R(SI) with BP-1C, using FND-EXE-485's
stride-000C record address expression. A mismatch increments current
SI and continues. A match calls 08E5 for the old BP-4 row with
object words four and six, removes twenty bytes and ignores the result.
It stores current SI into BP-4, then calls 08E5 for that current
SI row with object words eight and ten, again ignoring the result.
Each row argument group reloads the object and row state; no atomic
snapshot or callee SI preservation is established here.

After the second matching call, BP-24 equal to one selects 1000:2089
with six pushed words SS, offset BP-1028, 0019, 004E, 0001,
0001. The caller removes twelve bytes, sets BP-1C to 000D and
jumps directly to 0724. Other flag values continue scanning after
incrementing SI; matching alone does not always end the scan.
After scan exhaustion, 06EE rechecks current BP-1C for 000D.
No match leaves the same polling/exit test sequence in place.

The 06F7 exit path calls 1A7C:040F and 1A7C:0195, calls
1000:2089 with the same six words and removes twelve bytes, then
calls 1A7C:0164. It joins 0724, which loads current BP-4 into
AX and jumps through 02C6 to cleanup 072A. Cleanup restores DI
and SI, resets SP from BP, restores BP and returns far without
incoming cleanup. The matching flag-one exit bypasses those three
1A7C calls. FND-EXE-484's earlier FFFF return instead supplies
that word before the same 02C6 jump; it does not overwrite this path's
selected-word return. Native input meaning and visible effects remain open.

The eight far-call segment words here are relocation records 629 through
622 in descending call order. Encoded 0A7C and zero segments bind
at load 1000 to the named 1A7C and 1000 interfaces. Calls to
08E5 instead use push-CS/near-call, followed by twenty-byte cleanup.

## Interpretation

This completes the local caller's remaining selection and return paths,
without completing its callees or admitted state. Poll results, transformed
selectors and row-result matches are separate stages. Q-EXE-007 retains
08E5, 1A7C input/result interfaces, DS:18F6 and object producers,
selector/count domains, row/frame extents, aliases and preservation,
native effects and other caller coverage. No complete consumer reading,
key meaning, successful configuration or launch exclusion is claimed.

## Alternatives

Treating the selector tests as exclusive cases ignores the rewritten word
seen by the second stage. Treating any match as acceptance ignores BP-24.
Treating both selected-word exits as identical cleanup ignores bypassed
calls. Treating the selected-word return as FFFF confuses the earlier
size-rejection assignment with the shared jump itself.

## How to reproduce

At revision f7decaf require FND-EXE-350's SOUND_DS.EXE identity:
length 204593, XXH3-128 236c2dc23c071eca421eb5b427caee57.
Use Capstone 5.0.7 in sixteen-bit mode, MZ header size 1400 and
modeled load segment 1000. Decode shipped A9BA..AC20 at IP 04CA
and A7B6..A7B9 at IP 02C6, CS 190F. Verify opcode 99 at
0572 and 059D as word-to-pair sign extension; displayed mnemonic alone
does not establish operand width. In the relocation table at 003E with
958 records, check records 629..622 against segment words three bytes
after call starts 0544, 054F, 05AE, 06D6, 06F7, 06FC,
0717 and 071F. Track rewritten BP-1C through both selector tests,
decrement flags, signed division inputs, current SI, acceptance flag,
outgoing object-word groups and each exit's cleanup sequence. Licensed bytes
stay outside Git; no original process, DOSBox or emulated call runs.
