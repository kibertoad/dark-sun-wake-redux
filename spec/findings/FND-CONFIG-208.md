---
id: FND-CONFIG-208
title: The pointer acquisition wrapper gates archive state and distinguishes optional-output clearing paths
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 38FF:0438
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 39A9:012E
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 38FF:07B1
tool: Python 3.14.7 and Capstone 5.0.7 complete bounded resident wrapper/signature-helper readings with operand widths and MZ relocation checks
environment: null
---

## Observation

FND-CONFIG-207 calls 38FF:0438 with the FONT tag, selector
100 and a zero double word. The complete wrapper spans file
0x0002E628..0x0002E69A inclusive. It reserves four local bytes,
has the runtime stack-limit guard, and initializes its local far
pointer BP-04 to zero. It then calls 39A9:012E and zero-extends
returned AL into AX for its branch. The call's segment operand
at file 0x0002E642 is a declared MZ relocation.

The complete signature helper spans file 0x0002EDBE..0x0002EDE6.
After its own runtime stack-limit guard, it compares current double
word DS:9DA7 with GFFI. Mismatch writes word nine to DS:9D99
and returns AL zero; match writes zero there and returns AL one.
It does not itself verify an archive pointer, directory storage or
resource payload. FND-CONFIG-038 already identifies this gate
inside the shared reader. Guard and current-state validity remain
conditions on following normal returns.

A zero signature result makes the wrapper return DX:AX zero
immediately. That path bypasses all later optional-output accesses.
An admitted result tests its third double-word argument at BP+0E
as a far pointer. If nonzero, it clears the double word at that
pointer before the acquisition call. There is no own pointer-validity
or destination-capacity check. A null third argument skips both this
clear and the later metadata write; it is an optional output pointer,
not a separately interpreted request-mode word in this wrapper.

The wrapper calls same-segment 38FF:07B1 through a push-CS and
near-call far-return frame. It supplies five arguments in parameter
order: original tag double word, original selector double word,
double-word zero, double-word FFFFFFFF and SS:BP-04 by far
address. The caller removes twenty bytes. The zero and FFFFFFFF
pushes are four-byte operands, not two-byte arguments.
FND-CONFIG-038 records the shared reader's archive/type/number
search and retained open storage/state conditions.

Returned AX nonzero selects DX:AX zero return. AX zero selects
completion: when the optional output pointer is nonnull, the wrapper
writes current DS:9D91 there, then returns the local pointer from
BP-04 in DX:AX. There is no own test that this local pointer is
nonnull or that its storage is valid. It was initialized before the
call; the reader is responsible for producing its later value.
The wrapper does not roll back other callee effects.

Thus signature rejection leaves the optional output untouched by
this body. Reader rejection after signature admission normally leaves
the earlier own zero clear, subject to callee writes and aliases.
Reader zero completion copies current metadata and returns the local
pointer. Those paths cannot be summarized as one unconditional
output clear or as equivalent guarantees of successful acquisition.
FND-CONFIG-207 supplies a null optional output, so it bypasses
both optional writes on that observed call form.

## Interpretation

The FONT pointer producer uses the same shared archive reader
already reached by message-resource lookup. Its local return contract
separates signature and reader gates from pointer validity. A nonzero
pointer retained by FND-CONFIG-207 requires both gate admission
and a reader-produced local value, but does not prove valid current
FONT table capacity, lifetime or native presentation.

## Alternatives

Q-CONFIG-008 and Q-SCRIPT-003 retain archive/signature producers,
reader output/storage contracts, DS and frame aliases, later metadata
writers and FONT/current-record lifetime. One reading supplies a
valid selected resource and a reader-written local pointer; another
rejects signature/search or returns under changed storage. Complete
producer and shared-reader input/effect readings would distinguish
those cases. No native or emulated result is claimed.

The reading that all failure paths locally clear the optional output
is ruled out by signature rejection before the clear. A reading that
the third argument is only a scalar request option is ruled out by
its far-pointer dereferences. A zero reader result alone does not
establish a nonnull returned pointer.

## How to reproduce

Read 38FF:0438 through far return 04AA, following local-pointer
initialization, signature gate, optional clear, five argument widths,
reader-result branch, optional metadata write and returned local
pointer. Read 39A9:012E through 0156 and verify its signature and
error assignments, retaining runtime guards as dependencies. Compare
FND-CONFIG-038's shared-reader reading and FND-CONFIG-207's
null optional-output call. Keep output mutation paths separate from
resource validity, aliasing and native success.
