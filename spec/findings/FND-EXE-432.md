---
id: FND-EXE-432
title: Quantity addition wraps the requested total and publishes the previous quantity only after adjustment success
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1425:1879..1425:18F6
tool: Capstone 5.0.7, Python struct and xxhash 4.0.1
environment: null
---

## Observation

The 125-byte body at 1879 decodes as 47 instructions. It saves
BP and allocates six local bytes. Current DS:00C4 zero returns
AX three without the later calls or output store. Otherwise it calls
0FA3 with incoming word SS:BP+6 and far local output pointer
SS:BP-2, removes six outgoing bytes and tests full returned AX
through DX. Nonzero returns that result. FND-EXE-414 describes
the selector's effects before either success or failure.

On zero it compares current DS:3EEE indexed by local BP-2
times 000E against incoming word SS:BP+8. The scalar must
match and similarly indexed DS:3EEC must not equal one.
Either failed check returns AX eleven. These checks occur after
selection and do not roll back its effects.

### Requested total and nested call

The body reads current indexed DS:3EF0 as a doubleword and
stores it in local SS:BP-6. It adds incoming doubleword
SS:BP+0A to EAX at 32-bit width, with no overflow check.
It pushes that resulting doubleword, then the incoming doubleword
SS:BP+6, and calls 17AE. The latter four bytes contain the
identity/scalar words previously consumed at +6 and +8, not a
far-pointer argument to the adjustment wrapper.

Eight outgoing bytes are removed. Full returned AX is tested through
DX. FND-EXE-413 describes 17AE's own gate, repeated 0FA3
selection and scalar/head checks, unsigned quantity dispatch to signed
helpers, zero-quantity release and final quantity/dirty publication.
The outer wrapper's earlier checks do not independently admit stable
record state for that nested selection.

The addition can wrap the requested total to zero even when the
old quantity and delta are nonzero. For example, the local arithmetic
maps old FFFFFFFF plus delta one to zero. Under admitted stable
state and preserved inputs, the nested zero-quantity route can then
be selected. This illustrates the local width, not proof that native
callers admit that old quantity or delta. An added delta of zero
still invokes the adjustment wrapper, whose successful equal-quantity
route has final quantity and dirty stores.

### Output and failure ordering

Nonzero nested result returns without this wrapper's caller-output
store. Earlier outer selection and nested adjustment effects may remain;
their absence or rollback is not inferred from the missing output.

Only a zero nested result loads ES:BX from incoming far pointer
SS:BP+0E, re-reads the doubleword at local BP-6 and writes
that word pair through ES:BX. It returns AX equal to DX.
With admitted stable local storage, this output is the previous quantity,
not the requested total or the nested helper's reduced argument.
The local snapshot and caller pointer are consumed after the nested
call; writable aliases or native effects on those slots remain open.

The body does not save or restore DS or ES, and applies no local
output extent or alias check. The selected record index and current DS
are used before the nested call; the post-call output relies on local
snapshot and pointer preservation rather than another record load.
Its doubleword store does not independently establish admitted capacity
or persistence of the caller's destination.

## Interpretation

This supplies another caller of the quantity-adjustment wrapper and
records wrapping requested totals and conditional previous-quantity output.
Q-EXE-007 retains complete callers and delta/quantity/scalar admission,
state producers, repeated-selection consistency, snapshot/pointer aliases
and extents, current-segment preservation and native contracts. No
complete reading, admitted oversized input, atomic adjustment/output
or observed original bug is declared.

## Alternatives

Treating addition as checked or saturating invents an overflow test.
Treating the output as the requested new total contradicts the saved
old quantity and its later load. Treating delta zero as a no-op
ignores the nested adjustment's final stores. Treating the pushed
identity/scalar doubleword as a far pointer gives the nested frame
the wrong argument meaning. Treating the outer checks as proof of
unchanged state ignores the repeated selection and native dependencies.

## How to reproduce

At revision a80686ec require installed DSUN.EXE length 634416 and
XXH3-128 e296af55ba2ecde7e77f555c90f33d0b. Decode file base
0x9450 plus 1879..18F6 in sixteen-bit mode. Require 125 bytes
and 47 instructions. Follow the gate, selector output, scalar/head
checks, doubleword snapshot/addition, pushed identity/scalar and total,
eight-byte cleanup, full result and post-call snapshot publication.
Use FND-EXE-414/413 for dependencies. Preserve caller, input,
alias, extent, state-producer and native-effect gaps. Licensed bytes
remain outside Git; no game, DOSBox or emulated call runs.
