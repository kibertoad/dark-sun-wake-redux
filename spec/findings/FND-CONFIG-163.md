---
id: FND-CONFIG-163
title: The callback setter's guard reaches a mode-one runtime path that bypasses the exit-callback loop
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 39D1:0418
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:2E48
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:03EE
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:0388
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:019E
tool: Python 3.14.7 and Capstone 5.0.7 entry-based bounded 16-bit instruction checks and declared MZ relocation mapping
environment: null
---

## Observation

The complete resident callback setter 39D1:0418 occupies
file span `0x0002F328..0x0002F340`, ending with far
return at `0x0002F33F`. Before loading its supplied far
pointer and writing current DS:A0F1, it compares the word
at current DS:009C with SP. An unsigned-below result
skips the guard call; equality or above calls 1000:2E48.
The segment operand at `0x0002F334` is a declared MZ
relocation from raw zero to mapped segment 1000.
FND-CONFIG-083 records the pointer setter and
FND-CONFIG-162 reads its named error-helper caller.

Guard 2E48's own span is `0x00008048..0x0000805A`.
It saves DS in BX, loads DS from CS, sets DX to 2E36 and
AH to nine, and issues interrupt 21h. If control returns,
it restores DS from BX and jumps to resident 03EE. The
jump is a tail continuation, not a local return. The next
linear body at 2E5A is outside this path. The interrupt's
response and preservation of BX are external conditions;
no diagnostic writing is copied here.

The complete far wrapper 03EE, file span
`0x000055EE..0x00005600`, forwards three words to near
0388: its own stack word at BP+6, word zero, and word
one. The last supplied word becomes 0388's mode argument.
This guard call has not supplied an explicit result-code
argument, so the stack-derived word is not assigned a
fixed failure code here.

Near 0388, file span `0x00005588..0x000055DF`, tests
that mode word before its callback-table loop. Mode one
jumps past the loop, its following local 0163 call and
indirect call through current DS:3676. It reaches local
far calls 01F3 and 0176, supplied by push CS/near-call
pairs. The middle argument is zero, so after those calls
return, mode one also skips indirect calls through current
DS:367A and 367E and forwards the first argument to
local far entry 019E. The local 0388 return, if reached,
removes six argument bytes. Mode zero's callback loop is
recorded separately in FND-CONFIG-061.

Entry 019E issues interrupt 21h with AH equal to 4C and
AL loaded from its supplied stack argument. This is the
termination request already identified in the runtime
path of FND-CONFIG-061 and FND-CONFIG-062. The request
is at file offset `0x000053A5`. If it returns, the local
continuation changes CX and DX and jumps to 02B3; that
fallback is not characterized here. No ordinary return
is inferred from the earlier runtime wrapper's return.

The mode-one branch therefore bypasses the specific
exit-callback-table loop that can enter the registered
archive close-all callback (FND-CONFIG-061). This does
not rule out effects of other cleanup callees, the
interrupt or the uncharacterized fallback.

## Interpretation

The callback setter's pointer write has a prior runtime
guard dependency. Its guard route uses a different mode
from the ordinary main-return callback-table route.
Neither successful callback registration nor execution of
that registered archive-cleanup loop follows merely from
entering this setter. Actual guard occurrence, diagnostic
output and operating-system termination are not observed.

## Alternatives

Q-CONFIG-008 and Q-SCRIPT-003 retain the guard's current
DS:009C and SP inputs, its caller's stack provenance,
interrupt responses, BX/DS preservation, cleanup callees
01F3 and 0176, and fallback 02B3. One reading passes the
unsigned guard and writes the pointer directly; another
enters this interrupt/runtime path before that write.
Complete producers and external outcomes distinguish their
reachability and whether the setter continuation is reached.

The callback-loop bypass is a local mode distinction,
not proof that no archive closes elsewhere. The zero-mode
route and its conditional registrations remain as recorded
in FND-CONFIG-061; they cannot be substituted for this
mode-one branch. Native interrupt outcomes and observed
termination remain outside resident emulated-call evidence.

## How to reproduce

Read 39D1:0418 through 042F, resolve the declared guard
call relocation and follow 2E48's jump to 03EE. Stop the
guard reading at its tail jump rather than decoding the
following unrelated linear body as its continuation.
Read 03EE through 03FF and map its pushed argument order
to 0388's near-call frame. Follow mode one through 0388's
callback and indirect-call bypasses and the 019E request.
Keep earlier cleanup returns, the interrupt's response and
its 02B3 fallback uncharacterized. Compare the mode-zero
callback route in FND-CONFIG-061 separately.
