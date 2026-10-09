---
id: FND-EXE-264
title: Resident name construction has a bounded tail but no local prefix-length guard
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4AE5:0206..4AE5:0263
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4AE5:0263..4AE5:028B
tool: scientific-method-engine 13.6.0, executable-reader 2.5.0 and Capstone 5.0.7
environment: null
---

## Observation

The selected prefix helper saves DS, obtains its source segment through
another segment's word at offset `0x008C`, and scans zero-terminated strings.
Each candidate is compared with five bytes at CS-relative offset nine.
A failed comparison scans through the next zero; a second zero takes the
failure exit, restoring DS and setting carry. A matching candidate whose
next byte is zero also takes that exit. CLD precedes the string operations.
The source segment and admitted string contents remain unestablished.

For a nonempty match, destination DI starts at `0x008C`. ES is loaded by
the relocated immediate at shipped offset `0x00040286`: raw `0x45CE`,
loaded `55CE:0000` under load segment `0x1000`, mapped to shipped base
`0x0004AEE0`. This is FND-EXE-176's initial state base, not a numerical
displacement match alone. The loop copies successive source bytes until
zero or semicolon. It has no length counter or destination-end comparison.
The terminator is not copied. At zero, SI is decremented to point to that
zero again; at semicolon it already points after the separator.

AH retains the last copied byte, initially zero. Unless it is colon or
backslash, one backslash is appended. The helper saves source DS and SI,
calls `4AE5:0263`, restores those two registers and retries at the matching
candidate's value when the returned carry is set. A successful return
restores the original DS. This is conditional control flow: preservation
of other working registers across the interrupt is not established here.

The tail helper reads a far source pointer through SS-relative BP plus six.
Only the pair with both words zero skips copying. Otherwise it sets CX to
12 and copies through the first zero, including that zero, or copies twelve
nonzero bytes and appends one zero. Thus its local output bound is thirteen
bytes, independent of the prefix loop's unbounded admitted input. It uses
the incoming ES:DI; it does not reset the destination. It then loads DS
through the relocated immediate at shipped offset `0x000402CF`, resolving
to the same initial state base, sets DX to `0x008C`, AH to `0x3D` and AL
from DS-relative byte six, and invokes interrupt `0x21` before a near return.
No initialized output, DOS result or actual file-open success is inferred.

## Interpretation

These are concrete indexed byte-write candidates relevant to Q-EXE-010's
state-writer obligations. A direct search for displacement `0x0122` cannot
cover stores through advancing DI. In a single admitted, non-wrapping pass
with the destination segment preserved, let p be copied prefix bytes, s
the optional separator count (zero or one), and t the tail output (zero
through thirteen). The write interval starts at offset `0x008C` and has
p+s+t bytes. It reaches offset `0x0122` when p+s+t exceeds 150. A sufficient
bound excluding that overlap is p+s at most 137. Neither that input bound
nor an actual overlapping native input is established by this reading.

Native source/frame admission, source termination, DI wrap, repeat-entry
state, interrupt preservation, effective live segment identity and other
writers remain open under Q-EXE-001 and Q-EXE-010. There is no absence-of-writer
claim, observed corruption, complete_reading or inventory replacement.

## Alternatives

The tail's twelve-byte source limit does not bound the preceding prefix
copy or total destination output. Conversely, the absence of a local prefix
guard does not prove native overflow: admitted inputs may have a sufficient
bound elsewhere. Matching the relocated initial segment does not prove its
live preservation across intervening calls or repeated invocations.

## How to reproduce

At revision `a2b2023`, use the installed source identity in FND-EXE-236.
Decode shipped offsets `0x00040256..0x000402DB` in sixteen-bit mode with
initial IP `0x0206`, model segment `0x4AE5`, MZ header `0x5200` and load
segment `0x1000`. Independently run the committed x86-bounds wrapper for
entries `0x00040256` and `0x000402B3`, each as the sole region entry, with
sourceKind mz and region start `0x00040050`, exclusive end `0x00042050`,
segment `0x4AE5`, IP zero. Supply no seeds or summaries; defaults are 512
steps, 64 paths and depth eight. The prefix reports 50 instructions and
93 bytes, two near returns, and its assumed returning tail call; the tail
reports 19 instructions and 40 bytes, one near return and an assumed
returning interrupt. These conditional CFG closures do not prove termination.

Run the operand wrapper at sites `0x00040286` and `0x000402CF`, targetOffset
zero, with the same source, hash and sourceKind. Both must resolve as MZ
relocations to the initial state base above. Derive the output bound from
the zero test, twelve-iteration counter and appended zero separately from
the prefix loop. Original source, configurations and reports stay in GAME_DIR.
