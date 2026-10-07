---
id: FND-EXE-009
title: The disc sound helper names two absent jump labels and one colon-suffixed target
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: CD:SOUND.BAT
    offset: 0x00..0x1309
tool: Node.js 24.21.0, byte-preserving full-file label and jump inspection
environment: null
---

## Observation

In the complete manifest-matching file read of FND-EXE-008, jumps at file
byte offsets 1425 and 1726 target `G_SUCCESS`; the jump at 4623 targets
`RUN_ARIA`. No label with either name occurs anywhere in the file, including
case-insensitive comparison and optional leading whitespace.

The jump at 4386 uses the token `INS_GM1:`. Its apparent corresponding label
at 4487 is `INS_GM1`, preceded by the label-introducing colon and without a
trailing colon. These are distinct byte spellings. The terminal `END` label
at 4867 and the jump to it at 4855 provide a positive label/jump control
located by inspection independently of automated label enumeration.

## Interpretation

The two success-target names have no definitions in this shipped helper.
The small-bank target has an additional spelling difference. This is a
static property of the entire file, not proof of any observed runtime
failure, fallback or successful installer invocation. FMT-EXE-006 retains
the unresolved shell interpretation explicitly.

## Alternatives

Labels beyond a truncated preview are ruled out by the complete read and
manifest hash. Differing case or indentation does not supply the absent
names. Whether the target interpreter normalizes a trailing colon, stops
on a missing label, or handles either case differently is not settled.
External entry into labels and generated or replaced scripts were not
searched and are outside this file-content negative finding.

## How to reproduce

Read all 4873 bytes of `CD:SOUND.BAT` using the path, size/hash checks,
sector layout and 8192-byte limit in FND-EXE-008. Enumerate label lines with
case-insensitive pattern `^\s*:([^\s]+)` and GOTO tokens with
`\bgoto\s+([^\s]+)`. Compare literal tokens case-insensitively; do not silently
remove a trailing colon. Then independently inspect every CRLF-delimited
line beginning with optional whitespace and a colon, and search the entire
byte buffer for ASCII `G_SUCCESS`, `RUN_ARIA`, `INS_GM1` and `END`, checking
each hit's surrounding whole line. Offsets above are zero-based starts of
lines; use ASCII/Latin-1 byte-preserving indexing, not a UTF-8 re-encoding.
Confirm the independently observed `END` definition and reference are
returned. This tests only definitions and literal references in this file,
not callers, generated text or interpreter behavior. Execute nothing.
