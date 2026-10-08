---
id: FND-EXE-083
title: First cleanup target tail-selects paired pointer replacement without resetting adjacent words
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x00418800..0x00418860
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x00418860..0x0041886C
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x00418870..0x00418879
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    kind: file-data
    offset: 0x002EB318..0x002EB31B
tool: Ghidra 12.1.3 PUBLIC bounded instruction reading with physical constructor and cleanup target provenance
environment: null
---

## Observation

FND-EXE-082's initial shipped cleanup target is `0x00418870`. That entry sets
EDX to 65535, clears EAX to zero and jumps to `0x00418800`, with no new return
address or outgoing stack arguments. Its helper returns through the incoming
callback frame. FND-EXE-080's physically backed constructor table separately
contains target `0x00418860` at slot two, virtual `0x006EBF18`, physical
`0x002EB318`. That wrapper sets EDX to 65535 and EAX to one, then makes the
same tail jump. This proves each target's own table admission, not that every
constructor has an inverse cleanup callback or that either actually runs.

The helper saves incoming EAX in ECX. It compares full EDX with 65535 and
places that equality in DL. It compares full incoming EAX with one and places
that equality in AL. Its first store group requires both equalities. Later
it tests the saved incoming value in ECX, places its zero equality in CL and
combines it with DL; its second store group requires incoming EAX zero and
incoming EDX exactly 65535. These byte results do not test only the inputs'
low bytes. None of these local paths calls another function.

### Cleanup wrapper's selected path

For the first cleanup wrapper's fixed zero/65535 inputs, the first group is
skipped. The second group writes `0x00757D70` to `0x01B7BB18`, then writes the
same value to `0x01B7BB28`. A repeated conditional jump after the first store
still uses the nonzero result of the earlier byte conjunction: the intervening
moves change no flags. It therefore does not skip the second store on this
fixed path. The helper returns with `0x00757D70` in EAX.

No local store changes adjacent words `0x01B7BB1C` or `0x01B7BB2C` on this
path. There is no null substitution, allocation, release, close, pointer
validity test or callback in the complete local helper body. The pointer
value's semantic role and consumers remain unread; its numerical identity
does not establish a valid object or a cleared lifecycle state.

FND-EXE-082 discards this callback's return by reloading its global cursor
after normal return. The pointer writes precede that reload and the parent
cursor's later publication. The helper itself does not write that cursor.
This is direct local store ordering, not a proof excluding aliases, lifetime
interference or external actors.

### Separately admitted constructor wrapper's path

For the constructor wrapper's fixed one/65535 inputs, the first group writes
`0x00758620` to `0x01B7BB28`, then 48 to `0x01B7BB2C`, then `0x00758CD8` to
`0x01B7BB18`, then 48 to `0x01B7BB1C`. Its repeated conditional jump likewise
retains the nonzero byte-conjunction flags through intervening moves. The
second group is skipped because the saved incoming EAX is not zero. The
helper returns EAX 48; FND-EXE-080's dispatcher does not use that return as a
success predicate.

The four destination words fit inside FND-EXE-078's declared virtual-only BSS
interval. Narrow physical mapping of each fails while the constructor-table
word maps successfully. There are no shipped raw initializer words to read
at those destinations; no actual loader zero state, all writers, pointer
consumer or meaning for the adjacent 48-valued words is established here.
Other helper inputs and callers are not admitted by these two wrappers.

## Interpretation

The first concrete cleanup target now has a complete local fixed-input path,
including both pointer writes, untouched neighboring words and raw return.
The constructor table independently admits another fixed-input path into the
same helper. Q-EXE-009 retains actual lifecycle admission, other callers,
all writers/aliases, consumers and remaining cleanup/constructor targets.
These paths do not establish original startup or cleanup success.

## Alternatives

Treating the two wrappers as having equal inputs, testing only low input bytes,
skipping the second paired pointer store because of an intervening move,
resetting the adjacent words on cleanup, returning a Boolean, using the callback
return to admit parent progression or locally freeing the pointed-at object
is ruled out. A source pointer constant is not proof of its runtime validity.

## How to reproduce

Verify FND-EXE-011's identity and FND-EXE-082's first cleanup-table word.
Read three instructions at `00418870`, 33 at `00418800` and three at
`00418860`; restrict helper claims to `00418800..0041885F` and each wrapper
to its cited tail jump, excluding later entries after gaps. Inspect only
FND-EXE-080's 29 nonzero constructor slots for exact full target `0x00418860`;
record its slot-two physical mapping. Check complete four-byte mapping for
`0x01B7BB18`, `0x01B7BB1C`, `0x01B7BB28`, `0x01B7BB2C` against the declared
section extents, with that table slot as a positive physically backed control.
Trace full input comparisons, byte conjunctions, preserved flags, store order,
tail frame and raw return through the parent's fresh cursor load. Recover only
the physically grounded `00418860` entry; export allowed start/body-size
columns and review its added twelve-byte inventory row. Keep reports and the
analysis database local; no original callback or interpreter is executed.
