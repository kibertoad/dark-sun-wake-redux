---
id: FND-EXE-299
title: Allocator caller candidate requests a wrapped product and fills it in bounded chunks
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:18D0..1000:1959
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:07D9..1000:07F0
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:18BC..1000:18CE
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x000396FA..0x000396FF
tool: executable-reader 2.5.0, Capstone 5.0.7 and xxhash 4.0.1
environment: null
---

## Observation

A controlled incoming query for shipped 0x00006AD0 returned a
declared MZ call-byte candidate at 0x000396FA, whose segment operand
0x000396FD resolves to 1000:18D0. Limit 100 did not truncate it
and no candidate was unresolved. The independent known MZ control
0x00040DA9 resolved to 1000:15A5. This query supplies a target lead,
not an instruction path into its caller. Near/computed calls, unrelocated
pointers and instruction-boundary verification are excluded; no independent
FBOV-fixup control or caller-absence claim is supplied.

Decoding from the targeted candidate entry 1000:18D0 reaches
FND-EXE-298's allocator call at 18F0. It forms an SS-relative BP
frame with eight local bytes, saves SI, and loads two double-word inputs:
DX:AX from BP+08/+06 and CX:BX from BP+0C/+0A. It calls
1000:07D9. That helper saves SI and uses unsigned word multiplies
to form the low-word product and the two cross products. It adds the
cross products' low words to the low product's high word at word width,
restores SI and near-returns DX:AX. The result is the two input values'
product modulo 2^32; the original high-word product and carry beyond
the returned high word are not retained or tested.

The outer body overwrites BP+08/+06 with that product, pushes returned
DX then AX, pushes CS and near-calls 1000:15A5. Two pops into CX
remove the outgoing four bytes. It saves returned DX:AX at BP-02/-04
and tests AX OR DX. Zero skips all filling and returns the saved pair.
Nonzero copies the saved pair into a mutable local pointer at BP-06/-08.

While the overwritten product words remain nonzero, it selects a chunk:
FA00 when the high word is nonzero or the low word is unsigned-greater
than FA00; otherwise the low word. The preceding unsigned comparison
of the high word with zero also has a below branch, which cannot be
taken by an ordinary unsigned word value. It saves the chunk in SI,
pushes the local pointer's high then low words, pushes the chunk and
then pushes AX after changing AL to zero. AH is still the chunk's
high byte; the last argument word is not necessarily all zero.

The near callee 1000:18BC forms BP, saves DI, loads ES:DI from
SS:BP+08/+0A, loads CX from BP+06 and AL from byte BP+04,
then repeats byte stores. It restores DI and BP and near-returns while
removing eight argument bytes. It does not clear or restore the direction
flag locally, so store direction remains an incoming-state obligation.

After that call the outer body sets BX to saved chunk SI, CX to zero,
DX to SS and AX to the address of its local pointer at BP-08, then
calls 1000:0583. That pointer-update helper is not read here. It subtracts
SI from the remaining low word with borrow into the high word and
repeats. The shared exit reloads the originally saved allocator DX:AX,
restores SI, SP and BP and far-returns without incoming argument cleanup.

## Interpretation

This supplies the candidate body's request provenance and fill-call argument
writers. Each ordinary chunk is at most 64,000 byte stores; absent state
changes, the remaining-count subtraction accounts for the wrapped product.
This does not establish valid storage, pointer advancement, forward direction,
non-overlap or successful full initialization. The allocator's own rounding
and admission remain distinct from the caller's modulo-product request.

Q-EXE-010 retains the incoming candidate's grounded entry/path, argument
ranges and native CS, helper 1000:0583, direction-flag provenance,
callee preservation, aliases and admitted allocation extent and lifetime.
No complete-reading promotion or initialized-storage claim follows.

## Alternatives

Treating the product as checked full-width multiplication ignores discarded
high terms and carries. Treating the last pushed word as zero ignores its
retained AH, though the fill reads only AL. Treating byte stores as always
forward ignores the absent direction-flag initialization. Treating count
subtraction as proof of advancing valid storage ignores the unread pointer
helper and native extent admission.

## How to reproduce

At revision fc332c7 require FND-EXE-176's installed DSUN.EXE identity,
XXH3-128 e296af55ba2ecde7e77f555c90f33d0b. Run the committed
incoming reporter with loadSegment 1000, target 0x00006AD0, limit
100 and controls [0x00040DA9]. With locked Capstone 5.0.7 in
sixteen-bit mode decode shipped half-open 0x00006AD0..0x00006B59
at IP 18D0, 0x000059D9..0x000059F0 at IP 07D9 and
0x00006ABC..0x00006ACE at IP 18BC, modeled CS 1000. Follow
word multiply results, argument overwrites, retained local pointer, chunk
selection, callee cleanup and direction-flag dependencies. Source bytes
and reports remain outside Git. No original execution or complete incoming
search is claimed.
