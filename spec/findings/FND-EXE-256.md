---
id: FND-EXE-256
title: Second cleanup writer repeats bounded setup before callback publication and retains an endpoint low-word comparison
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4AE5:0AB5..4AE5:0BCF
tool: executable-reader 2.5.0 and Capstone 5.0.7
environment: null
---

## Observation

FND-EXE-178's second callback writer at `4AE5:0AB5` contains 112
instructions and 282 bytes, two near calls and a far return without
additional argument cleanup. It forms a BP frame, reserves two local
bytes, saves DS, loads DS through CS-relative word five and saves SI/DI.
BP-relative accesses use SS. Bit one of DS-relative byte `0x0010` set
returns AX zero before initialization or argument reads.

Otherwise it clears the local byte SS:BP minus two, pushes CS and calls
FND-EXE-179's helper at `4AE5:0ECD`. It combines returned AX and DX by
OR solely to reject a zero result, selecting AX `0xFFFF`. A nonzero
combined result enters argument/state processing; that returned pair is
not retained as the later output extent.

At `4AE5:0AE2` it loads BX/CX from SS:BP plus six/eight. It compares
this low/high pair unsigned against state words `0x003A`/`0x003C` and
raises it to that pair when smaller. It then compares against state words
`0x003E`/`0x0040`, rejecting when larger but admitting equality. It stores
the selected pair back into the two argument words before further checks.
Subtracting selected BX/CX from the latter state pair yields DX:AX at
component widths with borrow.

A zero pair in SS:BP plus ten/twelve leaves that difference selected.
For a nonzero requested pair it compares difference DX to the requested
high word: smaller retains the difference, larger selects the requested
pair. On equal high words, however, it compares difference AX against
state word `0x003E`, not SS:BP plus ten. Unsigned AX at most that state
word retains the difference; otherwise it selects the requested pair.
This exact low-word operand does not describe an ordinary pairwise
minimum of difference and requested length.

It loads ES through CS-relative word seven, compares selected DX:AX
unsigned against ES-relative words `0x356E`/`0x356C` and lowers it to
that pair when larger. It writes the selected low/high words to SS:BP
plus ten/twelve before another possible rejection. If DX is zero it
compares AX shifted right four against DS-relative word `0x011A`, while
saving and restoring AX; a smaller quotient rejects with AX `0xFFFF`.
Nonzero DX skips that threshold check. Earlier argument stores remain
on the explicit rejection paths.

The admitted path pushes CX, BX, DX and AX as four argument words.
It sets DI to `0x0130`, copies current DS to ES through a push/pop and
clears direction. Before the next call it writes six consecutive words
through ES: the selected base BX/CX, its sum with DX:AX at component
widths with carry, and the original base again. The interval is
ES-relative `0x0130..0x013C`. It then pushes CS and calls the helper at
`4AE5:107D`; FND-EXE-179 records that helper's far return with eight
bytes of additional cleanup.

Returned AX nonzero exits before final callback publication, retaining
those earlier explicit stores and any helper effects. Returned zero
checks the local byte SS:BP minus two. Zero increments that byte and
branches back to `4AE5:0AE2`, reloading argument words and current state
bounds; it does not repeat the earlier `4AE5:0ECD` call. Nonzero instead
continues to final publication. With intact local storage, no intervening
writer and normal zero-result calls, this gives two passes through the
`4AE5:107D` call. The native pass bound depends on those conditions:
callee, output and saved-stack aliases are not excluded by this reading.

The final path writes `0x0BFE` to DS-relative word `0x013C`, zero to
word `0x013E`, one to word `0x0112`, sets bit zero of byte `0x0010`,
stores cleanup offset `0x1155` to word `0x0082`, then `0x0D11` to word
`0x0080`, in that order. It clears AX and restores DI, SI and DS, resets
SP from BP, restores BP and returns far. Restoring registers and resetting
SP do not prove intact saved storage. DS identity after the two helpers
and the fresh argument/state values on the second pass remain obligations.

## Interpretation

This supplies the second writer's gates, actual arithmetic operands,
ordered publications and local repeat mechanism. It does not reduce the
requested-length selection to a conventional minimum or infer that the
first setup result alone publishes a cleanup callback. Failures after
argument/header stores retain a different state from the early zero-result
shortcut, and the second pass consumes fresh memory.

Q-EXE-001 and Q-EXE-010 retain native frames and all argument/local-byte
writers, state bounds and segment preservation, external helper effects,
independent extents and aliases, and actual callback-slot admission.
FND-EXE-253's second cleanup body remains a candidate target until these
writer and consumer bindings are connected. No complete_reading or
replacement inventory is established.

## Alternatives

A normal minimum for the requested pair is contradicted by the equal-high
comparison against state word `0x003E`. A single successful helper call
publishing the callback ignores the initially zero local byte and re-entry.
A repeated first helper call is contradicted by the target `4AE5:0AE2`.
A rejection that restores original arguments ignores the preceding stores.

## How to reproduce

At revision `9ebab58`, use FND-EXE-236's original-source identity, region
and default x86-bounds limits. Set entry and sole entries to `0x00040B05`,
with no seeds or summaries. Check interval `0x00040B05..0x00040C1F`,
112 instructions, near call sites `4AE5:0AD5` and `4AE5:0B90`, and far
return without cleanup. The traversal assumes both callees return; its
complete field is not a Standard complete reading or native loop bound.

Decode the interval directly from the shipped source in sixteen-bit
mode with Capstone. Check the low-word operand at `4AE5:0B2C`, each
argument store, four-word call setup, six output words, local byte and
re-entry target. Compare FND-EXE-179's callee cleanup and FND-EXE-253's
candidate target independently. Keep source, configurations and reports
in GAME_DIR and native assumptions unresolved.
