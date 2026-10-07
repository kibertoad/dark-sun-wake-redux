---
id: FND-EXE-124
title: Selected callee prefix publishes input byte and gates two reader calls or recursive fallback
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x00411A30..0x00411A8B
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x00411B62..0x00411B92
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x00411CD0..0x00411D4C
tool: Ghidra 12.1.3 PUBLIC bounded selected-callee prefix reading
environment: null
---

## Observation

FND-EXE-102 records a wrapper call to `0x00411A30` with a selected
second input of two or six. FND-EXE-122 and FND-EXE-123 record direct calls
with a slot field as first input, full zero as second input and retained
`0x0075B200` as third input. The callee saves four registers and
reserves seventy-six bytes, putting the first two full inputs at current
ESP plus `0x60` and `0x64`, respectively. It retains those values
as V and F, publishes V's low byte to `0x0075B0A1`, then calls
`0x004A0DE0`. That publication precedes all the local gates below;
the helper's effects and exceptional completion remain unread.

After an ordinary return it freshly tests byte `0x0075B14C`. Zero
takes a separate path at `0x00411B97`, not described here. Nonzero
reads full `0x0075B204` as M. Mask `0x20000` absent proceeds to
the indexed prefix; present takes additional gates. Those gates proceed
to the same prefix if F's mask one is absent, F's mask eight is present,
or M contains both bits of mask `0x3000`. Otherwise they publish
full zero to `0x0075B18C` and take the recursive fallback below.
Consequently the optional callers' F zero bypasses these extra gates,
without proving which initial byte branch is taken.

The indexed prefix forms T as eight times V in full thirty-two-bit
arithmetic, initializes both full locals at current ESP plus `0x40`
and `0x44` to zero, and compares full `0x0075B168` with T,
unsigned. The local stores preserve that comparison's flags. Only a
strictly greater bound takes the two-reader path. There it reads full
`0x0075B164` as a base B, publishes full zero to `0x0075B144`,
forms B plus T with wrapping arithmetic and calls `0x004F6740`
with that value in the first outgoing slot. It stores the returned full
EAX at local `0x40`, then calls the same reader with retained B plus
T plus four, also wrapping, and stores that return at local `0x44`.
After both ordinary returns it sets full EAX one, publishes full three to
`0x0075B144`, and joins the AL test at `0x00411A89`. This
success path continues to later gates at `0x00411A91`; their full
contracts are not established here. Neither reader result is tested for
failure before the full three publication. An exceptional reader exit
has no local guarantee of reaching that publication.

If the unsigned bound is not greater, both locals remain zero and AL
is zero at the join. That edge computes C as wrapped eight times V plus
F's low bit, compares C with `0xFFFFFFFE`, and publishes full zero
or one from equality to `0x0075B18C`. In this local computation C
has low three bits zero or one, so equality with the value whose low
three bits are six cannot hold. This edge therefore publishes zero,
without reading either indexed value. It still takes the recursive
fallback rather than an immediate failed return.

The fallback at `0x00411CD5` writes first outgoing input thirteen,
reads fresh full `0x0075B200`, writes second outgoing input six and
that fresh value as third input, then recursively calls `0x00411A30`
at `0x00411CEF`. Only after that call returns normally does it restore
the frame and return. The local restoration leaves the recursive EAX
unchanged. There is no local recursion-depth check, rollback of
`0x0075B18C` or restoration of the earlier published input byte.
The recursive call's gates and external effects must be read separately;
this prefix alone does not establish termination or a failure result.

## Interpretation

The selected callee can publish state before its helper and reader calls,
and an index-gate rejection leads to a recursive call with fresh global
input rather than an ordinary local failure. The zero second input in the
optional consumers removes only the extra mask gates, not the initial
byte gate, indexed bound or downstream dependencies. Q-EXE-009 in
FMT-EXE-006 remains open for the initial helper, alternate-byte path,
reader mapping and effects, later decoding/gates, recursive completion,
input producers and storage lifetime. These prefix facts do not establish
complete selected-callee effects or actual PATH behavior.

## Alternatives

- The bound is strict and unsigned, and compares a wrapped eight-times
  value; it is not an independently established range proof for V.
- The bound-failed edge's equality cannot hold for its locally formed C,
  but zero publication does not imply a zero return: recursion follows.
- Reader returns are stored without a local failure test. The full-three
  publication proves only reaching the ordinary continuation locally.
- Optional F zero bypasses the extra gates, while the fresh byte branch
  and remaining reads still apply. The third input is not consumed in
  this bounded prefix; its later use remains open.

## How to reproduce

Verify FND-EXE-011's executable identity and FND-EXE-099's six physical
controls. FND-EXE-102, FND-EXE-122 and FND-EXE-123 independently supply the
direct target. Use the saved Ghidra program with -noanalysis and
ReportInstructionWindow.java at `0x00411A30` limit 100,
`0x00411CD5` limit 36 and `0x00411CC4` limit 14. Restrict claims
to the declared prefix, extra gates, reader-call and recursive-return ranges;
exclude other decoding paths and following functions. Track the ninety-two
byte frame displacement, full input versus low-byte publication, fresh reads,
flags across local stores, unsigned strict bound, wrapping arithmetic and
last writer of each recursive outgoing slot. Follow AL at the join, the
low-three-bit invariant on the bound-failed equality and recursive EAX through
frame restoration. A supplementary window at `0x004F6740` limit 20
identifies reader mapping/indirection branches for further study, not a
complete reader contract. No native or emulated execution is part of this
observation.
