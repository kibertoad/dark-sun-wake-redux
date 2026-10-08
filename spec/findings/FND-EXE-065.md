---
id: FND-EXE-065
title: Callback matching combines signed pair branches and preserves a full-word fallback test across a shared jump
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x005F53A7..0x005F546C
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x005F54B8..0x005F5549
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x005F51D1..0x005F5267
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x005F52A0..0x005F52D3
tool: Ghidra 12.1.3 PUBLIC, bounded instruction reading
environment: null
---

## Observation

FND-EXE-196's positive counted-read path enters this matching region only
when its saved incremented word and derived cursor word are both nonzero.
This region initializes two independent local bytes to zero: a seen-zero
marker and a match marker. These describe the observed writes, not inferred
meanings for the metadata. It tests bit eight of the second original
argument's low byte. Set initializes a saved object word to zero. Clear
rereads FND-EXE-165's third/fourth signature pair; mismatch likewise uses
zero, while equality reads the full word at the saved fifth-argument-minus-48
base into that object local. Both arms begin at the saved derived cursor.

Each iteration calls FND-EXE-063's signed byte reader twice into separate
full-word locals at frame offsets minus 120 and minus 124. The first return
is saved at frame offset minus 152 and is the second call's input cursor.
The second call's returned cursor is not saved or tested on this path.
It then classifies the first decoded word at signed 32-bit width:

| First decoded word | Local branch |
|---|---|
| Zero | Set the seen-zero byte to one, then inspect the second decoded word |
| Positive | Use FND-EXE-064's stride/typed helper and conditional object match |
| Negative | Use the index scan when saved object is nonzero, or a separate unsigned decode when it is zero |

On the positive branch it passes the metadata local and first decoded word
to FND-EXE-064's stride/typed helper. A decoded return of zero sets the match
marker directly. A nonzero return with saved object zero does not match and
moves to the second-word test. With saved object nonzero it writes all ones
to its nested record's state word, prepares the decoded return and saved
object with the address of its candidate local, and calls FND-EXE-064's
conditional object matcher. A true low byte sets the match marker; false
moves to the second-word test.

On the negative branch with saved object nonzero it prepares the metadata
local, saved object and candidate value in registers and the negative first
decoded word in one outgoing stack slot. It writes the nested state to all
ones, calls FND-EXE-064's index scan, and removes sixteen outgoing bytes.
Here a true low-byte return moves to the second-word test; false sets the
match marker. This is the reverse of the positive object's matching test.
The scan takes the candidate by value into its own local, whereas the
positive object matcher receives the callback candidate's address.

With saved object zero, the negative branch forms the unsigned reader's
input as the metadata offset-twelve word plus the bitwise complement of the
negative first decoded word, wrapped at 32-bit width. Equivalently it is
that stored target minus the first word minus one. It calls FND-EXE-059's
reader into a distinct local at frame offset minus 132. It ignores the
returned cursor, compares the decoded full local word with zero, and jumps
straight to the shared conditional branch, bypassing its preceding low-byte
return test. The jump preserves the comparison's flags. A nonzero decoded
word moves to the second-word test; zero sets the match marker. Thus this
route is not governed by the unsigned reader's cursor return or its low byte.

As instruction controls, a decoded fallback word of 256 takes the nonzero
route although its low byte is zero. A decoded zero takes the match route
even when the returned cursor is nonzero. These are arithmetic/control-flow
checks, not claims that those values occur in shipped inputs.

At the second-word test, zero ends matching without setting the match marker.
Nonzero adds the second decoded word to the saved first reader's returned
cursor at 32-bit width, stores that as the new derived cursor, and starts
another pair. It does not add to the second reader's return or to the
previous pair's starting cursor. This displacement is not a count and need
not advance monotonically; no local stream-length, hop-count, cycle or
cursor-wrap guard establishes termination.

At the classification exit, a set match marker stores the first decoded
word into the separate classification-associated state local and supplies
classification three. Otherwise a byte comparison and borrow sequence supplies
classification zero when the seen-zero byte is zero, or two when it is one.
Under the local zero/one writers, this distinguishes a terminated sequence
that included a zero first word from one that did not. It does not turn a
successful negative scan into classification three: that scan's true return
continues rather than setting the match marker.

All these classes enter FND-EXE-196's common join, which first saves return
status eight. Classification zero goes to cleanup/status return immediately.
With bit one of the second argument set, classification two also returns
eight through cleanup; classification three takes the signature-conditioned
five-store branch and returns six after cleanup, even on signature mismatch.
For that branch, the base offset-forty store reads the current candidate local.
FND-EXE-165 initializes it from fifth argument plus 32, but a successful
positive object match can replace that word through its prepared address
before this store. The initial address is not its unconditional final value.

With bit one clear, the shared suffix checks bit eight and the signature
pair. Bit eight clear with a matching signature enters FND-EXE-057's route.
Classification three's saved nonnegative first word joins seven-return
preparation; a negative first word first rereads metadata and writes the
modifier result to saved-base offset 36 before that preparation. Other
suffix admission decrements the classification; nonzero then tests the
saved state at signed width, with a negative word reaching the call boundary
at `0x005F5556`, and a nonnegative word joining preparation. Terminal helper
effects beyond that boundary remain unread. Classification is therefore a
local selector, not a universal callback return value or proof of transfer.

## Interpretation

The ordinary matching classification producers and exact helper-result
combinations are now bounded. The two signed outputs have different roles;
the first return, second displacement and explicit fallback comparison must
remain separate. Q-EXE-009 retains concrete dispatch targets, stream and
object provenance, metadata offset-eight initialization, aliases, terminal
helper/handler effects, classification-one terminal branches from other admissions,
selected-record identity and higher dispatcher/caller contracts. No complete
record lifecycle, matching schema or shell outcome is established or implemented.

## Alternatives

Using one helper truth convention for every branch, branching on the unsigned
reader's return, falling through its skipped low-byte test, hopping from the
second reader's end, treating a zero first word as immediate termination,
or equating classification three with unconditional return seven is ruled out.
A seen-zero byte and match byte are independent; a scan that returned true can
still end with classification zero or two. Treating the initial candidate
address as immutable would discard the positive matcher's conditional write.

## How to reproduce

Use FND-EXE-011's length and XXH3-verified PE and image base. Read one hundred
instructions from `0x005F5391`, sixty-five from `0x005F551A`, forty from
`0x005F51D1`, and seventeen from `0x005F52A0`, restricting claims to the cited
ranges and excluding stored-handler and other function entries. Use
FND-EXE-165/057/058 for prior writers, signature and cleanup, and
FND-EXE-059/063/064 for helper outputs, cursor returns and candidate updates.
Track byte initialization, full-word signed branches, both reader returns,
displacement base, opposite helper truth tests, the fallback comparison's flags
across its jump, classification arithmetic and conditional six/seven/eight
consumers. Keep terminal callees, bounds, aliases and exceptional effects
conditional. Keep rich reports local and execute no interpreter or game.
