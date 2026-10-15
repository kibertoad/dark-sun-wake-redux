---
id: FND-COMBAT-049
title: The menu adjustment aligns and bounds the view before optional display work
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2C5F:018C..2C5F:01B2
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2C5F:01F3..2C5F:0271
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2C5F:0139..2C5F:0182
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 362C:06B9..362C:06C4
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006FC5E..0x0006FC72
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006A4C5..0x0006A501
tool: tools/research/exec-census/resident_listing.py, overlay_listing.py, trampoline_target.py and direct_callers.py; Python 3.12.10, Capstone 5.0.7, xxhash 4.0.1
environment: null
---

## Observation

FND-COMBAT-048 identifies the menu branch that requests adjustment
through 2C5F:018C. A complete local reading of that wrapper shows that
it takes a slot word at SS:BP+6 and reads only the low byte of the word
at SS:BP+8. It pushes that byte in AX without defining AH, then pushes
the slot's anchor position_y and position_x words from DS:67BB plus
37 times slot, at record offsets 0C and 0A. It calls 2C5F:01F3 using
push CS and a near call, removes six argument bytes and returns far.
There is no local slot guard or other child call.

The callee reads x and y as words at SS:BP+6/+8 and tests only the
byte at SS:BP+0A. Thus the wrapper's unspecified AH does not affect
that local mode test. This is an argument-width observation, not proof
of independent native stack and DS storage.

### The view calculation precedes every child call

The callee stores the low word of x minus 160, AND FFF8 hexadecimal,
at DS:1408, then the low word of y minus 100, AND FFF8, at DS:140A.
These are the view_left/view_top words used by the menu. All additions
and subtractions below wrap to 16 bits; comparisons are signed words.

It then performs these four tests in order:

1. If view_left is less than DS:14D7, replace it with DS:14D7.
2. If view_top is less than DS:14D9, replace it with DS:14D9.
3. If the low word of view_left plus 320 is greater than DS:14DB,
   replace view_left with the low word of DS:14DB minus 320.
4. If the low word of view_top plus 200 is greater than DS:14DD,
   replace view_top with the low word of DS:14DD minus 200.

The lower-bound replacements precede the upper-bound replacements.
There is no second lower-bound pass after either upper correction, no
later alignment mask in this calculation and no local test of the
rectangle's width, height, ordering or producer domain. Consequently
this is not an unrestricted mathematical clamp that guarantees both
bounds for every possible input rectangle. FND-COMBAT-050 separately
identifies some bound producers; this body does not initialize them.

### Later calls do not establish visible success

After all view stores, it calls 362C:06B9. That complete local helper
only writes word eight to current DS:29EB and returns far; it supplies
no local wait or result normalization.

The callee next compares its mode byte with one. Only equality calls
2C5F:0139 and then 56E9:004D. Other byte values skip both. After those
calls return, or directly on the skipped branch, it writes byte zero
to current DS:143F and returns far. It neither tests child results nor
rolls back its earlier view stores locally.

FND-PARTY-130 already describes the complete local 2C5F:0139 sequence.
The fresh reading agrees: it calls 362C:035C with one and current view
coordinates, 31BA:000E with those coordinates, one and three,
362C:06B9, local 070E with one and local 06CB with one and zero,
then clears byte DS:143F. Transitive drawing and storage effects are
not replaced with assumed successful display operations.

The trampoline 56E9:004D resolves to descriptor 186 +055E, file
6FC5E hexadecimal. Its complete local body writes byte one at current
DS:33BA, then byte one at current DS:2FB8, calls 4328:0B4C and returns.
These are byte writes. The called service's gates and flag clear use
words, as FND-CONFIG-171 records; its other input gates and rendering
effects remain separate. The two byte writes are not a proof that all
of that service's continuation conditions hold.

The menu caller of FND-COMBAT-048 pushes mode word one and its current
slot, ignores the returned registers, then reloads its current slot,
figure coordinates and view words. Those rereads, rather than the
adjustment's returned AX, determine its subsequent menu coordinates.
Their preservation across every child call is not assumed here.

## Interpretation

This answers the wrapper arguments, aligned view calculation, signed
bound-test order and conditional child-call sequence of Q-COMBAT-014.
The observed local calculation is conditional on actual bound inputs
and does not guarantee a fully visible or exactly centered figure.
Admitted bounds, slot/segment/storage identity, preservation across
children and final native presentation remain open.

## Alternatives

- Always centering the figure omits both the alignment and bound tests,
  and confuses its anchor with its separately offset figure position.
- Aligning the final bounded result changes the observed store order.
- Treating the mode argument as an unrestricted word gives significance
  to a high byte the local callee never tests.
- Child return is not visible success or rollback; the parent has
  already written its view words and does not test those results.

## How to reproduce

Use the approved installed DSUN.EXE, size 634416, XXH3-128
e296af55ba2ecde7e77f555c90f33d0b, after Bootstrap-Project.ps1
-ValidateFactsOnly. With the project evidence Python, read
resident_listing.py ranges 2C5F:018C..2C5F:01B2,
2C5F:01F3..2C5F:0271, 2C5F:0139..2C5F:0182 and
362C:06B9..362C:06C4. Their ends agree with the committed inventory.
Resolve 56E9:004D with trampoline_target.py and read overlay_listing.py
descriptor 186, 6FC5E..6FC72; descriptor 182, 6A4C5..6A501.
File bounds are hexadecimal and half-open. Inspect the actual mode-byte
loads, far-return frames, six-byte cleanup, signed branches and every
store before the first call. Cross-check direct_callers.py targets
2C5F:018C and 2C5F:01F3, retaining the known menu call at 6A4DB and
wrapper call at resident file 2199A as controls; its other byte-pattern
hits and indirect transfers are not exhaustive caller evidence.
Reports remain under GAME_DIR/analysis/combat-actions/. These static
queries execute no original function and make no pixel claim.
