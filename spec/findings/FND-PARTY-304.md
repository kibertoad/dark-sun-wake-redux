---
id: FND-PARTY-304
title: Both encoded mode-setter callers clear the list count on their selected-member mode-zero branch
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007AA5A..0x0007AC12
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000960C5..0x0009654D
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0009662B..0x00096EDA
tool: window_operands.py, direct_callers.py and overlay_listing.py at 6bdaefb9; Python 3.12.10, Capstone 5.0.9, xxhash 4.0.1
environment: null
---

## Observation

FND-PARTY-303 leaves admission of a positive count with a non-list mode
unresolved. This checks the own ordering of the known mode-zero setters
and their two encoded callers. All code offsets below are hexadecimal.
The licensed installed DSUN.EXE has size 634416 and XXH3-128
e296af55ba2ecde7e77f555c90f33d0b, as in FND-PARTY-301.

The controlled operand census finds 51 operands for DS:43E8..43E9,
including eight writes: 190+284A/28C5 write zero, 190+28A2/28B0/28BE
write one/two/three, and 211+0C02/0C1C/0C44 write three/one/two.
There are also 23 candidates outside established instruction starts;
they are not admitted as additional executable setters. The bounded
count census DS:9D79..9D7B has five operands, three writes and no
outside-start candidates: zero stores at 211+0600 and +0AFD, and
the builder-count publication at +1AEF from FND-PARTY-279. Its two
reads are FND-PARTY-303's handler gates. The count-touching inventoried
routines contain no string stores. This does not cover every computed,
copied, wider or segment-aliased write.

190+271A has no own count store or child call after either zero store
through its return at +28D1. Its +284A suffix can reselect a mode;
the +28C5 suffix retains zero and stores FF in the corresponding
DS:9D7B member byte. The setter itself is not a combined mode/count
refresh. Its argument supplies the member index; caller order matters.

The direct-call census recovers two encoded callers through trampoline
571F:00D9: 211+04CD and +09D0. The first iterates the setter over four
member indices. The second passes the current 4E71:0B44 member word.
Both later reload that member word and inspect its DS:43E8 mode:

| Caller | Selected-member mode test | Zero branch's own action |
|---|---|---|
| 211+02B5 | +0543 compares with zero | +054A transfers directly to +0600, which stores count zero |
| 211+081B | +0A40 compares with zero | +0A47 transfers directly to +0AFD, which stores count zero |

There is no child call between either test and its zero store. The
nonzero branches perform presentation work and bypass these stores;
that bypass is not a mode-zero counterexample. Calls occur after the
setter and before the selected-member test, and after the count clear.
Consequently this establishes their own tested zero-branch behavior,
not preservation of the mode, member or count across every helper or
until the later handler event. The refresh function 211+1A4F has one
encoded PUSH-CS/near caller at 211+020B; this direct census does not
prove it runs after every possible mode/member change.

## Interpretation

Q-PARTY-134 no longer needs to determine what the two encoded callers
themselves do when their selected-member test observes zero: both clear
the count. Under retained bindings and no subsequent changes, that clear
excludes FND-PARTY-303's positive-count gate and ambient-SI route.

Remaining admission concerns actual mode/member/count histories across
the intervening helpers and later event, alternate setters/transfers,
and writes outside the census. The known eight literal setters do not
produce modes outside zero through three, but initial, restored or other
histories have not been excluded. No entire client, queue item or complete
reading is closed; FMT-PARTY-007 remains supported.

## Alternatives

- The mode setter alone refreshes the count: its own suffix does not.
- The two callers leave the old count on their tested zero branch:
  contradicted by both explicit zero stores.
- These local branches prove every later handler sees consistent state:
  not established; intervening helpers and alternate histories remain.

## How to reproduce

Use window_operands.py with the installed DSUN.EXE, its current committed
inventory and windows 43E8..43E9 and 9D79..9D7B. FND-PARTY-243's mode
setters and FND-PARTY-279/303's count publication/reads are positive
controls. Keep outside-start candidates distinct from owned instructions.
Use direct_callers.py for 190+271A and 211+1A4F. Retain its exclusions
for indirect, jump, bypass and differently bound runtime targets.

Use overlay_listing.py for descriptor 190 file 7AA5A..7AC12 and
descriptor 211 files 960C5..9654D and 9662B..96EDA; respect the inventory's
body gaps. The exploratory setter report extends past its return;
bytes at and after 7AC12 belong to the next helper and support no
claim about the setter. Retained reports mode-count-writers,
mode-refresh-callers, mode-setter190 and mode-caller211-first/second
are in GAME_DIR/analysis/handler-census-20261106/. No original game,
emulator session or emulated call ran.
