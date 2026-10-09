---
id: FND-CONFIG-164
title: Shared-helper polls return the driver's BX and overwrite one aliased scratch word
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 45B9:0034
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 576C:0BC1
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 576C:0039
tool: Python 3.14.7 and Capstone 5.0.7 entry-based bounded 16-bit instruction checks and declared FBOV/MZ relocation mapping
environment: null
---

## Observation

Resident 45B9:0034's complete local body occupies file
span `0x0003ADC4..0x0003ADE9`, ending with far return
at `0x0003ADE8`. FND-INPUT-010 previously records it
among the INT 33h wrappers. This reading focuses on the
two shared-helper caller sites in FND-CONFIG-161 and
FND-CONFIG-162 rather than re-inventorying those wrappers.

The function saves ES, BX, CX, DX, DI and the incoming
flags, clears interrupt enable, loads AX with three and
issues interrupt 33h. If control returns, it loads the
first supplied far output address from BP+6 and stores
returned CX there. It then loads the second supplied far
output address from BP+0A and stores returned DX there.
Finally it copies returned BX to AX, restores the saved
flags and registers, and returns far. There is no own
other call, loop, callback dispatch or DS write.

Restoring saved flags does not make the initial interrupt
enable state unconditionally set. The interrupt's results,
DS preservation and other effects are external conditions;
the wrapper's own save/restore sequence is not proof of
what the installed driver does.

Overlay 199 code 0BC1 passes the same SS:BP-2 local far
pointer twice at file offsets `0x000884F6..0x00088502`.
Its caller 0C21 does the same at
`0x0008865A..0x00088669`. In each case, under valid and
unchanged call/output storage, the second store overwrites
the first: the scratch word retains returned DX rather
than returned CX. Neither caller tests that word in its
poll condition. Instead it tests bit zero of the returned
AX, which the wrapper copied from the driver's BX.
The callers repeat while that bit is set and continue
when it is clear.

These loops therefore depend on the interrupt's returned
BX sequence. A finite result sequence containing a clear
bit can reach the continuation; a continually set bit
keeps the local back edge active. No iteration count or
observed driver result sequence is supplied by the body.
The wrapper itself does not invoke the newly registered
fallback callback; any driver or other dispatch effect
remains separate from its direct instructions.

## Interpretation

The previously external common poll has a concrete local
register/output contract and a direct interrupt dependency.
The aliased output arguments discard its first stored
word, while the callers' continuation depends on the
returned register's low bit. A successful returning poll
is not evidence that the surrounding loop eventually ends
or that earlier state is preserved by the driver.

## Alternatives

Q-CONFIG-008 and Q-SCRIPT-003 retain driver responses,
valid output storage, incoming flags, DS preservation and
actual input/dispatch state. One reading produces a clear
returned BX bit and continues; another keeps that bit set
or does not return from the interrupt. Static register
copies settle the local predicate, not the live sequence.
No physical-button label or native observation is assigned
by this finding.

Q-SCRIPT-007's resident-only harness cannot establish
what this interrupt returns. Replacing interrupt results
with fixtures can exercise a caller's local predicate,
but does not establish the driver, callback or input
effects and cannot execute the overlay callers themselves.

## How to reproduce

Read resident 45B9:0034 through 0058 from its entry.
Record the flags save/clear/restore, AX service value,
ordered CX and DX stores, returned BX-to-AX copy and
saved registers separately. Revisit the two verified
caller spans from their overlay entries and compare both
pushed far pointers before their call to 45B9:0034.
Follow their exact AX-bit test, not the overwritten local
word, around the back edge. Keep FND-INPUT-010's wrapper
inventory separate from any claim about live results.
