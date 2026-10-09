---
id: FND-EXE-359
title: Sound utility cleanup formats its retained word through bounded decimal conversion
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1000:05AC..1000:05CC
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1000:052A..1000:05AC
tool: Capstone 5.0.7 and xxhash 4.0.1
environment: null
---

## Observation

FND-EXE-358's 05F5 path leaves its first argument word on the stack
after removing 1487's eight argument bytes. It then pushes returned DX
and AX before near-calling 05AC. The latter forms BP: SS:BP+04/06
are that returned offset/segment, and SS:BP+08 is the retained word.
It pushes zero, the retained word, destination segment/offset, decimal base
ten, zero sign flag and alphabet-base word 0061, then near-calls 052A.
After that return it restores BP and near-returns removing six argument
bytes. This consumes both the returned pair and the earlier retained word.

The 052A callee reserves 34 local bytes and saves SI, DI and ES. It loads
the destination pair before checking the unsigned base word is at most
36 and its low byte is at least two. An invalid base skips digit generation
but still writes a zero byte through the destination. For an accepted base
it reads the low/high value words from SS:BP+0E/10. A signed-negative
high word and nonzero low byte of the sign flag cause a minus-sign store,
destination increment and two-word negation before magnitude conversion.
The fixed 05AC path supplies a zero high word and zero sign flag, so it
does not reach that sign-prefix branch.

The callee computes quotient and remainder by the accepted base, first
handling a nonzero high word and then the remaining low word. Each
remainder is stored as a byte in its SS-relative local array. It stores at
least one digit even for zero. It walks those digits backwards into the
destination, maps remainders below ten to decimal characters, uses the
alphabet-base byte for larger remainders, then writes a zero terminator.
There is no destination-capacity argument or local capacity check.

For 05AC's fixed base ten and zero-extended word input, the magnitude
is at most 65535 and therefore produces at most five digit bytes followed
by one zero byte. This bounds output production independently of the
destination's validity. The general accepted-base path handles a 32-bit
magnitude and requires at most 32 local remainder bytes at base two;
the optional sign prefix is written directly to the destination rather than
stored in that array. The general output bound is 34 bytes including an
optional sign and terminator, not an admitted destination extent.

The callee makes no calls or interrupts. It restores ES, reloads the
destination pair from SS:BP+0A/0C into DX:AX, restores DI/SI and its frame,
and near-returns removing fourteen incoming argument bytes. It does not
restore flags or BX/CX. Together with 05AC's six-byte cleanup, this resolves
the retained-word stack obligation in FND-EXE-358. It does not resolve
1487's returned destination contract or other helpers' stack/register behavior.

## Interpretation

The retained cleanup word is the numeric input to this selected conversion,
not an unexplained leftover stack argument. Both local cleanup sizes come
from direct return instructions rather than inference from the outer caller.
The selected path's output has a concrete six-byte maximum, but successful
formatting still requires a writable destination of that extent.

The callee's SI/DI/ES restoration resolves their local preservation across
this conversion. FND-EXE-358's root SI return still depends on its other
unread callees. No meaning for the numeric word or generated text is inferred.

Q-EXE-007 retains 1000:1487's destination/result and preservation,
other cleanup helpers, source-state admission, aliases and lifetime. No
complete-reading promotion, native operation result or launch exclusion follows.

## Alternatives

Treating the retained word as an unremoved argument ignores 05AC's six-byte
return cleanup. Treating the destination pair as the numeric input reverses
the BP-relative bindings. Treating the bounded digit count as proof of buffer
capacity assumes a destination contract this callee never checks. Treating
SI restoration here as preservation through all cleanup calls extends evidence
beyond the two local bodies read.

## How to reproduce

At revision f06031b require FND-EXE-350's SOUND_DS.EXE identity:
length 204593, XXH3-128 236c2dc23c071eca421eb5b427caee57.
With locked Capstone 5.0.7 in sixteen-bit mode decode shipped half-open
0x000019AC..0x000019CC at IP 05AC and
0x0000192A..0x000019AC at IP 052A, modeled CS 1000,
MZ header size 1400. Bind each push to FND-EXE-358's retained word
and returned pair, follow accepted-base checks and two-word quotient/remainder
updates, and count output independently of destination admission. Check both
near-return cleanup widths and the saved-register restoration order.
Original bytes and reports remain outside Git. No original process,
DOSBox, interrupt thunk or emulated call is executed.
