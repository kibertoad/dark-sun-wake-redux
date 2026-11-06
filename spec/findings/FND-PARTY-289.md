---
id: FND-PARTY-289
title: Reflection helpers preserve DI intrinsically, by a saved word, or only on the getter's local return branches
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00062AC5..0x00062B1E
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2C5F:0003..2C5F:0051
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007FD94..0x0007FE93
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 38FF:04AB..38FF:04D5
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 38FF:07B1..38FF:08D4
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 38FF:08D7..38FF:09D6
tool: tools/research/exec-census/overlay_listing.py and resident_listing.py at 93ab66fa; Python 3.12.10, Capstone wheel 5.0.9 and xxhash 4.0.1; reuse FND-ITEM-057, FND-PARTY-167/288 and FND-COMBAT-120
environment: null
---

## Observation

Q-PARTY-134 retains DI preservation after FND-PARTY-288's reflection
interval test. Installed DSUN.EXE is 634416 bytes, XXH3-128
`e296af55ba2ecde7e77f555c90f33d0b`. This reads the three named
helpers and the getter's immediate resource-reader boundary. Offsets
below are hexadecimal.

### The level helper has no DI dependency

The complete overlay 177 +09C5 body saves SI, reads its number
argument and boundary-table bytes, and returns an AL result through
one epilogue. It has no child call, interrupt, DI/EDI assignment or
memory write except its stack pushes. In ordinary admitted execution,
it preserves DI without depending on its input number, table contents
or a child register convention. Both calls on the bounded reflection
route can therefore be removed from the unknown DI-clobber list.
This does not prove valid table storage or a particular level result.

### The effect helper restores a specific saved DI word

Overlay 193 +1AA4 pushes DI at +1AA8, before replacing DI with
its slot argument at +1AA9. All normal own branches converge on
POP DI at +1B9F, then restore SI/BP and far-return. Its child calls
are 3143:0061, overlay 195 +147C and local +1873. Their live-DI
clobbers need not preserve the incoming caller value: this helper
restores that value from its own saved word, SS:BP-4.

The remaining obligation is the saved word and normal continuation,
not a blanket child-register assumption. Reached effects/record writers,
aliases, nested frame use and removal/copy effects must not change
that word or the return frame. FND-PARTY-167 already records the
effect-byte mutation and removal meaning. A push/pop pair by itself
does not admit its backing storage or rule out those effects.

### Getter hit and miss have different register contracts

The complete 2C5F:0003 body saves SI and uses no DI or EDI operand.
Its FFFF-selector return and equal-key cache-hit return have no child
call, so their ordinary local paths preserve DI intrinsically. On a
miss, however, it calls 38FF:04AB at 0037 without saving DI.
Its subsequent local result/key branches do not restore DI either.
FND-ITEM-057 supplies the cache/pointer contract; an accepted return
does not independently establish register preservation or content validity.

38FF:04AB has no own DI operand/save and forwards to 07B1. Its
stack-guard branch can call 1000:2E48 before forwarding. The complete
07B1 owned instruction ranges likewise have no own DI/EDI operand
or save/restore. Their reached targets are:

- stack guard 1000:2E48;
- archive signature/type/plain-number/traversal helpers 39A9:012E,
  0045, 01BC and 0157;
- indexed lookup 38FF:0615;
- allocation wrapper 444C:0008;
- archive preparation 37FC:08FB;
- positioning 38FF:0A72 and transfer 44DE:01AF.

Only reached paths need the corresponding child contract. Absence of
an own DI write in the wrappers does not prove the children preserve
it. The miss-path obligation is therefore this explicit target set,
normal guard/return behavior and actual state/frame/alias admission;
the resource service's unrelated users are not included.

## Interpretation

Q-PARTY-134 no longer needs a DI reading of 177 +09C5 or the
getter's ordinary cache-hit path. Overlay 193 +1AA4's register
restoration mechanism is answered; its SS:BP-4 and return-frame
preservation remain. Getter misses retain the explicit reached-child
contracts above rather than an unspecified helper-preservation caveat.

These are interface/effect results for FND-PARTY-288's producer chain,
not whole acquisition or effect-subsystem closure. The queue's
number/index/kind lifetime, alternative number producer, eighty other
direct getter sites and three word-client domains remain. No whole
getter site or queue item closes; FMT-PARTY-007 stays supported and
no complete reading is declared.

## Alternatives

- A leaf with no DI writes and a nonleaf with no own DI writes do not
  supply the same evidence. The getter miss needs reached-child effects.
- A restoring helper may tolerate live child-register changes while still
  requiring its saved memory and continuation to survive.
- Cache-hit register preservation does not prove the cached buffer matches
  its key, or that a later read remains a hit.

## How to reproduce

Run overlay_listing.py for descriptor 177 at 62AC5..62B1E and
193 at 7FD94..7FE93. Run resident_listing.py with the four resident
locations above; the 08D4..08D7 inventory gap is not decoded as a
continuation. File bounds are hexadecimal and half-open.
Reports di-preservation177-09C5, di-preservation-data-getter,
di-preservation193-1AA4 and di-preservation-resource-reader remain
under GAME_DIR/analysis/class-table-closure-20261106/.
Check all own exits, DI/EDI operands and reached calls, and locate the
saved DI word relative to BP separately. No original game ran.
