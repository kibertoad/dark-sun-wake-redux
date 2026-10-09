---
id: FND-EXE-293
title: Supplied-storage producer increments its request and writes through a returned header count
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 444C:00FA..444C:0162
tool: executable-reader 2.5.0, Capstone 5.0.7 and xxhash 4.0.1
environment: null
---

## Observation

FND-EXE-292's storage producer reaches 444C:00FA. It forms an
SS-relative BP frame with eight local bytes. It reads the incoming high
word at BP+08 into AX and low word at BP+06 into DX. It adds one
to DX and propagates carry into AX, with sixteen-bit words and no
subsequent overflow rejection. It pushes adjusted high AX, then low DX,
and calls 1000:15A5. The segment operand at shipped 0x000397D1
is a declared MZ relocation resolving to that allocator. It removes four
argument bytes and saves returned DX at BP-02 and AX at BP-04.

It tests AX OR DX. A zero pair clears DX and AX, restores SP from BP
and far-returns after restoring BP. There is no local header access on
this zero-result path.

For a nonzero pair, it copies saved DX into AX and saved AX into DX,
subtracts four from DX without propagating borrow into AX, and saves
that pair at BP-06/BP-08. It loads ES:BX from this saved far pair and
reads a word at ES:BX. It overwrites its incoming high argument word
at SS:BP+08 with zero and incoming low word at SS:BP+06 with the
word just read. It loads that low word into AX, sets CL=04 and shifts
AX left four at word width.

It reloads the same saved far pair into ES:BX, adds shifted AX to BX
at word width, and writes byte 77 to ES-relative BX-01. Neither the
subtraction, shift, addition nor final displacement changes ES on wrap.
It does not locally bound the header count or prove the resulting byte
is inside the allocation. It finally reloads saved original DX:AX,
restores SP and BP and far-returns without removing incoming arguments.
It does not locally save CX, ES or BX; the nonzero path explicitly
changes CL, ES and BX. DS restoration depends on the allocator path,
not a private DS save in this wrapper.

## Interpretation

For FND-EXE-292's incoming double word 00000400, the request passed
to the allocator is 00000401. The returned pair reaches the caller
unchanged by this wrapper's header-derived byte write. This does not
establish that it describes 1,024 writable bytes, that the preceding word
is valid initialized storage, or that the final byte lies in that extent.
The full-width increment wraps FFFFFFFF to zero; the later header
arithmetic wraps each offset independently. No segment normalization is
performed by these local steps.

FND-EXE-272 reads the allocator's rounding and shared DS restoration;
FND-EXE-273 through FND-EXE-283 supply its helper and publication
readings. Q-EXE-010 retains header-count production, admitted returned
offset/segment ranges, state writers and lifetime, aliases and interrupt
preservation. Those obligations must settle the wrapper's actual writable
extent before its nonzero result can establish usable supplied storage.
No complete-reading promotion or caller-absence claim follows.

## Alternatives

Treating the outgoing request as the unchanged input ignores the increment.
Treating offset minus four as a normalized far-pointer subtraction ignores
the missing segment borrow. Treating the shifted header count as a
full-width byte length ignores the word shift. Treating the final byte
write as validated storage ignores its unchecked header-derived address.
Treating incoming arguments as unchanged ignores their in-place overwrites.

## How to reproduce

At revision 1460d4d require FND-EXE-176's installed DSUN.EXE identity,
XXH3-128 e296af55ba2ecde7e77f555c90f33d0b. With Capstone 5.0.7
in sixteen-bit x86 mode decode shipped half-open
0x000397BA..0x00039822 at initial IP 00FA, modeled CS 444C.
With the committed operand reporter, loadSegment 1000, site
0x000397D1 and targetOffset 15A5, resolve the allocator target.
Track the SS-relative locals, both incoming argument overwrites, word-width
offset arithmetic and each cleanup path. Source bytes and reports remain
outside Git. No original execution or allocator emulation is involved.
