---
id: FND-CONFIG-181
title: The filename helper clears an archive handle and mixes far SS output with near DS processing
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 56BD:0297
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 56BD:0000
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:406D
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:31B9
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:0716
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:384D
tool: Python 3.14.7 and Capstone 5.0.7 bounded complete local 16-bit readings and declared FBOV/MZ operand mapping
environment: null
---

## Observation

FND-CONFIG-179's initial word-two branch passes current
word DS:140C to local far 0297 before its later mode
re-read. Descriptor 182's complete 0297 body spans
`0x00068AE7..0x00068BB5`, ending with far return at
`0x00068BB4`. Byte current DS:068F selects a source
at near offset 0FE7 when nonzero or 0FF1 when zero.
The loaded-image strings there are filename templates
`RGNXX.GFF` and `RGN0XX.GFF`. The body copies the
selected string through runtime 1000:406D to an explicit
far output at SS:BP-20. Later writers to those sources
and the gate remain open.

Nonzero far field current DS:145A is passed to
38FF:02B5; after that call returns the field is cleared,
without a result test. FND-CONFIG-041 identifies this
numeric archive-handle close site, and FND-CONFIG-037
bounds active archive changes. A cleared field does not
prove successful close or the archive's removal.

The body passes its word argument, near offset BP-12
and radix word 16 to runtime 1000:31B9, then passes
that helper's returned near offset to 1000:384D.
31B9's complete span is `0x000083B9..0x000083E1`.
Radix ten sign-extends its word through unprefixed 99
(CWD in this 16-bit body); other radices, including the
named sixteen, supply high word zero. It forwards value,
near output, radix, sign flag and alphabet base to near
0716, complete span `0x00005916..0x00005993`.
For the named unsigned word/radix-sixteen case, that
helper repeatedly divides, emits one to four lowercase
hexadecimal digits and a zero terminator through ES=DS,
and returns the near output offset. It has no caller-
supplied output capacity. Its scratch bytes also use
near offsets formed from BP. The complete 384D span,
`0x00008A4D..0x00008A6D`, uppercases ASCII a through
z in place under DS until a zero byte and returns the
same near offset; it has no independent length limit.

0297 then reads SS-frame bytes BP-12 and BP-11.
If the second is zero, it copies the first to the second
and writes ASCII zero at the first. It only uses those
two bytes when modifying the filename. A re-read nonzero
DS:068F selects filename offset three for the first byte;
zero selects offset four. A further re-read selects the
following offset for the second. Those filename stores
use BX near addressing under current DS, despite the
initial copy's explicit SS output. There is no own
check or segment change proving DS and SS equivalent.

For maintained DS/SS storage identity, stable templates/
gates and valid disjoint buffers, argument zero yields
hexadecimal bytes 00, five yields 05 and AB hexadecimal
yields AB. A value 0100 hexadecimal produces digits
100 but only its first two bytes, 10, are injected;
FFFF produces FF. These are conditional static conversion
cases, not an established argument range or native filename
choice. The source-template gate can also change between
its initial copy and later re-reads through intervening calls.

The complete far-copy helper 1000:406D spans
`0x0000926D..0x00009296`. It scans at most FFFF bytes
for a zero in the supplied source segment, copies the
consumed count to the far destination, restores DS, SI
and DI, and returns the destination pointer. A reached
terminator is included; no terminator still copies FFFF
bytes. It leaves ES changed and the direction flag clear,
and provides no destination-capacity check. Loaded template
lengths fit the named 32-byte local output, but that does
not prove unchanged runtime sources or buffer identity
for the near conversion and later stores.

0297 clears current DS:145A again and calls local far
0000 with its near filename offset, that field's far
output address and words 000A and 04D4. The complete
0000 span is `0x00068850..0x000688A6`. It copies a
DS:44F2 prefix into local SS:BP-52 through 406D,
then passes that far local output, a far filename source
formed with current DS and word 80 (50 hexadecimal) to 2D40:3DC2.
It calls 38FF:0066 with the resulting far local path,
a zero-extended 04D4, word 000A and the handle-output
address. Full returned AX FFFF becomes AL zero; other
results become AL one. The prefix is initially empty,
but its later writers and the external path helper's
complete effects remain open. No successful open or
path-content guarantee is assigned by the AL conversion.
FND-CONFIG-182 subsequently reads the external append helper,
including its signed length gates and prefix-dependent zero store.

A zero AL returned to 0297 passes its near filename
offset to 56B2:0034; nonzero skips that call. The parent
in FND-CONFIG-179 ignores 0297's return and proceeds to
later callees and its mode re-read. Thus a returning
failed open or error-helper request does not locally stop
that parent's continuation. All overlay segments above
were verified through declared FBOV fixups. DS-relative
fields mean DS at each instruction.

## Interpretation

The local helper attempts to close a stored archive handle,
clears it without a success test, constructs a two-character
filename selection under explicit segment conditions, then
requests another open and an optional error helper. Its
parent's later gate is independent of this returned result.
Loaded templates and numeric examples do not establish
native argument ranges, successful I/O or archive retention.
The mixed far-SS and near-DS accesses must not be summarized
as one proven stack buffer without provenance.

## Alternatives

Q-CONFIG-008 and Q-SCRIPT-003 retain argument, gate,
template/prefix and archive-handle producers, all callers,
DS/SS and near-buffer identity, valid capacities and aliases,
external path/error helpers, archive open/close outcomes and
later state changes. One reading supplies maintained shared
storage, stable gates and accepted archive I/O; another
changes a gate or segment relationship or returns an error.
Complete producer/callee and runtime evidence would distinguish
their reachable outcomes. No such change or failure is claimed
as a native observation.

The mapped initial MZ stack segment is 622F, while ordinary
DS is 57E0 in FND-SCRIPT-005. Initial header state alone
does not establish either equality or inequality at this later
call; startup and subsequent segment producers remain to be
read. The loaded strings likewise do not prove their runtime
contents. The digit examples are static branch derivations,
not an emulated experiment. No native run is claimed, and
Q-SCRIPT-007 cannot execute this overlay.

## How to reproduce

Resolve descriptor 182 and read 0297 through 0364 and
local 0000 through 0055. Verify every fixup and preserve
the close-before-clear order, three gate reads, far versus
near filename/output addresses, packed 000A/04D4 arguments,
full AX to AL conversion and error-helper branch. Read
1000:406D through 4095, 31B9 through 31E0, near
0716 through RET 12, and 384D through 386C. Check
unprefixed CWD and the named radix-sixteen zero-extension,
near DS writes and SS-frame reads separately. Inspect only
bounded null-terminated filename sources at ordinary DS:0FE7,
0FF1 and prefix 44F2. Keep loaded values, conversion cases,
segment identity and actual path/archive outcomes distinct.
