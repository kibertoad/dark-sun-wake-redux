---
id: FND-EXE-355
title: Sound utility mode continuation publishes a record result before downstream configuration
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1000:2AF6..1000:2BC5
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1000:2A3A..1000:2AF4
tool: Capstone 5.0.7 and xxhash 4.0.1
environment: null
---

## Observation

FND-EXE-360 reaches this near callee with word zero, the mode far
pointer, the pathname far pointer and the selected record far pointer.
The callee reserves four local bytes and calls 1000:2A3A with the
mode pointer and two SS-relative output pointers to BP-02 and BP-04.
That parser removes twelve argument bytes on near return.

For the shipped `rt` mode identified in FND-EXE-360, conditional on
the same current data-segment binding, the parser's first byte selects
DX=0001, SI=0001 and DI=0000. Its second byte is `t`; the following
zero byte does not select its alternative plus-sign branch. It sets bit
4000 in DX. It writes zero to current DS:DBA2 and 3C90 to
DS:DBA0, writes DX=4001 through the output pointer at its BP+08,
DI=0000 through the pointer at BP+04, and returns AX=0001.
Other initial mode bytes and suffix combinations have separate branches;
this evidence does not establish their admitted callers or full mode grammar.

The outer callee stores returned AX into selected record word two
before testing it. AX zero selects failure. Otherwise a nonnegative
record byte four skips the following pathname request. A negative byte
pushes local BP-04, local BP-02 OR the first incoming word, and the
pathname segment/offset, then calls 1000:317F through pushed CS and
a near call. It removes eight argument bytes and stores returned AL
into record byte four before testing AL as signed. Negative selects
failure; nonnegative continues. For the selected `rt` and incoming zero,
the outgoing words after the pathname are 4001 and 0000.

The failure path writes FF to record byte four and zero to record word
two, returns DX:AX zero, restores SP/BP and near-returns while removing
fourteen incoming argument bytes. There is no separate release call on
this particular local failure suffix.

The continuing path sign-extends record byte four into AX, passes it
to 1000:0519 and removes its argument word. A nonzero returned AX
sets bit 0200 in record word two. It then calls 1000:37F1 with the
record far pointer, double-word zero, word one or zero according to that
bit, and word 0200, removing twelve argument bytes afterward. Nonzero
AX calls 1000:2873 with the record pointer, removes four bytes and
returns a zero pair. Zero AX instead clears record word sixteen and
returns the selected record's original segment/offset. These three callees'
effects and preservation contracts are not read here. In particular the
2873 result is not tested before the outer zero-pair return.

## Interpretation

This resolves the local mode/request/record continuation left open by
FND-EXE-360 and FND-EXE-354. It identifies the selected mode's
downstream argument words without attributing an operating-system
operation to an unread callee. The signed returned-byte test, earlier
record publication and subsequent configuration are distinct decisions.
The local zero-pair result can follow preceding record writes or a cleanup
call, and does not imply unchanged state.

FND-EXE-370 independently reads the conditional lower request route.
This finding adds the subsequent 0519/37F1/2873 call sequence and
separate configuration failure continuation; it does not replace that evidence.

Q-EXE-007 retains 1000:317F, 1000:0519, 1000:37F1 and
1000:2873, caller/data-segment and record/count admission, mode/path
extents, aliases and lifetime, and later consumers. The other parser paths
remain outside the selected `rt` interpretation. No complete-reading
promotion, native file-operation result or execution exclusion follows.

## Alternatives

Treating the record publication as conditional on successful downstream
work ignores the earlier word-two and returned-byte stores. Treating the
request result as a whole-word success test ignores its signed AL branch.
Treating the `rt` request words as proof of a particular native operation
assumes the unread 317F contract. Treating every zero return as the same
rollback path ignores the separate configuration/cleanup continuation.

## How to reproduce

At revision 3170371 require FND-EXE-350's SOUND_DS.EXE identity:
length 204593, XXH3-128 236c2dc23c071eca421eb5b427caee57.
With locked Capstone 5.0.7 in sixteen-bit mode decode shipped half-open
0x00003EF6..0x00003FC5 at IP 2AF6 and
0x00003E3A..0x00003EF4 at IP 2A3A, modeled CS 1000,
MZ header size 1400. Bind the selected mode to FND-EXE-360's `rt`
pointer and retain its segment condition. Track the parser output pointers,
all ordered record stores, signed byte tests, pushed arguments, each
callee-result test and both distinct failure continuations. The byte-to-word
extension at IP 2B5F is the sixteen-bit instruction, despite Capstone's
wider printed mnemonic. Source bytes and reports remain outside Git.
No original process, DOSBox or emulated call is executed.
