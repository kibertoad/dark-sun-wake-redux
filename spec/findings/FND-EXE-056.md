---
id: FND-EXE-056
title: Callback access helpers reread a selected-record local and use full-width indexed stores and wrapped count adjustments
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x00600A30..0x00600A44
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x00600A50..0x00600A5E
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x00600A60..0x00600A71
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x00600A80..0x00600A8D
tool: Ghidra 12.1.3 PUBLIC, bounded instruction reading
environment: null
---

## Observation

These four direct helpers use original 32-bit stack arguments. Each reads
its first argument as an address and dereferences its first word to obtain
a record address. None locally checks either address for zero or valid
storage, and none calls another helper in its cited body. Each performs
ordinary frame restoration and returns.

The helper at `0x00600A80`, called by FND-EXE-055 with the callback's
sixth argument, returns the full word at record offset 28. The zero-result
early exit in that finding therefore tests this fetched field, not an
independently computed success status. The helper rereads the selected local
and then the selected record on every call; it does not retain an earlier
selector's record address.

The helper at `0x00600A50` returns the record-offset-four word plus one
at 32-bit width. The increment wraps; it does not store the adjusted value
back. The helper at `0x00600A60` reads its second original argument,
subtracts one at 32-bit width, stores the adjusted word at record offset
four, and returns with that adjusted word in the return register. It does
not read the old offset-four value or saturate either endpoint. The apparent
inverse adjustments do not establish a semantic count, valid range or stable
record identity across separate calls.

The helper at `0x00600A30` reads its second argument as an index and
third as a value. It stores the full value at record plus eight plus four
times the index, with 32-bit effective-address arithmetic. There is no
local index, length, allocation or overflow guard. It reads no fourth
original argument. Its return register contains the index after the store;
there is no explicit success calculation. This direct writer does not prove
that any caller's outgoing index or value is admissible or that the destination
cannot alias the selected-local address or other state.

FND-EXE-055's callback prefix supplies a selected-local address as the first
argument to the offset-28 reader. These helpers perform
indirections through the caller-supplied address, not direct accesses to one
cached record. The later callback's field meanings and complete input producers
remain outside this finding, even where local reports display its calls.

## Interpretation

The callback helper boundaries now have concrete field, input-width and
return contracts. They do not validate selected-record storage or resolve
the callback's matching and dispatch algorithms. Q-EXE-009 retains field
producers and meanings, caller indices and values, pointer lifetime and aliases,
remaining callback branches and dispatcher frame mapping. No bounded record
schema or complete lifecycle is inferred from these small accessors alone.

## Alternatives

Interpreting the offset-28 reader as a boolean computation, treating the
plus-one reader as a store, clamping the minus-one writer at zero, or reading
a fourth index-writer argument are ruled out by the direct bodies. Treating
the indexed store as intrinsically bounded by a known record size is unsupported:
its guard must come from caller evidence. Separate accessor calls may select
different records because each rereads the supplied local.

## How to reproduce

Use FND-EXE-011's verified PE and image base. Read thirty instructions from
`0x00600A30` and seven from `0x00600A80`, restricting claims separately
to each cited body and excluding gaps. Follow original argument widths,
the two-stage dereference, exact read/store offsets, indexed effective address,
wrapped arithmetic, unchanged versus written fields and return-register last
writers. Use FND-EXE-055 for the sixth-argument caller and its zero-field
consumer. Keep caller bounds, field semantics, lifetime and alias admission
conditional. Keep rich reports local and execute no interpreter or game.
