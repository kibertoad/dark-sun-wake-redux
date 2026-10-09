---
id: FND-EXE-370
title: Sound utility rt interface reaches conditional attribute and open request paths
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1000:2A3A..1000:2AF6
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1000:2AF6..1000:2C11
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1000:317F..1000:31B8
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1000:3238..1000:32EE
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1000:32EE..1000:333D
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1000:2799..1000:27B6
tool: Capstone 5.0.7, executable-reader 2.5.0 and xxhash 4.0.1
environment: null
---

## Observation

FND-EXE-360's wrapper calls 1000:2BC7 before 1000:2AF6. The former
copies current DS into a stack-local segment word and initializes the
local offset to 0xDBA8. It examines the signed byte at each selected
record plus four. A negative byte selects the record. Otherwise it
advances the local offset by twenty and compares the previous offset
unsigned against the low word of twenty times DS-relative word 0xDD38
plus 0xDBA8. Arithmetic wraps at word width. On leaving that comparison
loop it still examines the newly advanced record's byte plus four before
returning either its far pointer or zero DX:AX. This reads a further
record after the comparison stops; the state word's meaning and admitted
storage extent are not established here. The selector makes no calls and
does not alter DS.

At 1000:2AF6, the incoming near frame carries a zero flag word, a far
mode pointer, a far pathname pointer and a far record pointer, in that
order. It passes the mode pointer and two far pointers into its own
SS-relative local words to 1000:2A3A. That helper returns with twelve
argument bytes removed. For an unchanged `rt` followed by zero, its
local parsing path returns AX one and publishes 0x4001 and zero through
the two supplied output pointers. It also stores zero and 0x3C90 in
DS-relative words 0xDBA2 and 0xDBA0. Their later consumers are unread.
The source mode's initialization and preservation remain conditional.

The initializer stores returned AX to record word plus two. A zero AX
takes a failure path that stores 0xFF to record byte plus four, clears
record word plus two, and returns zero DX:AX. Otherwise a nonnegative
existing signed byte plus four bypasses the low-level request. A negative
byte supplies the pathname, parsed 0x4001 OR incoming zero, and the
other parsed output zero to 1000:317F. Its returned AL is stored to
record byte plus four and tested signed. A negative AL takes the same
failure publications. A nonnegative byte reaches further handle/buffer
helpers before eventual success; their effects are not read here.

The local prefix at 1000:317F loads the two incoming option words into
SI and DI. If SI retains parsed 0x4001, its high-bit default-setting
load is bypassed. It pushes zero and the pathname's far pair, pushes CS
and near-calls 1000:2799, whose far return consumes that explicit CS/IP
frame. The wrapper loads AH 0x43 and AL from its word argument plus ten,
loads DS:DX from the pathname pair, and reaches INT 21h. Thus this call's
AL is zero. DS is locally saved and restored. A carry-clear continuation
returns post-interrupt CX in AX; a carry-set continuation calls the error
helper 1000:04CE. The returned AX is retained by 1000:317F.

The attribute wrapper also loads CX from SS-relative BP plus twelve.
The caller pushed only three argument words. With its four local bytes
and saved SI/DI, that access selects the caller's saved incoming DI word,
not a fourth explicitly pushed argument. The current DI option word was
loaded after that register save; it is not the last writer of this slot.
No claim that the incoming saved value is zero follows. The interrupt's
use and preservation of that value are external obligations.

If SI still equals 0x4001 after that request, its 0x0100 gate is clear
and the continuation reaches 1000:32EE with pathname and SI, bypassing
the intervening creation-mode branches. The wrapper selects AL zero
when the option word's bits two and four are clear, merges its low-byte
0xF0 mask into AL, sets AH to 0x3D, loads DS:DX from the pathname and
reaches INT 21h. For that retained option value, AL remains zero.
Both wrappers' reached service selectors are direct instruction evidence;
the earlier interrupt's preservation of SI is not established by local code.

On carry clear, 1000:32EE saves returned AX, shifts its copy in BX left
one, and stores a masked option word OR 0x8000 to restored-DS storage
at that index minus 0x22C6, without a local index bound. It returns the
saved AX. Carry set calls 1000:04CE instead. The outer request body
tests returned AX signed, reaches further helpers only on nonnegative
values, and can later overwrite the indexed option storage. Its remaining
callees, interrupt effects and failure-prefix publications remain open.

## Interpretation

This narrows Q-EXE-007 from literal text to a record-backed pathname
interface with explicit INT 21h service selectors 0x43 and 0x3D on the
described conditional route. It does not prove successful file operations
or exclude every process-launch route elsewhere in the utility or game.
The creation-mode branches are not part of the retained-rt-option route,
but interrupts and intervening callees must preserve the tested state
before that route is admitted natively.

The selector's comparison is not an admitted array bound. The initializer
has signed-byte result consumption and observable local failure stores;
a null return is not proof that the record or handle state was untouched.
The wrapper's CX input comes from a saved register, not an explicit
zero argument. Input/state writers, record lifetimes and aliases, remaining
helpers, global-word consumers and external contracts remain Q-EXE-007.
No status promotion or complete_reading declaration follows.

## Alternatives

Treating the pathname as an execution request from its batch suffix ignores
the directly reached service selectors. Treating the entire path as admitted
from `rt` alone assumes preservation across the preceding interrupt.
Treating the selector's word as a validated count ignores wrapping and the
additional final-record read. Treating every word read by a wrapper as an
explicitly pushed argument misses the saved-DI slot. Treating nonzero or
nonnegative returned words as successful signed-byte handle results ignores
the initializer's AL-width test.

## How to reproduce

At revision c6b394d, use the stable SOUND_DS.EXE identity in FND-EXE-350
and FND-EXE-360's caller argument construction. With locked Capstone 5.0.7
in sixteen-bit x86 mode and source header length 0x1400, decode shipped
half-open ranges 0x00003E3A..0x00003EF6 at IP 0x2A3A,
0x00003EF6..0x00003FC7 at 0x2AF6,
0x00003FC7..0x00004011 at 0x2BC7,
0x0000457F..0x000045B8 at 0x317F,
0x00004638..0x000046EE at 0x3238,
0x000046EE..0x0000473D at 0x32EE, and
0x00003B99..0x00003BB6 at 0x2799. Each is a resident 1000-segment
reading. Follow parser ret 0x000C and initializer ret 0x000E, both full
widths of returned pointers, signed byte/word tests and the request gates.
At the attribute call account for four local bytes, both saved registers,
three pushed argument words, explicit pushed CS, near return IP and the
wrapper's saved BP; its BP plus twelve is the caller's saved incoming DI.
Do not replace the printed sixteen-bit sign-extension instruction with an
EAX-width operation. Keep all reports/instruction context outside Git.
No original process, DOSBox or emulated call runs; remaining callees and
interrupt preservation are explicit unresolved inputs.
