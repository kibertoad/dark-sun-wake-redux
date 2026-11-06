---
id: FND-PARTY-307
title: Character-box selection publishes the new member before redraw and the cast handler's later mode-zero count clear
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000788E9..0x000789F4
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000790C9..0x00079103
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000794D4..0x000794E1
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0009677F..0x00096913
tool: overlay_listing.py and trampoline_target.py at b42aca7e; Python 3.12.10, Capstone 5.0.9, xxhash 4.0.1; retained mode-caller211-second; FND-PARTY-304/306
environment: null
---

## Observation

FND-PARTY-306 leaves empty-member admission and the prior-count
history unresolved. This checks the selection helper actually reached
by the cast window, rather than assuming that an empty record cannot
become selected. The licensed installed DSUN.EXE is size 634416,
XXH3-128 e296af55ba2ecde7e77f555c90f33d0b. Offsets are hexadecimal.

On 211+081B's character-box branch, the no-pending-cast path reaches
+096F. A nonzero 4C10:0019 instead requests the active-character
message and leaves this branch. With that word zero, +0994 records
the old member's view state, then +09AD calls 190+0D89 with a copied
24-byte event and callback 57C1:0043. The relocation/trampoline
mapping is 571F:003E to 190+0D89, file 790C9.

### Selection happens before the empty-record test

190+0D89 first calls 28C9:2ABD, copies the event onto its outgoing
stack, and calls its local 190+05A9 at +0DA1. It does this before
testing the event's word at BP+18 against eight. On the below-eight
branch it then returns through +1195 without entering its additional
character-action menu paths. Those other paths are not needed for
the selection sequence below.

190+05A9 subtracts 2C24 from the event identifier at BP+8. If this
equals current 4E71:0B44, it returns immediately. Otherwise it calls
28C9:2ABD and the old control's state helper, then recomputes the
index from the event and stores it in 4E71:0B44 at +05F3.
No own target-index bound or target-record +0E test precedes this
store. Intended character-box identifiers 2C24..2C27 supply indices
zero through three under preserved event bindings; other callers
and changed argument bytes are not thereby admitted.

After the member store it updates the new control state and rebinds
DS:1429 and DS:142D from current member and DS:19C5/19C9. At +0659
it requests 3A8E:060D with DS:1431. Only after that request returns
does +0673 test the selected 66-byte record's word +0E. That test
chooses the name or alternate text; it neither rolls back selection
nor performs an own count clear. The complete 267-byte helper has
no own DS:9D79 count store.

Consequently, under retained event/frame/segment bindings and normal
helper returns, an empty target is not rejected by this helper before
the refresh request. The refresh may reach FND-PARTY-306's empty-member
bypass while an earlier count remains. Actual callback execution still
requires the registered window and refresh gates; selection alone
does not prove that this path invokes the list callback.

### The cast handler subsequently repairs its tested mode-zero state

On return from 190+0D89, 211+09BA tests that its window still exists.
Null exits this branch. Otherwise +09D0 calls the mode setter for
current 4E71:0B44, then +09D6 calls 28C9:2ABD. The handler subsequently
tests current selected-member mode at +0A40. As FND-PARTY-304 records,
observed zero transfers directly to +0AFD's count-zero store; observed
nonzero takes the other branch. There is no call between that zero
test and store.

Thus the own ordering is member publication, refresh request,
post-refresh record test, return to cast handler, window test,
mode setter and state helper, then selected-member mode test and
conditional count clear. The member switch and count update are
not an atomic pair. A surviving zero-mode state after the tested
clear needs a later change; a use before that clear requires an
intervening callback/reentrant or alternate transfer. Neither a
native stale-count event nor such reentrant execution is established.

## Interpretation

Q-PARTY-134 no longer needs to ask whether the own selection helper
requires a nonempty target before publishing it: it does not. For
this known below-eight route, the remaining stale-state admission is
the old count and actual callback/window/member binding during the
publication-to-clear interval, interruption/changed inputs within it,
or later changes after the known zero branch. Stack/event/register
preservation remains Q-PARTY-136; unrelated menu behavior is outside
this obligation.

The ordering narrows the producer-state question but excludes no
whole client. No queue item closes or complete reading is declared;
FMT-PARTY-007 remains supported.

## Alternatives

- An empty target is rejected before selection: contradicted by the
  member store preceding the first own target-record test.
- A redraw necessarily leaves a permanently stale zero-mode count:
  the returning cast handler has a later explicit tested-zero clear.
- The intermediate state is necessarily observed by a frame event:
  callback/reentrant admission has not been established.

## How to reproduce

Use overlay_listing.py for descriptor 190 file 788E9..789F4 and
790C9..79103 plus 794D4..794E1, and reuse descriptor 211's retained
mode-caller211-second report. The latter is rooted at 9662B and
respects its inventoried gaps. The 190+05A9 body ends at 789F4;
an exploratory report extends into the next helper and supports no
claim about 05A9 after that boundary. Retain cast-member-publish190
and cast-member-selection190 only in
GAME_DIR/analysis/handler-census-20261106/. Verify the full 05A9 body,
the 0D89 first-child call and below-eight return, and FND-PARTY-304/306's
count/refresh paths. No original game, emulator session or emulated
call ran.
