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
evidence: [FND-EXE-491, FND-EXE-492, FND-EXE-496, FND-EXE-646, FND-EXE-590, FND-SOUND-019, FND-SOUND-020, FND-SOUND-021, FND-SOUND-022, FND-SOUND-023, FND-SOUND-024, FND-SOUND-025, FND-SOUND-026, FND-SOUND-027, FND-SOUND-037, FND-SOUND-029, FND-SOUND-097, FND-SOUND-098, FND-SOUND-099, FND-SOUND-100, FND-SOUND-101, FND-SOUND-102, FND-SOUND-103, FND-SOUND-104, FND-SOUND-105, FND-SOUND-106]
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

Each directory entry has this structure (FND-EXE-491, FND-EXE-646,
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
(FND-EXE-646, FND-EXE-590). This does not establish every actual setup
selection or successful driver initialization.

## Open questions

- Which remaining setters/callers and state changes affect mode/current
  settings preservation through later driver use, beyond the local tag/name,
  group-to-writer and initial pointer bindings (FND-SOUND-019,
  FND-SOUND-020, FND-SOUND-021, Q-SOUND-007)?
- Which actual list-choice row/index and storage/continuation contracts admit
  the record used by those writers? One reading preserves correspondence
  under the small printable-label, vector and child conditions of
  FND-SOUND-022; other input/state profiles are not established. Actual
  producer/retention evidence must distinguish those cases before native
  admission (FND-SOUND-021, FND-SOUND-022, Q-SOUND-010).
  FND-SOUND-097 binds registered completion ordinals and byte-wide event
  consumption, including transformed-byte continuation; child preservation
  and native table/frame/input admission remain required.

  FND-SOUND-098 settles pure attribute updates, rectangle bounds/copy interfaces
  and conditional delay/copy register restoration. Native BIOS SI/DI/ES,
  port progress, scrolling scratch/coordinate and backing/frame admission remain
  Q-SOUND-010; none is inferred from successful local traversal.

  FND-SOUND-099 assigns the mode-request and geometry/copy-gate producers.
  The loaded enabling word is1, but queried width/rows, copy synchronization
  and retained mode/flag state still require native/history admission.

  FND-SOUND-100 verifies both fresh post-label endpoints: all26 written
  prefixes and populated records match the projection, with six disjoint
  modeled allocation headers. Intermediate and later lifetime, vector/chooser
  state and native index admission remain Q-SOUND-010.

  FND-SOUND-101 observes matching mode/geometry/copy gates and the declared
  vector at two post-label endpoints. Retention to use remains open; the
  chooser-use attempt stopped at the shared handler guard before entry.
  FND-SOUND-102 identifies its caller as164C:050D in two fresh attempts;
  its input/object/frame and continuation contract remains Q-SOUND-010.

  FND-SOUND-103 derives a wrapped handler frame from102's entry state;
  its upper interval intersects100's earlier negative record-block model.
  Simultaneous lifetime/alias admission remains Q-SOUND-010; no native
  overwrite or failure is inferred.

  FND-SOUND-104 verifies both fixed incoming objects, selector zero and
  terminated small text prefixes at two fresh guard stops. Their retention,
  wrapped-frame consumption and input/continuation remain Q-SOUND-010.

  FND-SOUND-105 verifies the fixed notice's first pre-poll endpoint twice:
  wrapped frame, three body rows, one registered choice and matching entry
  objects/prefixes. Later retention, input/return and live allocation ownership
  remain Q-SOUND-010; endpoint coherence is not continuous preservation.

  FND-SOUND-106 observes000D after one Enter event and normal notice
  return0000 in two fresh runs. The queued event remains pending at return;
  full delivery, later continuation and retention remain Q-SOUND-010.
