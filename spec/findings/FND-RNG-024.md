---
id: FND-RNG-024
title: A scripted d20 threshold test draws once per candidate and rejects its highest scaled result
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000736A9..0x00073704
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 172C:1804..172C:18A5
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 172C:367B..172C:36CE
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2D40:0776..2D40:07DE
tool: tools/research/exec-census/direct_callers.py, trampoline_target.py, overlay_listing.py and resident_listing.py; installed-build fingerprint and relocation/fixup checks; synthetic exhaustive scaled-draw arithmetic
environment: null
---

## Observation

The relocation/fixup-aware generator query recovers overlay 188 +0817.
The exported trampoline 5702:0093 resolves to overlay 188 +0809, file
736A9. Decoding from that entry through its far return confirms the call
as an instruction, not a pattern inside unrelated bytes. The incoming
query for that entry recovers calls at 172C:1843 and 172C:186F after
canonical segment conversion. Both are reached from 172C:1804, the
shipped opcode-59 target of FND-SCRIPT-005's dispatch table. Other
computed/aliased incoming paths are not excluded.

### Local test

The helper receives three caller-cleaned word arguments: a combatant
index, an adjustment-table index and a field offset. It initializes a
local result byte to zero and unconditionally draws once from
1000:0822, before reading either indexed threshold component. It forms
the signed 32-bit product of that nonnegative draw and 20, then divides
by 32768. The quotient r is 0..19. If r is 19 it returns its zero byte
without reading either threshold component.

Otherwise it reads an unsigned byte at the combatant-table far pointer
DS:19C9 plus 49 times the combatant index, plus the field offset, plus
19 hexadecimal. Its other component is the signed byte at
DS:1943 plus the adjustment-table index. It adds the sign-extended second
component to the zero-extended first in a word, and sets the result byte
to one only when that signed sum is strictly greater than r. Equality
returns zero. With valid independent byte sources the sum is -128..382,
so this addition itself does not overflow a signed word.

The final return writes AL only. After a negative adjustment, the rest
of AX need not be zero. Both reviewed callers clear AH before extending
the result into the 32-bit script accumulator. Treating the helper's
unextended AX as a Boolean word would lose that caller contract.

The combatant stride agrees with FMT-COMBAT-001. This reading does not
name the selected field or adjustment table as a particular saving throw,
ability or skill. Index admission, table producers, storage lifetime and
aliases remain prerequisites. Address arithmetic keeps the original word
widths; it does not bound an arbitrary selector to a valid field.

### Caller draw order

Opcode 59 initializes its accumulator to zero and calls the existing
three-parameter reader before either reviewed call to the test. That
reader evaluates the expressions in order and stores full-width values
as FND-SCRIPT-010 describes. Draws made during expression evaluation,
and any effect of nested expression execution, precede the draws here;
they are not counted as this helper's one draw.

If the first full-width parameter is not 7FFE hexadecimal, the caller
passes the low words of the three parameters in order, calls the test
once, widens its returned byte and exits. There is no threshold or
zero-value bypass of that helper draw.

The special first-parameter path initially bypasses the test and reaches
the accumulator check. While that accumulator is zero, it asks
2D40:0776 for the next candidate, initially with sentinel 9999. A result
of 9999 exits without another draw. Any other result supplies the first
argument of the test, with the other two parameter low words unchanged.
A true test exits; a false test requests another candidate. Thus the
local loop consumes one helper draw for each candidate actually tested
and stops testing on its first true byte. It need not consume a draw
when the iterator supplies no candidate. This statement retains the
actual accumulator check after parameter evaluation rather than assuming
all expressions preserve its initial zero.

The iterator body has no RNG call. Its ordinary scan considers indices
below four, selecting a record only when its word at offset 6 is signed
negative. A separate branch after starting at index four can select the
stored current index under two additional byte gates. Its complete
candidate-domain and storage admission are not established here, so no
unconditional four-roll bound or fixed party-membership claim follows.

### Synthetic arithmetic boundary check

Enumerating all 32768 possible generator return values with
r = floor(draw * 20 / 32768), and accepting only r != 19 and threshold > r,
gives these counts. This checks arithmetic only, not original execution,
probability of reaching the caller, table values or a rebuild fixture.

| Threshold | True results among 32768 draws |
|---|---|
| -128 or 0 | 0 |
| 1 | 1639 |
| 19, 20 or 382 | 31130 |

The highest scaled result is produced by 1638 generator outputs and is
always false. Raising the threshold above 19 therefore cannot make this
helper always true. The bucket counts are not all equal.

## Interpretation

This qualifies another direct generator consumer and its local draw
order for Q-RNG-001. It is separate from FND-PARTY-099's effect-saving
path, which calls roll_sum with one die of 20 and explicitly handles
natural 1 and 20. Similar d20 arithmetic does not merge their result
meaning or caller effects.

FND-PARTY-099 already supplies a recovered roll_sum consumer, contrary
to RULE-RNG-001's stale open-question wording that none was recovered.
That wording can be corrected without claiming a complete reducer census.
Other direct candidates, shipped instruction instances, index/iterator
admission, helper callers and indirect generator/state access remain open.

## Alternatives

- A modulo-20 draw, a 1..20 intermediate, an inclusive threshold comparison
  or a threshold-at-least-20 automatic true bypass does not match this body.
- A true byte is only this helper's test result. Calling it a saving-throw
  success or failure requires the selected fields and game-content uses.
- Direct-call patterns and inventory starts alone do not establish entries.
  Here the trampoline and shipped opcode table supply the reviewed entries;
  candidate decoding from a paragraph boundary is not used as evidence.
- The original's overlays are outside the delivered resident-only Unicorn
  harness. No emulated-call fixture, native observation, complete rule
  reading or parity promotion is claimed.

## How to reproduce

Use the locked evidence Python on the approved installed DSUN.EXE.
Run direct_callers.py with targets 1000:0822 and 188+0809, using the
already documented resident generator sites as controls. Resolve
5702:0093 with trampoline_target.py, then run overlay_listing.py with
descriptor 188 and file bounds 736A9..73704. Read the listed resident
ranges with resident_listing.py, starting the opcode handler at its
table-derived entry rather than an inventory's interior start.

Check opcode 59's entry against FND-SCRIPT-005, parameter-reading effects
against FND-SCRIPT-010, the two far frames and caller cleanup, and the
unprefixed byte sign extension as a default word operation. Trace the
highest-result branch before the indexed reads, signed strict comparison,
AL-only return, sentinel checks and accumulator-based early exit.
For the synthetic counts, enumerate integer draw from 0 through 32767
using the formula above and count each tested threshold. Keep any
original-derived listing only in GAME_DIR. No original is executed.
