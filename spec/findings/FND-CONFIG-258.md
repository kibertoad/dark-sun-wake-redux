---
id: FND-CONFIG-258
title: Setup adjustment selects masked address IRQ and DMA values before mode-specific settings writes
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 14A2:0087..14A2:0ACD
tool: Ghidra 12.1.3 ReportFunctionSummary.java and tools/research/exec-census/resident_listing.py; checkout-local evidence Python 3.12.10; Capstone 5.0.7
environment: null
---

## Observation

Q-CONFIG-005 retains later writes and input admission beyond the two
constructors in FND-CONFIG-257. FND-SOUND-021 identifies their later
adjustment caller14A2:0087. This reads its complete local selection/apply
graph and connected initial-index helper, with instructions deciding widths
and order. Source SOUND_DS.EXE remains204593 bytes, XXH3-128
236c2dc23c071eca421eb5b427caee57. Addresses, offsets and masks below
are hexadecimal; counts and mode values are decimal.

The function receives a far selected-record pointer at BP+6 and a mode
word at BP+A. Data segment is1E36. It initializes eight result locals
toFFFE and its dialog index local toFFFF. If record word+DE is zero,
it calls190F:0000 with its two fixed far arguments and returns without
its own settings-buffer stores. That child's effects are not inferred.
Otherwise it processes these eight independent bits in the listed order:

| Mask in record+DE | Count offset | Word-array offset | Current-value offset supplied to lookup |
|---|---|---|---|
| 01 | 2C | 2E | EE |
| 02 | 6E | 70 | F0 |
| 04 | 84 | 86 | FA |
| 08 | 2C | 2E | EE |
| 10 | 58 | 5A | F0 |
| 20 | 42 | 44 | F6 |
| 40 | 6E | 70 | F8 |
| 80 | 84 | 86 | FA |

FND-SOUND-037 binds the arrays to MIDI address, MIDI IRQ, digital address,
IRQ and DMA fields and+DE to parsed jumper masks. The table deliberately
retains the shared DMA branch's current-value sourceFA rather than assuming
it usesF2 from the first constructor block. The mode word does not suppress
these list-selection arms: its local tests occur later, in the apply phase.

Each selected arm formats its counted numeric array into80-byte label
slots, using signed SI<count tests and low-word multiplication/addition.
Address labels use DS:E696, IRQ labels E376 and DMA labels E056; a small
stack buffer receives each formatted number. After label preparation, the
arm calls14A2:0AA5 with the count, far array pointer and current value,
passes its returned index through the prompt/list wrapper path, then uses
19CF:000D's returned index to read a word from the same record array.
That word is retained in the arm's own local result slot. Formatting,
actual dialog completion and writable label/frame bounds remain admitted
child/storage conditions, not conclusions from a positive source count.

### Initial index and returned selection are different results

The complete helper14A2:0AA5..0ACD has no child. It starts DX at zero,
compares it signed with the supplied count and returns the first index
whose array word equals the supplied current value. If there is no match,
or count is nonpositive, it returns zero. The array address uses a one-bit
word shift and low-word addition with its original far segment. It does
not prove that zero names a valid member of an empty array.

The adjustment caller consumes the later dialog index without its own
FFFF/cancel or index<count check. The low-word doubled result is added to
the record base and the array field before the word read. This is distinct
from the earlier card-list callers' FFFF checks in FND-SOUND-021. It does
not establish a native cancelled-dialog result, a valid returned index or
the contents of storage outside the admitted array.

### Ordered mode-specific apply phase

After all selected arms, the function reloads record+DE for each apply
test. Under retained record/frame/segment state, valid disjoint spans and
completed child calls, the explicit stores relative to output DS:E01B are:

| Mask/result | Mode1 destinations | Mode2 destinations | Mode3 destinations |
|---|---|---|---|
| 01 | 0A,00 | 0A | 00 |
| 02 | 0C,02 | 0C | 02 |
| 04 | 0E,10,04,06 | 0E,10 | 04,06 |
| 08 | 00 | none | 00 |
| 10 | 02 | none | 02 |
| 20 | 0A | 0A | none |
| 40 | 0C | 0C | none |
| 80 | 0E,10 | 0E,10 | none |

Modes other than1,2,3 have no settings stores in this apply graph even
though their earlier selected dialogs can run. Within each recognized
mode, tests follow the table order: the specialized results can replace
earlier shared results when both masks are retained. No child call occurs
between these explicit apply tests/stores. The stores are words; no local
apply store changes card IDs, output flags, names or the nine-byte tail.
The body then restores its frame and returns far without its own success
result assignment. A return-register value is not treated as an API result.

Mask tests are separate reads across child calls; this is not a cached
one-time choice. A changed mask could select an apply store whose local
remainsFFFE, or suppress an earlier chosen result. This finding records
that conditional ordering, not an actual native intervening writer.

Ghidra reports the full local body through0AA4 and the helper through0ACC,
with three ordinary incoming sites1429:02DC/038B/0414 for the adjustment
and eight local helper calls. Instructions corroborate the signed loops,
word shifts, repeated mask tests, mode comparisons and output-store order.
This is not an all-entry or writer census.

## Interpretation

The two constructors do not alone determine the final saved settings.
This later adjustment can replace address/IRQ/DMA words, and its mode and
mask order determine which block receives each chosen value. It does not
itself supply the constructors' EC or EE..FC record producers.
Q-CONFIG-005 no longer needs this local selection/apply mapping or the
initial-index helper's algorithm. It retains actual count/index admission,
label/frame/storage preservation, EC/extended-field producers, other writers,
file outcomes and downstream driver behavior. No complete reading or rule/
format promotion is declared; no original executable was run.

## Alternatives

A generic cancel check cannot be imported from the earlier card-choice
caller. Similarly, mode2/3 suppressing stores does not imply suppressing
dialogs. The no-local-store and precedence statements assume the named
bindings and disjoint storage; unadmitted child effects or record aliases
cannot be dismissed by this local graph. FND-SOUND-021/022's conditional
dialog/list contracts remain narrower than native input completion.

## How to reproduce

Reuse the verified SETUP-blocks project under
GAME_DIR/analysis/config-block-construction-20261108/. Its retained
record-settings-producer-ghidra.log and record-settings-index-ghidra.log
come from packaged ReportFunctionSummary.java for14A2:0087 and14A2:0AA5,
using read-only/noanalysis processing. Regenerate record-settings-instructions.txt
with checkout-local Python -I tools/research/exec-census/resident_listing.py
against the verified SOUND_DS.EXE, ranges14A2:0087..14A2:0AA5 and
14A2:0AA5..14A2:0ACD. All original-derived reports remain local.
