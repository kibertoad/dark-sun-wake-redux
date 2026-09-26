---
id: FND-PARTY-016
title: DSUN.EXE holds the eight class labels in one run at 5000:908C, with no direct reference
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 5000:908C..5000:90CD
tool: Ghidra 12.1.3, 16-bit real-mode MZ loader, JDK 21.0.12.1
environment: null
---

## Observation

The 65 bytes at `5000:908C..5000:90CD` hold eight adjacent NUL-terminated upper-case labels, in
this order: Cleric at `908C`, Druid at `9093`, Fighter at `9099`, Gladiator at `90A1`, Preserver
at `90AB`, Psionicist at `90B5`, Ranger at `90C0` and Thief at `90C7`. The run of ability labels
that follows starts at `5000:90CD` (FND-PARTY-017).

After a full auto-analysis, Ghidra reports no reference to the labels at `908C`, `9099`, `90A1`
and `90AB`. The four bytes `8C 90 00 50`, a far pointer to `5000:908C` in stored order, occur
nowhere in the file.

The View Character captures show each member's classes in mixed case, separated by slashes, with
the psionicist class as Psionic (FND-PARTY-020), which is not how this run spells it.

## Interpretation

This is the executable's list of the eight classes in alphabetical order. The order gives the
class numbering the spec uses (the glossary term `character_class`), but nothing here shows that
the game stores a class as its position in this list.

## Alternatives

The labels may be reached through a pointer built at run time or from overlay code. The class
line of the View Character screen is drawn from other text, so these labels may serve another
screen, such as character generation, or none.

## How to reproduce

In Ghidra, run `ReportBytePattern` for each label with its NUL, `ReportDataBytes` over
`5000:908C..5000:90EC`, and `ReportReferences` on each label's address; then search the file for
`8C 90 00 50`.
