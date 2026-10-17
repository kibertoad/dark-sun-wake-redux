---
id: FND-COMBAT-052
title: Post-menu restoration rereads mode and conditionally rebuilds input and resource state
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00069090..0x00069118
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006A248..0x0006A40A
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006A581..0x0006A58E
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00059557..0x000595BF
tool: tools/research/exec-census/overlay_listing.py, trampoline_target.py and direct_callers.py; Python 3.12.10, Capstone 5.0.7, xxhash 4.0.1
environment: null
---

## Observation

Trampoline 56BD:00BB resolves to descriptor 182 +19F8 at file
6A248. The end-of-move menu reaches a pushed-CS near call to this
entry at file 6A582, followed by 3D72:0D83. Neither result is tested
before the caller restores its saved registers and returns. This is
the shared continuation of FND-COMBAT-026 and FND-COMBAT-047,
including their unhandled-choice route.

FND-COMBAT-051 separately identifies a returning path in descriptor
172's local +0127 cleanup that calls this same entry through its
trampoline at file 595A5. These are two distinct, qualified local
call sites. The direct-call byte-pattern search reports other
candidates, but their instruction reachability has not all been
qualified. Indirect transfers and jumps are outside that search.
The search is not a whole-program caller census.

### Local mode-dependent restoration

At +19F8 the first stores clear bytes DS:0DA4 and DS:0DA3.
Only when the current word DS:0DAB equals two does it perform this
initial sequence:

1. Visit indices zero through three. At each, load the far pointer
   at DS:19C9 into ES:BX and read the word at offset six of the
   49-byte record. If nonzero and the record byte at offset 14
   hexadecimal is zero, store one in that byte. Offset reloads use
   current DS:19C9 and the ES selected by that iteration's initial
   far load. There are no child calls within the four-iteration loop.
2. Call local +0840, then 362C:06B9.
3. Pass the current word DS:140C to local +0297, then pass a newly
   read DS:140C and zero to 362C:01FA.
4. Pass doubleword 92E0 hexadecimal to 5773:0025, which resolves to
   descriptor 200 +0386. The root removes four argument bytes.

After these returning children, it tests the current DS:0DAB
again. Two or three enters the continuation; any other value
returns. An initial two therefore does not imply the later test
still reads two. Initial three skips the entire four-record and
panel-request sequence but can enter the continuation.

The continuing path calls local +01EF with the current far pointer
at 4E28:00D7, identifiers 4B01 and 61 hexadecimal, and far pointer
28C9:0166. It then passes a null far pointer to 39D1:0430, stores
byte one at DS:143F, calls 2C5F:0139 and 28C9:250A in order, and
clears DS:143F. Failure or abandonment inside a child is not a
rollback to the earlier byte value.

It passes 28C9:152C to 39D1:0448, stores word one at DS:0D9C,
and passes 28C9:0CFF to 571F:0034. FND-PARTY-127 identifies the
latter registration path. If word 4C10:0019 is zero, it copies
the current word 4C13:0369 to DS:426D. Otherwise it leaves that
slot word unchanged locally. It then stores one at DS:0DAB.
These are local mode and slot stores, not proof that every child
completed its native presentation or input-registration work.

When DS:445A is zero, it now returns. Otherwise it stores FFFF at
DS:0FCC and performs conditional resource requests in this order:

- If the far pointer at DS:9BE8 is null, request BMP identifier
  19002 through 38FF:04AB, with output address DS:9BE8.
- If the far pointer at 4E71:0A59 is null, request ICON identifier
  19109 through the same service, with output address 4E71:0A59.
- If the far pointer at 4E71:0A5D is null, request ICON with the
  current DS:445A word zero-extended to a doubleword, using output
  address 4E71:0A5D. Only this third-request route reaches the
  following DS:1440 test. A previously nonnull 0A5D returns without
  making that test.

After each request it tests AX with OR but immediately proceeds
without a conditional branch using those flags. It does not
locally stop on a nonzero request result. After the third request,
only DS:1440 equal to five reaches the final branch: when current
4E71:0A59 is nonnull it passes that pointer and zero to 3D72:12ED;
then, whether that pointer was null or not, it clears DS:9BE7.
The root provides no normalized success result; the menu caller
does not consume its AX.

### Panel-image request and release wrappers

Local +0840 first tests the whole far pointer at DS:0F3B. If null,
it requests BMP 19000 through 38FF:04AB with output address
DS:0F3B. A nonzero AX from that call returns immediately. A
preexisting nonnull first pointer, or zero AX after the request,
reaches the second pointer test. Only when DS:0F3F is null does it
request BMP 19003 using that output address. Its resulting AX does
not control another request. FND-COMBAT-004 independently identifies
the second pointer's status-panel consumer. The local wrapper does
not prove either request produced an admitted image or established
a native drawing result.

Local +088B tests DS:0F3B and DS:0F3F in order. Each nonnull
pointer is passed to 444C:0092, and that service's returned DX:AX
is stored back into the corresponding pointer. The second pointer
test occurs after the first service returns. It does not simply
clear both pointers independently of the service results.

## Interpretation

This replaces FND-COMBAT-008's panel-wrapper and four-record
observations with exact current locations, conditional child results,
post-call mode rereads and the remaining local restoration sequence.
It removes the singular-caller claim. The four-record mark operation
and image-request identities remain directly supported; the game
meaning of mode values and admitted record/pointer storage remain
dependencies rather than a complete combat-start contract.

For Q-COMBAT-016, local branch order, flag widths, slot selection,
conditional resource requests and ignored root result are answered.
Full child effects, input/mode/pointer producers, aliases, lifetime
and native presentation remain open. The final resident call is
already read in FND-CONFIG-171, with conditional bracket preservation
in FND-CONFIG-175. Its callback targets and wider children remain
dependencies; it does not supply an assumed successful refresh.

## Alternatives

- FND-COMBAT-008's single direct caller does not account for the
  qualified menu near call and cleanup trampoline call. Raw search
  candidates do not justify replacing it with another exact total.
- Treating the initial mode test as a snapshot misses the later
  rereads and permits the wrong continuation after children return.
- Treating any nonzero resource-service result as an early return
  confuses +0840's first-request gate with +19F8's unbranched tests.
- A nonnull third icon pointer skips the final mode-five branch;
  the final byte clear is not common to all resource paths.
- Calling the root does not prove combat begins, rendering succeeds
  or the final resident callback accepts its inputs.

## How to reproduce

Use installed DSUN.EXE, size 634416, XXH3-128
e296af55ba2ecde7e77f555c90f33d0b, and the locked evidence Python.
Resolve 56BD:00BB with trampoline_target.py. Read overlay_listing.py
descriptor 182 at 69090..69118 and 6A248..6A40A, hexadecimal,
half-open, using the committed function inventory bounds. Follow the
known menu continuation at 6A581..6A58E from FND-COMBAT-047's root,
and descriptor 172 at 59557..595BF from FND-COMBAT-051's cleanup.
The menu near call and cleanup trampoline call are independent
positive controls for those two searched call forms.

Run direct_callers.py for 182+19F8 only as a candidate inventory;
do not turn raw hits into qualified callers or an absence claim.
Check each resource result's following branch, the two mode tests,
post-child pointer reloads and far output addresses. Retain the
distinct wrapper's early exit as a control against interpreting
every OR AX,AX as a branch. Read FND-CONFIG-171 and FND-CONFIG-175
for the existing resident callback-bracket contract. These static
queries execute no original function. Reports remain under
GAME_DIR/analysis/combat-actions/post-menu-20261018/.
