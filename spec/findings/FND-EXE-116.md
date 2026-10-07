---
id: FND-EXE-116
title: Nonzero local flags increment a counter before fresh record arithmetic and reread the current byte after writing
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x005A42AF..0x005A4314
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x005A4367..0x005A4373
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x005A45FC..0x005A4618
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x005A471C..0x005A471F
tool: Ghidra 12.1.3 PUBLIC bounded nonzero-local-flag second-record prefix
environment: null
---

## Observation

FND-EXE-113 and FND-EXE-114 record entry to the local-F test at
`0x005A42AF`. F nonzero tests mask `0x10` in local F. When present
it ORs mask eight into F before testing mask one of O's byte `0x0164`.
When absent it tests that object mask without modifying F. Mask absent
selects `0x005A4379`, whose later flag path is outside this observation.
Mask present reads second-record pointer R from O plus `0x0154`, size L
from R plus eight and count C from R plus sixteen. Exact full equality
selects `0x005A4540`, a separate path not described here.

Inequality increments full O plus `0x0158` modulo thirty-two bits before
freshly reading count, index at R plus twelve and size from the retained R.
Those reads follow the counter store; aliases remain unresolved, so the
initial unequal pair must not replace these fresh values. It computes P as
index plus count modulo thirty-two bits and subtracts size once when P is
at least size unsigned. It then compares the freshly read count with size.
Count below size reads the local-F byte, reads R's full base, writes F at
base plus selected P, then increments the current full count at R plus
sixteen modulo thirty-two bits.

Count at least size instead chooses selected P minus one when nonzero,
or size minus one modulo thirty-two bits when selected P is zero. It reads
R's full base, loads local F as a byte, writes it at base plus that position
and makes no direct count increment. Neither branch changes the record's
index locally. No size-zero, position bound, count validity or base admission
is checked in these bounded paths. Local F can have changed by the earlier
mask-eight OR and is not necessarily the entry third-input byte.

After either byte write it freshly reads O plus `0x0154` as record R2,
reads R2's full base and current full index, and tests the byte at that
base plus index against zero. It does not assume that this is the location
just written, nor that R2 equals retained R. Byte zero selects
`0x005A43DB`; nonzero proceeds into later flag publication starting at
`0x005A461E`. Those continuations and the exact-equality path remain
unread contracts. No call occurs within the described counter/arithmetic/
write/reread sequence, but earlier calls may already have changed its inputs.

## Interpretation

The unequal-count path makes counter publication precede fresh record
arithmetic and makes a fresh current-byte read follow append/replacement.
Both freshness points matter when object/record storage may alias. Q-EXE-009
in FMT-EXE-006 still requires the equality merge, later flag/counter paths,
record producers/bounds/lifetime and callee effects. No complete first-callee
reading or actual PATH outcome is claimed.

## Alternatives

- Exact equality and unsigned count-at-least-size tests are distinct decisions;
  an initial inequality does not bound the later fresh count.
- The counter increments before the fresh arithmetic inputs, not after the write.
- Append increments count after writing; replacement does not directly increment it.
- The continuation reads the current indexed byte, not necessarily the byte
  just written, and rereads the record identity through the object.
- One size subtraction does not establish general modulo or a valid bound.

## How to reproduce

Verify FND-EXE-011's executable identity and FND-EXE-099's physical controls.
FND-EXE-113 supplies the local-F test and frame. Use the saved Ghidra program
with -noanalysis and ReportInstructionWindow.java at `0x005A41F0` limit
140, `0x005A45FC` limit 35 and `0x005A471C` limit 3. Restrict observations
to the declared ranges, excluding later priority paths. Track F's mask-sixteen
branch and mask-eight update, initial equality versus post-counter fresh reads,
full arithmetic and count stores, replacement zero-position target, and fresh
record/base/index reads at `0x005A4609`. Keep reports in
GAME_DIR/analysis/exe-batches; commit no original listings or bytes and execute
neither interpreter nor game.