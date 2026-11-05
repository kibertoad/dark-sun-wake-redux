---
id: FND-SOUND-048
title: Setup plain signed-word formatting produces bounded decimal text and turns the CardGroup sentinel into minus one
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1000:052A..1000:05AC
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1000:0F25..1000:0FFE
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1000:10F7..1000:1157
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1000:1248..1000:1348
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1000:1398..1000:13B8
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1000:390E..1000:3970
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    kind: file-data
    offset: 0x0001D5F8..0x0001D5FA
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    kind: file-data
    offset: 0x000027B8..0x000027E8
tool: tools/research/exec-census/resident_listing.py and tools/evidence/report.mjs table at b8be1796; Python 3.12.10, Capstone 5.0.7, Node.js 24.15.0
environment: null
---

## Observation

Q-CONFIG-012 retains numeric-summary completion after FND-SOUND-047's
CardGroup argument trace. This reads only the exact percent-d-NUL profile
already measured there, with no flags, width, precision or length modifier.
FND-SOUND-027 supplies the common formatter initialization, copying callback
and disjoint storage prerequisites. Addresses/masks are hexadecimal;
lengths and numeric ranges below are decimal.

### Dispatch and argument width

The two adjacent controlled classification reads map c to class 16 and d
to class 10. The complete 24-word dispatch table maps these classes to
11A7 and 10F7 respectively, retaining FND-SOUND-027's independent c control.
Plain d therefore takes the signed numeric arm, not the string arm.

The formatter starts flags at 20, width and precision at FFFF, zero extra
padding and no explicit sign prefix. At 10F7 it selects base ten and signed
conversion, reads one word through the SS-based variadic pointer and
sign-extends it into two words. Flag 0010 is clear, so no second argument
word is read; the argument-list pointer advances by two bytes.

### Decimal generation and bound

052A receives that signed value, base ten, signed-mode byte one and a
stack-local destination. For a negative high word it emits a minus sign
and negates the two-word value. It repeatedly divides the unsigned
magnitude by ten, retaining remainders in reverse order, then emits the
digits in forward order and appends NUL. Zero performs one division and
emits a zero digit. Its accepted base range includes ten; the alphabetic
digit branch cannot be reached by decimal remainders zero through nine.
It restores ES/SI/DI/BP and removes its fourteen argument bytes.

Every signed-word input is consequently in -32768..32767. Its magnitude
has at most five digits, so the signed output has one through six
characters, plus one NUL. In particular input word FFFF becomes `-1`,
and input word 8000 becomes `-32768`; these are derived static cases,
not recordings of original execution.

Precision FFFF takes the default path. No explicit plus/space sign,
alternate-base prefix, zero-fill or leading/trailing padding is added.
The common loop copies the complete generated text into its eighty-byte
scratch buffer. At most six characters do not reach the internal flush
threshold. The final format NUL flushes once through callback 390E,
which uses FND-SOUND-027's bounded 30EB copy contract, advances the external
destination by that count and appends NUL. Its nonzero count prevents
the formatter's output-error path; the return equals the character count.

This requires retained format/class/dispatch data, valid SS-based argument
and scratch storage, a valid disjoint external destination of at least
seven bytes for all word inputs, and nonwrapping offsets. FND-SOUND-047's
summary destination is a ten-byte local slot under the same storage
conditions, so this conversion fits it.

## Interpretation

This removes local numeric-summary string generation from Q-CONFIG-012.
For the non-FFFF DMA branch with CardGroup four, FND-SOUND-047's substituted
FFFF argument produces `-1` followed by NUL. With another CardGroup value,
the retained DMA word is formatted as a signed decimal word. This proves
the local buffer result, not its subsequent drawing, chosen record,
native stack admission or any hardware/device meaning of CardGroup.

## Alternatives

An unsigned interpretation would give different text for FFFF/8000 and
contradicts the signed arm and sign extension. Treating the parser's
general numeric behavior as the formatter contract would confuse two
different helpers. Modified formats, long arguments, aliases, changed
tables and screen rendering remain outside this profile. No original was
run, complete reading declared or parity status promoted.

## How to reproduce

Use approved SOUND_DS.EXE length 204593, XXH3-128
236c2dc23c071eca421eb5b427caee57. Run resident_listing.py with the six code
ranges above in order. Retained setup-plain-word-format.txt and its -repeat
pair under GAME_DIR/analysis/disc-adv-survey-20261104/ have SHA-256
549b9afc60aacaf3ac5afe74ff15fafb48e1aa2bebde5afdcb4ed83961f2ad20.

Use report.mjs table for two stride-one, width-one rows at file 1D5F8
(c and d classifications) and 24 stride-two, width-two rows at 27B8
(bounded dispatch). Local setup-numeric-classes/dispatch-controls.json
retain these inputs. setup-numeric-classes.json and its -repeat pair have
SHA-256 8ac650c93b9601ab399efbb1392f271b73ec678067fde4086be72efbd3e0dfab;
the dispatch pair has
70ae33cea0588711656a331fb4a8da23070188f667e156f6457cf3284ef53958.
Keep original-derived reports and listings only in that local store.
