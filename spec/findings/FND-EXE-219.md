---
id: FND-EXE-219
title: A physical short-jump candidate reaches the reader from a gap with no controlled direct incoming transfer
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x00600A71..0x00600A73
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x00600A60..0x00600A71
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x00600A80..0x00600A8D
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x006008E7..0x006008E9
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x005F4D59..0x005F4D5B
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x005F5150..0x005F5156
tool: Ghidra 12.1.3 PUBLIC, engine 13.6.0 and controlled physical PE transfer adapter
environment: null
---

## Observation

Extending FND-EXE-208's physical reader search to short unconditional,
short conditional and near conditional opcode forms finds one candidate
targeting `0x00600A80..0x00600A8D`: a two-byte short-jump encoding at
`0x00600A71`, shipped offset `0x001FFE71`, targeting the reader entry.
Neither conditional form supplies a candidate into that interval. These
are raw encodings at every eligible byte, not verified instruction starts.

The independently read controls are:

| Kind | Site | Target | Physical candidates to its one-byte target interval |
|---|---|---|---|
| Short jump | `0x006008E7` | `0x006008D3` | 1 |
| Short conditional | `0x005F4D59` | `0x005F4D40` | 1 |
| Near conditional | `0x005F5150` | `0x005F5322` | 5 |

All three control forms pass. The reader query reports eight candidates
including controls, below its 4096 limit. The other near-conditional control
candidates are unverified encodings; the table does not establish five callers.

The saved listing rejects `0x00600A71` as an instruction start because it
is undisassembled; its next instruction starts at `0x00600A80`. The
preceding count setter in FND-EXE-056 has nine decoded instructions ending
with its one-byte RET at `0x00600A70`, exclusive end `0x00600A71`.
Its ordinary return does not fall through into the gap. This does not exclude
another entry or instruction stream reaching the candidate.

A second physical query targets the entire gap `0x00600A71..0x00600A80`
and includes all five supported forms: near call/jump, short jump and both
conditional forms. It finds no candidate into the gap. Its type-filtered
controls produce respectively 1, 31, 1, 1 and 5 candidates, 39 in total,
below 4096. The near-call and near-jump controls are FND-EXE-208's
independently read sites. Its earlier 240 candidates to the free-target
control were not kind-filtered; the new 31 counts only the requested jump
form. That difference is query selection, not changed source or boundaries.

A separate enumeration of resolved decoded call/jump targets also finds
none into the gap. Independent call control to the reader finds the call
at `0x005F5136`; independent jump control to `0x006008D3` finds two
jumps, including the directly checked short-jump control. All finish below
the 50-result cap, independently of function ownership.

## Interpretation

The reader's excluded short/conditional domains now contain a concrete
candidate that the saved instruction listing does not admit. Its gap has
no incoming transfer in the five controlled physical forms or resolved
decoded domain. Neither a proved second caller nor proved alignment padding
follows. Q-EXE-012 retains its admission through excluded transfers,
alternate streams, prefixes or runtime-written code. Q-EXE-011's segment
identity and Q-EXE-013's input/writer dependencies remain separate.
No inventory boundary or complete_reading declaration changes.

## Alternatives

Relying only on decoded instructions or E8/E9 misses this raw short-jump
candidate. Treating it as ordinary fall-through from the preceding setter
contradicts that setter's terminal return. Treating the empty controlled
gap searches as proof of no indirect, far, computed or generated incoming
route would exceed their domains. Kind-filtered control totals are not
function-discovery changes or evidence that earlier candidates disappeared.

## How to reproduce

Verify shipped size 3,802,624 and XXH3-128
`09861838aa3018346f9f15c9a4f5925c`. Run the pe-transfers adapter from
tooling commit 5c34df2 with sourceKind pe32 and limit 4096. First query
target `0x00600A80..0x00600A8D`, forms jump-rel8, conditional-rel8 and
conditional-rel32, and the table's one-byte target controls, each explicitly
typed with its form. Next query target `0x00600A71..0x00600A80` with those
forms plus call-rel32 and jump-rel32; add typed one-byte controls at
`0x00600A50` and `0x00601D00` from FND-EXE-208. The mapped physical domain
is its shipped `0x00000400..0x002EEBF4`, preferred
`0x00401000..0x006EF7F4`. Inspect target and control indices separately.

In the saved project read-only, analysis disabled, run ReportInstructionWindow
at `0x00600A71` count one and `0x00600A60` count nine. Read one instruction
at each of the table's control sites. Run ReportCallsToRange in mode all,
with start `0x00600A71` and final inclusive address `0x00600A7F`; start
`0x00600A80` and final inclusive address `0x00600A8C`; and start
`0x006008D3` and final inclusive address `0x006008D3`. These are reporter
arguments, not half-open body locations. Retain unresolved/undecoded targets.
The physical scan excludes rel16, LOOP/JCXZ-family, far, indirect, computed,
cross-region, virtual-only, non-executable, unmapped-padding and runtime-written
forms; prefixes and instruction boundaries are unverified. The decoded scan
excludes unresolved targets and undecoded instructions. Keep reports local and
execute no interpreter or game.
