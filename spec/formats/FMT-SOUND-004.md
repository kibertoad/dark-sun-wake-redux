---
id: FMT-SOUND-004
title: ADV files on the disc
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
files: ["CD:*.ADV"]
byte_order: little
size: null
text: false
definition: fmt_sound_004.ksy
evidence: [FND-EXE-491, FND-EXE-492, FND-EXE-496, FND-EXE-585, FND-EXE-590, FND-SOUND-019]
conflicting: []
split_with: []
related: []
---

## Layout

All nineteen files share a Copy driver envelope with a little-endian
function-directory pointer. Driver code and bytes outside that envelope
remain opaque in this format description (FND-SOUND-019).

| Offset | Size | Type | Name | Meaning | Status | Evidence |
| --- | --- | --- | --- | --- | --- | --- |
| 0 | 2 | UINT16LE | directory_offset | Pointer to function directory; 45 in all nineteen files. | supported | FND-SOUND-019 |
| 2 | 4 | char[4] | signature | Copy. | supported | FND-SOUND-019 |
| 6 | directory_offset - 6 | BYTE[] | opaque_header | Not interpreted here. | supported | FND-SOUND-019 |
| directory_offset | 4 per nonterminal entry, then 2 | Function[] | directory | Function-number/target pairs terminated by number FFFF. | supported | FND-SOUND-019 |
| After directory | Remainder | BYTE[] | opaque_body | Driver code/data; no instruction or device semantics implied. | supported | FND-SOUND-019 |

Each directory entry has this structure (FND-EXE-491, FND-EXE-585,
FND-SOUND-019):

| Offset | Size | Type | Name | Meaning | Status | Evidence |
| --- | --- | --- | --- | --- | --- | --- |
| 0 | 2 | UINT16LE | number | Function selector, or terminal FFFF. | supported | FND-SOUND-019 |
| 2 | 2, absent for FFFF | UINT16LE | target | File-relative target offset, interpreted with the driver's native segment mapping. | supported | FND-SOUND-019 |

Thirteen directories contain 38 entries, five contain seventeen and one
contains nineteen, totaling 598. Their numbers and targets are distinct
within each file; every target follows its directory and lies inside the
file. These are observations, not a universal count or uniqueness rule.
Native target mapping and handler/secondary entries remain separate from
the file envelope (FND-SOUND-019, FND-EXE-496).

## Enumerations and flags

FFFF terminates the function directory. Other selectors identify driver
functions; their device-specific behavior is outside this bounded envelope.

## Differences between builds

None known.

## Coverage

The nineteen `CD:` `.ADV` paths of BLD-GOG-EN-1.1.

The setup utility loads named files from its current device record after
the batch copies disc ADV files into its working directory (FND-EXE-491/492).
The game requests embedded ADV resources; the installed default resource
numbers eleven and eight equal SBP2FM.ADV and SBPDIG.ADV respectively
(FND-EXE-585, FND-EXE-590). This does not establish every actual setup
selection or successful driver initialization.

## Open questions

- Which parser/selection paths populate the setup device record's two
  inline driver names and which remaining loader paths admit them, beyond
  the recorded filename consumers and shipped game-resource matches
  (FND-SOUND-019, Q-SOUND-007)?
