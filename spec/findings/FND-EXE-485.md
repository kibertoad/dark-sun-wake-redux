---
id: FND-EXE-485
title: Sound utility spacing consumer derives row records and passes a mixed-provenance argument word
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 190F:02DF..190F:04CA
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    kind: file-data
    offset: 0x00000A16..0x00000A1A
tool: Capstone 5.0.7 and xxhash 4.0.1
environment: null
---

## Observation

FND-EXE-484's quotient at SS:BP-16 continues at 190F:02DF.
The caller adds it to BP-0C at word width, stores the result at
BP-16C4 and copies that word into BP-1764. It sets SI one and
continues while SI is less than or equal to BP-22 signed.

For describing these local accesses, let P(i) be the word at
SS:BP-16C4 plus low signed i-times-002C, L(i) the word at
SS:BP-16C6 plus that same product, and R(i) the offset
BP-1766 plus low signed i-times-000C in SS. These are address
expressions, not admitted array capacities or field semantics.
The recurrence reads through BP-171A plus 002A and 0028,
each displaced by the low signed SI-times-002C product. At word
width these are respectively P(SI-1) and L(SI-1).
It adds those words and current BP-16, stores P(SI), then reloads
P(SI) and stores it at R(SI)+2. SI increments and the signed
limit is rechecked. No row extent, product overflow or offset-wrap check
occurs. Under stable nonaliased state this seeds P(0) and R(0)+2,
then derives indices one through a count from 0001 through 7FFE inclusive,
one beyond the indices later used by the row loop. Count 7FFF admits
every signed index after increment and wrap, so the local comparison alone
does not terminate that recurrence. Nonpositive counts skip it.

The next loop resets SI zero and continues while SI is less than
BP-22 signed. Let T(i) denote SS:BP-16EE plus low signed
i-times-002C. Each iteration pushes ten words in order:
0001, 0000, SS, T(SI)'s offset, words four and six from the
second incoming far object, BP-12 minus two, P(SI) plus L(SI),
BP-12 minus four and P(SI). Each object word uses a separately
reloaded pair from SS:BP+0A. Push-CS and near call 08E5 form
a call to the local far-returning interface; the caller removes twenty
bytes and ignores its result. These arguments use word-width arithmetic.

After the call it increments BP-1E, freshly reloads P(SI) and L(SI)
and stores their word sum at R(SI)+6. It stores current BP-12
minus four at R(SI)+4 and current BP-12 minus two at R(SI)+8.
These accesses use current SI and state after the interface call, not a
retained snapshot of the prior outgoing words. Callee preservation and
nonaliasing are needed to identify them with the same row.

It next computes T(SI)'s offset in AX and loads only AL from the
first text byte there, then pushes full AX. Thus the low byte comes
from text and the high byte remains from the derived offset. No local
zero- or sign-extension replaces that high byte. It pushes words at
R(SI)+8, +6, +4 and +2, calls 1A7C:01C6 and removes ten
incoming bytes. It retains returned AX on the stack while deriving
R(current SI) afresh, then stores that returned word at R(current SI).
It increments SI and rechecks the signed limit. The returned word's
meaning, the callee's consumed argument widths and SI preservation remain
outside this reading. Continuation at 04CA is also outside it.

The segment word for the far call at 04A5 is MZ relocation record
630 at shipped A16..A1A, targeting shipped A998. Encoded segment
0A7C binds to modeled 1A7C at load segment 1000. The local
08E5 call instead supplies current CS explicitly.

## Interpretation

This follows the spacing quotient into an inclusive recurrence and a separate
row loop, while keeping before-call and after-call state distinct. The mixed
argument word is not a proved character parameter until its callee's width
is read. Q-EXE-007 retains row/frame extents, count and object producers,
aliases, both interfaces and preservation, record consumers, 04CA's
continuation and remaining return paths. No complete consumer reading,
visible layout or launch exclusion is claimed.

## Alternatives

Treating both loops as the same index range ignores the inclusive recurrence.
Treating the later stores as the outgoing-call snapshot ignores fresh state
reads. Calling the mixed word a zero-extended character ignores its high
byte's last writer. Treating returned AX as a successful operation assumes
an unread result contract and later consumers.

## How to reproduce

At revision 6d65fbb require FND-EXE-350's SOUND_DS.EXE identity:
length 204593, XXH3-128 236c2dc23c071eca421eb5b427caee57.
Use Capstone 5.0.7 in sixteen-bit mode, MZ header size 1400 and
modeled load segment 1000. Decode shipped A7CF..A9BA at IP 02DF,
CS 190F. In the relocation table at 003E with 958 records, inspect
record 630 and target segment word A998 at call 04A5. Track low
stride products, previous-row address equivalence at word width, inclusive
versus exclusive loop tests, every outgoing word, AL versus AX writes,
post-call current SI and the returned-word stack hold. Licensed bytes stay
outside Git; no original process, DOSBox or emulated call runs.
