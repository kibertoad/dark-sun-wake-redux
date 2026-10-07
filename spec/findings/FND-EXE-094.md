---
id: FND-EXE-094
title: PATH source readers separate word-boundary dispatch from bounded byte copy and final terminator
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x004F6570..0x004F6639
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x004F7330..0x004F738B
tool: Ghidra 12.1.3 PUBLIC bounded instruction reading
environment: null
---

## Observation

FND-EXE-093 supplies a computed object-derived address to word reader
`0x004F6570`. It saves its first argument and tests the low twelve bits
against 4094 at unsigned width. Values at most 4094 select one word read;
the only larger low-twelve-bit value, 4095, selects two byte reads. The
single-word path indexes pointer table `0x0075B6D0` by address shifted
right twelve. A nonnull entry reads a 16-bit word at entry plus the full
original address, zero-extends it and returns. It does not add only the
low twelve address bits or prove the table entry's allocated extent.

A null direct entry instead loads object table `0x00F5B6D0` at the same
index, reads its first-word table and calls target offset twelve with
object and original address in two outgoing slots. Only AX is retained
and zero-extended on return. There is no local object/target null guard.
For low offset 4095 the low byte uses the analogous direct byte read,
or that object table's target offset eight and its returned AL. Address
plus one is then computed with 32-bit wrap and independently selects
the next direct/object table entry. Its direct byte or returned AL becomes
the high byte through shift eight and OR with the saved zero-extended low
byte. Both accesses and modes are separate; the initial entry does not
admit the second. The final ordinary return is zero-extended AX.

The separately called copy helper `0x004F7330` reads source position,
destination pointer and full limit from its three arguments. Its loop
first decrements the limit and compares the full result with all ones.
Equality exits to the final terminator store without a source read;
otherwise it saves current source position and increments the retained
source position at 32-bit width before reading a byte. It indexes the
same direct table by saved source shifted right twelve. A nonnull entry
reads byte at entry plus that full saved source. A null entry selects
the object table's virtual target at offset eight, supplying object and
saved source position, then zero-extends AL. Targets, table writers and
storage validity remain unresolved.

Each zero byte exits immediately without copying that byte first. Each
nonzero byte is stored at the current destination and increments the
destination at 32-bit width before repeating the decrement gate. Every
normally completing path stores one terminating zero at the current
destination, including an initial limit of zero and exhaustion with no
source zero. It does not return a normalized length or separate success
status. EAX is not assigned a distinct output value at the final exit,
and callers must not interpret its last read/helper value as a byte count.

For FND-EXE-093's fixed limit 1024, at most 1024 source bytes are read
and at most 1024 nonzero bytes are copied, followed by the terminator
store. A zero source byte consumes a read but is represented by that final
terminator. A source that has no zero within the admitted read count is
truncated and terminated locally. The caller's next position follows its
local string length plus one, not this helper's retained final source.
It does not establish that truncation preserves original record boundaries.
The local buffer starts at caller stack offset 32; the latest terminator
offset is 1056, inside its fixed 1084-byte reservation. This arithmetic
does not prove safety against indirect callee writes, invalid mapped
source storage, aliases or the caller's later unbounded list iterations.

## Interpretation

The initial word read handles the two sides of a 4096-byte boundary
independently, and the record copy has a bounded read count plus a
separate mandatory terminator store. Q-EXE-009 retains table/object
initialization, concrete virtual targets, aliases, source-list extent,
output helpers and wrapper admission. These observations do not identify
the table as a complete memory model or prove actual PATH contents.

## Alternatives

Reading a word across low offset 4095 with one direct access, reusing its
first mapping for the high byte, keeping upper bits of virtual word returns,
reading a source byte with initial copy limit zero, omitting the exhaustion
terminator, or interpreting limit 1024 as total writes including terminator
is ruled out locally. A bounded per-record copy is not a bounded number
of records. The zero source test does not test a virtual callee's full EAX.

## How to reproduce

Verify FND-EXE-011's executable identity and FND-EXE-093's supplied arguments.
Read twenty-eight instructions at `004F6570` and fifty at `004F65CA`,
restricting claims through `004F6639`. Read forty-two at `004F7330`,
retaining only through `004F738B` before later functions. Follow each
source/address last writer, independent direct/virtual branch, low-byte
and low-word truncation, decrement-before-read loop, destination advance
and final terminator. Check arithmetic controls for low offsets 4094 and
4095, wrapped address plus one, limits zero and 1024, early zero and
nonzero exhaustion; these are instruction controls, not native observations
or evidence that the control inputs occur. Keep all reports local and
execute neither original code nor a substitute shell harness.
