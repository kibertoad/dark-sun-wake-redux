---
id: FND-EXE-378
title: Sound utility position helper retains second request pair after tested restoration request
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1000:2D48..1000:2E28
tool: Capstone 5.0.7 and xxhash 4.0.1
environment: null
---

## Observation

FND-EXE-377's pathname continuation calls far-returning 2D48 with a
record pair and saves returned DX/AX without testing it. The helper forms
an eight-byte local frame. Its first call to 05CC passes sign-extended
record byte four, zero double-word quantity and mode word one. It removes
eight argument bytes and saves returned DX/AX to SS:BP-02/-04.
Only full pair FFFF:FFFF selects immediate saved-pair return. FND-EXE-376
reads that wrapper's indexed flag clear, native register inputs and carry paths.

Otherwise it reloads the record pair and tests word zero signed. A
nonnegative value selects a 2C46 call with the record pair; that callee
removes four argument bytes. Its returned AX is sign-extended into DX
and subtracted with borrow from the saved pair at double-word width.
The root then returns the adjusted saved pair without another sentinel test.
FND-EXE-376 reads the count and newline scan that produce this AX.

A negative record word zero instead sign-extends record byte four,
doubles it at word width and tests bit 0800 in the current DS indexed
word at displacement DD3A. A clear bit skips further native requests.
A set bit calls 05CC with current ES's record byte four, zero quantity
and mode two. It reloads only the incoming offset into BX before this
byte read, because BX held the doubled table index. ES still comes from
the incoming-pair reload after the first request; no call intervenes between
that reload and this byte read. The second request therefore does not
depend on the first request preserving ES for this argument.

The second returned pair is held at SS:BP-06/-08. FFFF:FFFF returns
that current pair directly through common frame restoration. Any other
pair selects a third 05CC call with the first saved pair as quantity,
mode zero and a record byte loaded after a fresh incoming-pair reload.
The third returned FFFF:FFFF selects an explicit FFFF:FFFF return.
Any other result is discarded and the saved second pair replaces the
first saved pair, preserving its original high/low order.

Both negative-count routes then call 2C46 with the record pair. Returned
AX is sign-extended and added with carry to the currently saved pair at
double-word width. The root returns that adjusted pair without another
sentinel test. Thus without bit 0800 the first request supplies the base;
with bit 0800 and two later non-sentinel results the second request
supplies it. The third request's pair is never the arithmetic base.

All paths restore SP from BP, restore BP and return far without incoming
argument cleanup. The helper does not save DS, SI, DI or ES. It locally
updates its frame, not record fields, but its reached callees have the
state changes described by FND-EXE-376. No local rollback occurs when
a later request returns the sentinel. Native preservation and frame aliases
remain separate requirements for the ordinary stack and state readings.

## Interpretation

This resolves the local position helper used by FND-EXE-377. The first,
second and third returned pairs play different roles: initial arithmetic
base, conditional replacement base and tested-but-discarded result. Each
native request can precede a later failure or count adjustment. Neither
the final quantity nor a sentinel establishes an unchanged native position.

The count adjustment is signed at the caller despite a word-valued scan
return. Final arithmetic wraps at double-word width and is not subsequently
classified as success or failure inside this helper. Q-EXE-007 retains
record/count/flag producers, actual DS and indexed state, native results
and register preservation, scanned extents, frame aliases and later pathname
consumers. No admitted file-position contract or complete reading is claimed.

## Alternatives

Treating the third result as the returned quantity overlooks its discard.
Treating the second byte input as dependent on native ES preservation overlooks
the post-first-request pair reload and absence of an intervening call.
Treating every request failure as no preceding change assumes rollback absent
locally. Treating the adjusted pair as independently sentinel-checked adds a
test the suffix does not perform. Treating the scan return as unsigned
ignores its sign extension before double-word addition or subtraction.

## How to reproduce

At revision a5908e6 require FND-EXE-350's SOUND_DS.EXE identity:
length 204593, XXH3-128 236c2dc23c071eca421eb5b427caee57.
Decode shipped half-open offsets 0x00004148..0x00004228 at IP 2D48,
modeled CS 1000 and MZ header size 1400, using locked Capstone 5.0.7
in sixteen-bit mode. Bind FND-EXE-377's record pair, track every request
argument byte, record-pair reload, local saved pair and full sentinel test,
then follow both count-adjustment paths into common frame restoration.
Extensions at 2D5F, 2D8C, 2DAA and 2DD4 are sixteen-bit AL
extensions; 2E05 and 2E17 extend AX into DX despite wider mnemonics.
Original bytes remain outside Git; no original process, DOSBox, interrupt
thunk or emulated call is executed.
