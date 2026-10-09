---
id: FND-EXE-504
title: Game diagnostic write helper expands line bytes and returns mixed input and output counts on short writes
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:3DCC..1000:3EDA
  - build: BLD-GOG-EN-1.1
    file: CD:DSUN.EXE
    address: 1000:3DCC..1000:3EDA
tool: Capstone 5.0.7 and xxhash 4.0.1
environment: null
---

## Observation

FND-EXE-503's far callee 3DCC saves BP, reserves 0088 bytes and
saves SI and DI. It holds the incoming handle at SS:BP+6 in DI
and source offset at SS:BP+8 in SI. It compares the full handle
unsigned with current DS:37C2 installed or DS:3736 on disc.
A handle at or above the limit passes word six to FND-EXE-502's
near 06BA, which returns FFFF and removes its argument. The caller
then takes common cleanup. This limit test precedes the quantity test.

For an admitted handle it increments the incoming quantity word at
SS:BP+10 at word width and compares that result unsigned with two.
Results below two return zero: original quantities zero and FFFF both
take this path. Other quantities continue. It doubles the handle at word
width and tests indexed flags at DS:37C4 installed or DS:3738
on disc. Bit 0800 calls 07B0 with this handle, zero quantity pair
and mode two, removes eight argument bytes and ignores its returned pair.
It then reloads the indexed flags and tests bit 4000. Clear passes the
original quantity, held source and handle to FND-EXE-500's 3EDA,
removes six argument bytes and returns that helper's AX unchanged.

Set bit 4000 clears indexed bit 0200 before processing. The helper
holds the original source offset at SS:BP-6, remaining input quantity
at SS:BP-2 and sets SI to the numerical offset BP-0088.
Each nonzero remaining count is decremented before consuming one byte
through current DS at the old source offset; the saved source offset
advances by one at word width. The byte is retained at SS:BP-3.
For byte 0A it writes an extra 0D through DS:SI and advances
SI, then writes the retained byte through DS:SI and advances SI.
Every other byte makes only the latter write.

After those stores it computes SI minus BP-0088 at word width and
compares that difference signed with 0080. A smaller difference continues
the input-count test. Otherwise it holds that difference in SI and calls
3EDA with handle, numerical source offset BP-0088 and that held
quantity, then removes six argument bytes. Returned AX equal to SI resets
SI to BP-0088 and continues. A different AX equal to FFFF returns
FFFF. Every other mismatch returns, at word width, original input quantity
minus remaining input plus returned output quantity minus requested chunk
quantity. It makes no retry or rollback on this path.

Once remaining input is zero, it computes the final SI difference from
BP-0088. Zero returns the original input quantity without another call.
Nonzero requests that many bytes through 3EDA with the same numerical
source offset. Equality returns the original input quantity. FFFF returns
FFFF. Other mismatches return original input quantity plus returned output
quantity minus requested final quantity, at word width. Thus a short-write
return mixes input-byte consumption with output-byte counts after expansion;
it is not locally a verified count of delivered original bytes.

Under admitted nonwrapping frame offsets, unchanged segments and callee
preservation, each admitted input byte emits one or two bytes. The chunk
threshold is checked after emission, so an ordinary chunk request can reach
0081 bytes when a two-byte emission begins at 007F. A fully returned
chunk resets the output offset. The local source and output offsets have
no segment adjustment. Critically, forming an offset from BP does not
switch DS to SS: byte stores and 3EDA's native source use current DS,
while the frame locals use SS. The reserved stack frame alone does not
admit that destination or prove DS and SS equal. Source/destination aliasing,
frame overlap under unequal segments or wrapped offsets and native preservation
remain unresolved.

Every local exit restores DI and SI, resets SP to BP, restores BP
and returns far without incoming cleanup. Both editions have matching local
control flow with the distinct limit and flag-table offsets above. This helper
contains no interrupt itself; its selected callees are the error, positioning
and write helpers already recorded in FND-EXE-500 and FND-EXE-502.

## Interpretation

This resolves the local write helper below the diagnostic flush, including
quantity edges, line-byte expansion, post-store chunk threshold and short-write
arithmetic. It does not prove that its stack-derived offsets address writable
storage through DS or that the native write succeeds. Q-EXE-007 retains
actual DS/SS and frame admission, table and record writers, extents, aliases
and lifetime, native results and preservation, the other counted-byte branches,
surrounding callers and broader game launch-capability coverage. No complete
output reading or whole-game launch exclusion is claimed.

## Alternatives

Treating quantity FFFF as a large ordinary request ignores the wrapping
increment guard. Treating every chunk as bounded to 0080 ignores the
post-emission threshold. Treating BP-derived output as stack storage ignores
DS-based stores. Treating the positioning call as a success prerequisite
ignores its discarded result. Treating a short-write result as delivered input
bytes ignores the distinct output quantity after expansion.

## How to reproduce

At revision 408a0a5 require both DSUN.EXE identities from FND-EXE-350.
Use Capstone 5.0.7 in sixteen-bit mode, header size 5200 and modeled
load segment 1000. Decode shipped 8FCC..90DA at corresponding
1000:3DCC..1000:3EDA in both editions. Follow the unsigned handle
and wrapping quantity guards, separately reloaded flag tests, positioning result
disposal, SS frame locals and DS byte accesses. Track the pre-emission
remaining decrement, post-emission signed threshold, separate full-chunk and
final-chunk mismatch arithmetic and common cleanup. Review quantities zero,
FFFF and one, byte 0A at output difference 007F, and returned
quantities equal to, shorter than and FFFF relative to each request.
These are static branch readings, not executions. Licensed bytes stay outside
Git; no game process, DOSBox or emulated call runs.
