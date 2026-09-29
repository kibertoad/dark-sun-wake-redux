---
id: FND-CONFIG-174
title: The guarded EBOX dependency discards call results and locally returns zero
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 409B:1675
tool: Python 3.14.7 and Capstone 5.0.7 complete local 16-bit entry readings and declared MZ relocation mapping
environment: null
---

## Observation

FND-CONFIG-172's 409B:12A4 calls local far 1675 when
object word +96 has bit 8000 set and current word
DS:A179 equals one. It returns FFFF if that call returns
nonzero AX. FND-CONFIG-171's 4328:0B4C also calls
409B:1675, with pointer current DS:A17F. The complete
local 1675 body occupies file span
`0x00037225..0x000372C0`, ending with far return at
`0x000372BF`.

After its stack-limit guard through 1000:2E48, it
requires nonzero word current DS:A179, nonzero far fields
DS:A17F and A183, a nonnull supplied pointer, and that
pointer equal to DS:A17F. Any failed gate reaches the
common zero return without the subsequent calls or stores.
There is no dereference of the supplied object in this body.

The continuing branch re-reads DS:A179. Nonzero together
with words DS:A18F and A191 both unequal to FFFF calls
3D72:0B84, passes those two words to 2D40:3B1D, then
calls 3D72:0942. Those returned values are not tested.
The supplied word order is A191 as the first word
argument and A18F as the second. Their semantics and
successful external effects remain open.

It then separately compares words current DS:A18F and
A191 with one using signed comparisons. Each strictly
greater value is passed to 1BF3:28C5, A18F first. Other
values skip that respective call; FFFF and other high-bit
words are signed-negative here. These returned values
are also not tested. The continuation sets A191 and
A18F to FFFF in that order, then sets AX to zero and
returns. The earlier bypass reaches that same AX clear.
All external segments were verified through declared MZ
relocations. DS means its value at each corresponding
instruction; transitive preservation remains conditional.

Every local normal-return path therefore supplies zero.
The unread external callees can change state, fail or
not return, but their returned AX is not a local nonzero
producer. Under valid state and ordinary balanced returns,
12A4's nonzero-result edge from this call has no local
origin: its continuing path clears A179, A17F/A183 and
the object bit before returning zero. Its bit-clear
bypass also returns zero, as read in FND-CONFIG-172.

The other normal-return dependency in 409B:0AFB,
3CFA:0544, also explicitly returns zero. Thus 0AFB's
nonnull finite valid path has no locally originating
nonzero result through those helpers. Its own null-input
FFFF remains a distinct local result. This does not
prove that the caller always supplies a valid nonnull
object, that runtime requests succeed, or that every
external dependency returns.

FND-CONFIG-188 and FND-CONFIG-189 subsequently read the
shared 3D72 service guards and complete local bodies. Their
indirect targets, primitive/runtime effects and state producers
remain separate from the returning local gate and assignment paths.

## Interpretation

Two more encoded failure edges test helpers whose local
normal return is explicitly zero. Their presence does
not establish an EBOX cleanup error origin under valid
normally returning state. Local calls and prior writes
still occur, and the outer caller ignores EBOX's result
before requesting the pointer operation. The gate and
signed-word distinctions remain relevant to those effects,
even when the normal result is zero.

## Alternatives

Q-CONFIG-008 and Q-SCRIPT-003 retain A179/A17F/A183 and
A18F/A191 producers, all callers, valid nonnull objects,
DS/state preservation, aliases, guard paths and complete
3D72:0B84, 2D40:3B1D, 3D72:0942 and 1BF3:28C5
effects. One reading supplies continuing gates; another
bypasses a gate or changes state through a callee. The
local effects differ, but both local normal returns are
zero. External non-return or invalid state is not an
observed native failure or a returned nonzero origin.

Resident gate/signed-word cases belong to Q-SCRIPT-007
once supported layouts and the harness exist. They cannot
prove native external outcomes. No native or emulated
observation is claimed.

## How to reproduce

Read 409B:1675 through 170F from its entry. Follow all
five initial gates, the re-read word and FFFF checks,
ordered calls, signed greater-than-one tests and final
word assignments. Resolve every declared MZ segment and
trace all local returns to the common AX clear. Compare
12A4's nonzero-result edge and 0AFB's null and 0544
results in FND-CONFIG-172, and 0B4C's call in
FND-CONFIG-171. Keep discarded results distinct from
accepted operations and successful native returns.
