---
id: FND-EXE-057
title: Signature-selected callback reloads saved state and prepares selected-record fields before returning seven
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x005F5268..0x005F533E
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x005F5470..0x005F54B7
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x005F5596..0x005F55A7
tool: Ghidra 12.1.3 PUBLIC, bounded instruction reading
environment: null
---

## Observation

FND-EXE-165's exact signature and second-argument-six guard enters
`0x005F5268`. It uses the saved fifth-argument-minus-48 base to read full
words at offsets 24, 32 and 36 into separate locals. It compares the
last word with one at unsigned 32-bit width and constructs a classification
of one for zero, or three for every nonzero value. This includes all ones;
it is not a signed-positive test.

It tests bit eight in the low byte of the second original argument. A set
bit bypasses a further signature comparison. A clear bit rereads the third
and fourth arguments and checks the same pair of constants recorded in
FND-EXE-165. A matching pair enters `0x005F5470`. On the signature-six
entry with unchanged argument words and valid storage, that is the direct
selected route. This shared suffix also has other incoming paths whose
classification producers remain outside this finding.

At that route it decrements the classification and tests for zero. For the
signature entry's zero offset-36 word, classification one selects
`0x005F5596`. That branch reserves twelve outgoing stack bytes and passes
the saved base plus 48 to `0x005FABA0`; the callee and its later continuation
remain unread. No seven-return claim is made for that branch.

For a nonzero offset-36 word, classification three takes the other route.
It tests the saved offset-24 word at signed 32-bit width. A nonnegative
word joins transfer preparation directly. A negative word first prepares
the sixth original argument, the saved offset-32 word and a local output
address in registers, writes all ones to its nested record's offset-four
state, and calls `0x005F4EA0`. On normal return it zero-extends a byte
from that local output, prepares the sixth argument in an auxiliary register,
and calls `0x005F4CC0`. Both callee contracts remain unread. It stores
the latter full result at offset 36 of the saved fifth-argument-minus-48
base, then joins transfer preparation. It does not locally replace the
separately saved offset-36 word with that new result before the later setter.

Transfer preparation passes the sixth argument, index zero and saved base
plus 48 to FND-EXE-056's indexed writer. It then calls that writer with the
sixth argument, index one and the saved offset-24 word. Thus, subject to
valid storage, the two full-word destination offsets are eight and twelve
of the records selected by each helper's fresh local dereference. There is
no proof that both calls select the same record or that these stores cannot
alias the saved base or callback locals.

It next passes the sixth argument and separately saved offset-36 word to
FND-EXE-056's minus-one setter. That setter writes its second argument
minus one, wrapped at 32-bit width, to the freshly selected record's offset
four. The saved value can therefore differ from the value just written at
saved-base offset 36 on the negative path. These are separate storage paths;
identity is not inferred from similar offsets or numbers.

After normal helper return it saves return status seven, calls ordinary
nested-record cleanup as in FND-EXE-165, reloads that saved status and
returns through normal frame restoration. It does not return any accessor's
or cleanup's result. No direct indexed-writer status test gates this join.
Unread callees' register/local preservation, alias effects and exceptional
control transfer remain conditional.

The shared suffix's other route, reached when bit eight is set or the
reread signature differs, has distinct terminal-helper boundaries. Those
boundaries and the later matching paths are outside this finding. The above
signature-selected route is not a description of every callback invocation.

## Interpretation

This bounds the signature-selected saved-state reads and seven-return
preparation, separating the saved offset-36 value from a later store to its
source location. Q-EXE-009 retains the zero-value callee and continuation,
negative-path helper contracts, other suffix admission, remaining matching
paths, selected-record identity and dispatcher frame mapping. No complete
record schema, stable pointer contract or exception lifecycle is established.

## Alternatives

Treating all ones at offset 36 as negative rejection, always returning seven
for a zero word, always using a single cached selected record, or passing the
new negative-path result to the later minus-one setter are ruled out by the
bounded instructions. Those helper stores do not prove successful transfer:
the forwarding caller still needs a valid selected record and saved target.
The saved base and the record obtained through the sixth argument cannot be
merged into one storage identity without separate evidence.

## How to reproduce

Use FND-EXE-011's verified PE and image base. Read eighty instructions from
`0x005F5268`, forty-eight from `0x005F5470`, and twelve from
`0x005F5596`, restricting claims to the cited ranges and excluding later
continuations. Use FND-EXE-165 for base derivation, signature admission and
nested cleanup, and FND-EXE-056 for helper argument consumption and stores.
Track unsigned classification, low-byte flag and signature rereads, signed
state guard, separate saved/source words, outgoing writer indices and values,
minus-one setter input and saved seven after cleanup. Keep alias, callee and
exceptional effects conditional. Keep rich reports local and execute no
interpreter or game.
