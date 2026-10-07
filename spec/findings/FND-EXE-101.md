---
id: FND-EXE-101
title: Larger fallback flag gates select mapping states and preserve upper bytes in copied-word publication
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x00689519..0x0068959C
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x00689650..0x0068975F
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x0068979B..0x006897D9
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x00689800..0x00689845
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x00689CF9..0x00689D7C
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x00689E28..0x00689F3F
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x00689F7B..0x00689FB9
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x00689FE0..0x0068A025
tool: Ghidra 12.1.3 PUBLIC bounded flag and publication reading
environment: null
---

## Observation

FND-EXE-099 grounds the larger fallback byte/word targets and their copied
first and second indexed words. FND-EXE-100 records subsequent mapping
publication and source reentry. This reading follows the intervening nonzero-
mode decisions. Call them copied full words W1 and W2, retained page index I,
retained original address A, category C and local mapping state S. These names
are neutral reading labels, not established resource-field meanings.

In the byte method W1 is at current ESP plus forty, W2 at plus thirty-six,
and S at plus twenty-eight. The corresponding word-method locations are
plus twenty-four, twenty and twelve. Each method initializes full S to zero,
extracts mask four from each copied word's low byte, and freshly reads full C
from `0x006F0090`. It computes C minus 64 modulo thirty-two bits and tests
that result unsigned at most sixteen before using it as a shift count in
mask `0x00010021`. The selected categories are exactly 64, 69 and 80.
The out-of-range route does not shift by an arbitrary category.

For those three categories the next gate is true if either W1 or W2 lacks
mask four. For all other categories it is true only if both lack it. The
special route uses separate zero tests, byte SET results, full OR and a final
mask one; preserved upper register bytes do not change its final bit-zero
result. A false gate skips to the mask-two tests with S still zero. A true
gate reads `0x0075B144` and `0x0075B140`, ANDs their full values and
compares the result with full three. Equality makes S three. Otherwise C
in the exact set 48, 64, 69, 80 makes S one; other C leaves S zero. These
later category branches use unsigned comparisons and exact equality, not the
shift-mask selection alone.

The next step tests mask two in W2's low byte and then W1's low byte.
If both are present it goes directly to the state-three decision. If either
is absent and S is nonzero it still goes there. If either is absent with S
zero, C in the set 48, 64, 69, 80 makes S two; another category keeps S
zero. Thus the following local state is one of zero, one, two or three on
these direct routes before unresolved callee effects.

State three supplies A first, `(W1 AND 0xFFFFF000) + 4*(I AND 1023)`
second and full five third to `0x00417CF0`, using thirty-two-bit arithmetic.
Only after normal return does it write full zero to S and enter the flag-update
step. It does not reload the copied full W1/W2 words from their original
indexed storage there. The helper's return is not tested; its effects and
exceptional completion remain unresolved. A reset state is not evidence that
the helper succeeded or that earlier memory was restored.

Next, if W1's low byte already has mask 32, no first-word store is made.
Otherwise the method ORs 32 into that local byte, preserving the other three
bytes of the copied full word, then writes the resulting full W1 through
freshly read global base `0x01D4A380` plus
`(fresh 0x0075B6C8 shifted left twelve) + 4*(I shifted right ten)`.
The low-byte mutation precedes that full-word publication. There is no local
comparison proving that the destination still equals the original W1 source.
All address additions and shifts use thirty-two-bit arithmetic.

It then tests W2's low byte against mask 96. If both mask bits are already
present, no second-word store is made. Otherwise S zero ORs 96 into that
local byte; S nonzero ORs only 32. It preserves W2's other three bytes, then
writes the resulting full W2 through fresh `0x01D4A380` plus
`(current W1 AND 0xFFFFF000) + 4*(I AND 1023)`. S and the copied words
are separate storage; updating a low byte does not replace the full copied
word with a zero-extended flag value. Aliases, destination validity and changes
from the earlier helper remain conditional.

The common continuation computes K as current full W2 shifted right twelve.
S zero supplies I and K to `0x004180A0` and joins FND-EXE-100's normal
route that clears the retained cleanup selector before reading. S one also
calls `0x004180A0`, but writes full one to that retained selector before the
call and joins the source reader without the clearing step. Other S supplies
I and K to `0x00417F90`, then joins the route that clears the retained
selector before reading. These are distinct publishers in FND-EXE-100;
S two is not interchangeable with S one. A state-three helper that normally
returns has already reset S to zero before this choice.

For the byte method the two publication call sites are `0x00689726` and
`0x0068980C`, with the S-zero join through `0x006895B2`. Their word
counterparts are `0x00689F06` and `0x00689FEC`, with the zero join
through `0x00689D92`. Both use the K computed from the copied W2 at
this stage, rather than an unconditionally fresh original-table read. The
source AL/AX retention and post-read cleanup stay as FND-EXE-100 records.
No callee success result is checked in these publication continuations.

## Interpretation

The larger fallback paths retain copied-word flags, a category-dependent
state and fresh global address inputs independently. The state-three request,
ordered low-byte/full-word updates and different mapping publishers are part
of admission before source reentry; none is a generic success boolean. These
branches do not establish the meaning or initialization of C, the globals or
copied words, all indirect targets, source bounds or termination. Q-EXE-009
in FMT-EXE-006 retains those dependencies.

## Alternatives

- The special mask selects 64, 69 and 80, whereas the later equality set also
  includes 48. Treating those category sets as identical loses a branch.
- The ordinary initial gate requires both flags absent; the special one
  requires either absent. One universal conjunction does not describe them.
- Local byte stores preserve the other bytes, and later publication reads
  the full copied words. Zero-extending the changed byte would change the
  stored high bits and the downstream shifted K.
- State three is cleared only after its helper normally returns, without a
  result test or automatic reload of the original indexed words.
- Current global address inputs and copied flags have different provenance.
  A recomputed destination is not proved identical to the original source.
- S one retains a cleanup selector of one at its source-read join; S two
  uses the other publisher and then clears that selector. The selectors are
  not simply the same local storage under two names.

## How to reproduce

Recheck FND-EXE-011's shipped identity and FND-EXE-099's six physical slot
controls; use the saved Ghidra program with -noanalysis. Fresh windows:
`0x00689519` limit 25; `0x00689CF9` limit 25; `0x00689E28` limit 125.
Combine with FND-EXE-100's `0x0068955B` and `0x00689D3B` windows,
each limit 200, restricting claims to the ranges listed above. The word
state-three continuation through `0x0068A025` is in that latter window,
not inferred from the byte method's similar body. No negative search is made.

Follow low-byte SET operations through the final mask, distinguish the two
category sets, and retain S across the mask-two tests. Trace first-word update
before second-word addressing, all other copied-word bytes, fresh global
loads, each publisher's full outgoing arguments and the source-read join.
FND-EXE-099 establishes the copied-word prefix and FND-EXE-100 the publishers
and retained-result cleanup; neither supplies unread helper effects. Keep
reports and identity controls in GAME_DIR/analysis/exe-batches. No native or
emulated execution and no original listings or bytes in Git.