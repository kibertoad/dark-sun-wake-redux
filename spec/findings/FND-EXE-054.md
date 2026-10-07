---
id: FND-EXE-054
title: Second selector calls a saved argument target before the current record target and distinguishes zero, seven and eight
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x00600CC0..0x00600D88
tool: Ghidra 12.1.3 PUBLIC, bounded instruction and function reporters
environment: null
---

## Observation

FND-EXE-052 chooses this selector when its argument's offset-12 word is
nonzero. The selector consumes the incoming argument address and selected-local
address from the prepared registers, like FND-EXE-053. It saves the argument's
words at offsets 12 and 16 in separate local words, then reads the current
record through the selected-local address. It does not reread the saved
offset-12 target or offset-16 word on later iterations. It has no local
null guard on either incoming address.

Each iteration starts a local record target at zero and a status word at
five. A nonzero current record loads its offset-24 target and sets status
to zero. A subsequent boolean gate would reject a status that is nonzero
and different from five. With only this body's direct local producers,
status is zero or five, so this gate does not reject either locally produced
value. No other status producer or reachable additional value is inferred.

It calls the saved argument-offset-12 target even when the current record
is zero. The eight prepared outgoing 32-bit words are: one; a branch-specific
flag; the argument's freshly read offset-zero word; its freshly read offset-four
word; the argument address; the selected-local address; the saved offset-16
word; and a current auxiliary-register word. The flag is twenty-six for
status five and ten for status zero, computed at 32-bit width. The saved
target is called indirectly without a new local target-null check. The
caller adjusts the stack by thirty-two bytes on normal return. These slots
are not a complete target parameter contract.

Any nonzero full result from this first callback returns exactly two,
including results seven and eight. A zero result with saved status five
returns five, without traversing or calling a record target. A zero result
with saved status zero proceeds using the record-offset-24 target captured
before the first callback. It does not recompute current-record nullness or
reload that target after possible callback mutation of the selected local.

A zero saved record target advances the selected local: it rereads the
current local value, reads that record's first word, writes the obtained link
back through the selected-local address, and repeats. A nonzero saved record
target is called with eight outgoing words: one; ten; newly read argument
offset-zero and offset-four words; the argument address; the selected-local
address; and two copies of the saved target value. The argument fields can
therefore be read at different times for the two callbacks. After the second
call it adjusts the outgoing stack by thirty-two bytes and saves the full
result. Seven returns seven without advancing the local. Eight advances
using the same fresh local reread as the zero-target path. Every other result
returns exactly two.

A callback can modify the selected local through the supplied address,
subject to its unread contract. The saved status and record target remain
those loaded before the first callback, while link advancement uses the
later local value. There is no local cycle counter or iteration bound, and
no local record-null guard at that later link dereference. Stores through
the supplied local address are not a proof of non-aliasing with other data.

Unlike FND-EXE-053, this selector does not compare the record address with
the saved argument-offset-16 word or locally call the abort import on a
match. The saved word is prepared for the first callback instead. This
negative claim concerns only the cited direct body, excluding callback effects.

## Interpretation

The second selector's local callback order and full-width result classes
are now bounded. FND-EXE-052 accepts only seven, so this selector's normal
returns two and five reach that caller's abort boundary rather than its
saved-state transfer. A first-callback result seven cannot directly produce
that transfer; a second-callback result seven can, subject to valid storage
and normal callees. Q-EXE-009 retains both callback bodies and target
provenance, selected-record field writers, mutable-local admission, link cycles,
argument producers and dispatcher frame mapping. No complete record or
exception lifecycle is established.

## Alternatives

Always calling the record target first, treating the two callback results
identically, skipping the first callback for a null record, or rereading its
saved target on every iteration are ruled out by the instructions. Returning
seven after the first callback returns seven is also ruled out. A match-based
abort copied from FND-EXE-053 would misdescribe this body. Treating the later
link as belonging to the originally inspected record ignores the local reread.
The redundant local status gate does not justify inventing another input class.

## How to reproduce

Use FND-EXE-011's verified PE and image base. Summarize `0x00600CC0` and
read one hundred five instructions there, restricting claims to the cited
range and excluding the later function printed. Use FND-EXE-052 for incoming
register writers and its seven-only return consumer. Track the saved argument
fields, exact status producers and boolean widths, branch-specific flags,
all outgoing slots, first-callback zero/nonzero results, retained record target,
second-callback seven/eight/other results and the fresh local reread on advance.
Compare FND-EXE-053 without assuming equivalent branches. Keep callback and
alias effects conditional, including saved-register and local-word preservation
by unread callees. Keep rich reports local and execute no interpreter
or game.
