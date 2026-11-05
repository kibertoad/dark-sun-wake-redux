---
id: FND-ITEM-100
title: Rectangle target enumeration has signed class gates and capacity-free four-byte insertion
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0005E1BF..0x0005E2EC
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0005ABCA..0x0005AC5D
tool: tools/research/exec-census/overlay_listing.py at c3a3fff7; Python 3.12.10, Capstone 5.0.7; composition with FND-EXE-600 and FND-ITEM-074/099
environment: null
---

## Observation

Q-PARTY-116 retains the immediate handler's target-list producer and bounds.
FND-EXE-600 identifies overlay 173 +389F and its insertion child +02AA.
This reads both complete local bodies and binds their footprint to
FND-ITEM-099's caller. Offsets and masks below are hexadecimal; counts
and coordinates in examples are decimal.

### Literal rectangle preprocessing

Let incoming signed words be x0,y0,x1,y1. The helper retains y0 in DI.
If x0>x1, it instead assigns DI=x1 and writes old x0 into argument x1,
leaving argument x0 unchanged. Thus both horizontal argument words become
old x0, and DI no longer holds incoming y0. It next compares DI with y1;
if DI>y1 it exchanges those two values. The resulting y lower bound is DI.
This is not a conventional independent swap of reversed x endpoints.

It forms centre coordinates by adding the resulting horizontal words and
the resulting vertical bounds at word width, then signed dividing each
wrapped sum by two, truncating toward zero. Input ordering and arithmetic
admission remain requirements for a geometric midpoint interpretation.

### Slot, class, position and optional geometry gates

The loop visits slots zero through 319 under intact local/register state.
Only kind byte two at 4F49:0C33+3*slot continues. Its adjacent owner word
selects a 49-byte record through DS:19C9; byte +15 supplies the shift
count for a word value initially one. The bitwise intersection with the
supplied mask must be signed greater than zero, not merely nonzero.
For class values 0..15, bits 0..14 can pass while bit 15 cannot, even
with mask FFFF. Other shift counts require their own admitted domain;
the helper does not add a class-range check.

Object coordinate words +0A/+0C must lie within the resulting signed
inclusive bounds. A nonzero flag byte additionally requires 1B0B:00A4
from centre cell to target cell to return AX zero, with both pairs signed
divided by sixteen and the two extra words zero. FND-ITEM-099's caller
supplies flag zero, so that caller skips this geometry-child request.

Each accepted slot requests 2D40:382F from the retained centre to its
captured coordinates, then calls +02AA with current list count, near list,
slot and returned distance. It increments its own count regardless of
the insertion child's result. There is no early list-capacity stop; all
320 slots remain candidates. Valid owners, table spans, local state and
child preservation are not established by the bounded slot loop.

### Sorted insertion footprint

+02AA takes a count n, near list address, slot word and distance word.
Its signed comparison search locates an insertion position p in 0..n
for admitted nonnegative bounded counts and readable prior records. With
an admitted sorted prefix it orders signed distance ascending and inserts
before existing equal values. It does not validate sortedness itself.

When p differs from n, it requests 289B:01DC to shift 4*(n-p) bytes
from list+p*4 to list+(p+1)*4, with the byte count formed at word width.
It then stores distance at record +0 and slot at +2. FND-ITEM-074 supplies
the mover's overlap contract. Neither producer nor insertion helper accepts
a capacity argument, and the insertion child does not update a shared count.
Near reads/writes use DS; the caller's stack address requires DS/SS identity
and separate valid storage. SI/DI/BP are restored by both local bodies.

With admitted child preservation, accepted count m is at most 320 and
requires 4*m writable bytes, plus readable prior records during insertion.
The helper does not initialize a sentinel or clear unused records.
The producer returns its count in AX; no separate success predicate exists.

### Immediate handler storage consequence

FND-ITEM-099's list begins at BP-11C; its next named local begins at
BP-1C. That 256-byte region holds 64 records. A 65th accepted insertion
requires a record beyond that intended region, regardless of insertion
position, because shifting or the final append reaches record index 64.
There is no local 64-record cap in either reached helper.

This is a conditional footprint requirement, not an observed overflow or
native failure. A proof that handler frame/state survives needs an actual
accepted-count bound at most 64 or other independently admitted storage/
alias evidence. Kind/class/rectangle population may constrain that count,
but the loop's upper slot bound alone does not prove it.

## Interpretation

This removes the target producer's local rectangle, class/slot/position,
optional geometry and insertion/capacity contracts from Q-PARTY-116.
Actual rectangle/class/owner inputs, accepted population, distance effects,
DS/SS and frame aliases, valid list spans and downstream casting remain.
Mask FFFF and a 320-slot census are not a 64-entry list guarantee or proof
of native row preservation. No complete-reading or parity promotion follows.

## Alternatives

Repairing the horizontal swap changes the literal stores. Replacing signed
positive mask intersection with nonzero admits an excluded class bit.
Assuming a caller buffer silently caps output invents a missing argument
and branch. Conversely, the capacity-free loop does not show a native
population exceeding 64 accepted targets. No game defect is claimed.

## How to reproduce

Use licensed DSUN.EXE length 634416, XXH3-128
e296af55ba2ecde7e77f555c90f33d0b and project-local evidence Python -I
with overlay_listing.py descriptor 173, ranges 5E1BF 5E2EC and
5ABCA 5AC5D. GAME_DIR/analysis/item-removal-20261104/ holds
effect-rectangle-target-list.txt and its -repeat pair, SHA-256
25219af7ff90372ec8b5058f23f99cd3b156b8b4ae4fb0e24f032a99b082c981;
effect-target-list-insertion.txt and its -repeat pair, SHA-256
9bc78655cacf4212923643653bdb27d1c36aa8b7af14fecb8754cf25edd9397b.
Reuse FND-ITEM-099's caller frame and FND-ITEM-074's mover conditions.
Keep reports local. No original game or isolated function was executed.
