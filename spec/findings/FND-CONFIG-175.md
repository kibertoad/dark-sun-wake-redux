---
id: FND-CONFIG-175
title: Callback bracket helpers preserve DS and SI on guard-bypass copy paths
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4328:0118
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4328:00E0
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4328:007C
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4328:00BC
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1BF3:5945
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1BF3:7AAB
tool: Python 3.14.7 and Capstone 5.0.7 complete local 16-bit entry readings and declared MZ relocation mapping
environment: null
---

## Observation

FND-CONFIG-171's callback wrapper retains the indirect
call's result in SI across resident 39D1:05E1, which
calls 4328:00E0 and 1BF3:7AAB. The preceding 0609
calls 4328:0118 and 1BF3:5945. This reading follows
those dependencies and the shared copy/coordinate helpers,
retaining each runtime stack guard as a separate condition.

The complete 4328:0118 body occupies
`0x00038598..0x000385C9`. After its guard through
1000:2E48 it passes the supplied far pointer to local
far 005A. That helper copies 138 bytes from that supplied
address to current DS:A2B4 using 4237:0076, as bounded
in FND-CONFIG-099. It then reads words at supplied
address+8A and +8C and passes them to local 009E,
which assigns current DS:A348 and A346 respectively.
0118 returns zero. Its supplied-pointer reads have no
own initial null check, even though the copy primitive
it uses has pointer tests.

The complete 4328:00E0 body occupies
`0x00038560..0x00038598`. After its guard it passes
the supplied far pointer to local far 007C, complete
span `0x000384FC..0x0003851E`. After its own guard,
007C calls 4237:0076 in the reverse direction: source
current DS:A2B4 and destination the supplied address.
It then returns zero. Next 00E0 passes addresses at
supplied offset+8A and +8C, retaining its segment and
word offset wrap, to local far 00BC. That helper's
complete span is `0x0003853C..0x00038560`; after its
guard it stores current DS:A348 through the first far
address and DS:A346 through the second, then returns
zero. 00E0 also returns zero. Neither coordinate helper
has an own null check on these two output addresses.

Both copy directions use the complete 4237:0076 body
at `0x000375E6..0x00037616`. After its guard, a null
source or destination skips the copy and returns zero.
Otherwise it supplies count 138 to 1000:0452. The latter
complete body, `0x00005652..0x0000566E`, saves SI,
DI and DS, loads the source into DS and destination into
ES, clears the direction flag and copies forward, words
then any remaining byte. It restores SI, DI and DS and
returns with eight argument bytes removed. ES is not
restored by this primitive. A zero result from 0076
therefore does not distinguish a performed copy from
its null-pointer bypass. These local bodies do not
validate capacity, overlapping input/output safety or
buffer semantic identity.

The complete 1BF3:5945 body at
`0x00016A75..0x00016A86` loads the supplied far pointer
into AX and ES, then stores its offset and segment at
CS:1052 and 1054. It does not write DS or SI. The
complete 1BF3:7AAB body at
`0x00018BDB..0x00018BE5` loads those CS words into
AX and DX and returns; it also does not write DS or SI.
The shared fields' other producers and timing remain open.

Thus, on guard-bypass paths with valid distinct memory
and balanced returns, all these local bodies preserve
incoming DS and SI through the bracketed calls. The
copy primitive temporarily replaces DS but restores it.
The outer 39D1:0609 and 05E1 bodies also do not otherwise
write SI or DS. Under those conditions 3D72:0D83's
retained-SI test reflects the indirect callback's AX,
rather than an intervening own SI rewrite. This resolves
a conditional part of FND-CONFIG-171, not all guard
outcomes or actual target preservation.

For the named ordinary DS value 57E0 and supplied
address DS:A05B, the 0118 copy reads the half-open
range DS:A05B..A0E5; 00E0 writes that same range.
The coordinate outputs are at A0E5 and A0E7. The fixed
copy range DS:A2B4..A33E, coordinate globals A348/A346,
and outer stored pointer DS:A057/A059 do not overlap
DS:A119. The two CS words belong to resident 1BF3.
These named, valid nonaliasing writes therefore do not
by themselves replace the callback field at A119. This
is not an exhaustive writer or interrupt analysis, and
other supplied pointers or changed DS can alias fields.

All external call segments were checked at declared
MZ relocation operands. The local 005A, 009E, 007C
and 00BC calls use push-CS/near-call pairs with their
far returns. The guarded helpers locally return zero;
that is not evidence of successful copies or native
rendering/state restoration.

## Interpretation

The bracketing helpers copy fixed buffers and coordinate
words in opposite directions and set/get a shared-CS
far pointer. Their normal guard-bypass paths preserve
the register holding the callback result and the ordinary
DS value. They leave ES changed and have explicit
null-copy bypasses, so a generalized preservation or
successful-operation summary would be too broad.

## Alternatives

Q-CONFIG-008 and Q-SCRIPT-003 retain all guard outcomes,
valid capacity and contents, buffer/stack disjointness,
other callers, DS and alias provenance, shared-CS writers,
callback target/result producers and interrupt timing.
One reading uses ordinary 57E0, valid distinct named
buffers and bypassed guards; another changes one of those
conditions. The local copy/write contracts distinguish
them without proving either native invocation. Fresh
indirect target reads in FND-CONFIG-171 still require
the target's producer and preservation analysis.

Q-SCRIPT-007 retains resident cases once layouts and the
harness exist: both copy directions, pointer-null bypass,
coordinate outputs, guard-bypass SI/DS and changed ES.
No native or emulated result is claimed.

## How to reproduce

Read 4328:0118 through 0148 and 00E0 through 0117,
following local 005A/007C and 009E/00BC in argument
order. Verify the two copy directions and coordinate
output addresses. Read 4237:0076 through 00A5 and
1000:0452 through RETF 8; inspect saved SI/DI/DS and
unrestored ES. Read 1BF3:5945 through 5955 and 7AAB
through 7AB4. Trace SI and DS across every guarded or
bypassed call before relating them to 3D72:0D83's
retained result. Compare FND-CONFIG-099 and the named
A05B arguments and A057 output in FND-CONFIG-171.
Keep valid-memory, other-writer and guard conditions explicit.
