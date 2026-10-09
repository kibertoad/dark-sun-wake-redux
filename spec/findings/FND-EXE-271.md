---
id: FND-EXE-271
title: Startup initializer selection reaches a loader wrapper with an explicit root call frame
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:013C..1000:014C
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:0220..1000:0264
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4AE5:0D27..4AE5:0D82
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x00050994..0x000509BE
tool: Capstone 5.0.7, Python 3 and xxhash 4.0.1
environment: null
---

## Observation

The later startup continuation zeros BP, loads ES through CS-relative word
`0x02C4`, sets SI `0x3994` and DI `0x39BE`, and calls `1000:0220`.
Under FND-EXE-269's retained initial segment publication, the table maps
to shipped `0x00050994..0x000509BE`: seven six-byte records.

The selector initializes AX to `0x0100` and DX to DI, scans BX from SI
in steps of six until BX equals DI, skips records whose first byte is
`0xFF`, and compares the zero-extended second byte against AX unsigned.
Only a strictly smaller value replaces AX and DX, so equal priorities
retain the earlier record. DX equal to DI at scan completion returns near.
Otherwise it compares the selected first byte with zero and marks that
byte `0xFF` before calling. It saves ES. A nonzero original first byte
uses a far call through the words at record plus two; zero uses a near
call through its offset word in current CS. After return it restores ES
and restarts selection. SI, DI, record memory and saved stack preservation
across selected callees are not locally enforced.

The initial first record's bytes are type one and priority one, followed
by offset `0x0D27` and stored segment `0x3AE5`. The segment site
`0x00050998` is an MZ relocation, giving `4AE5:0D27` under load segment
`0x1000`. The remaining initial type/priority pairs are 0/2, 0/16, 1/16,
0/16, 0/16 and 1/30. Therefore the first record is selected first under
the stated table binding and initial contents. The pre-call mark changes
its state even if that invocation later fails or never returns.

The wrapper at `4AE5:0D27` saves DS, SI and DI, loads the loader state
segment, calls `4AE5:029B` and reads its current word `0x011A` into BX.
It then loads the startup data segment into DS and ES. If BX is below
ES-relative word `0x3570`, it adopts that word; otherwise it doubles BX
at word width. It increments BX, saves it, multiplies it unsigned by
sixteen and pushes DX then AX before far-calling an external target.
Three POPs into BX discard those argument words and recover saved BX.

The returned DX:AX pair is tested for zero by copying AX to CX, OR with
DX and JCXZ. Zero takes the far failure tail. Nonzero increments DX at
word width, adds it to BX at word width, and pushes DX, BX, zero, zero.
PUSH CS followed by the near call at `4AE5:0D72` to `4AE5:0010` constructs
the root's far-return frame. Under its intact SS:BP frame, its words at
BP plus six/eight are zero, plus ten is BX, plus twelve is DX. Its far
return with eight-byte cleanup removes those four argument words.

The wrapper tests the root's AX at full word width with OR AX,AX. Nonzero
takes the failure tail; zero restores DI, SI and DS and far-returns without
argument cleanup. The failure tail is a relocated far jump with offset
`0x02AD`; it bypasses those local restores. External target effects,
failure continuation and actual initialized allocation are not established.

## Interpretation

This supplies a source-backed incoming root call and complete local frame
construction beyond an arbitrarily selected entry. It connects the declared
startup selector, initial far-pointer record and wrapper to FND-EXE-175's
root, conditional on startup segment/table preservation and callee effects.
The original indirect selector is not admitted solely from an analyzer
caller list: its indexing, bound, call kind and target pair are read together.

Q-EXE-001 and Q-EXE-010 retain preceding startup effects, all other root
callers, input/header writers, external allocation result and failure target,
saved-frame preservation and live table mutation. Native reachability after
the external request is conditional, not guaranteed. No complete_reading
or inventory replacement follows.

## Alternatives

Treating the root call as an ordinary near frame ignores PUSH CS and its
far-return cleanup. Treating every initializer as a far call ignores the
record type branch. Treating priority as a slot index ignores the scan and
strict unsigned minimum selection. A callback mark is not rolled back by
the caller before a failed or nonreturning invocation.

## How to reproduce

At revision `f142931`, verify the installed source identity in FND-EXE-236.
Decode shipped `0x0000533C..0x0000534C` at IP `0x013C`,
`0x00005420..0x00005464` at IP `0x0220`, and
`0x00040D77..0x00040DD2` at IP `0x0D27`, in sixteen-bit mode.
Use model segments `0x1000` and `0x4AE5`, MZ header size `0x5200` and
load segment `0x1000`. Read seven records at shipped start `0x00050994`,
stride six, each as two bytes and two little-endian words. Check site
`0x00050998` in the MZ relocation table and retain its complete target pair.
Track each push/pop and the root's far-return cleanup using FND-EXE-175,
with the argument consumers in FND-EXE-244 and FND-EXE-265. Do not assume
external target effects from the local frame. Source and reports stay in GAME_DIR.
Verify wrapper segment operand sites `0x00040D7B`, `0x00040D87` and
`0x00040D8C` with the operand wrapper and targetOffset zero; they resolve
to initial segments `0x55CE`, `0x57E0`, `0x57E0`. The failure-tail segment
site `0x00040DD0` resolves to `0x1000`; retain its separate offset `0x02AD`.
