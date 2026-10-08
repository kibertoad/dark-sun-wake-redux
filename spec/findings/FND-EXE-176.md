---
id: FND-EXE-176
title: Resident loader candidate state holds an initial interrupt-3F vector and far handler pointer
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x00040055..0x00040057
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x0004AEE2..0x0004AEE6
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x0004AFF0..0x0004AFF2
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4AE5:0140..4AE5:016B
tool: executable-reader 2.4.0 and Ghidra 12.1.3 PUBLIC
environment: null
---

## Observation

The word at shipped offset `0x00040055`, corresponding to the candidate
code segment's offset five, has an MZ relocation. Its stored value is
`0x45CE`; loading at segment `0x1000` gives `55CE:0000`, shipped offset
`0x0004AEE0`. This is a relocated segment word, not a segment inferred from
an analyzer variable name.

At this state base, offsets two and four initially hold the two words of
a far pointer: offset `0x04F4` and stored segment `0x3AE5`. The segment
word at shipped offset `0x0004AEE4` also has an MZ relocation. The resulting
initial pointer is `4AE5:04F4`. The word at state offset `0x0110` is
`0x3FCD`; its high byte, at offset `0x0111`, is `0x3F`.

The resident procedure at `4AE5:0140` saves DS, then reads DS through
CS-relative offset five. It reads the byte at state offset `0x0111` into
the vector argument for the DOS get-vector service. It saves that service's
returned segment and offset, reads the vector byte again, obtains the
replacement offset from state offset two and replacement segment from
state offset four, and invokes the DOS set-vector service. After restoring
the state segment, it stores the old vector's offset and segment into
state offsets two and four. Thus those fields are mutable, and after this
call they need not retain their shipped initial handler pointer.

## Interpretation

Under the declared candidate CS binding, the source-derived initial state
connects the vector procedure to interrupt `3Fh` and handler candidate
`4AE5:04F4`. This advances Q-EXE-001 and Q-EXE-010 beyond a literal-DOS-wrapper
search. It does not establish the installed vector at any execution point:
the native caller, preservation of the CS-relative state word, every writer
of the vector/pointer fields, intervening callees and DOS behavior remain
unread. Neither the handler nor the loader has a complete reading.

## Alternatives

Treating state offsets two and four as an immutable handler pointer is
ruled out by their replacement with the old vector. Treating the shipped
vector byte as proof of the live vector is likewise unsupported. Another
writer or an earlier invocation could alter these fields. A linear analyzer
alias does not itself establish the CS-relative state binding.

## How to reproduce

Use the installed original with XXH3-128
`e296af55ba2ecde7e77f555c90f33d0b`, sourceKind `mz`, and default load segment
`0x1000`. Run `node tools/evidence/report.mjs operand <local-config.json>`
with site `0x00040055`, targetOffset zero; separately use site
`0x0004AEE4`, targetOffset zero. Both resolutions must identify MZ relocations.

Run the committed `table` command against the same hash-guarded source with
start `0x0004AEE0`, count one, stride `0x0112`, limit one, and countEvidence
stating one candidate state header with only explicitly accessed fields.
Fields are handlerOffset at offset two, width two; handlerSegment at offset
four, width two; stateWord at offset `0x0110`, width two; vectorByte at
offset `0x0111`, width one. Retain the initial/raw distinction.

In the saved original resident project, run ReportInstructionWindow at
`4AE5:0140`, count 34, with analysis disabled and read-only mode. Restrict
the observation here to `4AE5:0140..4AE5:016B`, excluding the later file
handling that the window also prints. Follow both words and the separately
loaded vector byte, including the stores of the previous vector.
No empty reference search establishes absence of other writers or callers.
Configurations and reports remain in the licensed local store, outside Git.
