---
id: FND-EXE-477
title: Sound utility string converter accumulates decimal prefix without error or overflow result
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1000:2713..1000:2788
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    kind: file-data
    offset: 0x0001D1FB..0x0001D1FC
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    kind: file-data
    offset: 0x0001D226..0x0001D227
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    kind: file-data
    offset: 0x0001D228..0x0001D229
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    kind: file-data
    offset: 0x0001D22B..0x0001D235
tool: Capstone 5.0.7 and xxhash 4.0.1
environment: null
---

## Observation

FND-EXE-476 passes terminated local strings to far-returning 2713 and
uses only returned AX. The converter saves SI, DI, ES and its frame BP,
loads the incoming pair into ES:SI and initializes DX/AX zero. It reads
successive bytes, incrementing SI at word width, while bit 0001 in the
current DS byte table at displacement DA9B is set for that unsigned byte.
There is no independent source bound, iteration cap or terminator check
in this leading-classification loop.

The first byte not selected by that table is tested for plus or minus.
Either sign consumes one more byte; only minus sets the local sign state.
Digit checks then accept exactly byte values 30 through 39. A first
non-digit ends accumulation with the initialized zero pair. A later
non-digit ends with the accumulated prefix; no full-string or digit-count
validation occurs, and no separate conversion-error value is produced.

For each accepted digit d, the small-value path multiplies current AX
unsigned by ten and adds d to the low word. Its high contribution is
the multiplication's DX plus the low addition's carry: the product high
word is at most nine, so DH is zero and the byte-width ADC into DL
does not lose a high-byte contribution on this selected path. A zero DL
continues the small-value path; nonzero selects the wider path for subsequent
digits. This is a result-width transition, not an overflow rejection.

The wider path combines low-word times ten, high-word times ten and
their carry contributions to implement V=(10*V+d) modulo 2^32.
The overflow beyond the resulting high word is not tested or returned
separately. Once selected, the wider loop does not return to the small
loop even if later wrapping produces a zero high word. Both paths consume
bytes through ES with word-width SI advance and no segment adjustment.

On termination the sign state selects the accumulated pair unchanged or
its double-word two's-complement negation. Zero stays zero under either
sign. The converter restores its saved BP, ES, DI, SI and outer BP and
returns far without incoming argument cleanup. It makes no call or interrupt,
does not write the source or table locally and does not return a source
endpoint. All source, frame and table aliases remain separate admission.

Under FND-CONFIG-004's data-segment binding 1E36 and MZ header size
1400, the inspected shipped classification entries for byte 00, plus,
minus and each digit 30..39 all have bit 0001 clear. This is shipped
file-data only; actual DS and later table writers are not established here.
With those entries admitted at runtime and noninterfering initialized
terminated strings, three-digit strings in FND-EXE-476's three-byte field convert
as decimal values from zero through 999 with DX zero. For example,
the three bytes 220 produce AX decimal 220 and DX zero, and the
fixed mapper then returns 0220. The one-byte decimal-digit field similarly
returns zero through nine. These conditional examples do not prove the
actual read bytes were digits or validate a hardware setting.

## Interpretation

This resolves the converter's local radix, classification gate, optional sign,
prefix stopping and arithmetic widths. Its result is a wrapped signed-prefix
quantity without an error or complete-consumption indicator. The caller's
fixed mapping and unconditional second-field store are therefore additional
decisions, not evidence that conversion itself validated the entire field.

Q-EXE-007 retains actual DS and table producers, source/frame termination
and extent admission, caller bytes and later stored-word consumers, aliases
and lifetime/re-entry. No complete reading or execution exclusion follows.

## Alternatives

Treating the fixed mapper's output as proof of a hexadecimal parser conflicts
with the converter's multiply-by-ten accumulation. Treating non-digit input
as a rejected conversion ignores its zero or prefix return. Treating the
small-to-wide transition as an overflow check ignores continued accumulation
and later wrapping. Treating a local zero terminator as unconditionally stopping
the initial loop assumes its current classification bit and source admission.

## How to reproduce

At revision 1b4b089 require FND-EXE-350's SOUND_DS.EXE identity:
length 204593, XXH3-128 236c2dc23c071eca421eb5b427caee57.
Decode shipped half-open offsets 0x00003B13..0x00003B88 at IP 2713,
modeled CS 1000, using locked Capstone 5.0.7 in sixteen-bit mode.
For table evidence, bind displacement DA9B under segment 1E36 and
MZ header size 1400, inspect only indices 00, 2B, 2D and 30..39
and test only bit 0001. The corresponding half-open shipped intervals
are listed in Locations. Track the initial classification loop, sign state,
unsigned digit gates, multiplication widths and carries, permanent wider
loop, modular sign application and saved-BP cleanup. The extension at
2720 is sixteen-bit AX into DX despite a wider decoder mnemonic.
Original bytes remain outside Git; no original process, DOSBox, interrupt
thunk or emulated call is executed.
