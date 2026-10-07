---
id: FND-EXE-117
title: Exact-equality record merge conditionally increments a counter before fresh write-position arithmetic
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x005A4540..0x005A45A6
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x005A46B0..0x005A46B5
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x005A4716..0x005A4717
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x005A4724..0x005A4725
tool: Ghidra 12.1.3 PUBLIC bounded exact-equality byte merge and fresh write reading
environment: null
---

## Observation

FND-EXE-116 records exact count/size equality selecting `0x005A4540`
with retained object O, second-record pointer R, count C and size L. This
path reads full R plus twelve as index I and forms I + retained C modulo
thirty-two bits, subtracting retained L once when the sum is at least L
unsigned. It tests retained C against L again. For this entry they are equal,
so the count-below-size arm is not selected: it chooses the reduced position
minus one if nonzero, otherwise L minus one modulo thirty-two bits. It
reads R's full base and zero-extends the byte at base plus that position.
No local size-zero, index bound or valid-storage admission is proved.

If that byte is zero it increments full O plus `0x0158` modulo thirty-two
bits; nonzero skips that counter store. It ORs the read byte with local F
from FND-EXE-113 and retains the combined byte in another local slot, current
ESP plus thirteen. Local F at plus fifteen is not directly replaced by this
merge. It freshly reads R's count at sixteen and index at twelve before
storing that combined local byte, then freshly reads size at eight. These
record reads occur after any counter increment and are not the retained
equal count/size pair. Object/record aliases remain unresolved.

It forms fresh index plus fresh count modulo thirty-two bits and subtracts
fresh size once when that sum is at least size unsigned. Fresh count below
fresh size loads the combined local byte, then enters `0x005A4601` to
read R's base, write that byte at base plus the selected position and
increment the current full count at R plus sixteen after the write. This
shared append suffix is read in FND-EXE-116. Fresh count at least fresh size
instead chooses selected position minus one when nonzero or fresh size
minus one when zero, reads R's base and writes the combined local byte
without a direct count increment. It then joins `0x005A4609`.

Both joins enter FND-EXE-116's fresh object-to-record lookup and current
indexed-byte test. The initial replacement position is not retained as an
assumed write address; fresh arithmetic determines the write. Initial equality
alone does not prove that the later fresh pair is equal when the counter
store may alias it. No call occurs in this merge/calculation/write sequence;
subsequent flag paths and exceptional storage effects remain unresolved.

## Interpretation

The exact-equality path merges a previously stored byte with local F and
conditionally counts a zero byte before rereading its write inputs. Retained
initial equality, fresh count/size and current-index continuation are distinct
observations. Q-EXE-009 in FMT-EXE-006 still requires storage/alias contracts,
record bounds and producers, later flag/counter paths and callee effects.
No complete first-callee reading or actual PATH outcome is claimed.

## Alternatives

- The counter increments only for a zero read byte, not for every merge.
- The combined byte is kept separately; local F is not replaced in place.
- Initial equality selects a replacement read but does not replace the later
  fresh count/size decision with an equality assumption.
- The write position is recomputed after the conditional counter store.
- One subtraction does not establish general modulo or a valid byte bound.

## How to reproduce

Verify FND-EXE-011's executable identity and FND-EXE-099's physical controls.
FND-EXE-116 independently supplies equality admission and retained inputs.
Use the saved Ghidra program with -noanalysis and
ReportInstructionWindow.java at `0x005A4540` limit 40,
`0x005A46B0` limit 2 and `0x005A4724` limit 2. The earlier window at
`0x005A46F9` limit 10 in FND-EXE-114 covers replacement decrement
`0x005A4716`. FND-EXE-116's `0x005A45FC` limit-35 window covers both
shared suffixes at `0x005A4601` and `0x005A4609`. Restrict observations
to the declared ranges and cited suffixes; exclude following flag paths.
Track retained equality, single-subtraction positions, conditional counter
publication, local-byte slot distinction and fresh write inputs. Keep reports
in GAME_DIR/analysis/exe-batches; commit no original listings or bytes and
execute neither interpreter nor game.