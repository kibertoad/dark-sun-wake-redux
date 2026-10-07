---
id: FND-EXE-092
title: Composed metadata caller keeps first-field stores separate from later marker and relative-target stages
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x005F4EA0..0x005F4F77
tool: Ghidra 12.1.3 PUBLIC bounded caller reading composed with recorded modifier and typed-reader contracts
environment: null
---

## Observation

FND-EXE-060 records the metadata caller at `0x005F4EA0`; FND-EXE-061,
FND-EXE-062 and FND-EXE-063 subsequently bound its modifier, typed reader
and nibble-nine reader. This reading follows their results back through the
caller's first-field destination, continued cursor and later writes. It does
not change the historical limits of those earlier observations.

The caller saves context, cursor and output pointer separately. It stores
zero at output offset zero before reading the first marker byte. Nonzero
context takes a local query whose direct return is also zero, as recorded
in FND-EXE-060. Source/output aliasing can therefore change even that first
marker; zero context does not defer the initial output store.

First marker 255 stores zero at output offset four without calling the
modifier or typed reader. Any other first marker is zero-extended, supplied
with the saved context to `0x005F4CC0`, and retained separately across the
call. Its normal full-word zero return from FND-EXE-061 is moved to the
typed reader's modifier input. The same saved marker is supplied to the
typed reader, with the advanced cursor and output-plus-four as its first
stack argument. The output destination is saved before the modifier call
and freshly reloaded for the push; it is not the modifier's returned word.
The typed reader's returned cursor replaces the caller's parsing cursor.
Its stored result is not used as the parsing cursor.

The composed ordinary first-marker admission needs both helper gates.
Masked marker classes 96 and 112 reach the modifier's abort boundary before
typed decoding, except that exact marker 255 takes the caller's bypass.
For the remaining classes, low nibbles 5, 6, 7, 8 and 13 through 15 reach
the typed reader's abort boundary. The other low nibbles take its recorded
read branches. Exact marker 80 takes that reader's separate alignment path,
despite its ordinary low nibble being zero: cursor plus three is aligned
down to a four-byte boundary, one full word is stored at output offset four,
and aligned cursor plus four is returned. That path skips ordinary base
adjustment and indirection. These are branch admissions, not proof that any
particular marker occurs in a valid shipped stream.

On an ordinary typed branch, decoded full zero is stored directly at output
offset four. A nonzero decoded word with masked class sixteen adds the
typed reader's original input cursor, which here is the byte after the first
marker. Other normally admitted classes add the supplied zero modifier.
Marker bit seven may select one full indirect read after that adjustment.
FND-EXE-062 records the width, cursor advances and absence of an adjusted-zero
recheck; FND-EXE-059 and FND-EXE-063 record the two variable-length branches.
Their output result remains separate from the continued cursor. There is
no validation status or local rollback attached to this destination store.

Only after that normal completion, or the first-marker-255 bypass, does
the caller read the second marker at its current cursor and store its original
byte at output offset 20. Exact second marker 255 stores full zero at output
offset twelve without the first relative-offset decode. Every other value
calls `0x005F4D30`, then stores its returned cursor plus decoded word at
output offset twelve, using 32-bit arithmetic. The parser keeps the returned
cursor separately, not that calculated target. Thus second marker 255 is
independent of first-marker admission and does not skip all later input.

Both second-marker routes read a third byte at their retained cursor and
store it at output offset 21. They call `0x005F4D30` again and store returned
cursor plus decoded word at output offset sixteen. This decode is not
bypassed when the third byte is 255. The final ordinary return is that last
decoder's continued cursor, not either stored relative target. For this
caller's complete normal path, output offsets zero, four, twelve and sixteen
have these distinct full-word producers, while 20 and 21 are byte writes.
There is no direct write to output offset eight or the remaining adjacent
bytes. FND-EXE-091 records a particular caller's earlier writer of offset
eight; this reader does not establish that field for every caller.

An abort boundary in the first-field helpers is reached after the initial
offset-zero store. A later failure, invalid access or nonterminating decode
does not locally reverse any earlier output stores. Every output write can
affect later source reads under aliases. This records execution order and
direct destinations, not native import outcomes or guaranteed termination.

## Interpretation

The previously separate helper contracts explain this caller's first-field
last writer and its continued cursor, without conflating them with later
marker fields or stored relative targets. Q-EXE-009 retains stream producers,
lengths, allowed marker combinations, context/frame admission, indirect
addresses, aliases and downstream target use. A locally returning branch
does not establish a bounded schema or complete wrapper execution.

## Alternatives

Using the modifier return as an output pointer, treating the typed result
as the next parsing cursor, admitting every modifier-zero marker, interpreting
the second-marker bypass as end of stream, bypassing the final decode for
third marker 255, or deferring all stores until final success is ruled out.
Adding zero modifier does not remove the distinct original-cursor adjustment
class or optional indirect read. A full typed word store does not initialize
the separate offset-eight modifier or the bytes beside marker fields.

## How to reproduce

Verify FND-EXE-011's executable identity. Read eighty instructions at
`005F4EA0`, eight at `005F4F5D` and five at `005F4F73`, restricting claims
to the cited caller body. Read forty at `005F4CC0` and sixty-five at
`005F4DD0` as the call/argument controls; use FND-EXE-061/062 for their
remaining branches, exact local zero helpers and physical dispatch table,
and FND-EXE-059/063 for variable-length outputs and cursor returns. Track
each saved marker, full return, destination push, cursor replacement and
store before the next read. Distinguish normal callee completion from
abort, invalid-pointer and nontermination outcomes. Keep query reports and
identity controls local and execute no interpreter or game.
