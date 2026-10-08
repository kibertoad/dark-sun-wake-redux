---
id: FND-EXE-255
title: Setup helper publishes its repeat-entry gate before external failures and duplicates the final status byte
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4AE5:0E3D..4AE5:0E4F
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4AE5:0E50..4AE5:0E5B
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4AE5:0E5C..4AE5:0EA2
tool: executable-reader 2.5.0 and Capstone 5.0.7
environment: null
---

## Observation

FND-EXE-254's helper at `4AE5:0E3D` covers thirty-eight instructions
and ninety-nine bytes across the three intervals above, excluding one
byte at each of `4AE5:0E4F` and `4AE5:0E5B`. It has three interrupt
sites, no calls and a far return with six bytes of additional cleanup.
It saves BP and DS, forms a BP frame and loads DS through CS-relative
word five. Argument reads use SS-relative BP storage, not the loaded
state DS by numerical offset alone.

Bit zero of DS-relative byte `0x0038` must be set; otherwise it returns
AX `0xFFFF`. If bit zero is set and bit one is already set, it clears
AX and returns before reading arguments or requesting any interrupt.
Only bit zero set with bit one clear enters setup. It sets bit one
immediately, then reads SS:BP plus eight into AX and stores it to
DS-relative word `0x0034`. It reads SS:BP plus six into BX and stores
that word to `0x0036`, then reads SS:BP plus ten into DX.

FND-EXE-254's pushes place the selected count at the callee's BP plus
six, the caller's BP-plus-eight word at plus eight, and the caller's
BP-plus-six word at plus ten, provided the return frame and argument
storage survive. These are the pushed values as read by the callee;
its caller later reloads its own arguments separately.

DX nonzero skips the first interrupt. DX zero selects AH `0x43` and
interrupts through vector `0x67` at `4AE5:0E77`, then tests returned AH
for zero. Nonzero returns AX `0xFFFF`; zero sets bit two of state byte
`0x0038` before continuing. In either continuing route it stores current
DX to state word `0x0032`, selects AH `0x47`, interrupts at
`4AE5:0E88` and tests returned AH. Nonzero again returns AX `0xFFFF`.
No explicit rollback clears the earlier bit-one, argument-word or
possible bit-two and handle publications on either error path.

After the second status check passes, it reloads DX from current
DS-relative word `0x0032`, selects AH `0x48` and interrupts at
`4AE5:0E94`. It copies returned AH into AL and returns without another
status branch. Thus the explicit final AX contains the same returned
status byte in both halves: value s gives s times 257. It is not a
partial-byte return with an independently retained upper byte. The caller
in FND-EXE-254 tests that full AX for zero. Carry is not consumed by
these status tests; external register effects and state identity after
interrupts remain separate obligations.

All explicit paths restore saved DS and BP and return far, removing
three argument words. These restoration instructions do not prove that
saved words remain intact. The first replacement cleanup body in
FND-EXE-253 tests the bit two written on the first interrupt's zero-status
continuation and reads word `0x0032`; the nonzero incoming-DX route does
not itself set that bit. Its cleanup gates therefore cannot be inferred
from setup's bit-one gate alone.

## Interpretation

This supplies concrete producers for the first replacement cleanup's
flag and handle, and the full-width result consumed by its setup caller.
Publication of the repeat-entry gate precedes failure outcomes. Under
preserved state and a later admitted entry, that bit can cause the helper
to return zero without repeating the failed operation. This is a
conditional counterexample to transactional initialization, not evidence
that the original reaches a failed-then-repeated sequence natively.

Q-EXE-001 and Q-EXE-010 retain actual caller/frame admission, all flag
and argument writers, external results and register preservation,
effective state/stack aliases and complete lifecycle ordering. No
complete_reading or replacement inventory is established.

## Alternatives

A gate set only after successful setup is contradicted by the first store
on the setup arm. A failure that explicitly rolls back that gate is
contradicted by the retained state on the error exits. Treating only AL
as the returned status ignores its duplication into the full word.
Treating every setup route as producing cleanup bit two ignores the
nonzero-DX bypass of the first interrupt and its flag store.

## How to reproduce

At revision `e2c0e99`, use FND-EXE-236's original-source identity, region
and default x86-bounds limits. Set entry and sole entries to `0x00040E8D`,
with no seeds or summaries. Check intervals `0x00040E8D..0x00040E9F`,
`0x00040EA0..0x00040EAB` and `0x00040EAC..0x00040EF2`, thirty-eight
instructions and the far return with six-byte cleanup. The interrupt
sites are `4AE5:0E77`, `4AE5:0E88` and `4AE5:0E94`; the traversal
assumes they return. Its complete field is not a Standard complete reading.

Decode those intervals directly from the shipped source in sixteen-bit
mode with Capstone. Follow each flag gate, ordered word/byte store,
status width, DX reload and return frame. Compare the caller pushes in
FND-EXE-254 and cleanup consumers in FND-EXE-253 independently, retaining
native admission and external effects. Keep source, configurations and
reports in GAME_DIR.
