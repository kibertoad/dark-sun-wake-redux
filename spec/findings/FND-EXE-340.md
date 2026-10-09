---
id: FND-EXE-340
title: Declared GOG configuration pair lacks exact codepage= and country= tokens
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: dosbox_darksun2.conf
    offset: 0x0000..0x2CB9
  - build: BLD-GOG-EN-1.1
    file: dosbox_darksun2_single.conf
    offset: 0x0000..0x0434
tool: PowerShell 7 physical byte-pattern reporter and executable-reader XXH3
environment: null
---

## Observation

FND-EXE-010 identifies the two configuration files named by the shipped
primary launch task. Their complete source lengths and XXH3-128 identities
are respectively 11449 and fea45ead019fd253fcb5589b9dc84330, and
1076 and b838a6b91eb47f1a3694ed8281305b3e. The files retained those
lengths and modification times across the inspection.

An exact, case-sensitive physical-byte search of each complete file finds
zero occurrences of either ASCII token `codepage=` or `country=`. A
positive control in the first file finds exactly one ASCII
`keyboardlayout=auto` at decimal offset 11430. Reading that setting in
the configuration places it under the `dos` section; its written value
is `auto`, rather than a numeric code-page identifier. A separate positive
control in the second file finds exactly one `[autoexec]` at decimal
offset 64, independently identified by FND-EXE-010.

Every search uses maximumMatches 16384 and completes without truncation.
The search covers comments, section text and command text as physical bytes;
it does not depend on executable mappings or analyzer instruction ownership.

## Interpretation

These bounded observations do not supply the exclusive decoder required by
Q-EXE-005. In particular, the literal automatic keyboard-layout setting
cannot by itself identify the font or code page used for the disc helper's
display bytes. FND-EXE-010 describes an installed GOG launch task, not a
recording of the original disc helper's executing environment.

The negative result applies only to the two exact ASCII tokens in these
two identified files. Other capitalization, whitespace before the equals
sign, different setting names, commands, interpreter defaults, host locale,
mutable files, or a different disc-launch configuration remain outside
this search. No runtime code-page absence or default value is claimed.
FMT-EXE-006 retains an unknown display encoding.

## Alternatives

Treating `auto` as a numeric code-page selection supplies semantics absent
from the declaration. Treating missing literal tokens as proof that no
code page is selected ignores interpreter defaults and excluded forms.
Treating the installed launch settings as the disc helper's observed
environment ignores the separate source and launch identities.

## How to reproduce

At revision 8fd14ac, verify both complete files against the identities
above and BLD-GOG-EN-1.1's manifest using the committed XXH3 helper.
Run the committed tools/ghidra/ReportPhysicalBytePattern.ps1 under
PowerShell 7, with SourcePath naming each file, Pattern as below and
MaximumMatches 16384. For each file,
query hex patterns `63 6F 64 65 70 61 67 65 3D` and
`63 6F 75 6E 74 72 79 3D`. For the first file's control query
`6B 65 79 62 6F 61 72 64 6C 61 79 6F 75 74 3D 61 75 74 6F`;
for the second query `5B 61 75 74 6F 65 78 65 63 5D`.
The complete search intervals are 0..11449 and 0..1076, exclusive ends.
Read the first control's setting and containing section directly in the
source configuration, and use FND-EXE-010's independently identified
autoexec location to check the second control. Keep source text and raw
reports outside Git. No original executable, DOSBox or emulated call runs.
