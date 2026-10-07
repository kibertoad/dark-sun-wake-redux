---
id: FND-EXE-108
title: Fixed callback selects one of four object slots and forwards a shifted low word only for a nonnull slot
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x005A4BB0..0x005A4BE5
tool: Ghidra 12.1.3 PUBLIC bounded fixed-callback reading
environment: null
---

## Observation

FND-EXE-107 records wrappers that install target `0x005A4BB0` with
full argument V formed by a shifted zero-extended word OR a full object
field. This target reserves twelve stack bytes and reads its full input
at current ESP plus sixteen, corresponding to entry ESP plus four.
It retains V, computes V AND three and reads the full slot at
`0x02417280` plus four times that index. This expression bounds the
local index to zero through three, independently of any prior wrapper
input bound. Slot contents and backing-storage lifetime remain unresolved.

If the slot is zero it restores ESP and returns without a downstream call.
EAX at this return still holds V AND three: a null slot does not invariably
return zero. The caller in FND-EXE-104 does not use this as callback success.
For a nonzero slot O it writes O as its outgoing first full argument,
logically shifts retained V right two, zero-extends the low word of that
shifted result and writes it as its outgoing second full argument. It
calls `0x005A47C0`, then restores ESP and returns without normalizing
that callee's full EAX. There is no direct table or object-field store
in this callback, but callee effects are unread.

The forwarded second value is (V logically shifted right two) AND
`0xFFFF`. Bits eighteen through thirty-one of V are discarded there;
bits zero and one selected the object slot and are discarded by the shift.
With V from FND-EXE-107, the forwarded word combines the original input
word with the corresponding shifted object-field bits. Recovering the
original word unchanged would require an additional field contract that
these wrappers and this callback do not prove. A valid slot index also
does not prove that its nonzero contents identify a valid object.

## Interpretation

The fixed callback supplies a concrete consumer of the wrapper's constructed
argument: two low bits select a slot, and the next sixteen bits select a
forwarded input. Q-EXE-009 in FMT-EXE-006 still requires slot writers,
object-field admission, downstream behavior and native/callee conditions.
This local reading establishes neither a complete callback chain nor actual
PATH behavior and promotes no format status.

## Alternatives

- Null slot returns the retained index, which may be nonzero; no unconditional
  zero-success convention follows from the early return.
- Slot selection and downstream input have different bounds and bit widths.
- Bitwise OR in the wrapper need not be an invertible encoding of its input.
- Absence of direct stores here does not establish absence of callee effects.

## How to reproduce

Verify the executable length/hash from FND-EXE-011 and physical controls
from FND-EXE-099. The fixed immediate in FND-EXE-107 supplies the target
independently of analyzer naming. In the saved Ghidra program with
-noanalysis, use ReportInstructionWindow.java at `0x005A4BB0` limit
80; restrict this observation to the declared range ending at
`0x005A4BE5`, excluding following procedures. Track the twelve-byte
reservation, original argument retention, AND-three indexing, null-path
EAX, logical shift and word zero-extension, outgoing stores, and unchanged
callee return. FND-EXE-104 supplies the independently read callback invocation
and unused result. Keep reports in GAME_DIR/analysis/exe-batches; commit no
original listings or bytes and execute neither the interpreter nor the game.