---
id: FND-EXE-360
title: Sound utility constructs a stack pathname before its existing file-interface call
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 158E:0006..158E:008F
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 158E:06F9..158E:06FF
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1BD4:0318..1BD4:0344
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1000:3A0B..1000:3A4A
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1000:2C11..1000:2C46
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    kind: file-data
    offset: 0x0000FD1F..0x0000FD32
tool: Ghidra 12.1.3, scientific-method-engine 13.6.0, executable-reader 2.5.0 and Capstone 5.0.7
environment: null
---

## Observation

FND-EXE-350's utility suffix maps to a data-segment offset of 0x05C0
under FND-CONFIG-213's segment 1E36 and the 0x1400-byte MZ header.
The shipped file contains `:\autoexec.bat` followed by a zero byte and
`rt` followed by a zero byte. The source's zero byte before the colon is
not the byte this consumer uses as its pathname's first character.

A decoded scalar search for 0x05BF, 0x05C0, 0x05C1, 0xE91F and
0xD904 returns two operands: immediate 0x05C0 at 158E:0057 and
immediate 0xD904 at 1AF6:0016. The second is the independently recorded
sound.ini argument in FND-CONFIG-213. The report covers every disassembled
instruction and does not reach its 300-match cap. Undecoded bytes and
computed references remain outside this search; it is not a writer census.

The entry at 158E:0006 allocates 0x019C stack bytes and saves SI and DI.
It calls 1BD4:0318, writes returned AL to SS-relative BP minus 0x0138,
and writes zero to the following byte. It passes a far pointer formed
with SS and that local offset, and a second pointer formed with current
DS and 0x05C0, to 1000:3A0B. It removes eight argument bytes afterward.
Thus it initializes a separate stack string and supplies the suffix pointer;
this prefix does not write a drive character into the shipped leading-zero
data position. Whether the returned byte is a usable drive character is
not established here.

The append helper at 1000:3A0B loads the first far pointer into ES:DI,
saves its offset in DX, and scans for a zero byte with CX initially
0xFFFF. It keeps the resulting destination endpoint minus one. It then
loads the second far pointer into ES:DI and scans it with a fresh
0xFFFF count, inverts the remaining count and recovers its start. It
selects the source segment through DS and the destination segment through
ES, exchanges the source and destination offsets, and copies that count:
an odd source offset first copies one byte and decrements the count;
the remaining words are copied, followed by a final byte on an odd remainder.
It returns the saved destination offset in AX and its segment in DX,
restoring DS, DI, SI and BP. It does not test whether either scan found
a terminator before count exhaustion, validate extents, or prevent offset
wrapping. These are its local instructions, not admitted caller bounds.

The caller next supplies current DS with offset 0x05CF and the same
SS-relative local far pointer to 1000:2C11, again removing eight bytes.
If current DS is 1E36, the first pointer selects the shipped `rt` mode.
FND-CONFIG-213 identifies the same callee at the sound.ini opening site.
The returned DX:AX pair is saved in SS-relative BP minus two and minus
four, then reloaded and OR-tested. A zero pair sets AX to zero and
jumps to 158E:06F9, which restores DI, SI, SP and BP and far-returns.
A nonzero pair continues at 158E:008F, outside this prefix reading.

The complete local body of 1000:2C11 first calls 1000:2BC7 and saves
its DX:AX pair. A zero pair returns zero DX:AX. Otherwise it pushes
that pair, its first incoming far pointer, its second incoming far pointer,
and zero, then near-calls 1000:2AF6 before common frame restoration and
far return. Those two near callees remain unread here; the local wrapper
alone does not settle the downstream operating-system operation.

The first byte's producer at 1BD4:0318 allocates sixteen stack bytes,
writes 0x3305 at SS-relative BP minus sixteen, and passes that same
far stack pointer twice plus word 0x0021 to 1000:20DF. After removing
ten bytes it reads the byte at BP minus ten, adds 0x40 at byte width,
and restores its frame before far-returning. There is no local status
test before that read or addition. The called wrapper's effects and the
remaining initially unwritten record bytes are not established here.

## Interpretation

This follows the utility's literal suffix into a stack-path constructor
and the interface also used with sound.ini. A stored filename therefore
does not itself establish batch execution. The deeper file-interface
callees, later consumer paths, initial DS, external-wrapper effects,
record initialization and storage lifetime still require reading under
Q-EXE-007. A complete execution exclusion is not claimed.

The source and destination far pointers explicitly carry different segment
choices; their offsets do not require DS to equal SS. The suffix and mode
identities remain conditional on the incoming/current data segment selecting
the source storage described above. Neither the append helper's finite scans
nor the caller's stack reservation proves a safe initialized copy extent.
The game editions' sound.bat consumers remain separate unread targets.
FMT-EXE-006's status remains unchanged; no complete_reading is declared.

## Alternatives

Treating the source's leading zero as a missing in-place drive-letter writer
misses the caller's separate stack string. Treating the producer's byte as
an admitted drive character assumes the unread wrapper's contract and result
record. Treating the callee name as a complete file-operation reading ignores
its two near callees. Treating a 0xFFFF scan count as a destination-capacity
check ignores the absent extent and terminator tests.

## How to reproduce

At revision 18f03bf, use the SOUND_DS.EXE identity in FND-EXE-350.
Query its unchanged saved resident snapshot read-only with noanalysis.
ReportBytePattern's pattern 003A5C6175746F657865632E62617400
maps to 1000:E91F; its exact-start reference list supplies no consumer.
This empty list is a lead only, not an absence claim. Then run the pinned
ReportScalarConstants with the five values listed above, both operand kinds,
and its 300-match cap. Use the independently located 1AF6:0016 argument
as the immediate-kind control; no memory-kind negative claim is made.
ReportInstructionWindow at 158E:0006, count 140, and ReportFunctionSummary
at that entry locate the candidate prefix and common suffix; their analyzer
body is not a native whole-function declaration.

Check the instructions directly with Capstone 5.0.7 in sixteen-bit mode:
shipped 0x00006CE6..0x00006D6F at IP 0x0006,
0x000073D9..0x000073DF at 0x06F9,
0x0000D458..0x0000D484 at 0x0318,
0x00004E0B..0x00004E4A at 0x3A0B, and
0x00004011..0x00004046 at 0x2C11. Resolve operand site/targetOffset
pairs (0x00006D2B, 0x0318), (0x00006D44, 0x3A0B),
(0x00006D57, 0x2C11) and (0x0000D474, 0x20DF) with the committed
operand reporter and loadSegment 0x1000. The first target is natively
1BD4:0318, not its analyzer alias 1000:C058. Inspect utility data only
over shipped interval 0x0000FD1F..0x0000FD32 for the suffix and mode.
All ends are exclusive. Keep reports and instruction/data context outside
Git. No original process, DOSBox or emulated call runs.
