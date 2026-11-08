---
id: FND-CONFIG-249
title: The menu Preferences arm reads two event words and forms an argument-free far opener frame after close
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00067EB0..0x00067F43
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006810E..0x00068139
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000682E4..0x000682F8
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000684C2..0x000684F8
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008B240..0x0008B281
tool: retained FND-CONFIG-243 menu-dispatch/menu-arm reports; overlay_listing.py at 7f5baded for caller exit, close and Preferences entry; reused FND-UI-046/055/065 and FND-CONFIG-235/236
environment: null
---

## Observation

This gives the finite named-frame contract for Q-CONFIG-014's actual menu
parent/event admission. It reuses FND-CONFIG-243's two-caller census and
does not repeat that census or exclude other incoming transfers. Offsets,
stack displacements and addresses are hexadecimal; event/button values and
byte counts are decimal. The approved DSUN.EXE is 634416 bytes, XXH3-128
e296af55ba2ecde7e77f555c90f33d0b.

Overlay181's handler at +0000 saves BP, sets BP to SP, reserves two bytes
and saves SI. Its event kind is the word at SS:BP+6. Kind1 chooses close;
kind6 forwards the 24-byte event starting at that same slot, except for the
special Tab word at BP+12. Kind2 reaches the button table. Its control word
is SS:BP+8; after the first frame/flush children it reloads that word and
copies it to its local BP-2 before the fourteen-entry lookup. These are
different consumers of the event: selection before the children alone does
not prove the later control word unchanged. FND-UI-065 supplies the existing
builder's kind2/button-number interpretation.

On the known control10303 arm at +025E, the handler reads BP+8 again for
the frame-five setter, then invokes the flush child and the menu-close
helper. FND-UI-055 records their order. Temporary setter arguments total
ten bytes and are explicitly removed; the flush word is removed after its
return. The close call is a same-segment near call preceded by pushing CS,
so its callee's far return consumes that constructed four-byte return pair.
On an intact ordinary-return frame these operations leave the same SP at
+025E and the later pre-Preferences call +0281. The handler has no own BP
reassignment between these sites; no callee preservation is inferred.

The close helper +0612 checks its far menu pointer at 4E46:0000 and, when
nonnull, passes that pointer and word one to overlay182 +0128. After ordinary
return, and also on the initial-null route, it clears both DS:1431 and
4E46:0000, then restores its saved BP and returns far. Its child's lifetime,
nonlocal effects and actual native bindings remain conditions.

At +0281 the handler directly far-calls 5782:0048 without pushing opener
arguments. Its ordinary return pair is therefore menu-segment:+0286 at
SS:SP-4 relative to the pre-call SP. FND-CONFIG-243 identifies that exact
trampoline as overlay203 +0000. At the trampoline's first instruction BP
is still the caller's register value; the call itself changes SP by four.
This provides a bounded observation point before requiring a loaded overlay
entry address. It does not assume the trampoline is already patched to its
final jump before the first call.

Preferences +0000 saves BP and sets its own BP. Before its music getter
call, its own word argument to the earlier child is removed. On the intact
ordinary stack model, the getter's four-byte return frame is six bytes below
the outer Preferences return pair. Its saved parent BP lies two bytes below
that outer pair. FND-CONFIG-235/236 separately supplies the enable gates,
getter exits and paired AL store. Actual values and frame retention across
the reached children are not supplied by this static frame arithmetic.

After Preferences eventually returns, the menu jumps to its common tail,
calls 2C5F:0182, produces zero, restores SI and leaves its BP frame before
returning far. The getter/store observer stops inside Preferences and cannot
claim to have observed this later menu return or exit-time reentrant entry.

## Interpretation

The remaining native question has finite fields and points: event kind and
control at the admitted menu BP frame, its caller return pair at BP+2,
pre-arm/pre-opener SP/BP/SS identity, the two menu pointer fields before/after
close, the actual outer Preferences return pair and the saved parent BP.
An observer can compare these at +025E, +0281, the resident Preferences
trampoline, getter entry and paired store without reading arbitrary stack
contents or following released menu backing after close.

Q-CONFIG-014 still needs those actual states and post-child retention. Other
callers, keyboard gate/key retention, reentrant/nonlocal entry, unknown
indirect/constructed transfers and the existing driver/writer/storage
obligations remain. RULE-CONFIG-003 stays supported, with no complete_reading.
No native run, queue closure or parity promotion occurs in this finding.

## Alternatives

- The menu passes its event as new arguments to Preferences: the known call
  pushes no such arguments; the event remains in the parent's existing frame.
- The close helper is an ordinary near-return child: its caller constructs
  a far return pair and the helper ends with a far return.
- Static balanced stack arithmetic proves native preservation: children can
  affect state or frames, so actual paired bindings and values remain needed.

## How to reproduce

Reuse FND-CONFIG-243's retained menu-dispatch.txt and menu-arm.txt in
GAME_DIR/analysis/preferences-root-contract-20261108/. Their source windows
are descriptor181 file67EB0..67F43 and6810E..68139, with established entries
and branch targets. With the committed source-guarded overlay_listing.py,
regenerate descriptor181 file682E4..682F8 and684C2..684F8, and descriptor203
file8B240..8B281. Retained outputs are menu-exit.txt, menu-close.txt and
preferences-entry.txt in GAME_DIR/analysis/preferences-parent-frame-20261108/.
Use the already-controlled trampoline mapping in FND-CONFIG-243. Confirm
argument cleanup and return widths in the instruction reading, not inferred
decompiler parameters. Keep all original-derived outputs local.
