---
id: FND-EXE-174
title: Corrected relocation mapping retains physically non-code overlay body fragments
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
    offset: 0x0006D081..0x0006D08F
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x00067D99..0x00067DCF
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x00077F40..0x00077F41
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x0008716E..0x000871B7
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x000870C4..0x00087143
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x000499DC..0x000499FE
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x00094E46..0x00094E49
  - build: BLD-GOG-EN-1.1
    file: CD:DSUN.EXE
    kind: file-data
    offset: 0x00055519..0x0005553D
  - build: BLD-GOG-EN-1.1
    file: CD:DSUN.EXE
    kind: file-data
    offset: 0x0006AD00..0x0006AD70
  - build: BLD-GOG-EN-1.1
    file: CD:DSUN.EXE
    kind: file-data
    offset: 0x00072D6B..0x00072E3B
  - build: BLD-GOG-EN-1.1
    file: CD:DSUN.EXE
    kind: file-data
    offset: 0x0007497D..0x000749AB
  - build: BLD-GOG-EN-1.1
    file: CD:DSUN.EXE
    kind: file-data
    offset: 0x00077F35..0x00077F36
  - build: BLD-GOG-EN-1.1
    file: CD:DSUN.EXE
    kind: file-data
    offset: 0x00081642..0x000816AE
  - build: BLD-GOG-EN-1.1
    file: CD:DSUN.EXE
    kind: file-data
    offset: 0x000870C4..0x0008710E
  - build: BLD-GOG-EN-1.1
    file: CD:DSUN.EXE
    kind: file-data
    offset: 0x00087278..0x00087285
  - build: BLD-GOG-EN-1.1
    file: CD:DSUN.EXE
    kind: file-data
    offset: 0x00095EE2..0x00095F74
  - build: BLD-GOG-EN-1.1
    file: CD:DSUN.EXE
    kind: file-data
    offset: 0x0009933E..0x00099380
tool: Ghidra 12.1.3 PUBLIC and bounded source-layout classifier at b69bbdd
environment: null
---

## Observation

Fresh overlay analysis after correcting the derivative MZ relocation-pair order
still assigns seventeen out-of-row body spans to entries inside overlay code:
seven installed and ten disc spans. The table gives every span and its complete
physical partition. The classification reads the shipped files, not the mapped
derivatives. A resident fragment can be code without validating its ownership.

| File | Function entry (descriptor) | Complete anomalous span partition |
|---|---|---|
| `DSUN.EXE` | `0x0006B581` (183) | 0x0006D081..0x0006D08F zero-padding |
| `DSUN.EXE` | `0x0006C01D` (183) | 0x00067D99..0x00067DCF overlay-fixups |
| `DSUN.EXE` | `0x000796DB` (190) | 0x00077F40..0x00077F41 overlay-fixups |
| `DSUN.EXE` | `0x00087859` (198) | 0x0008716E..0x000871B7 overlay-fixups |
| `DSUN.EXE` | `0x0008AA6A` (202) | 0x000870C4..0x00087143 overlay-fixups |
| `DSUN.EXE` | `0x00092A83` (208) | 0x000499DC..0x000499FE resident-load-image |
| `DSUN.EXE` | `0x0009439E` (209) | 0x00094E46..0x00094E49 overlay-fixups |
| `CD:DSUN.EXE` | `0x0005E2ED` (173) | 0x00055519..0x0005553D resident-load-image |
| `CD:DSUN.EXE` | `0x00069F69` (182) | 0x0006AD00..0x0006AD70 overlay-fixups |
| `CD:DSUN.EXE` | `0x0006B9C8` (183) | 0x00072D6B..0x00072E18 overlay-fixups; 0x00072E18..0x00072E3B zero-padding |
| `CD:DSUN.EXE` | `0x0006EA00` (184) | 0x0007497D..0x00074988 overlay-fixups; 0x00074988..0x000749AB zero-padding |
| `CD:DSUN.EXE` | `0x000796AB` (190) | 0x00077F35..0x00077F36 overlay-fixups |
| `CD:DSUN.EXE` | `0x0007A577` (190) | 0x00081642..0x000816AE overlay-fixups |
| `CD:DSUN.EXE` | `0x00087929` (198) | 0x000870C4..0x0008710E overlay-fixups |
| `CD:DSUN.EXE` | `0x00088A01` (199) | 0x00087278..0x00087285 overlay-fixups |
| `CD:DSUN.EXE` | `0x0009674B` (211) | 0x00095EE2..0x00095F1A overlay-fixups; 0x00095F1A..0x00095F30 zero-padding; 0x00095F30..0x00095F74 overlay-code |
| `CD:DSUN.EXE` | `0x00097BED` (211) | 0x0009933E..0x00099371 overlay-fixups; 0x00099371..0x00099380 zero-padding |

## Interpretation

The relocation repair alone does not reconcile native function boundaries.
Fixup lists and padding do not become native code because an analyzer includes
them in a function body. Do not widen Code ranges or omit suspect fragments
to force a clean report. Native segment admission and transfer producers remain
open under Q-EXE-010. This is not a complete reading.

## Alternatives

FND-EXE-173 records the earlier snapshots and its segment-alias comparison;
it remains historical evidence and is not superseded by a different snapshot.
The reading that corrected relocation pairs alone remove all non-code body
fragments is ruled out by the fresh partitions. A remaining alias or target
admission error, rather than native execution of these bytes, is still open.

## How to reproduce

Use New-FbovMappedImage.ps1 at 7e48eb9 (mapper contract revision 2).
Installed source XXH3-128: e296af55ba2ecde7e77f555c90f33d0b; disc source:
318cd5ec0559901add3780097162a919. Derivative hashes respectively:
f48148049b7bd26475845bfcbeb2d11e and f18924d7d9dfc8f62c1371bb5eb1f17f.
Import each derivative into a fresh Ghidra 12.1.3 PUBLIC project with default
analysis. Export each saved snapshot twice with -noanalysis -readOnly using
ExportResearchBaseline.java (SHA256
28ffd9e2f7a195e9d2d6db7f522b4301f736c48cec3a5f9221dd73a1b77f4725).
Select every overlay code interval that the bounded source reader gives for
all 49 descriptors; convert mapped load addresses back to original shipped-file
offsets using load segment 0x1000 and each source MZ header. Both exports
agree exactly for inventory, provenance and region-partition sidecars.

Run tools/evidence/report.mjs overlay-bodies against each original, with
sourceKind mz, the source hash above, formatControls overlays 49 and fixups
8262 installed or 8280 disc. Give ranges with start/end and separate entry
exactly as the table lists; ends are exclusive. The bounded reader validates
fixup dimensions, operand bounds and descriptor-index admission. Every part
is retained, including resident and padding fragments. No native run is used.
