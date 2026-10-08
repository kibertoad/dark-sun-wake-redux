---
id: FND-EXE-239
title: Shared cleanup gate explicitly clears carry on its sole decoded normal return
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4AE5:0C2E..4AE5:0CD5
tool: executable-reader 2.5.0 and Capstone 5.0.7
environment: null
---

## Observation

FND-EXE-238's initial carry gate at `4AE5:0C2E` decodes from the shipped
source as 61 instructions and 167 bytes, ending exclusively at `4AE5:0CD5`.
Its only decoded normal exit is a near return at `4AE5:0CD4`, with no extra
argument cleanup. Every reached path to that exit joins the same ending:
explicit carry clear at `4AE5:0CD2`, restore saved DS at `4AE5:0CD3`, return.
No intervening arithmetic or conditional instruction changes carry between
the clear and return. This is a conditional explicit-instruction result,
not native execution or proof that the saved return address is unchanged.

The gate saves incoming DS and loads DS from incoming AX before accessing
its state words. It also saves BP and ES. The bounded CFG includes six
direct call sites, all assumed to return:

| Site | Target |
| --- | --- |
| `4AE5:0C48` | `4AE5:0C22` |
| `4AE5:0C57` | `4AE5:0CD5` |
| `4AE5:0C72` | `4AE5:0D04` |
| `4AE5:0C8C` | `4AE5:0C22` |
| `4AE5:0C97` | `4AE5:0CD5` |
| `4AE5:0CC4` | `4AE5:0D04` |

There are state/header memory writes between entry and the final carry clear.
Calls and intermediate borrows can affect the selected paths, but their flags
do not become a carry-set normal return: the final explicit clear follows
them. The source traversal reports no missing instruction gaps, unresolved
computed transfers or interrupt instructions within this candidate. This
does not exclude effects inside callees or asynchronous interrupt changes.

FND-EXE-238's caller tests carry immediately after this gate and would skip
the far callback if set. Under the declared entry and returning-call CFG,
the gate's decoded normal return supplies carry clear and takes the caller's
other arm. This does not prove that the native call occurs, completes or
preserves the caller's storage.

## Interpretation

This closes one explicit flag-result dependency of the candidate cleanup
path, rather than assigning a guessed failure meaning to its carry test.
It does not establish the gate's resource semantics, its state-word writers,
all effects of its three distinct callees, stack-slot preservation or native
return behavior. A caller's conditional branch alone is not evidence that
the callee has an ordinary path producing both flag values.

Q-EXE-001 and Q-EXE-010 retain incoming state/segment admission, all three
callee readings, writes and effective aliases, saved-stack integrity,
interrupt-enabled changes and the remaining cleanup dependencies. No
complete_reading or replacement inventory is established.

## Alternatives

A carry-set normal return representing gate failure is contradicted by the
sole decoded exit's explicit clear. Passing an intermediate subtraction's
borrow or callee's carry unchanged to the caller likewise ignores that clear.
Guaranteed native success or no state changes would go beyond this flag
reading and are not established.

## How to reproduce

At revision `77e42ea`, use FND-EXE-236's original-source region, source hash
and default x86-bounds settings. Set entry and sole entries value to
`0x00040C7E` (`4AE5:0C2E`), with no seeds or summaries. Check the 167 covered
bytes, 61 instructions, six returning-call assumptions and sole near return.
CFG completion is not a Standard complete reading.

Independently hash-check the shipped source against XXH3-128
`e296af55ba2ecde7e77f555c90f33d0b`. Using Capstone 5.0.7 in x86 sixteen-bit
mode, decode only file interval `0x00040C7E..0x00040D25`, initial IP `0x0C2E`.
Inspect all branches and the ending above, keeping callee effects conditional.
Read FND-EXE-238's immediate caller carry test separately. Sources,
configurations and listings remain in GAME_DIR.
