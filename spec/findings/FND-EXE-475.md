---
id: FND-EXE-475
title: Sound utility constructs sw32.ini pathname and sequentially matches section and field markers
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 158E:01D3..158E:0284
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    kind: file-data
    offset: 0x0000FD3B..0x0000FD57
tool: Capstone 5.0.7 and xxhash 4.0.1
environment: null
---

## Observation

FND-EXE-474's reverse separator match continues at 158E:01D3. The
caller copies the retained byte at SS:BP-09 into BP-0098 and writes
zero to the next byte. That retained byte came from the earlier record
read and AL-only FF test, not the original prefix-byte producer directly.
The new local string is initialized separately from the truncated indexed
string at BP-00E8.

It makes four 3A0B append calls to destination SS:(BP-0098), each with
eight incoming argument bytes removed and no result test. Their sources
in order are current DS:05DB, SS:(BP-00E8), current DS:05DE and
current DS:05E0. FND-EXE-360 reads that helper's finite but unchecked
terminator scans and copy, pointer return and local register restoration.
The caller supplies no destination capacity to those calls.

Under FND-CONFIG-213's data-segment binding 1E36 and MZ header
size 1400, the bounded shipped strings at those data offsets are colon
followed by backslash, a single backslash and the filename sw32.ini,
respectively. Each ends in zero. The mode at offset 05E9 is rt followed
by zero; markers at 05EC and 05F1 are [SB] and Port=, each followed
by zero. These identities come from shipped file-data, not an assumption
based on function naming. Applying them at the caller still requires its
actual current DS to select that storage.

Thus under admitted frame, string and data-segment state, the local
construction combines the retained byte, colon/backslash, earlier truncated
string, backslash and sw32.ini filename. It then calls 2C11 with that
stack pathname and current DS:05E9 mode pointer, removes eight bytes
and stores returned DX/AX into SS:BP-06/-08. An all-zero returned
pair takes the previously recorded AX-zero common exit. FND-EXE-360
reads that wrapper and FND-EXE-370 the selected mode interface.

A nonzero pair is passed to 1BD4:02AA with current DS:05EC. Full AX
zero exits; any nonzero AX continues. It next passes the same retained
pair to the same matcher with DS:05F1 and makes the same full-word
result decision. There is no intervening position-reset call between these
matches. FND-EXE-377 reads the matcher's consumed-byte progression,
pattern restart and result tests; the second search consequently continues
from the record state left by the first under ordinary noninterference.

Both searches remove eight incoming argument bytes. If both return nonzero,
the caller continues at 158E:0284, outside this reading. This local body
contains no direct interrupt, conversion of the matched field or command
execution. Its calls, string scans and returned records retain their separate
input, extent, preservation and lifetime obligations.

## Interpretation

This follows the actual pathname consumer into a second constructed filename
and two sequential configuration markers. The earlier stack-path read supplies
both a retained prefix byte and a truncated source string; shipped source
strings supply the separators, filename, mode and marker identities.
The current caller's data-segment admission and string extents remain distinct
from those shipped-file identities.

Q-EXE-007 retains continuation from 0284, field conversion and subsequent
consumers, actual DS and frame state, constructed-string extents and aliases,
record/native preservation, producers and lifetime/re-entry. No complete
launch exclusion or complete reading follows from this bounded path.

## Alternatives

Treating the second filename as batch execution ignores the sw32.ini source
and configuration-marker consumers. Treating its first byte as the original
producer's direct return ignores the intervening record read. Treating either
marker search as an independent reset ignores their shared record and lack
of a position-reset call. Treating shipped strings as admitted caller pointers
assumes actual DS and frame/string guarantees not established here.

## How to reproduce

At revision 7f878d5 require FND-EXE-350's SOUND_DS.EXE identity:
length 204593, XXH3-128 236c2dc23c071eca421eb5b427caee57.
Decode shipped half-open offsets 0x00006EB3..0x00006F64 at IP 01D3,
modeled CS 158E, using locked Capstone 5.0.7 in sixteen-bit mode.
Read file-data only over 0x0000FD3B..0x0000FD57 and bind its
source offsets 05DB, 05DE, 05E0, 05E9, 05EC and 05F1 under
segment 1E36 and MZ header size 1400. Use FND-EXE-360's native
call bindings. Track the retained byte, separate stack strings, source order,
argument cleanup, second record stores and sequential matcher outcomes.
Original bytes stay outside Git; no original process, DOSBox, interrupt
thunk or emulated call is executed.
