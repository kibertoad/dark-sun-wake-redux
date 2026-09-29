---
id: FND-CONFIG-165
title: A pointer wrapper reads metadata before its null test and returns zero after the runtime call
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 444C:0092
tool: Python 3.14.7 and Capstone 5.0.7 entry-based bounded 16-bit instruction checks and declared MZ relocation mapping
environment: null
---

## Observation

The complete far wrapper 444C:0092 occupies file span
`0x00039752..0x000397B6`, ending with far return at
`0x000397B5`. It takes one far pointer from BP+6/BP+8.
Before testing either pointer word for zero, it reads a
word four bytes before the supplied offset in that segment
and saves the word locally. This is an actual memory read
before the null-pointer branch, not a guarded metadata read.

For a nonnull pointer, the body shifts that saved word
left four in word arithmetic, adds it to the supplied
offset in word arithmetic and tests byte minus five from
the resulting address against 77. A match writes zero to
that byte. A null pointer or a nonmatching byte instead
writes one to byte 55CD:0000. The flag segment operand
is resolved by its declared MZ relocation. There is no
own length, segment-wrap or accessibility validation for
these metadata reads and writes.

Both branches then forward the original pointer to
resident 1000:149B. The segment operand is another
declared MZ relocation. The runtime callee is not read
here. If it returns to the normal continuation, the wrapper
sets DX and AX to zero, restores its frame and returns.
It does not retain a callee return as a success result,
and neither the null nor marker-failure branch suppresses
that runtime call.

FND-CONFIG-162's 0BC1 caller tests current DS:6554 for
nonzero before requesting this wrapper. It stores returned
DX:AX back into that field. FND-CONFIG-161's 0C21 caller
does the same with current DS:6558. On each normal wrapper
return, under valid unchanged caller storage and maintained
DS, the respective field is therefore cleared. This is not
proof that the runtime callee successfully releases memory
or that either supplied pointer and metadata was valid.
Neither named caller supplies null on its tested branch;
other callers and intervening state are not inventoried.

There is no own DS write in this wrapper. Runtime-callee
DS preservation and other effects remain separate conditions
on the caller's later store. A malformed or aliased pointer
can affect memory not described by the ordinary metadata
reading; no safe-pointer invariant is established.

## Interpretation

The local wrapper couples metadata/marker checks, a flag
write and an unconditional runtime request, then returns a
zero pointer on its normal continuation. Its later null
test does not protect the earlier word read. Clearing the
caller's field follows from the returned zero, not from a
verified successful release or rollback contract.

## Alternatives

FND-CONFIG-167 subsequently reads the runtime dispatcher and one
route's local rejection/result conversion. The outer returned zero
discards that route's result; native reachability and valid metadata,
shared-slot and interrupt dependencies remain open.

Q-CONFIG-008 and Q-SCRIPT-003 retain runtime 1000:149B,
55CD:0000's producers/consumers, valid pointer metadata,
all caller inputs and DS preservation. One reading supplies
ordinary valid metadata and a returning runtime call;
another supplies a mismatched marker, inaccessible or
changed pointer, or a non-returning/error callee. Complete
producers and the runtime body would distinguish reachability
and the actual effects, not merely the cleared output field.

The null-before-read concern is a local ordering fact. It
does not show that the two named nonnull callers reach an
invalid null access, nor that a native failure is observed.
Q-SCRIPT-007's resident fixtures cannot establish operating-
system effects of the unread runtime callee or execute the
overlay callers themselves.

## How to reproduce

Read 444C:0092 through 00F5 from its entry. Verify the
initial far-pointer load and word read before the two-word
null test. Keep the shifted metadata and offset arithmetic
word-sized. Follow both marker outcomes into the same
runtime call, resolving its MZ segment and the flag's
segment separately. Verify the explicit DX:AX zero after
that call, then compare the named callers' pointer tests
and stores in FND-CONFIG-161 and FND-CONFIG-162. Keep
runtime semantics, pointer validity and native results open.
