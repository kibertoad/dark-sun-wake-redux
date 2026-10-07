---
id: FND-EXE-130
title: Full-width reader publishes retained entry flags before mapping and conditionally removes the last reset-list entry
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x00689A70..0x00689B1B
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x006899A2..0x006899EE
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x00689B54..0x00689B86
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x00689BC5..0x00689BC9
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x00689BD0..0x00689C35
tool: Ghidra 12.1.3 PUBLIC bounded full-width publication read and cleanup reading
environment: null
---

## Observation

FND-EXE-128 records retained address A, page J, first-level W0 at local
current ESP plus `0x18` and second-level W1 at plus `0x14`.
FND-EXE-129 records local selector C at plus twelve. C three calls
`0x00417CF0` at `0x00689C2A` with A, (W0 AND
`0xFFFFF000`) plus four times (J AND 1023), and full five. After
ordinary return it clears C to zero and rejoins at `0x00689A7B`.
It does not reload the retained W0/W1 locals before that join or test
the helper's EAX. Effects on globals, saved registers and local aliases
remain conditional on the unread helper.

For C other than three, or after that normal rejoin, it tests W0's low
mask `0x20`. If absent, it freshly reads root word `0x0075B6C8`,
ORs that mask into W0's local low byte, freshly reads backing pointer
`0x01D4A380`, and writes the full retained W0 there at offset
(root shifted twelve) plus four times (J shifted right ten). Shifts,
adds and addresses use thirty-two-bit arithmetic. The byte update keeps
W0's other three local bytes; publication uses that retained full word,
not a fresh backing entry. If mask `0x20` was present it skips both
this local update and backing publication.

It next tests whether W1's local low byte contains both bits of mask
`0x60`. Both present skip the second entry publication. Otherwise
C zero ORs `0x60` into that low byte; C nonzero ORs only `0x20`
into it. Neither path clears an already present bit. It then freshly reads
backing pointer `0x01D4A380` and writes full retained W1 at offset
(W0 AND `0xFFFFF000`) plus four times (J AND 1023). The W0
used is the retained local word after its possible low-byte update.
Neither publication checks backing extent, reloads the entry being
replaced or restores its old value locally. They precede mapping and read.

It forms K by logically shifting retained full W1 right twelve. The
mapping/read selection is:

| C at this gate | Publication call with J and K | Selector before subsequent read |
| --- | --- | --- |
| zero | `0x004180A0` | explicitly zero after publisher returns |
| one | `0x004180A0` | set to one before publisher call |
| other nonzero | `0x00417F90` | explicitly zero after publisher returns |

FND-EXE-100 records both publishers' ordered mapping, list and indirect
call effects. This caller does not test either publisher's EAX. It calls
full-width reader `0x004F6740` with original A after an ordinary
publication return. C one's route retains its pre-call selector in EBX;
other routes explicitly clear EBX at the shared join before the read.
These local assignments do not independently prove unread indirect
callees preserve the selector or other saved values.

After an ordinary read return it tests current EBX, saves the full read
EAX in ESI without changing those flags, and returns the saved full value
immediately when EBX is zero. Nonzero reads fresh full reset-list count
N at `0x01B5B6D0`. N zero skips the last-entry read. N nonzero
reads full L at `0x01B5B6D0` plus four times N, corresponding to
the last entry when the list starts four bytes after its count. It compares
L with original A logically shifted right twelve. No local bound on N
precedes the indexed read, and this is not proof of list backing extent.

An equal last entry first publishes N minus one to the count, then in
order clears direct read mapping `0x0075B6D0[L]`, clears direct
write mapping `0x00B5B6D0[L]`, publishes fixed object
`0x01B7BB28` to fallback read mapping `0x00F5B6D0[L]`, then
to fallback write mapping `0x0135B6D0[L]`. It does not erase the
old list entry or clear metadata in this local reset. A nonmatching last
entry leaves count and those four mappings unchanged here. Both paths
then compare current EBX unsigned with one. At most one returns the
saved full read result. A larger selector calls `0x00417F90` with
original A shifted right twelve and that selector, then returns the saved
full result after ordinary completion, ignoring the publisher's EAX.
This shared cleanup body alone does not establish that the larger-selector
route occurs from the locally initialized selectors under valid callee
preservation.

All ordinary local return paths preserve the read value's full width;
there is no AL/AX truncation or conversion to a success/count result.
Cleanup and any later publication occur after the read and may change
state without changing its saved value. Exceptional completion, reentry,
array aliases, register/local preservation and publisher effects remain
unresolved; no local rollback is implied.

## Interpretation

The selector gates feed ordered retained-entry publication, a separate
mapping call, full-width reentry and post-read last-entry cleanup. The
count used for cleanup is fresh after the read, while the entry payloads
published before it are retained locals. Q-EXE-009 in FMT-EXE-006 remains
open for helper and publisher indirect effects, mapping/list/root/entry
producers and lifetime, caller input admission and remaining selected-callee
branches. These local contracts do not establish complete mapping safety,
termination or actual PATH behavior.

## Alternatives

- C three's helper return resets the selector but does not locally refresh
  W0/W1; fresh backing pointers are not fresh entry payloads.
- Entry updates are low-byte ORs followed by full retained-word stores,
  not unconditional replacement with a newly read backing word.
- C one and other nonzero values select different publishers/read-selector
  initialization, and publication results are not the read result.
- Last-entry cleanup uses the post-read count and conditionally removes
  one matching last entry, not a search for every occurrence of J.
- A cleanup publication can change mappings while the full read result
  stays saved; a normally returning call does not prove rollback or safety.

## How to reproduce

Verify FND-EXE-011's executable identity, FND-EXE-099's physical controls
and FND-EXE-126's full-width target. Use the saved Ghidra program with
-noanalysis and ReportInstructionWindow.java at `0x00689A70`
limit 42, `0x00689C06` limit thirteen, `0x00689B04` limit
seven, `0x00689BF0` limit seven, `0x00689B54` limit fourteen
and `0x00689BD0` limit thirteen. Read `0x00689991` limit
sixty for the shared publication/read/cleanup join, and `0x00689B8B`
limit twenty-one for only the byte update at `0x00689BC5` and its
rejoin. Restrict claims to the declared ranges, excluding following
methods and already-recorded selection gates. Track byte versus full
local writes, fresh global pointers versus retained words, each outgoing
slot's last writer, selector publication before or after the publisher,
full read-result retention, fresh count and matching last-entry test,
ordered reset stores and full return through later publication. Keep
helper/register/alias conditions explicit. No native or emulated execution
is part of this observation.
