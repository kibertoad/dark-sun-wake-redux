---
id: FND-EXE-258
title: Second initializer retains provider publications across fallback and returns current endpoint difference
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4AE5:0ECD..4AE5:107D
tool: executable-reader 2.5.0, Capstone 5.0.7 and pypcode 4.0.0
environment: null
---

## Observation

FND-EXE-256's initial helper at `4AE5:0ECD` contains 155 instructions
and 432 bytes, six computed far calls, one direct near call and four
interrupt sites. It reserves 34 local bytes, saves DS/SI/DI and loads DS
through CS-relative word five. BP-relative locals use SS.

With bit zero of state byte `0x0042` set and bit one set, it skips setup
and returns the current endpoint difference. With bit zero clear it
replaces the entire byte with one. The other admitted routes initialize
state pairs `0x003A`/`0x003C` and `0x003E`/`0x0040` to low zero/high
`0x0010`, before environment checks. There is no rollback of those stores
on the later zero-result exits.

It probes the value produced by pushing SP and compares it with the
post-pop SP. Inequality selects the zero-result exit. Equality requests
DOS service AH `0x30`; returned AL at least ten also selects zero.
Otherwise it exchanges AH/AL and stores AX to SS:BP minus 34. It reads
a byte through ES=`0xF000`, offset `0xFFFE`, rejecting unsigned values
above `0xFC` or equal to `0xFB` or `0xF9`. The retained version word
below `0x0031` selects fallback; at least that value reaches
FND-EXE-180's multiplex requests and pointer publication.

The first multiplex response must have AL `0x80`; otherwise it falls
back. The next request's BX/ES become state words `0x0043`/`0x0045`,
and a far call through that current pair receives AX zero. Returned AH
below two falls back without explicitly clearing the published pair.
Otherwise another far call receives AH eight. AX zero skips to the
endpoint-difference return. A nonzero result becomes SI; DX is exchanged
into AX, AH becomes nine and a further call is made through the pointer.
AX zero again selects the difference return without rollback.

A nonzero result stores current DX to word `0x0047`, sets AH `0x0C`
and calls the pointer. On AX nonzero it stores BX/DX to the lower state
pair, multiplies current SI by `0x0400` at full DX:AX width and adds that
lower pair to form the upper endpoint. On either result of that last
call it then requests AH `0x0D` and AH `0x0A` through the pointer,
reloading word `0x0047` into DX before each call. It clears word
`0x0047` afterwards without testing either result and returns the current
endpoint difference. Pointer/DS and SI/result preservation across all
calls remain unread external contracts.

Fallback requests interrupt `0x15` with AH `0x88`. AX zero selects the
zero-result exit. Nonzero multiplies AX by `0x0400`, adds `0x10` to DL
at byte width and stores AX/DX to the upper endpoint. It tests both words
of the previously stored pointer. A nonzero pair changes only byte
`0x003C` to `0x11` and clears lower word `0x003A`; a zero pair skips
those stores. The pointer's earlier publication can therefore affect
fallback even when its preceding admission call rejected.

It forms BX/CX from SS times sixteen plus the offset of SS:BP minus 32,
sets AX/DX to zero/`0x0010`, SI/DI to `0x0020`/zero and calls
`4AE5:11A9`. Carry set selects zero result. Carry clear compares five
bytes from current DS-relative `0x0020` against SS:BP minus 29. Equality
multiplies words at SS:BP minus 21 and minus 13, adds `0x10` to DL and
stores AX and only DL to lower word `0x003A` and byte `0x003C`.
The local bytes, callee-produced extent and upper byte's last writer
remain admission obligations; compare success alone does not prove them.

Next it obtains ES from zero-segment word `0x0066` and compares five
bytes at ES:`0x0012` against current DS:`0x0020`. Equality reads an
endpoint word at ES:`0x002C` and byte at ES:`0x002E`, comparing the byte
first and the word second against the lower state components. A larger
pair replaces lower word `0x003A` and byte `0x003C`. Inequality or a
non-larger pair leaves them. These byte stores do not explicitly clear
the adjacent high byte. Numeric segment and offset values alone do not
establish initialized, disjoint native storage.

The difference exit loads the upper pair and subtracts the whole lower
words into DX:AX with borrow. It has no explicit final carry test or clamp.
The zero-result exit clears AX and sign-extends that sixteen-bit value
into DX, giving DX:AX zero on the explicit path. Independent pypcode
semantics identify AX as the input and DX as the output at `4AE5:1075`;
Capstone's displayed mnemonic alone does not identify the operand width.
Both paths restore saved DI/SI/DS, reset SP from BP and return far without
argument cleanup, conditional on intact saved storage.

## Interpretation

This supplies the full initializer's ordered branches and current-pair
result consumed by FND-EXE-256. Its fallback is not fresh initialization:
earlier pointer and state publications remain relevant. The returned
pair can also consume partial-byte state through whole-word subtraction,
requiring actual upper-byte writers and preservation to be followed.

Q-EXE-001 and Q-EXE-010 retain environment results, live pointer and flag
writers, external contracts, the direct callee's initialized output extent,
segment/stack aliases and native lifecycle admission. No complete_reading
or replacement inventory is established.

## Alternatives

Clearing the pointer on every provider rejection is contradicted by the
fallback path after its publication. Treating every failure as a zero
result ignores routes returning current endpoint difference. Whole-word
high-component stores are contradicted by the byte-width writes. Reading
the epilogue as a thirty-two-bit EAX extension ignores its actual semantics.

## How to reproduce

At revision `ce5827f`, use FND-EXE-236's original-source identity, region
and default x86-bounds limits. Set entry and sole entries to `0x00040F1D`,
with no seeds or summaries. Check interval `0x00040F1D..0x000410CD`,
155 instructions, six computed far calls and direct call `4AE5:1006`.
Interrupts occur at `4AE5:0F11`, `4AE5:0F40`, `4AE5:0F49` and
`4AE5:0FBE`. Continuations assume interrupts and calls return; complete
CFG does not establish a Standard complete reading or initialized locals.

Decode that interval directly from the shipped source in sixteen-bit
mode with Capstone. Independently translate `0x000410C3..0x000410C6`
with pypcode language x86:LE:16:Real Mode, base address `0x1073`, and
check AX clearing and AX-to-DX extension. Follow each retained publication,
partial write, effective segment and result consumer; compare
FND-EXE-180's pointer prefix without studying the host emulator. Keep
source, configurations and reports in GAME_DIR.
