---
id: FND-EXE-358
title: Sound utility cleanup distinguishes preliminary failure from later record clearing
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1000:2873..1000:292B
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1000:27B6..1000:27DE
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1000:05F5..1000:0652
tool: Capstone 5.0.7 and xxhash 4.0.1
environment: null
---

## Observation

FND-EXE-355's configuration-failure continuation calls 1000:2873
with the selected record far pointer and ignores its returned AX before
returning a zero pair. The helper saves BP/SI and initializes SI=FFFF.
It rejects an all-zero incoming pair or a mismatch between record word
eighteen and the incoming offset, returning current SI through the common
suffix. The equality checks no segment and proves no admitted extent.

When record word six is nonzero and record word zero is signed negative,
the helper calls 1000:292B with the record pair and removes four bytes.
Nonzero returned AX selects the common suffix before the following record
clears. Otherwise, for nonzero word six and bit four in word two, it calls
1000:1ACC with the pair at record words eight/ten and removes four bytes.
That result is not tested. Both callees and their register contracts remain
unread; the later SI return is not proven unchanged across those calls.

It reloads the record pair. A signed negative byte four skips 1000:27B6.
A nonnegative byte is extended to a word, passed to that helper, and the
argument word removed; returned AX replaces SI. Regardless of that result,
the continuing local path reloads the record and writes zero to words two,
six and zero, then FF to byte four. It does not locally clear the two far
pairs at words eight/ten and twelve/fourteen.

The 27B6 helper loads its argument into DX and compares it unsigned with
current DS:DD38. Out-of-range calls 1000:04CE with word six. The other
path clears current DS storage at low-word twice the argument minus 22C6,
then calls 1000:27DE with that argument. It returns AX without a local
success test. The error helper, lower helper and their preservation and
native effects are not established by this reading.

After the record clears, a zero record word sixteen skips the final helper
sequence. A nonzero word is passed to near helper 1000:05F5 after four
zero words have been pushed. The returned DX:AX pair is passed to
1000:0EF5 and its four argument bytes removed. Without testing that
result, the root reloads the record pointer and clears word sixteen. It
then returns current SI in AX, restores SI/BP and far-returns without
incoming argument cleanup. Neither final helper's effect or SI preservation
is established here, so returned AX is not necessarily the earlier 27B6 result.

The 05F5 helper forms BP and replaces an all-zero pair at SS:BP+0A/0C
with current DS:EDEC. It similarly uses DS:DDC2 for an all-zero pair at
SS:BP+06/08. It pushes the first argument word, calls 1000:1487 with
those two pairs and removes eight bytes, then pushes returned DX/AX and
calls near 1000:05AC. It subsequently calls 1000:3A0B with DS:DDC6
and the pair at SS:BP+0A/0C, removing eight bytes, returns that latter pair
in DX:AX, restores BP and near-returns removing ten incoming bytes.
The retained first word and later result pair require 05AC's stack contract;
this local return suffix alone does not establish that call's cleanup.

## Interpretation

The cleanup return is selected separately from record clearing. An early
292B nonzero result skips the clearing suffix, whereas an unfavorable
27B6 result does not locally prevent it. FND-EXE-355 ignores the cleanup
return altogether. This does not prove successful release or rollback.

Later result propagation also depends on SI preservation through unread
helpers. The final word-sixteen clearing follows those helpers without a
success test. The default pointer pairs in 05F5 are conditional on actual
DS and do not establish their contents, capacity or lifetime.

Q-EXE-007 retains 1000:292B, 1000:1ACC, 1000:27DE, 1000:04CE,
1000:1487, 1000:05AC and 1000:0EF5, register/stack preservation,
record/state admission and later consumers. FND-EXE-360 reads 3A0B's
local append behavior; its argument extents remain open. No complete-reading
promotion, native release outcome or execution exclusion follows.

## Alternatives

Treating every nonzero cleanup result as an unchanged record ignores the
unconditional clears after 27B6. Treating the root return as always its
initial FFFF or earlier 27B6 result assumes SI preservation by later callees.
Treating the offset check as far-pointer identity ignores its missing segment
comparison. Treating the ten-byte near-return cleanup as proof of 05AC's
stack contract ignores the separate retained arguments at that call.

## How to reproduce

At revision 3db0d07 require FND-EXE-350's SOUND_DS.EXE identity:
length 204593, XXH3-128 236c2dc23c071eca421eb5b427caee57.
With locked Capstone 5.0.7 in sixteen-bit mode decode shipped half-open
0x00003C73..0x00003D2B at IP 2873,
0x00003BB6..0x00003BDE at IP 27B6, and
0x000019F5..0x00001A52 at IP 05F5, modeled CS 1000,
MZ header size 1400. Track BP-relative arguments, signed/unsigned tests,
ordered record/global stores, result tests and the SI writer through each
call. The byte extension at IP 28DB is the sixteen-bit instruction despite
Capstone's wider printed mnemonic. Keep the 05AC stack-cleanup obligation
open rather than assigning its contract from the caller's suffix.
Original bytes and reports remain outside Git. No original process,
DOSBox, interrupt thunk or emulated call is executed.
