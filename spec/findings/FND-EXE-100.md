---
id: FND-EXE-100
title: Fallback publishers bias direct mappings and append reset indices after mode-dependent object selection
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x00417F90..0x00418097
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x004180A0..0x004181E5
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x004F6F60..0x004F6FB5
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x004F64B0..0x004F64EA
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x006895A1..0x00689603
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x00689764..0x00689796
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x006897E0..0x00689811
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x00689D81..0x00689DDF
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x00689F44..0x00689F76
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x00689FC0..0x00689FF1
tool: Ghidra 12.1.3 PUBLIC bounded caller, publisher and selector reading
environment: null
---

## Observation

FND-EXE-099 physically identifies larger byte and word fallback entries
`0x00689470` and `0x00689C50`. Their continuations call two publishers,
`0x004180A0` and `0x00417F90`, before reentering source reads. Each publisher
pushes four saved registers, reserves twelve more stack bytes, and reads full
first input J at current ESP plus thirty-two and second input K at plus
thirty-six. It saves J shifted left twelve modulo thirty-two bits as bias B,
then calls `0x004F6F60` with K. The returned object O is retained before
checking J and K, each unsigned, against 1048575. If either is larger,
the publisher supplies fixed pointer `0x0071F405` to the byte-transfer
callee `0x0058F890` in FND-EXE-099. Object selection has already occurred;
there is no rollback in this direct path or locally established safe normal
return from the transfer call.

The selector `0x004F6F60` reads its full first input K. If the full limit
at `0x01D4A3C8` is unsigned greater than K, it reads the pointer at
`0x01D4A3CC` and returns its full indexed word at K times four. Otherwise
it retains lower bound L from `0x01D5E3D8`. If L is at most K and the
fresh upper bound at `0x01D5E3DC` is greater than K, it returns the word
at `0x01D5E3E4`. Failing that interval, it separately tests L plus 4096
at most K and L plus 4112 greater than K, with additions modulo thirty-two
bits. That interval returns `0x01D5E3E8`'s word; all other cases return
fixed address `0x01D4A3A8`. All comparisons are unsigned. No calls,
object-null check or local bound-producer validation occurs in this body.
The intervals' additions have no separate overflow guard.

After admission, each publisher initially reads reset-list count N from
`0x01B5B6D0`. Unsigned N greater than 32767 runs an inline reset loop:
read N consecutive full indices starting at `0x01B5B6D4`; for each, clear
its full direct entries in `0x0075B6D0` and `0x00B5B6D0`, then put
`0x01B7BB28` in its full fallback entries in `0x00F5B6D0` and
`0x0135B6D0`. Each nonfinal remaining count is published to
`0x01B5B6D0`. This repeats FND-EXE-098's retained-count pattern; it is
not a local proof of the list's actual allocated extent or valid indices.

For `0x00417F90`, completion of that flush clears the count, then stores K
at metadata array `0x0175B6D0` indexed by J. Without the flush it stores
that metadata directly. It reads byte flags at O plus four. Flag bit one
absent gives a zero direct read entry. Flag bit one present calls O's
current first-word table target at offset thirty-two with O and K; after
normal return it subtracts saved bias B from full EAX and publishes that
result to `0x0075B6D0[J]`. It then publishes O to `0x00F5B6D0[J]`,
freshly reads the reset-list count, publishes distinct fixed object
`0x01B7BB18` to `0x0135B6D0[J]`, and zero to `0x00B5B6D0[J]`.
Finally it writes J at `0x01B5B6D4` plus fresh count times four and
publishes that count plus one. The read-object flag bit two does not
select a write-side virtual target in this helper.

For `0x004180A0`, the flush route stores K in `0x0175B6D0[J]` before
clearing the final count, then reads O's full word at offset four. The
nonflush route likewise stores metadata before reading those flags. Bit
one present calls current table offset thirty-two with O and K and
publishes its full return minus B to `0x0075B6D0[J]`; it then freshly
reloads the flags word. Bit one absent publishes full zero to that direct
entry and uses the previously read flags' bit two. Bit two present calls
current table offset thirty-six with O and K, subtracts B from full EAX
and retains the result for the second direct table. Bit two absent uses
zero for that result. The common tail, in order, publishes O to
`0x00F5B6D0[J]`, freshly reads the list count, publishes O also to
`0x0135B6D0[J]`, publishes the retained write result to
`0x00B5B6D0[J]`, appends J at the fresh count's list position and
publishes count plus one. It does not install the separate fixed write
object used by the other helper.

The methods never normalize either virtual pointer result to a success
boolean or reject a zero result before bias subtraction. Later source
readers test the published biased word, not the virtual result directly.
FND-EXE-094 adds the full requested address to a nonzero direct entry:
conditionally, with unchanged mapping and address A in J's page,
(R minus (J shifted left twelve)) plus A equals R plus A's low twelve
bits modulo thirty-two bits. This explains the bias without proving R's
validity, allocation, returned extent or identity with another table.
Virtual effects, aliases and concurrent state remain conditional. In
particular, the count initially tested against 32767 is not the fresh count
used by the append after an indirect call; this body alone does not prove
that fresh count is bounded by the earlier test.

The byte source helper `0x004F64B0` reads its full input address A,
indexes `0x0075B6D0` by A shifted right twelve, and for a nonzero entry
reads a byte at entry plus full A. A zero entry reads the parallel fallback
object, then its current first-word table target offset eight with object
and A. The ordinary return is zero-extended AL in either case. It has no
local object/target-null guard. This is a possible reentry into the larger
fallback byte method, not a direct proof that publication eliminated it.

On the larger methods' zero-mode routes, page index J is compared unsigned
with 271. J at most 271 chooses the metadata-like word from
`0x01B7B6D4[J]`; a larger J uses J itself as K. Both call publisher
`0x004180A0`, clear their local cleanup selector to zero, then supply
original A to a source reader: `0x004F64B0` for the byte method and
`0x004F6570` for the word method. The byte method saves returned AL in
its local byte and returns it zero-extended; the word method saves returned
AX zero-extended and returns that saved full value. The zero selector
skips their local post-read temporary-list restoration route.

Other locally admitted routes can instead set that selector to one, call
`0x004180A0`, and join the same source reader without clearing it. After
normal read return, a nonzero selector freshly reads list count; if count
is nonzero and the last stored index equals original A shifted right twelve,
it publishes count minus one, then the four ordered reset stores at that
index. The byte route's first fallback pointer is retained in EDI and the
word route's in EDX; both supply `0x01B7BB28` to each fallback table.
Neither erases the consumed list word. After this route or a failed last-index
comparison, selector greater than one calls `0x00417F90` with original
page first and retained selector second. The already saved source result
is returned after normal completion of that restoration call. Selector
one skips it. The incoming selector's wider admission and preceding flag
updates are not established by these post-read windows.

## Interpretation

Mapping publication and subsequent source reading are distinct operations.
The publishers append the page to the retained reset list and can expose
biased direct pointers or fallback objects; they differ in write-side target
selection and object identity. The larger fallback methods can read through
those newly published tables and then remove or restore temporary mappings
while retaining the source value. No ordinary read termination, object
construction, indirect target extent, complete flag route or final PATH list
content follows solely from these bounded paths. Q-EXE-009 remains open in
FMT-EXE-006.

## Alternatives

- A range rejection does not precede every effect: object selection occurs
  before the publishers' local J/K bounds checks.
- The two publishers are not equivalent. One selects write virtual offset
  thirty-six and keeps O in both fallback arrays; the other supplies a
  distinct fixed write object and clears its direct write entry.
- A virtual return of zero is not the same value as a zero biased entry.
  Pointer arithmetic and the later table test must be followed separately.
- The first count check does not bound the fresh append count after an
  unresolved call. Nor does appending prove unique indices or complete
  list capacity.
- A source read is not rolled back by temporary mapping cleanup. The result
  is retained before the count/reset and optional restoration effects.
- Reentry through a source helper does not prove progress; a null biased
  entry or changed object can reach fallback again.

## How to reproduce

Use FND-EXE-011's shipped identity and FND-EXE-099's physical target controls.
Read the saved Ghidra program statically with -noanalysis. Window queries:
`0x0068955B` limit 200; `0x00689D3B` limit 200; `0x004180A0` limit 90;
`0x00417F90` limit 80; `0x004F64B0` limit 30; `0x004F6F60` limit 30.
Keep full reports in GAME_DIR/analysis/exe-batches and restrict each observation
to the location listed above. Larger-method prefixes are FND-EXE-099;
word source reading is FND-EXE-094; reset-list pattern is FND-EXE-098.
No negative search or complete caller/writer claim is made.

Track the publishers' stack reservation and saved-register pushes when
identifying J/K. Follow K into the selector before the range tests, the retained
bias, each virtual call's arguments, the fresh flags and count reloads, ordered
mapping writes and append. For larger caller continuations, retain source AL/AX
before cleanup and distinguish the zero and nonzero selector routes. Do not
infer that the 200-instruction caps establish all caller branches or unresolved
callee effects. Keep originals, analyzer output and databases out of Git and
run neither the interpreter nor the game.