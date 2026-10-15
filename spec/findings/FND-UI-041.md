---
id: FND-UI-041
title: Window callback setters and refresh have direct overlay callers, including the effect window
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 3A8E:05AA..3A8E:05EF
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 3A8E:060D..3A8E:08E3
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 3A8E:0CDC..3A8E:0D6C
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 3A8E:1048..3A8E:10A7
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00068946..0x00068978
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008A776..0x0008A815
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008A690..0x0008A6AE
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008A9E7..0x0008AA6A
tool: tools/research/exec-census/direct_callers.py, trampoline_target.py, resident_listing.py and overlay_listing.py; installed-build fingerprint and relocation/fixup checks
environment: null
---

## Observation

This replaces FND-UI-009. Its local before/after-child callback
observations survive, but its single-direct-caller and no-direct-setter-
caller claims do not cover the executable's overlay calls.

### Direct-call counterexamples

The relocation/fixup-aware direct_callers.py query recovers the known
resident near call at 3A8E:107A as a control. It also returns overlay
far-call candidates, including these qualified instruction sites:

| Caller | Target | Qualification |
|---|---|---|
| Overlay 182 +0110, file 68960 | 3A8E:02B3 | Reached local nonnull-window branch of exported wrapper +00F6. |
| Overlay 182 +01CC, file 68A1C | 3A8E:060D | Previously read creation wrapper +016B, FND-COMBAT-040. |
| Overlay 202 +034C, file 8A7EC | 3A8E:0D0C | Effect-window setup +0287 supplies 577D:0043. |
| Overlay 202 +035F, file 8A7FF | 3A8E:0D3C | The same setup supplies 571F:00A7. |

The resident targets follow the overlay operand fixup word 0140,
descriptor 40. These are encoded direct far calls, not merely a
callback pointer found in data. They are sufficient counterexamples
to the older caller-absence claims; the full list of raw candidates
is not treated as a complete reached-caller census.

The query searches encoded direct near calls and relocated/fixed-up
far calls. Computed calls, jumps and unsupported aliases remain outside
that search. The independently known resident call controls the near
kind, and the setup's known literal setter calls control the overlay
far kind. No absence conclusion is drawn for excluded kinds. Earlier
Ghidra reference results cannot be promoted to whole-program absence
when its recovered references omit these overlay sites.

### Preserved field and dispatch observations

At the listed registration tail, the loaded window's +F5 field receives
DS:A0F1, then its +F9 and +FD fields are cleared. These are distinct
double-word assignments. The three setters 3A8E:0CDC, 0D0C and 0D3C
take a window far pointer and callback far pointer. After their stack
guard, a nonnull window receives the callback at +F5, +F9 and +FD
respectively. A null window skips the store. Each normal return sets
AX to zero, including the null-window bypass. Their guard, window
validity and any aliasing remain admission conditions.

The local refresh routine 3A8E:060D keeps its supplied window pointer
in a local double word and rejects null with AX = FFFF. On its
continuing path it may update the linked-window chain, records the
supplied pointer in DS:A11D, and sets up presentation state. It does
not directly assign that pointer to DS:1431 in its own body. These
two named globals cannot be substituted for each other on the basis
of a common informal name such as current window.

Before child dispatch it tests +FD and, when nonnull, calls through
that field with the window far pointer as the four-byte argument.
Nonzero returned AX takes the FFFF exit; zero continues. After the
child loop it similarly tests/calls +F9 and tests full AX. Successful
later cleanup returns zero. This preserves the order before_children,
children, after_children described by FMT-UI-001, without claiming
that every callback returns or that its effects preserve the window.

The child loop reads the loaded child_count and child records. Its
indirect tag dispatch and presentation/cleanup callees remain separate
contracts; this replacement does not establish all backend behavior or
prove exclusive program-wide reads of +F9/+FD. The older resident
loop 3A8E:1048 remains a real direct caller: it visits the linked-window
chain, skips records with flags value 0x0004 set and calls 060D for the others.
Its existence is retained, its supposed exclusivity is not.

### Effect-window handoffs and pointer lifetime

FND-COMBAT-040 identifies the setup's create/reuse split. Creation
stores its returned pointer in both DS:2690 and DS:1431. Reuse instead
calls 3A8E:0CDC with DS:2690 and 577D:002F. The setter's own successful
or null-bypass zero return cannot establish that DS:1431 was rebound
to the reused object; its body contains only the conditional +F5 store.
The setup's subsequent diagnostic branch tests AX, but that local
setter return is zero on both normal branches.

The setup separately requests +F9 = 577D:0043 on DS:2690 and
+FD = 571F:00A7 on DS:1431. Trampoline 577D:0043 enters overlay 202
+0000. In that callback's listed selected-owner branch, it reads owner
4E71:0B44, indexes the table at DS:19C5 with stride 66, and calls
the icon builder +07CF only when the record word at +0E is nonzero.
This supplies a concrete post-child callback route to the builder in
FND-COMBAT-038. Calls and data-table admission before this branch
remain dependencies; no window argument is passed to that near icon
call to replace the builder's own global pointer reads.

The close entry +0547 first records the current owner in other state,
requests a null hook through 39D1:0430 and conditionally passes the
background-image pointer at 4E71:002F to 444C:0092. It writes that
call's returned pointer back, retaining FND-CONFIG-165's distinction
between returned zero and successful release.

If DS:2690 is nonnull, close requests overlay 182 +0128 with that
window and low flag zero, ignores its returned AX, then explicitly
clears DS:2690 and DS:1431. If DS:2690 is null it skips both clears.
This is the local cleanup ordering, not proof that a failed close
restores earlier pointers or that every DS:1431 value names this window.
FND-COMBAT-039 retains the frame-event/removal branches and their own
shared cache/owner requirements.

## Interpretation

The callback fields have direct overlay setup and refresh paths. The
older claim that only indirect use could explain their setters is
superseded. FMT-UI-001's before/after field meanings keep direct
evidence from the actual clear, setter and dispatch bodies.

For Q-COMBAT-010, the effect window now has a qualified local +F9
producer/consumer route to icon construction, and explicit create,
reuse and close pointer operations. Actual DS:2690/DS:1431 identity
across reused windows, nested callbacks and all other writers remains
open. Calling generic refresh does not itself prove that game-level
global identity from an own direct assignment.

Q-UI-002 retains complete screen/handler and native control-state
admission. Q-COMBAT-010 retains loaded graph, archive/buffer validity,
callback completion and presentation outcomes. No complete-reading
declaration, original run or implementation is supplied.

## Alternatives

- FND-UI-009's wrong inference was program-wide caller absence from
  incomplete Ghidra references. The qualified overlay calls above
  contradict it. Its clear/setter/before-and-after observations are
  retained here; its original text remains in the superseded record.
- A setter returning zero for null input does not prove that a live
  callback was installed. A refresh argument stored at DS:A11D does
  not prove the same value is in DS:1431.
- Conditional callback order is not successful drawing or termination.
  Pointer validity, callback effects, linked-list state and the stack
  guard remain prerequisites, not harmless-return substitutions.
- Clearing saved pointers after a close request is not a checked
  close/release transaction. No native failure case is asserted.

## How to reproduce

Use the locked evidence Python and fingerprinted installed DSUN.EXE.
Run direct_callers.py for 3A8E:02B3, 3A8E:060D, 3A8E:0D0C and
3A8E:0D3C. Qualify the listed overlay callers and retain the known
resident 107A call as a near-call control. Resolve 577D:0043 with
trampoline_target.py rather than treating its offset as the body entry.

Read resident_listing.py for the listed resident ranges and
overlay_listing.py descriptor 182 at 68946..68978, descriptor 202 at
8A776..8A815, 8A690..8A6AE and 8A9E7..8AA6A. Bounds are hexadecimal
and half-open. Follow pointer-store destinations, normal null/nonnull
setter returns, before/after callback AX tests and the actual global
loads at the icon-builder gate. Keep original-derived reports/listings
only in GAME_DIR/analysis/combat-effects/. Preserve the query-model
exclusions when reproducing the counterexamples.
