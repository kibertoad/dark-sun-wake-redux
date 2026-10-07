---
id: FND-EXE-019
title: Compiled external-command drive selection requires an exact colon suffix
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x00593C35..0x00593D09
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x00593E24..0x00593ECA
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x00593EFF..0x00593F30
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x00601D20..0x00601D26
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x00601E50..0x00601E56
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    kind: file-data
    offset: 0x0033807C..0x0033807E
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    kind: file-data
    offset: 0x00338083..0x00338086
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    kind: file-data
    offset: 0x003593A2..0x003593AF
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    kind: file-data
    offset: 0x0035941E..0x00359427
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    kind: file-data
    offset: 0x00359436..0x0035943E
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    kind: file-data
    offset: 0x003597C2..0x003597CA
tool: Ghidra 12.1.3 PUBLIC and independent physical PE import/header reading
environment: null
---

## Observation

FND-EXE-018 leaves the external-command caller's imported conversion and
preceding gates unread. This finding follows those local gates in the
function identified by FND-EXE-014. In its retained frame the command pointer
is at stack offset 4628 and the unconsumed suffix pointer at 4632.

The first suffix byte selects a local setup path: NUL branches to
`0x00593E24`, ASCII space branches to a separate copying helper path, and
another byte takes a different copying setup before joining the command
check at `0x00593C86`. The copying helpers' full effects remain unread.
The following description is conditional on reaching that check with the
stated command pointer intact, or taking the suffix-NUL path directly.

Both checks advance a working command pointer by one byte, then compare its
remaining bytes against the NUL-terminated colon literal. The comparison
uses a two-byte repeated string comparison, so the NUL byte is part of the
match. A match enters classification. A mismatch compares the same remainder
against colon, backslash and NUL with a three-byte repeated comparison.
That match also enters classification; a second mismatch branches to the
filename lookup continuation at `0x00593CE7`. The direction flag is cleared
before these comparisons. The literals are at virtual addresses
`0x0073AE7C` and `0x0073AE83`, physically at the file-data offsets above.
Thus merely containing a colon or beginning with a drive-like prefix does
not satisfy these particular exact-suffix tests.

The classification path reads the first command byte with signed extension.
Its import slot at `0x02431908` names `msvcrt.dll::__mb_cur_max`. When the
integer reached through that slot equals one, the path reads the word for
that signed byte through `msvcrt.dll::_pctype` at slot `0x02431934`, masks it
with 259 and admits a nonzero result. Otherwise it calls the thunk at
`0x00601E50`, which jumps through slot `0x0243192C` to the imported
`msvcrt.dll::_isctype`, passing the signed byte and 259; its full 32-bit
nonzero result admits the path. A zero classification result joins filename
lookup. The external CRT's table contents, locale and behavior for each
possible signed byte remain outside this reading.

An admitted first byte is passed to `0x00601D20`, whose sole instruction
jumps through slot `0x02431A8C` to imported `msvcrt.dll::toupper`. The caller
subtracts 65 from that return's low byte, zero-extends the resulting byte and
calls the selector writer at `0x004B88E0`. FND-EXE-018 describes its return
and update distinction. The caller tests the low return byte and branches
on nonzero to `0x00593E00`; zero enters the diagnostic continuation. Neither
an all-locale input bound nor the full diagnostic outcome is established.

At filename lookup, the command pointer and retained object are passed to
`0x005933F0`. FND-EXE-015 records the lookup's bounded candidate order.
For unchanged command tokens `ravager` and `sound` from FND-EXE-010, the
remainder after their first byte matches neither exact suffix. These gates
therefore select lookup rather than this selector-writer call, conditional
on the parser/dispatcher delivering those same tokens as described in
FND-EXE-013. This does not prove their eventual selected file.

## Interpretation

The differing selector-writer guards in FND-EXE-018 are relevant to the
exact drive-command path, but cannot be treated as the direct branch for
the two bare helper stems merely because they are external commands. Their
local branch proceeds to the filename helper, whose own initial selector
and later drive-object boundaries remain open in Q-EXE-009.

## Alternatives

An arbitrary command prefix followed by more path text selecting this drive
branch is ruled out by its comparisons including the terminating NUL. Treating
only the first byte's classification as the branch gate omits the prior
syntax check. Import names identify the linked CRT API, not its actual loaded
implementation, locale tables or behavior on every input; no universal
alphabetic-range claim is made from those names alone.

## How to reproduce

Use FND-EXE-011's verified PE and image base. Read 100 instructions from
`0x00593BE0` to follow the frame arguments, suffix split, first comparison
and lookup join. Read 24 from `0x00593E24`, then the existing 45-instruction
window from the exact boundary `0x00593E64` for the alternate comparison,
classification, conversion and return test. Read three instructions from
`0x00593F26` to check the copying path's direct join, disregarding the next
branch after that join. Read one instruction each from
`0x00601D20` and `0x00601E50`. Query symbol references for the four imported
names and match their precise slots; Ghidra's relocated external-slot values
are not shipped pointers to readable name bytes.

Independently walk the physical PE's import descriptors, name thunks and
first-thunk slots with bounded, terminated lists. Check that the four names
belong to the same imported DLL and map to the slots above. Convert the two
literal virtual addresses through their section's raw-data mapping and verify
the complete short literals including NUL. Compare both literal sequences
against the named wrapper tokens after their first byte. Interpret only the
cited locations. Keep rich reports local and execute no interpreter or game.
