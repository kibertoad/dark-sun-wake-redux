---
id: FND-PARTY-309
title: Unchanged shipped APFM fields exclude the member-switch graphics callback routes under the known mode writers
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 3D72:0635..3D72:06D5
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 3F96:0755..3F96:07B3
tool: window_operands.py, direct_callers.py, far_pointer_words.py and resident_listing.py at 8fe510be; Python 3.12.10, Capstone 5.0.9, xxhash 4.0.1; FND-EXE-588, FND-UI-005, FND-CONFIG-189 and FND-PARTY-308
environment: null
---

## Observation

FND-PARTY-308 identifies graphics record +5A/+5E calls as concrete
mechanisms possibly reached before the cast handler's count clear.
This reuses the existing APFM census before following more graphics
code. Installed DSUN.EXE is size 634416, XXH3-128
e296af55ba2ecde7e77f555c90f33d0b. Offsets are hexadecimal.

FND-UI-005 reads all 97 shipped APFM resources as zero at both far
callback fields +5A and +5E. FND-EXE-588 maps their setters and the
mode producer, retaining non-resource records, different-base stores,
block copies and runtime-built pointer exclusions. These conditions
are preserved here; its broad historical title is not an unconditional
absence proof for arbitrary runtime records.

### Current literal mode and pointer census

With the current committed function inventory, window A13D..A13F
has 12 established operands and five writes: 39D1:0236 assigns FFFF,
39D1:06A1 assigns zero, and 3D72:06A2/06C5/06CD assign three/two/zero.
Four outside-instruction-start candidates are retained as candidates,
not admitted stores or transfers. The pointer window A15B..A15F
has six established operands and two writes, at 3D72:06AC and
3F96:079B, matching FND-EXE-588's known controls.

There are three raw pointer-window candidates. The two apparent
word stores at 3DDC:000D and 400F:000C begin inside the known
operand-size-prefixed double-word stores 3D72:06AC and 3F96:079B.
The apparent read at 3DDB:0009 likewise begins inside the preceding
double-word A15B read. They supply no additional established writer.
Alternate transfers into those bytes are not excluded. No string
store appears in the inventoried routines touching either window;
computed/wider/copied/aliased accesses remain outside this census.

### The known producer cannot select mode two from zero fields

3D72:0635 checks the selected APFM record and coordinates, then
can assign mode three when its existing-record conditions hold.
Its alternative +06B2 path tests the record's full double words
+5A and +5E separately. Either zero transfers to +06CD's mode-zero
store. Only both nonzero reach +06C5's mode-two store. The selected
record pointer is then copied from DS:A125 to DS:A15B. No child
call intervenes between the two field tests and the mode-two store.
The initial runtime guard and actual record/segment/frame identity
remain required.

The other known A13D writes produce FFFF, zero or three. Thus the
known literal mode history with unchanged shipped fields cannot
produce mode two for 3D72:0942's +5E call. Independently, an unchanged
zero +5A makes 0B84's own callback test skip that call even if its
separate mode input A155 is two. FND-CONFIG-189 supplies both
service contracts; no assumption that A13D and A155 are identical
is needed for the second exclusion.

### Known setters have no encoded callers in the tested forms

Fresh direct_callers.py searches for 3F96:0211, 025E and 0603 return
no encoded direct sites. The same search recovers all 28 known far
calls to the independent +62 setter 3F96:02AB, including 172+0981.
far_pointer_words.py likewise finds no matching relocated/fixed-up
far pointer to the three unused setters; its one relocated pointer
candidate for offset 0603 names segment 50C3, not 3F96. The +62
setter's known far-call words provide the positive pointer control.

These results exclude only the reporter's encoded/direct and
adjacent relocated/fixed-up pointer forms. Separate runtime-built
words, unrelocated targets, different incoming instruction streams,
unsearched near transfers and direct field writes remain outside
the fresh caller search. FND-EXE-588's earlier record-field writer
assessment is reused under its own bounds; it is not claimed to
be a newly exhaustive writer proof from these pointer searches.

## Interpretation

Unchanged shipped APFM records plus the known mode writers exclude
these two specific graphics callback mechanisms during the interval.
Q-PARTY-134 retains a mode/record/field history that defeats those
conditions, computed or aliased publications and alternate transfers
before treating either as an actual cursor-event route. The ordinary
field-zero case no longer needs a new callback-target reading.

This is a conditional mechanism exclusion, not proof of every reached
service's behavior or absence of all interval events. Other named
helpers, interrupts, later state changes and Q-PARTY-136's preservation
obligations remain separate. No client, queue item or complete reading
closes; FMT-PARTY-007 remains supported.

## Alternatives

- A shipped APFM's zero fields can alone select mode two through
  0635: contradicted by both full-pointer tests.
- The +5E service itself rejects a null target: FND-CONFIG-189 shows
  it does not; the exclusion here relies on the producer history.
- No encoded setter caller means no possible field writer: unsupported;
  the named computed, copied, aliased and alternate-entry forms remain.

## How to reproduce

Run window_operands.py with the current DSUN.EXE inventory and windows
A13D..A13F and A15B..A15F. Reuse the known literal writes above as
positive controls. Read resident 3D72:0635..06D5 and 3F96:0755..07B3
for the producer and containing pointer stores. Run direct_callers.py
and far_pointer_words.py on 3F96:0211, 025E, 0603 and control 02AB.
Reassess FND-EXE-588's explicit resource/writer/search exclusions.

Retained cast-graphics-mode-record-writers, cast-graphics-mode-entry,
cast-graphics-setter-direct-callers and cast-graphics-setter-pointer-words
reports are in GAME_DIR/analysis/handler-census-20261106/. Exploratory
alias-address windows in mode-entry support only the same linear byte
identity, not new entries. No original game, emulator session or
emulated call ran.
