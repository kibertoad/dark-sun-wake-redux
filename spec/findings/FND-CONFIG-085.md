---
id: FND-CONFIG-085
title: Overlay 182 installs the resident key callback and clears it on separate paths
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 56BD:11EF..56BD:1AE1
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 28C9:0CFF
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 571F:0034
tool: Python 3.14.7 FBOV fixup inspection; Capstone 5.0.7 bounded 16-bit disassembly
environment: null
---

## Observation

Six direct far calls in overlay 182 resolve through descriptor 190's
`571F:0034` global-callback setter (FND-CONFIG-084). Three pass the same
nonzero far pointer and three pass zero:

| Call file offset | Argument | Adjacent state write |
|---|---|---|
| `0x00069A3F` | `28C9:0CFF` | Stores a returned far pointer at `0340:00D7`; no adjacent `DS:0D9C` write was observed. |
| `0x00069DE6` | Zero | Sets `DS:0D9C` to zero. |
| `0x00069ECB` | Zero | Sets `DS:0D9C` to zero. |
| `0x00069FA1` | `28C9:0CFF` | Sets `DS:0D9C` to one. |
| `0x0006A209` | Zero | Sets `DS:0D9C` to zero. |
| `0x0006A331` | `28C9:0CFF` | Sets `DS:0D9C` to one. |

The nonzero pointer uses the resident segment's encoded FBOV descriptor 22
(`0x00B0`) with offset `0x0CFF`. FND-AI-004 reads that resident routine as a
key-event handler. The setter copies any passed pointer into `DS:61A2` and
then `DS:A0F1` (FND-CONFIG-083), so these calls can replace the list callback
or clear the global fallback if it is the current value.

## Interpretation

Overlay 182 has paths that register the resident key-event callback and
paths that clear the global callback. The paired `DS:0D9C` writes accompany
five of the six sites, but this bounded reading does not assign a meaning
to that word or establish the order of these paths relative to the
stored-character list's lifetime.

## Alternatives

FND-CONFIG-169 subsequently reads the complete local body containing
site 0x0006A331, reached after a re-read two/three mode gate and intervening
calls. That bounds one site's local timing, while upstream state,
transitive effects and order relative to the list remain open.

These calls may run before the list callback is registered, after it is
removed, or while it is live. Earlier guards, callers, and indirect cleanup
effects remain to be read. No site shown here proves that the list's global
fallback is replaced during ordinary list input.

## How to reproduce

Select overlay 182's declared FBOV fixups targeting descriptor 190 offset
`0x0034`. Disassemble bounded windows around file offsets `0x00069A3F`,
`0x00069DE6`, `0x00069ECB`, `0x00069FA1`, `0x0006A209`, and `0x0006A331`.
Resolve the `0x00B0:0x0CFF` argument through resident descriptor 22 and
compare its target with FND-AI-004. Follow the setter as in FND-CONFIG-083.
