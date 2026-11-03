---
id: FND-SCRIPT-030
title: No source-declared adjacent relocated pointer pair names either GPLI conversion
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x00000000..0x0009AE30
tool: executable-reader 4.3.0 pointers through tools/evidence/report.mjs at 4d65d9fa; source-derived MZ/FBOV tables; overlay_listing.py for the independent fixup control
environment: null
---

## Observation

FND-SAVE-023 gives the known direct calls of the two GPLI conversion
routines. Q-SCRIPT-020 retains other transfer representations. This reading
checks source-declared adjacent offset/segment word pairs, not another
direct-call encoding search or a runtime incoming-call proof.

### Both target selectors resolve to the independently known entries

FND-EXE-596 maps forward conversion to overlay 187 +1615, file 0x71445,
through resident trampoline 56EF:00A7. Reverse conversion is +1829,
file 0x71659, through 56EF:00AC. The two queries independently supply
those canonical file targets and loaded addresses, so disagreement in
selector mapping is rejected rather than silently changing the target.

The reader identifies 4703 MZ relocation operands and 8262 FBOV fixup
operands, from 229 descriptors, 49 overlays and 854 trampolines. For
each selected target, 12,844 adjacent pairs were inspected within the
representation. No exact pair or aliased pair names either conversion.
There are zero unresolved pairs and no result truncation.

The other 121 segment operands are explicitly excluded: 120 preceding
word/segment combinations form nonwrapping addresses outside the resident
load image, and one preceding word and segment word do not lie in a
single source range. They are not admitted resident trampoline pointers
in this representation. They remain classified exclusions, not silently
omitted bytes or evidence about runtime pointer construction.

The source identity is DSUN.EXE, size 634416, XXH3
e296af55ba2ecde7e77f555c90f33d0b. Repeated forward and reverse reports
are each byte-identical. The report's qualified negative-use flag is true
for both full queries; its scope remains the adjacent relocated-pair model.

### Matching controls cover data, calls, trampoline mapping and aliases

The full queries require these independently known segment-operand sites:

| Representation | Segment-operand file site | Independently known target |
|---|---|---|
| MZ data pointer from FND-COMBAT-105 | 0x41342 | 4C10:0021, canonical file 0x41321 |
| MZ far call from FND-CONFIG-136 | 0x0D46E | 5787:0066, canonical overlay entry file 0x8BF29 |
| FBOV far call in FND-SAVE-023's forward converter | 0x71466 | 38FF:05B5, canonical resident file 0x2E7A5 |

These sites are all inspected and resolve to the expected canonical targets.
Separate target-matching queries also find their controlled rows, rather
than merely proving that unrelated relocations were visited. The data
query has two exact pairs; the trampoline-call query has one exact and one
alias pair; the resident resource-size query has sixteen exact pairs.

A separate query spells the data target as 4C11:0011, the same physical
resident byte as 4C10:0021. Its controlled row is classified as an alias:
zero exact pairs and two aliases. This is a query-address control, not a
claim that the game actually uses that alternative spelling at runtime.

Changing the required control to file 0x71467 causes an explicit missed,
excluded or unresolved positive-control refusal. Repeating the forward
query with result limit one reports truncation and disables negative use.
The accepted full queries use limit 10,000. The source-table analysis runs
in Node; no original program is executed.

## Interpretation

The adjacent relocated/fixed-up stored-pointer representation adds no
conversion-entry route to FND-SAVE-023's direct-call set. Q-SCRIPT-020
does not need to repeat this representation scan with unchanged source,
tool, controls and mappings.

Computed targets, unrelocated pairs, split pointer construction, runtime
copies and actual indirect use remain outside it. The excluded boundary
pair is not interpreted as a valid runtime pointer. No universal absence
of additional callers, live target immutability or complete reading is
established. Neither format's confidence or parity status changes.

## Alternatives

- An empty target match from an unchecked parser could be a mapping failure.
  The canonical-target checks, independently matched MZ/FBOV controls and
  alias control rule out that reading for the declared query representation.
- The zero match does not exclude a target built from separate words or
  selected through runtime state. Those representations remain Q-SCRIPT-020.
- A relocation candidate by itself would not prove instruction ownership or
  runtime pointer use. This result does not relabel arbitrary data as code.
- Ignoring a missed control or a truncated output would invalidate a negative;
  the refusal and cap controls demonstrate those boundaries explicitly.

## How to reproduce

Use executable-reader 4.3.0 and the repository wrapper at the tool commit,
with sourceKind mz, load segment 0x1000, the source identity above and
command `x86-pointers`. For forward conversion set query segment 0x56EF,
offset 0x00A7 and target 0x71445; reverse uses segment 0x56EF, offset
0x00AC and target 0x71659. Set controls to file segment-operand sites
0x41342, 0x0D46E and 0x71466 and result limit 10000. Keep all exact,
alias, unresolved and excluded classifications and check every control.

For separate matching controls use these loaded query/target/control triples:
4C10:0021 / 0x41321 / 0x41342;
5787:0066 / 0x8BF29 / 0x0D46E;
38FF:05B5 / 0x2E7A5 / 0x71466;
and 4C11:0011 / 0x41321 / 0x41342 for the alias control.
Each uses the same source identity and limit. Require the controlled site
in the exact-or-alias rows, specifically an alias for the last query.

FND-COMBAT-105 independently supplies the data binding; FND-CONFIG-136
supplies the MZ call. For the FBOV call, read descriptor 187 file
0x71445..0x714A8 with overlay_listing.py: the declared entry's call at
0x71463 has its segment operand at 0x71466 and resolved target 38FF:05B5.
Use FND-EXE-596 for the two conversion trampoline/body mappings. Do not
derive target identities solely from the empty-query results.

For refusal, replace the forward controls with 0x71467 and require failure.
For the cap control, retain its three valid controls and set limit one;
require truncated true and negativeUsable false. Repeat both accepted full
queries and compare report bytes. Accepted report SHA-256 values are:

- forward: bca90c26f99cc394ec82310ff0838a97d54b1c4e5fd46816e67057e72eb5db53;
- reverse: ce29cb47d030ca2edf2e4449f737e5dd15d7af099197342b4304aae131d40e4f.

Configs and reports remain under
GAME_DIR/analysis/conversion-incoming-20261103/: forward/reverse-pointers,
their repeated reports, control-mz-data, control-mz-call, control-fbov-call,
control-data-alias, missed-control and one-row-cap. The independent bounded
listing is forward-header-control.txt. No original code, resource bytes
or pointer dumps are committed; this entry records query identities and
qualified aggregate results only.
