---
id: FND-EXE-185
title: Provider setup mode sixteen selects an arm with six-or-ten-byte output and return
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x004162C0..0x004162F4
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x00416952..0x004169CB
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x00416B58..0x00416B62
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    kind: file-data
    offset: 0x0031C4E0..0x0031C4E4
tool: executable-reader 2.4.0, scientific-method-engine 13.5.0 and Ghidra 12.1.3 PUBLIC
environment: null
---

## Observation

FND-EXE-184's builder at `0x004162C0` saves EBX and reserves twenty-four
stack bytes. Its four incoming arguments are then at current `ESP + 0x20`,
`+0x24`, `+0x28` and `+0x2C`. It reads their index into ECX, mode into EDX,
destination offset into EBX, and the fourth argument's low byte into a
local byte at `ESP + 0x17`. It initializes EAX to zero and rejects an
unsigned index greater than `0x7F` before checking the mode. The mode is
compared unsigned with `0x14`; admitted values index dword pointers at
`0x0071F2A0` with scale four. Rejected-input paths remain outside this reading.

FND-EXE-183's caller supplies mode sixteen through FND-EXE-184's outgoing
argument mapping. The fingerprinted PE bytes map table entry sixteen at
file offset 3261664 to target `0x00416952`. Its stored pointer carries
PE relocation type three. The bounded dispatch query from the mode gate
selects that target for input sixteen; it does not establish native index,
object or memory admission, and excludes the preceding index gate.

The selected arm reloads a backing-memory base from `0x01D4A380` for its
writes. Relative to each loaded base and incoming EBX, it first performs
five byte writes at relative offsets zero through four. It tests the local
flag byte. On nonzero, it additionally performs byte writes at offsets
five and six and a word write at offset seven, using the low word of ECX
for that word. It advances EBX by four before its final byte write at
relative offset five, making that last write's offset nine relative to the
incoming EBX. On zero, a separate branch at `0x00416B58` reloads the base
and rejoins the final byte write without advancing EBX, so that write is
at offset five relative to incoming EBX. The payload bytes themselves are
not retained here.

Thus the local write widths total ten bytes on the nonzero path and six
on the zero path. A contiguous output interval additionally requires the
base to remain the same and writes not to alias its storage; neither is
established here. The input destination offset has no local extent check
in the selected arm. This bound on writes is separate from buffer capacity.

After comparing the flag byte with one, the arm uses the resulting borrow
to return six for zero and ten for any nonzero byte. This comparison rereads
the local flag after the output writes; equality with its earlier branch value
requires admission that those writes do not alias the local byte. It releases twenty-four
reserved stack bytes, restores EBX and returns near at `0x004169CA`, with
no extra argument cleanup. FND-EXE-184 consumes EAX at full dword width
as zero versus nonzero; either selected-arm result passes that local test.
No intervening calls appear in this arm.

The two separate source traversals cover 121 bytes at
`0x00416952..0x004169CB` and ten bytes at `0x00416B58..0x00416B62`.
The first lists a conditional tail transfer to the second and its near
return; the second lists its tail transfer back to `0x004169B4`. Neither
lists a call or decoding gap. The second interval was queried independently,
not inferred from the first report's locally complete flag.

## Interpretation

This resolves the selected mode's target, local output count, cleanup and
return consumption for FND-EXE-184's builder dependency. It does not
establish the backing-memory allocation, initialized extent, base writers,
destination admission, generated-code execution or callback dispatch.
Those obligations remain Q-EXE-001 and Q-EXE-010, together with preceding
callee effects and the other setup failure paths. No complete reading follows.

## Alternatives

The callback-looking target is selected by an actual bounded index and
fingerprinted table bytes, not a neighboring global or inferred name.
Treating the selected-arm return as zero-or-one is contradicted by its
six-or-ten result. Treating that result as allocated capacity is unsupported:
it bounds local writes while backing storage and input extent remain unread.

## How to reproduce

Use shipped interpreter XXH3-128 `09861838aa3018346f9f15c9a4f5925c`.
Run the committed wrapper's `x86-dispatch` command with sourceKind `pe32`,
entry file offset 87780, one named region `[87780,87796)` with entries
`[87780]`, and evidence that this is the mode gate, excluding preceding
index admission. Use dispatch site 87789, inputRegister `edx`, indexRegister
`edx`, inputs `[16]`, indexEvidence naming the unsigned bound and unchanged
EDX. Set table start 3261600, count 21, stride four, width four, offset
7467680 (`0x0071F2A0`), countEvidence naming the comparison with `0x14`,
and mappingEvidence naming the fingerprinted PE section and entry sixteen
at file offset 3261664. Supply no register seeds or callee summaries.

Run `x86-bounds` twice with entry 89426 and then 89944. Both configurations
have named regions `[89426,89547)` entries `[89426]` and `[89944,89954)`
entries `[89944]`, with evidence naming the selected arm and its zero-flag
branch respectively. Omit segment/ip and let the reader derive PE mapping.
Preserve tail transfers as exits from each region instead of claiming that
one query traversed both. No original-game execution is involved.

In the saved shipped-PE project, read-only with analysis disabled, run
ReportInstructionWindow at `0x004162C0`, count 32; at `0x00416952`, count
38; and at `0x00416B3A`, count 12. Restrict observations to the declared
locations. Run ReportDataBytes at `0x0071F2E0`, count four, as an analyzer
cross-check only; the dispatch report reads the shipped table independently.
Keep rich reports and payload bytes in the local licensed-source store,
outside Git.
