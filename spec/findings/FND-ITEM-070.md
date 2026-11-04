---
id: FND-ITEM-070
title: Startup explicitly clears the equipment flag while nominal save spans do not prove later restore preservation
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:0000..1000:0163
tool: tools/research/exec-census/resident_listing.py at d1b4d0aa; composition with FND-PARTY-215, FND-SAVE-015/016 and FND-COMBAT-035/082
environment: null
---

## Observation

FND-ITEM-069 excludes additional direct byte/word/doubleword operands
only within its searched forms. Q-PARTY-118 retains startup and bulk
writers. FND-PARTY-215 already reads the startup clear and initialization
dispatch; this binds that producer to the flag and separates it from
claims about nominal save ranges or later resource transfers.

### Explicit startup bulk write

Program entry stores its relocated data-segment word in CS:02C4 before
the intervening DOS/startup work. On the path reaching 1000:00C0, it
zeros AX, loads ES from that saved word, sets DI to 39C4 and CX to A4EC
minus DI. It explicitly clears the direction flag, then performs the
repeated byte store at +00D0. There is no child call or conditional
branch between this setup and the store.

With the saved segment retained, valid memory and ordinary completion,
this writes zero to the half-open range ES:[39C4,A4EC), length 6B28 hex.
Offset 6174 lies inside that interval. Thus this startup operation is a
concrete zero producer for the equipment flag in the modeled data segment;
the exact-operand census did not cover it because the repeated store's
destination is implicit ES:DI. The direction is established locally,
unlike a fill that merely relies on inherited direction state.

The clear occurs before the initialization-record dispatcher called at
+0149 and before game main. FND-PARTY-215 records that the dispatcher
can call near or far targets and does not preserve DS/SS around them.
This observation therefore establishes the clear's local result, not
that the flag remains zero through initialization, main, restore, an
equipment action or a later consumer. Earlier service effects, the saved
segment's retention, reaching the clear and memory validity remain stated
conditions; no fresh startup execution was made for this finding.

### Nominal save ranges are a different contract

FND-SAVE-015's shipped pointer/length table and FND-SAVE-016's binding
helper identify the source/destination ranges a normal save/load selects.
Their fixed modeled spans do not explicitly name the flag byte. That
does not amount to a complete exclusion of restoration writes to it.
Some pointers are replaced from runtime globals, table identities can
alias and the lengths used by the lower reader are not the nominal
length array used by the writer.

In particular FND-COMBAT-035/082 read the complete local restoration
request: a nonnull pointer becomes an in/out local destination pointer,
and 38FF:04AB/07B1 receives the sentinel selecting the selected resource's
complete recorded length. The caller does not cap that transfer with
the corresponding 4F38 length or a destination capacity. The special
12000-byte placed-table prefill is also conditional on pointer identity.
Actual resource selection/length, pointer identity, span and later
snapshot preservation must be admitted separately before claiming that
this branch cannot cover a flag byte. No malformed file is constructed,
and no oversized transfer or native overwrite was observed.

These contracts also do not make every restore change the flag. They
show why stable nominal table ranges alone are insufficient evidence
for excluding bulk writes. Other implicit/string writers and computed
or aliased accesses remain Q-PARTY-118's distinct census obligations.

## Interpretation

Q-PARTY-118 no longer treats the startup flag state as having no known
producer: the explicit clear supplies zero before later initializers.
The question retains initialization targets, actual initial/use-time
segment and memory state, other bulk/computed/aliased writers and
preservation across immediate/queued effects. DATA provider/coherence
dependencies stay Q-PARTY-117. No runtime flag value, global no-restore
claim, complete writer census, parity or complete-reading promotion follows.

## Alternatives

Treating the three direct sites as every writer would omit the implicit
startup store. Treating the clear as relying on inherited direction
would omit its explicit CLD. Treating startup zero as the value at a
later effect consumer would skip the intervening targets and calls.
Treating nominal save lengths as restoration caps would substitute the
writer's array for the reader's recorded-resource length contract.

## How to reproduce

Use installed DSUN.EXE length 634416, XXH3-128
e296af55ba2ecde7e77f555c90f33d0b. Run resident_listing.py for
1000:0000..1000:0163. The complete entry-based listing is retained as
equipment-flag-startup.txt under GAME_DIR/analysis/item-removal-20261104/.
Trace the saved data-segment store, ES reload, zero value, exact interval,
CLD and repeated store before the later initialization dispatcher.
Check interval membership for offset 6174 rather than reading a raw file
byte as its runtime initial state.

Reuse FND-SAVE-015/016's complete nominal table and runtime binding
observations and FND-COMBAT-035/082's lower reader argument/length reading.
Keep their pointer/length and actual transfer conditions separate.
No original code, runtime memory or save fixture is committed or executed.
