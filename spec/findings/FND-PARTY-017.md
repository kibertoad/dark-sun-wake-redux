---
id: FND-PARTY-017
title: DSUN.EXE holds the gender, origin, ability and alignment labels in runs around the class labels
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 5000:9044..5000:9163
tool: Ghidra 12.1.3, 16-bit real-mode MZ loader, JDK 21.0.12.1
environment: null
---

## Observation

The bytes `5000:9044..5000:9163` hold NUL-terminated upper-case labels in adjacent runs:

| Run | Start | Labels, in order |
|---|---|---|
| Gender | `5000:9044` | Male, Female |
| Origin | `5000:9050` | Human, Dwarf, Elf, Half-Elf, Half-Giant, Halfling, Mul, Thri-Kreen |
| Class | `5000:908C` | the eight of FND-PARTY-016 |
| Ability | `5000:90CD` | STR, DEX, CON, INT, WIS, CHR, each followed by a colon |
| Alignment | `5000:90EB` | Lawful Good, Lawful Neutral, Lawful Evil, Neutral Good, True Neutral, Neutral Evil, Chaotic Good, Chaotic Neutral, Chaotic Evil |

The runs follow each other with no gap, and the alignment run ends at `5000:9163`, where the
format string and heading of the next labels begin. Half-Giant is at `5000:9069`, Thri-Kreen at
`5000:9081`, Lawful Good at `5000:90EB` and Chaotic Neutral at `5000:9146`.

After a full auto-analysis, Ghidra reports no reference to any of those four addresses, and no
decoded instruction has `0x9040`, `0x908C`, `0x90E0` or `0xAB20` as an operand; `5000:AB20`
lies in a table of far pointers to the Game Menu labels.

The View Character captures show a line with the gender and origin and a line with the
alignment, in upper case, in the spelling of these runs (FND-PARTY-020).

## Interpretation

These are the labels of the View Character screen's gender, origin and alignment lines and of
its six ability scores. The origin and alignment orders are the numbering the spec uses (the
glossary terms `origin` and `alignment`), but nothing here shows that the game stores an origin
or alignment as its position in these runs.

## Alternatives

The labels may be reached through a table of pointers built at run time or from overlay code. A
`CHAR` header byte holding these positions was looked for in two records and not found
(FND-PARTY-018).

## How to reproduce

In Ghidra, run `ReportDataBytes` over `5000:9040..5000:9090` and `5000:90E0..5000:9160`,
`ReportReferences` on the four label addresses, and `ReportInstructionText` for the four
operands.
